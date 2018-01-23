set -e

xbuild /p:Configuration="Debug [LINUX]" ./GamePlatformUtils/GamePlatformUtils.csproj
xbuild /p:Configuration="Debug [LINUX]" ./GPUTest/GPUTest.csproj
mono "./GPUTest/bin/Debug [LINUX]/GPUTest.exe"
