#!/usr/bin/env sh

sed -i '/^	# ~ Launch Resonite! :) ~$/c\
if [[ "$*" != *"--hookfxr-disable"* ]]; then\
    dotnet MonkeyLoaderWrapper.Linux.dll "$@"\
    exit 0\
fi' ./LinuxBootstrap.sh
"$@"
