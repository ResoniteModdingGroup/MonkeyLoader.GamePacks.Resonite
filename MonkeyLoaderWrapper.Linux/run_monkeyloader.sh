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

        echo -n "$RENDERITE"
    )"'
    # Replace all occurences of SEARCH with REPLACE
    NEW="$(awk -v search="$SEARCH" -v repl="$REPLACE" '
    {
        # Search with word-boundary separation
        pattern = "\\<" search "\\>"
        out = ""
        start = 1
        while ( match(substr($0, start), pattern) ) {
            before = substr($0, start, RSTART - 1)
            out = out before repl
            start += RSTART + RLENGTH - 1
        }
        out = out substr($0, start)
        print out
    }
    ' "$BOOTSTRAP_SCRIPT")"
    echo -n "$NEW" > "$BOOTSTRAP_SCRIPT"
fi

"$@"
