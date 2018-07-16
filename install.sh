#!/bin/bash

#default values
usr="DwarfSun"
rig="Donation"

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
    usr=$1
fi
#prompt for rig id or set from command line arguments
if [ $# -lt 2 ]
then
    echo "Enter this rig ID:"
    read rig
else
    rig=$2
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
cd miners;git pull;cd ..;

#Clone spelunker files from GitHub
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

#Build DwarfSun's spelunker
cd /spelunker/source/spelunker
dotnet publish
mv /spelunker/source/spelunker/spelunker/bin/Release/netcoreapp2.1/publish/* /spelunker
cd /spelunker
touch spelunk.sh
echo '#!/bin/bash' >> spelunk.sh
echo 'if [ $EUID -ne 0 ]' >> spelunk.sh
echo 'then' >> spelunk.sh
echo '   echo "spelunker must be run as root." 1>&2' >> spelunk.sh
echo '   exit 1' >> spelunk.sh
echo 'fi' >> spelunk.sh
echo 'cd /spelunker' >> spelunk.sh
echo 'dotnet spelunker.dll' >> spelunk.sh
chmod +x spelunk.sh

#Build CCMiner
mkdir -p /spelunker/source/ccminer
cd /spelunker/source/ccminer
./build.sh

#Build xmr-stak
mkdir -p /spelunker/source/xmrig-nvidia/build
cd /spelunker/source/xmrig-nvidia/build
cmake .. -DCMAKE_C_COMPILER=gcc-5 -DCMAKE_CXX_COMPILER=g++-5
make

#Build xmrig
mkdir -p /spelunker/source/xmrig/build
cd /spelunker/source/xmrig/build
cmake ..
make

#Create directories for miner binaries
mkdir -p /spelunker/ccminer
mkdir -p /spelunker/xmrig-nvidia
mkdir -p /spelunker/xmrig
mkdir -p /spelunker/zm
mkdir -p /spelunker/ethdcrminer
mkdir -p /spelunker/ewbf

#Move files
#CCMiner
mv /spelunker/source/ccminer/ccminer /spelunker/ccminer

#xmrig
mv /spelunker/source/xmrig/build/xmrig /spelunker/xmrig
cp /spelunker/source/xmrig/src/config.json /spelunker/xmrig
cd /spelunker/xmrig
sed -i "s/proxy.fee.xmrig.com:9999/europe.cryptonight-hub.miningpoolhub.com:17024/g" config.json
sed -i "s/YOUR_WALLET/DwarfSun.Donation/g" config.json
sed -i 's/"threads": null/"threads": 1/g' config.json
sed -i 's/"donate-level": 5/"donate-level": 1/g' config.json
sed -i 's/"background": false/"background": true/g' config.json

#xmrig-nvidia
mv /spelunker/source/xmrig-nvidia/build/xmrig-nvidia /spelunker/xmrig-nvidia

#DSTM's ZM
cp /spelunker/source/miners/zm/* /spelunker/zm

#Claymore's ETH Dual Miner
cp /spelunker/source/miners/ethdcrminer/* /spelunker/ethdcrminer

#EWBF's Equihash Miner
cp /spelunker/source/miners/ewbf/* /spelunker/ewbf

#start spelunking in screen session
cd /spelunker
screen -dmS spelunker dotnet spelunker.dll $usr $rig

echo "Would you like spelunker to start after a reboot? [y/n]"
read answer
if [ $answer == "y" ] || [ $answer == "Y" ]
then
    #add spelunk.sh to crontab
    touch crontab.txt
    crontab -l > crontab.txt

    if grep -q "@reboot screen -dmS spelunker /spelunker/spelunk.sh" "./crontab.txt"
    then
        echo "crontab already configured for auto-mining." 1>&2
    elif grep -q "@reboot" "./crontab.txt"
    then
        echo "Warning: crontab already contains a process which is launched on reboot. You will need to configure crontab manually." 1>&2
	sleep 20
        crontab -e
    else
        echo "@reboot screen -dmS spelunker /spelunker/spelunk.sh" >> crontab.txt
        crontab crontab.txt
	echo "@reboot launch added to crontab for root"
    fi
fi
echo "Installation complete, launching screen session." 1>&2
sleep 20
screen -r spelunker
