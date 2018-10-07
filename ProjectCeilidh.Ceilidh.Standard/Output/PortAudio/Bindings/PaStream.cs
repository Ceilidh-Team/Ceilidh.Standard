using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using ProjectCeilidh.Ceilidh.Standard.Decoder;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal sealed class PaStream : PlaybackHandle
    {
        private const int BUFFER_CHAIN_LENGTH = 5;
        private const ulong FRAMES_TO_BUFFER = 256uL;

        private static readonly PaStreamCallback PaStreamCallbackDelegate = StreamCallback;
        private static readonly PaStreamFinishedCallback PaStreamFinishedCallbackDelegate = StreamFinishedCallback;
        private readonly IntPtr _stream;
        private GCHandle _handle;
        private SemaphoreSlim _queueCount;
        private SemaphoreSlim _poolCount;
        private readonly ConcurrentQueue<BufferContainer> _dataQueue;
        private readonly ConcurrentBag<BufferContainer> _bufferPool;
        private Thread _dataThread;
        private readonly ManualResetEvent _requestThreadTermination;
        private readonly PortAudioContext _paContext;
        private volatile bool _isSeeking;

        public override AudioStream BaseStream
        {
            get;
        }

        public override event PlaybackEndEventHandler PlaybackEnd;

        public unsafe PaStream(AudioStream baseStream, int deviceIndex)
        {
            BaseStream = baseStream;
            _paContext = PortAudioContext.EnterContext();

            var paStreamParameters = new PaStreamParameters
            {
                ChannelCount = baseStream.Format.Channels,
                DeviceIndex = deviceIndex,
                SampleFormats = baseStream.Format.DataFormat.GetSampleFormat(),
                SuggestedLatency = PortAudio.GetDeviceInfo(deviceIndex).DefaultLowOutputLatency,
                HostApiSpecificStreamInfo = IntPtr.Zero
            };

            _handle = GCHandle.Alloc(this);

            var err = Pa_OpenStream(out _stream, null, &paStreamParameters, baseStream.Format.SampleRate, 256uL,
                    PaStreamFlags.NoFlag, PaStreamCallbackDelegate, GCHandle.ToIntPtr(_handle));
            if (err < PaErrorCode.NoError) throw new PortAudioException(err);

            err = Pa_SetStreamFinishedCallback(_stream, PaStreamFinishedCallbackDelegate);
            if (err < PaErrorCode.NoError) throw new PortAudioException(err);

            _dataQueue = new ConcurrentQueue<BufferContainer>();
            _bufferPool = new ConcurrentBag<BufferContainer>();

            for (var i = 0; i < BUFFER_CHAIN_LENGTH; i++)
            {
                var buffer = new byte[FRAMES_TO_BUFFER * (ulong)baseStream.Format.BytesPerFrame];
                var readLength = WriteAudioFrame(buffer);
                _dataQueue.Enqueue(new BufferContainer(buffer)
                {
                    ReadLength = readLength
                });
            }

            _queueCount = new SemaphoreSlim(BUFFER_CHAIN_LENGTH, BUFFER_CHAIN_LENGTH);
            _poolCount = new SemaphoreSlim(0, BUFFER_CHAIN_LENGTH);
            _requestThreadTermination = new ManualResetEvent(false);
            _dataThread = new Thread(DataThread);
            _dataThread.Start(this);
        }

        private int WriteAudioFrame(byte[] buffer)
        {
            int i = 0;
            int num;
            for (num = 0; i < buffer.Length; i += num)
            {
                if ((num = BaseStream.Read(buffer, i, buffer.Length - i)) <= 0)
                {
                    break;
                }
            }
            return num;
        }

        private static void DataThread(object ctx)
        {
            if (!(ctx is PaStream stream)) return;

            while (!stream._requestThreadTermination.WaitOne(0))
            {
                if (!stream._poolCount.Wait(10)) continue;

                BufferContainer result;
                while (!stream._bufferPool.TryTake(out result)) { }
                result.ReadLength = stream.WriteAudioFrame(result.Buffer);
                stream._dataQueue.Enqueue(result);
                stream._queueCount.Release();
            }
        }

        private static void StreamFinishedCallback(IntPtr userData)
        {
            var handle = GCHandle.FromIntPtr(userData);

            if (!handle.IsAllocated || !(handle.Target is PaStream stream) || stream._isSeeking) return;

            stream.PlaybackEnd?.Invoke(stream, EventArgs.Empty);
            handle.Free();
            stream._requestThreadTermination.Set();
            stream._dataThread.Join();
        }

        private static unsafe PaStreamCallbackResult StreamCallback(IntPtr input, IntPtr output, ulong frameCount, in PaStreamCallbackTimeInfo timeInfo, PaStreamCallbackFlags statusFlags, IntPtr userData)
        {
            GCHandle handle = GCHandle.FromIntPtr(userData);

            if (!handle.IsAllocated || !(handle.Target is PaStream stream)) return PaStreamCallbackResult.Abort;

            stream._queueCount.Wait();

            BufferContainer result;
            while (!stream._dataQueue.TryDequeue(out result)) { }

            fixed (byte* source = result.Buffer)
                Buffer.MemoryCopy(source, output.ToPointer(), result.Buffer.Length, result.Buffer.Length);

            var res = result.ReadLength <= 0 ? PaStreamCallbackResult.Complete : PaStreamCallbackResult.Continue;

            stream._bufferPool.Add(result);
            stream._poolCount.Release();

            return res;
        }

        public override void Start()
        {
            PaErrorCode paErrorCode = Pa_StartStream(_stream);
            if (paErrorCode < PaErrorCode.NoError)
            {
                throw new PortAudioException(paErrorCode);
            }
        }

        public override void Seek(TimeSpan position)
        {
            _isSeeking = true;
            var err = Pa_AbortStream(_stream);

            if (err < PaErrorCode.NoError) throw new PortAudioException(err);

            _requestThreadTermination.Set();
            _dataThread.Join();
            _requestThreadTermination.Reset();
            _poolCount.Dispose();
            _queueCount.Dispose();
            BaseStream.Seek(position);

            var array = new BufferContainer[BUFFER_CHAIN_LENGTH];

            var i = 0;
            for(;_bufferPool.Count > 0; i++)
                while (!_bufferPool.TryTake(out array[i])) { }

            for (; _dataQueue.Count > 0; i++)
                while (!_dataQueue.TryDequeue(out array[i])) { }

            foreach (var buffer in array)
            {
                buffer.ReadLength = WriteAudioFrame(buffer.Buffer);
                _dataQueue.Enqueue(buffer);
            }

            _queueCount = new SemaphoreSlim(5, 5);
            _poolCount = new SemaphoreSlim(0, 5);
            _dataThread = new Thread(DataThread);
            _dataThread.Start(this);

            _isSeeking = false;
            Start();
        }

        public override void Stop()
        {
            var err = Pa_StopStream(_stream);
            if (err < PaErrorCode.NoError) throw new PortAudioException(err);
        }

        private void ReleaseUnmanagedResources()
        {
            Pa_CloseStream(_stream);
            _paContext.Dispose();
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (!disposing) return;

            _requestThreadTermination.Set();
            _dataThread.Join();
            _requestThreadTermination.Dispose();
            _poolCount.Dispose();
            _queueCount.Dispose();
            BaseStream.Dispose();
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PaStream()
        {
            Dispose(false);
        }

        private class BufferContainer
        {
            public int ReadLength
            {
                get;
                set;
            }

            public byte[] Buffer
            {
                get;
            }

            public BufferContainer(byte[] buffer)
            {
                Buffer = buffer;
            }
        }

        #region Native

        [DllImport(PortAudio.LIBRARY_NAME)]
        private static extern unsafe PaErrorCode Pa_OpenStream(out IntPtr stream, PaStreamParameters* inputParameters, PaStreamParameters* outputParameters, double sampleRate, ulong framesPerBuffer, PaStreamFlags streamFlags, PaStreamCallback streamCallback, IntPtr userData);

        [DllImport(PortAudio.LIBRARY_NAME)]
        private static extern PaErrorCode Pa_OpenDefaultStream(out IntPtr stream, int numInputChannels, int numOutputChannels, PaSampleFormats format, double sampleRate, ulong framesPerBUffer, PaStreamCallback callback, IntPtr userData);

        [DllImport(PortAudio.LIBRARY_NAME)]
        private static extern PaErrorCode Pa_CloseStream(IntPtr stream);

        [DllImport(PortAudio.LIBRARY_NAME)]
        private static extern PaErrorCode Pa_SetStreamFinishedCallback(IntPtr stream, PaStreamFinishedCallback streamFinishedCallback);

        [DllImport(PortAudio.LIBRARY_NAME)]
        private static extern PaErrorCode Pa_StartStream(IntPtr stream);

        [DllImport(PortAudio.LIBRARY_NAME)]
        private static extern PaErrorCode Pa_StopStream(IntPtr stream);

        [DllImport(PortAudio.LIBRARY_NAME)]
        private static extern PaErrorCode Pa_AbortStream(IntPtr stream);

        #endregion
    }
}
