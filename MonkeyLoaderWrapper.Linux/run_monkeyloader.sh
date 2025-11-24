#!/usr/bin/env sh

# change shebang in LinuxBootstrap.sh to support bash syntax
sed -i '1s|/usr/bin/env sh|/usr/bin/env bash|' ./LinuxBootstrap.sh

sed -i '/^	# ~ Launch Resonite! :) ~$/c\
if [[ "$*" != *"--hookfxr-disable"* ]]; then\
    dotnet MonkeyLoaderWrapper.Linux.dll "$@"\
    exit 0\
fi' ./LinuxBootstrap.sh
"$@"
