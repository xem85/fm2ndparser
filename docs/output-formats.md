# Output Formats

Running `Fm2ndParser` on an input file produces:

1. Always: one **JSON** file describing the parsed contents.
2. With `--export-resources` (`-x`): a **resource folder** containing exported images
   (`.bmp`) and sounds (`.wav`).

All paths are relative to the **current working directory**, using the input file's
base name (`<basename>` = input filename without directory or extension).

## JSON file

- Path: `./<basename>.json` (or `./<basename>_<N>.json` with `--new-files`).
- One file per parsed input. A `.kgt` run yields one JSON for the project plus one
  JSON per referenced character/stage/demo.
- Encoding: UTF-8, pretty-printed (indented).
- The top-level `type` field identifies the file: `"game"` (kgt), `"character"`
  (player), `"stage"`, or `"demo"`.

The complete field-by-field schema is in [json-spec.md](json-spec.md).

## Resource folder (`--export-resources`)

For an input whose base name is `<basename>`, resources are written under:

```
<basename>/
├── img/        # images using the default palette
│   ├── 0000.bmp
│   ├── 0001.bmp
│   └── ...
├── snd/        # sounds
│   ├── 0000.wav
│   ├── 0001.wav
│   └── ...
├── 1/          # same images re-colored with global palette 1
├── 2/          # ... global palette 2
├── 3/
├── 4/
├── 5/
├── 6/
└── 7/          # ... global palette 7
```

- Files are named by their **zero-based index** in the source file, zero-padded to
  four digits (`0000`, `0001`, …). The index matches the position in the `images` /
  `sounds` arrays of the JSON.
- `img/`, `snd/` and `1/`–`7/` are always created when `-x` is used, even if a
  category is empty.
- An image is skipped (no file written) if its width or height is 0, if it has no
  pixel data, or if its data is shorter than required.

### What the numbered folders mean

Folders `1/` through `7/` exist because Fighter Maker characters support **palette
swaps** (alternate color schemes for the same sprite). They only apply to images that
use a *global* palette (see below). Each numbered folder holds the **same pixel data**
as `img/` but with a different global palette applied, so `1/0003.bmp` is image `0003`
recolored with global palette 1. Images that carry their **own embedded** palette are
written only to `img/`.

## Image format & palette handling

Images are exported as **8-bit indexed BMP** (Windows DIB, `BITMAPINFOHEADER`). Every
pixel is a 1-byte index into a 256-entry color table; this preserves the original
palette-based coloring exactly rather than flattening to true color.

### BMP layout

| Section        | Size (bytes) | Notes |
|----------------|--------------|-------|
| File header    | 14           | `"BM"`, file size, reserved (0), pixel-data offset. |
| DIB header     | 40           | `BITMAPINFOHEADER`: width, height, planes=1, **bpp=8**, compression=0 (BI_RGB), image size, 2835 px/m (~72 DPI) on both axes, colors-used=256, colors-important=256. |
| Color table    | 1024         | 256 entries × 4 bytes. |
| Pixel array    | row-padded   | Rows padded to a 4-byte boundary, stored **bottom-up** (last image row first). |

The color table is 256 × 4 bytes copied straight from the source palette bytes. The
source stores palette entries as **4 bytes each** (an RGBA-style quad); they are
written into the BMP color table verbatim, so the on-disk byte order is whatever the
source uses. (In the JSON, standalone colors are exposed as `{ r, g, b, a }` — see
`Rgba` in [json-spec.md](json-spec.md).)

### Two palette sources

Each image has a `paletteType` field (see `ImageResource` in
[json-spec.md](json-spec.md)) that determines where its colors come from:

- **`paletteType == 1` — embedded palette.** The image's own `data` begins with a
  1024-byte palette followed by the pixel indices. The tool splits these out and
  writes a single BMP to `img/`. No alternate-palette folders are produced for this
  image.
- **`paletteType != 1` — global palette.** The image `data` is pixel indices only;
  colors come from the file's shared `globalPalettes` array (8 palettes, indices 0–7,
  each 1024 bytes). The tool writes:
  - `img/<index>.bmp` using **global palette 0** (the default), and
  - `<p>/<index>.bmp` for `p = 1..7` using global palette `p`.

  If a given global palette is missing or shorter than 1024 bytes, that variant is
  skipped for that image.

## Sound format

Sounds are exported as-is to `snd/<index>.<ext>`, where the extension is chosen from
the `sounds[].type` field (see `SoundResource` and the `SoundType` enum in
[json-spec.md](json-spec.md)):

| `type` | Extension |
| ------ | --------- |
| Wave   | `.wav`    |
| Midi   | `.mid`    |
| CDDA   | `.cda`    |
| None   | `.bin`    |

The bytes written are the raw sound `data` from the source (empty file if a sound has
no data). The exporter only picks the extension from the type — it does not convert or
re-wrap the bytes, so non-Wave entries may still not be valid media files; the
extension just reflects the format the source declared.
