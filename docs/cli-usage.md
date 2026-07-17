# CLI Usage

`Fm2ndParser` is a command-line tool that reads **Fighter Maker 2nd** binary files
and writes their contents as JSON, optionally exporting embedded images and sounds.

## Requirements

- .NET 10 SDK/runtime or newer.

## Building

```bash
dotnet build
```

The build produces the `Fm2ndParser` executable (`Fm2ndParser.exe` on Windows) under
the project's build output. You can also run it directly from source:

```bash
dotnet run --project Fm2ndParser -- <input-file> [options]
```

## Synopsis

```
Fm2ndParser <input-file> [options]
```

- Exactly **one** input file must be given per invocation. Although the built-in help
  shows a "Multiple files" example, the current implementation processes a single
  file only — passing more than one is an error.
- The input file type is chosen by its extension (case-insensitive).

### Supported input extensions

| Extension  | Meaning              | Behavior                                                                 |
|------------|----------------------|--------------------------------------------------------------------------|
| `.kgt`     | Game/project file    | Parsed, then **cascades**: every referenced character, stage and demo is parsed too (see below). |
| `.player`  | Character file       | Parsed standalone.                                                        |
| `.stage`   | Stage file           | Parsed standalone.                                                        |
| `.demo`    | Demo file            | Parsed standalone.                                                        |

Any other extension prints
`Unsupported file type '<ext>'. Expected .kgt, .player, .stage or .demo.` and exits
without producing output.

### `.kgt` cascade

When the input is a `.kgt`, the tool first parses the project file, then reads the
character/stage/demo **names listed inside it** and parses the sibling files found in
the **same directory as the `.kgt`**:

- `<character>.player` for each entry in the project's character list
- `<stage>.stage` for each stage
- `<demo>.demo` for each demo

Each parsed file produces its own JSON output (and, with `-x`, its own resource
folder). Character files parsed this way are given the project context, so their
skill/stage/demo references can be resolved to names.

## Options

| Flag                     | Short | Default | Description |
|--------------------------|-------|---------|-------------|
| `--new-files`            | `-n`  | `false` | Do not overwrite an existing JSON. Instead write to the first free `<name>_<N>.json` (`_0`, `_1`, …). |
| `--clean-up`             | `-c`  | `false` | Produce a normalized JSON intended for diffing/comparison. **This is lossy** — see below. |
| `--export-resources`     | `-x`  | `false` | Also export embedded images (indexed `.bmp`) and sounds (`.wav`). See [output-formats.md](output-formats.md). |

### Output location

> **Important:** output paths are relative to the **current working directory**, not
> to the input file's directory. The tool strips the input path and uses only its
> base name.

- JSON: `<basename>.json` in the current working directory (e.g. running against
  `/games/ryu.player` writes `./ryu.json`).
- With `--export-resources`: resources go under `./<basename>/` (see
  [output-formats.md](output-formats.md)).

By default an existing `<basename>.json` is overwritten. With `--new-files`, existing
files are preserved and a new numbered file is created instead.

### What `--clean-up` does (lossy)

`--clean-up` is designed to make two files easy to compare, **not** to produce a
faithful, round-trippable dump. In addition to dropping raw-binary fields (see the
"modes" markers in [json-spec.md](json-spec.md)), it **mutates** skill data:

- Consecutive `I` blocks in a skill are merged into one, summing their `wait`.
- On the surviving `I` block, `i`, `x`, `y`, `turnX`, `turnY` are zeroed.
- On every `FA` block, `x`, `y`, `width`, `height` are zeroed.
- All block `index` values are reset to `0`.

Use the default (no-flag) output when you need complete, unmodified data.

## Error handling

- If the input file is locked by another process, the tool prints
  `The file <file> is locked, and can't be parsed.` and waits for the user to press
  Enter before continuing.
- With `--new-files`, if the computed target filename still exists, the run throws
  `File exists: <name>`.

## Examples

```bash
# Dump a single character to ./ryu.json
Fm2ndParser ryu.player

# Clean, comparison-friendly JSON
Fm2ndParser ryu.player -c

# Parse a whole project (game + all characters/stages/demos) and export resources
Fm2ndParser game.kgt -x

# Keep previous JSON files instead of overwriting
Fm2ndParser ryu.player -n
```
