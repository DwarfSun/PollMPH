#Pull spelunker files from GitHub
cd /spelunker/source/spelunker;git pull
#Build DwarfSun's spelunker
dotnet publish
mv /spelunker/source/spelunker/spelunker/bin/Release/netcoreapp2.1/publish/* /spelunker
cd /spelunker