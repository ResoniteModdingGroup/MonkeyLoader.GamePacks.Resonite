#!/usr/bin/env sh

sed -i '0,/dotnet Renderite.Host.dll "$@"/s//dotnet MonkeyLoaderWrapper.Linux.dll "$@"/' ./LinuxBootstrap.sh
"$@"
