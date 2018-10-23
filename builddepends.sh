#!/usr/bin/env bash

sudo apt-get update

echo Installing PortAudio v19...
sudo apt-get install -y libportaudio2

echo Installing development dependencies...
sudo apt-get -y install libasound-dev autoconf automake build-essential libass-dev libfreetype6-dev libtheora-dev libtool libva-dev libvdpau-dev libvorbis-dev libxcb1-dev libxcb-shm0-dev libxcb-xfixes0-dev pkg-config texinfo wget zlib1g-dev
sudo apt-get -y install yasm nasm

echo Downloading libebur128...
wget https://github.com/jiixyj/libebur128/archive/v1.2.4.tar.gz
echo Extracting libebur128...
tar -xvf v1.2.4.tar.gz
echo Compiling libebur128...
cd libebur128-1.2.4/cmake
cmake ..
make
echo Installing libebur128...
sudo make install
echo Complete!
cd ../..
rm v1.2.4.tar.gz
rm -rf libebur128-1.2.4

echo Compiling ffmpeg...
mkdir ~/ffmpeg_sources
cd ~/ffmpeg_sources
wget http://ffmpeg.org/releases/ffmpeg-snapshot.tar.bz2
tar jxf ffmpeg-snapshot.tar.bz2
cd ffmpeg
./configure --enable-shared --disable-protocols --disable-programs --disable-encoders
make -s -j
sudo make install
hash -r
cd $TRAVIS_BUILD_DIR

echo Refreshing libraries...
sudo ldconfig