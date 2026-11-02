#!/usr/bin/env bash
# Create a drag-and-drop macOS disk image from a published app bundle.
# Usage: make-bruskel-macos-dmg.sh <app-bundle> <output.dmg>
set -euo pipefail
APP="${1%/}"
OUTPUT="$2"
[[ -d "$APP" && -f "$APP/Contents/Info.plist" ]] || { echo "Invalid app bundle: $APP" >&2; exit 1; }
STAGE="$(mktemp -d "${TMPDIR:-/tmp}/bruskel-dmg.XXXXXX")"
trap 'rm -rf "$STAGE"' EXIT
mkdir "$STAGE/Bruskel"
ditto "$APP" "$STAGE/Bruskel/Bruskel.app"
ln -s /Applications "$STAGE/Bruskel/Applications"
cp LICENSE.md CHANGELOG.md \
    src/core/Bruskel.Resources/icons/bruskel-icons.md "$STAGE/Bruskel/"
cp third-party/pinta/LICENSE.md "$STAGE/Bruskel/LICENSE-BRUSKEL-LEGACY.md"
cp third-party/paint.net/LICENSE.md "$STAGE/Bruskel/LICENSE-PAINT.NET.md"
hdiutil create -quiet -volname "Bruskel" -srcfolder "$STAGE/Bruskel" -fs HFS+ -format UDZO "$OUTPUT"
hdiutil verify -quiet "$OUTPUT"
echo "Built $OUTPUT (unsigned and not notarized)"
