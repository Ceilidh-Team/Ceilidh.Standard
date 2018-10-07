#!/bin/env bash

echo Installing FFmpeg...
sudo add-apt-repository ppa:mc3man/trusty-media
sudo apt-get update
sudo apt-get install -y ffmpeg

echo Installing PortAudio v19...
sudo apt-get install -y libportaudio2

echo Installing development dependencies...
sudo apt-get install -y libasound-dev wget

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

echo Refreshing libraries...
sudo ldconfig