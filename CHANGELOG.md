# Bruskel changelog

Notable changes to Bruskel are documented here. The initial `0.0.0` entry records the fork
baseline; later entries track Bruskel-specific work.

## Unreleased

No changes yet.

## 0.0.4-alpha

### Added

- Ellipse and lasso selections, move selection and move selected pixels tools, and Clone Stamp.
- A dedicated Pan tool and temporary canvas navigation using Space/H and the middle mouse button.
- Brush presets, tip selection, spacing, smoothing, and optional pen-pressure size scaling.
- Compact brush options with percentage sliders and additional stroke settings.

### Fixed

- Selection clipping for painting, fill, gradients, recoloring, cloning, shapes, text, and region effects.
- Mask-aware copying, movement, flipping, and rotation; selection restoration during cancellation and raster undo.
- Persistent clipping masks for editable text and effects in ORA annotation metadata.
- Zoom anchoring after panning and rectangle zoom gestures.
- Active shape colors now follow the palette until the shape is finished.
- Paint.NET-style Magic Wand modifiers, shape rotation units, footer alignment, and tool icons.

### Known limitations

- This is an alpha preview. Keep backups of important artwork and project files.
- Brush presets are not yet exact GTK equivalents, and pen pressure requires real-device verification.
- Packages retain the legacy Bruskel executable and package names. macOS packages are unsigned and not notarized.

## 0.0.2-alpha

### Added

- Unified Open and Save As workflows for PNG, JPEG, WebP, TIFF, BMP, GIF, SVG layers, and ORA.
- Layer duplication, ordering, merge, opacity controls, and history management.
- Selection commands and configurable shortcuts for Select All, Deselect, and layer duplication.
- Avalonia About window and History window controls.

### Fixed

- Debian package creation now writes artifacts to the intended output directory.
- Layer opacity controls and standard undo behavior remain synchronized when changing layers.

## 0.0.1-alpha

First alpha preview of the Bruskel desktop editor.

### Added

- Cross-platform desktop shell and raster canvas.
- Core painting tools: brush, pencil, eraser, fill, gradient, color picker, selections, zoom, and
  editable shapes.
- Layers, common image operations, configurable keyboard shortcuts, and light/dark themes.
- Shape selection, move, resize, rotation, and live updates when shape options change.
- Self-contained Windows, Linux, and macOS builds, including MSI, AppImage, Debian, and DMG
  packages.

### Known limitations

- Feature coverage and file-format workflows remain incomplete compared with the original Bruskel
  application and mature raster editors.
- This is an early preview; keep backups of important artwork and project files.
- Packages are unsigned. macOS builds are not notarized and may show a security warning on first
  launch.

## 0.0.0 — Fork baseline

- Started from the Bruskel raster graphics editor codebase as the foundation for Bruskel.
- Preserved the original Bruskel and Paint.NET license notices and icon attributions.
