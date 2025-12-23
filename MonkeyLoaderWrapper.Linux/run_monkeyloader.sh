#!/usr/bin/env sh
set -e

BOOTSTRAP_SCRIPT="LinuxBootstrap.sh"

if ! grep -q "MonkeyLoader" "$BOOTSTRAP_SCRIPT"; then
    SEARCH='Renderite.Host.dll'
    REPLACE='"$(
        RENDERITE="MonkeyLoaderWrapper.Linux.dll"
        for arg in "$@"; do
            if [ "$arg" = "--hookfxr-disable" ]; then
                RENDERITE="Renderite.Host.dll"
                break
            fi
        done

        printf %s "$RENDERITE"
    )"'
    # Replace all occurences of SEARCH with REPLACE
    # See https://pubs.opengroup.org/onlinepubs/9799919799/utilities/awk.html
    NEW="$(awk -v search="$SEARCH" -v repl="$REPLACE" '
    {
        # Escape \ and &
        gsub(/[\\&]/, "\\\\&", repl)
        # Replace all occurences of search (word boundary aware)
        gsub("[ 	]" search "[ 	]", " " repl " ")
        print $0
    }
    ' "$BOOTSTRAP_SCRIPT")"
    printf %s "$NEW" > "$BOOTSTRAP_SCRIPT"
fi

"$@"
