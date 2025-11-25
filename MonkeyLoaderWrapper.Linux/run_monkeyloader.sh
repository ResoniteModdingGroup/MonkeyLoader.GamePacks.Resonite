#!/usr/bin/env sh
set -e

BOOTSTRAP_SCRIPT="LinuxBootstrap.sh"
SEARCH='Renderite.Host.dll'
REPLACE='"$(./GetRenderite.sh "$@")"'

grep -q "$SEARCH" "$BOOTSTRAP_SCRIPT"\
&& sed -i 's:\b'"$SEARCH"'\b:'"$REPLACE"':g' "$BOOTSTRAP_SCRIPT"

"$@"
