# Fm2ndParser

Parser CLI/library for **Fighter Maker 2nd** files.

Main goals:
- read FM2nd files completely
- export to readable, versionable JSON
- compile binary files back from JSON

## Supported files

- `.kgt`
- `.player`
- `.stage`
- `.demo`

## Requirements

- .NET 10+

## Quick start

```bash
git clone https://github.com/xem85/fm2ndparser
cd fm2ndparser
dotnet build
```

The compiled output is available in the `Publish` folder.

## CLI commands

The CLI supports the following commands:

- `parse`
- `compile`

If no command is specified, the default command is `parse`.

## Usage

### Parse binary files into JSON

Parse a project starting from `.kgt`:

```bash
Fm2ndParser parse game.kgt
```

The default command is `parse`, so this is equivalent:

```bash
Fm2ndParser game.kgt
```

Parse a single file:

```bash
Fm2ndParser parse character.player
```

By default, output files are written to a target folder named with the current timestamp, so existing output is not overwritten. The generated JSON files use the same name as the input file with `.json` appended.


Useful options for `parse`:

- `--clean-up` (`-c`): cleaner JSON for comparison
- `--output` (`-o`): specify the output folder instead of the default timestamped folder
- `--export-resources` (`-x`): export embedded images/sounds

Examples:

```bash
Fm2ndParser parse game.kgt
Fm2ndParser parse character.player --clean-up
Fm2ndParser parse stage.stage --export-resources
Fm2ndParser parse demo.demo --output output\demo_export
```

### Compile JSON into binary files

Compile a JSON file back to FM2nd binary format:

```bash
Fm2ndParser compile character.json
```

Examples:

```bash
Fm2ndParser compile character.json
Fm2ndParser compile stage.json
Fm2ndParser compile game.json
```

> ⚠️ Use this tool at your own risk. The author is not responsible for any damage or data loss.

## Documentation

Detailed reference lives in [`docs/`](docs/README.md):

- [CLI usage](docs/cli-usage.md) — invocation, input types, flags, output location.
- [Output formats](docs/output-formats.md) — JSON file, resource folder layout, indexed-BMP/palette handling, sounds.
- [JSON specification](docs/json-spec.md) — complete field-by-field schema, block types, and enums.

## Current status

- Parsing/export is supported
- JSON -> binary compilation is supported
- Few formats/fields may still be incomplete

## Contributing

Contributions are welcome.

## License

MIT License. See `LICENSE`.

## Credits

Developed by Xem85
