set -e

xbuild /p:Configuration="Release [LINUX]" ./GamePlatformUtils/GamePlatformUtils.csproj
xbuild /p:Configuration="Release [LINUX]" ./GPUTest/GPUTest.csproj
mono "./GPUTest/bin/Release [LINUX]/GPUTest.exe"
