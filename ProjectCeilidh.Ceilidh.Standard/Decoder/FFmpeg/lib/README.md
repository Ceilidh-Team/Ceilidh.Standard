# FFmpeg

This folder contains pre-compiled FFmpeg binaries for Windows x86_64, and `libwinpthread-1.dll` from MinGW.

These binaries were compiled on a Linux system using MinGW with the following configure command:

```bash
$ ./configure --arch=x86_64 --target-os=mingw32 --cross-prefix=x86_64-w64-mingw32- --disable-protocols --disable-programs --enable-shared --disable-pthreads --disable-encoders --enable-static
```

To obtain the FFmpeg source code used to compile these binaries, execute the following commands:

```bash
$ git clone https://git.ffmpeg.org/ffmpeg.git ffmpeg
$ cd ffmpeg
$ git checkout remote/4.0
```

The licenses for both [FFmpeg](FFmpeg.LGPLv2.1) and [MinGW](MinGW.MIT) are reproduced here in accordance with the license terms.