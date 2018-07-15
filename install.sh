#!/bin/bash
if [ $EUID -ne 0 ]
then
   echo "This script must be run as root." 1>&2
   exit 1
fi

#Install the .NET Core SDK
wget -q https://packages.microsoft.com/config/ubuntu/$(lbv_release -r -s)/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt-get --assume-yes install apt-transport-https
apt-get update
apt-get --assume-yes install dotnet-sdk-2.1
