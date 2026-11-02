#!/usr/bin/env bash
# Package a self-contained linux-x64 publish directory as a Debian .deb.
# Usage: make-bruskel-linux-deb.sh <publish-dir> <version> <output-dir>
set -euo pipefail

PUBLISH_DIR="$1"
VERSION="$2"
OUTPUT_DIR="$3"
mkdir -p "$OUTPUT_DIR"
OUTPUT_DIR="$(realpath "$OUTPUT_DIR")"
STAGING="$(mktemp -d "${TMPDIR:-/tmp}/bruskel-deb.XXXXXX")"
trap 'rm -rf "$STAGING"' EXIT

mkdir -p "$STAGING/usr/lib/bruskel" "$STAGING/usr/bin" \
    "$STAGING/usr/share/applications" "$STAGING/usr/share/icons/hicolor/96x96/apps" \
    "$STAGING/usr/share/doc/bruskel" "$STAGING/DEBIAN"
cp -R "$PUBLISH_DIR"/. "$STAGING/usr/lib/bruskel/"
chmod +x "$STAGING/usr/lib/bruskel/Bruskel"
ln -s /usr/lib/bruskel/Bruskel "$STAGING/usr/bin/bruskel"
cp installer/avalonia/bruskel.desktop "$STAGING/usr/share/applications/"
cp src/core/Bruskel.Resources/icons/hicolor/96x96/apps/com.github.BruskelProject.Bruskel.png \
    "$STAGING/usr/share/icons/hicolor/96x96/apps/bruskel.png"
cp LICENSE.md CHANGELOG.md \
    "$STAGING/usr/share/doc/bruskel/"
cp third-party/pinta/LICENSE.md "$STAGING/usr/share/doc/bruskel/LICENSE-BRUSKEL-LEGACY.md"
cp third-party/paint.net/LICENSE.md "$STAGING/usr/share/doc/bruskel/LICENSE-PAINT.NET.md"
sed "s/__VERSION__/$VERSION/" installer/avalonia/bruskel.control > "$STAGING/DEBIAN/control"
echo "Installed-Size: $(du -sk "$STAGING/usr" | cut -f1)" >> "$STAGING/DEBIAN/control"

echo '2.0' > "$STAGING/debian-binary"
tar -czf "$STAGING/control.tar.gz" --owner=root --group=root --numeric-owner -C "$STAGING/DEBIAN" .
tar -czf "$STAGING/data.tar.gz" --owner=root --group=root --numeric-owner -C "$STAGING" usr
DEB_PATH="$OUTPUT_DIR/bruskel_${VERSION}_amd64.deb"
(cd "$STAGING" && ar rc "$DEB_PATH" debian-binary control.tar.gz data.tar.gz)
echo "Built $DEB_PATH"
