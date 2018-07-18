# DwarfSun's Spelunker for Mining Pool Hub
Spelunker is a mining helper program, written for Ubuntu Linux using .NET Core 2.1, for mining multiple algorithms from miningpoolhub.com

Spelunker does not mine coins. It downloads and manages a number of miner programs, using the normalized profit data it retrieves from mining pool hub's REST API. Miner's are configured per algorithm in a single JSON file, making it relatively easy to add or remove miners and algorithms according to your own preference. It's bundled with an install script, which will attempt to download all necessary dependencies, compile itself and the miners, as well as update the crontab so that it launches at reboot (after prompting the user). 

Where possible, Spelunker has made use of open source miners, noteably xmrig-nvidia and ccminer, found at https://github.com/xmrig/xmrig-nvidia and https://github.com/tpruvot/ccminer respectively. It additionally makes use of Claymore's Eth miner, DSTM's zm Equihash miner and EWBF's Equihash miner (for mining Bitcoin-Gold).

# Build / Install
Spelunker comes with an install.sh file which, when executed, will attempt to download and install all dependencies. Simply running it should place all the miners in a folder named spelunker at /, as well as configure the miners to use the username and rig ID you specify.

The following command, typed in terminal, should be all that is needed: <br />
<code>sudo apt-get install git;git clone https://github.com/dwarfsun/spelunker.git; cd spelunker;sudo ./install.sh</code>
 
# Launching Spelunker
If launched by the crontab during a reboot, Spelunker can be viewed by typing the following in the terminal: <br />
<code>sudo screen -r spelunker</code>
  
To launch Spelunker manually, simply type the following in the terminal: <br />
<code>sudo /spelunker/spelunk.sh</code>
  
# Installing .NET Core 2.1 SDK
This should not be necessary on Ubuntu, as it should have been done by the install script. However, if the installation failed or you're attempting to use Spelunker on a different distribution of linux, instructions to install can be found at https://www.microsoft.com/net/download/linux-package-manager/ubuntu18-04/sdk-current
