namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal enum PaErrorCode
    {
        NoError = 0,
        NotInitialized = -10000,
        UnanticipatedHostError = -9999,
        InvalidChannelCount = -9998,
        InvalidSampleRate = -9997,
        InvalidDevice = -9996,
        InvalidFlag = -9995,
        SampleFormatNotSupported = -9994,
        BadIoDeviceCombination = -9993,
        InsufficientMemory = -9992,
        BufferTooBig = -9991,
        BufferTooSmall = -9990,
        NullCallback = -9989,
        BadStreamPtr = -9988,
        TimedOut = -9987,
        InternalError = -9986,
        DeviceUnavailable = -9985,
        IncompatibleHostApiSpecificStreamInfo = -9984,
        StreamIsStopped = -9983,
        StreamIsNotStopped = -9982,
        InputOverflowed = -9981,
        OutputUnderflowed = -9980,
        HostApiNotFound = -9979,
        InvalidHostApi = -9978,
        CanNotReadFromACallbackStream = -9977,
        CanNotWriteToACallbackStream = -9976,
        CanNotReadFromAnOutputOnlyStream = -9975,
        CanNotWriteToAnInputOnlyStream = -9974,
        IncompatibleStreamHostApi = -9973,
        BadBufferPtr = -9972
    }
}
