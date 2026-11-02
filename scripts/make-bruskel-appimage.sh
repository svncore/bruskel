#!/usr/bin/env bash
# Bundle a self-contained linux-x64 publish directory as an AppImage.
# Usage: make-bruskel-appimage.sh <publish-dir> <output-dir>
set -euo pipefail

PUBLISH_DIR="$1"
OUTPUT_DIR="$2"
APPDIR="$OUTPUT_DIR/Bruskel.AppDir"
APPIMAGETOOL="${APPIMAGETOOL:-appimagetool}"
mkdir -p "$APPDIR/usr/bin" "$OUTPUT_DIR"
cp -R "$PUBLISH_DIR"/. "$APPDIR/usr/bin/"
# AppImage requires every bundled file to be readable/traversable by all users.
chmod -R a+rX "$APPDIR"
chmod a+rx "$APPDIR/usr/bin/Bruskel"
cp installer/avalonia/bruskel.desktop "$APPDIR/Bruskel.desktop"
cp src/core/Bruskel.Resources/icons/hicolor/96x96/apps/com.github.BruskelProject.Bruskel.png "$APPDIR/bruskel.png"
cat > "$APPDIR/AppRun" <<'APPRUN'
#!/usr/bin/env bash
HERE="$(dirname "$(readlink -f "$0")")"
exec "$HERE/usr/bin/Bruskel" "$@"
APPRUN
chmod +x "$APPDIR/AppRun"
ARCH=x86_64 "$APPIMAGETOOL" "$APPDIR" "$OUTPUT_DIR/Bruskel-x86_64.AppImage"
