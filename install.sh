#!/bin/bash

#default values
usr = "DwarfSun"
rig = "Donation"

#Make sure script is run as root
if [ $EUID -ne 0 ]
then
   echo "This script must be run as root." 1>&2
   exit 1
fi
#prompt for username or set from command line arguments
if [ $# -lt 1 ]
then
    echo "Enter your username:"
    read usr
else
    usr = $1
fi
#prompt for rig id or set from command line arguments
if [ $# -lt 2 ]
then
    echo "Enter this rig ID:"
    read rig
else
    rig = $2  
fi

#Install the .NET Core SDK
wget -q https://packages.microsoft.com/config/ubuntu/$(lsb_release -r -s)/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt-get update
apt-get --assume-yes install apt-transport-https
apt-get --assume-yes install dotnet-sdk-2.1
rm packages-microsoft-prod.deb

#Install git
apt-get --assume-yes install git

#Install all additional requirements for compiling and running miner programs
apt-get --assume-yes install automake autotools-dev build-essential cmake libcurl4-openssl-dev libhwloc-dev libjansson-dev libssl-dev libuv1-dev nvidia-cuda-dev nvidia-cuda-toolkit gcc-5 g++-5 libmicrohttpd-dev screen

#create installation folder
mkdir -p /spelunker/source
cd /spelunker/source

#Get miners
git clone https://github.com/DwarfSun/miners.git

#Clone excavator files from GitHub
git clone https://github.com/DwarfSun/spelunker.git
cd spelunker;git pull;cd ..;

#Clone CCMiner source from GitHub
git clone https://github.com/tpruvot/ccminer.git
cd ccminer; git pull; cd ..;

#Clone xmrig-nvidia source from GitHub
git clone https://github.com/xmrig/xmrig-nvidia.git
cd xmrig-nvidia; git pull; cd ..;

#Clone xmrig source from GitHub
git clone https://github.com/xmrig/xmrig.git
cd xmrig; git pull; cd ..;

#Build CCMiner
mkdir -p /spelunker/source/ccminer
cd /spelunker/source/ccminer
./build.sh

#Build xmr-stak
mkdir -p /spelunker/source/xmrig-nvidia/build
cd /spelunker/source/xmrig-nvidia/build
cmake ..
make

#Build xmrig
mkdir -p /spelunker/source/xmrig/build
cd /spelunker/source/xmrig/build
cmake ..
make
