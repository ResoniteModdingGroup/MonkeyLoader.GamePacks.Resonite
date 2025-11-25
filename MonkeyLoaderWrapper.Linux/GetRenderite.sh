#!/usr/bin/env sh

RENDERITE="MonkeyLoaderWrapper.Linux.dll"
for arg in "$@"; do
	if [ "$arg" = "--hookfxr-disable" ]; then
		RENDERITE="Renderite.Host.dll"
		break
	fi
done

echo -n "$RENDERITE"
