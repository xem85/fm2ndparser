# Fm2ndParser

Parser CLI/library for **Fighter Maker 2nd** files.

Main goal:
- read FM2nd files as completely as possible
- export to readable, versionable JSON
- later support binary generation from JSON

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

## Usage

Parse a project starting from `.kgt`:

```bash
Fm2ndParser game.kgt
```

> ⚠️ Use this tool at your own risk. The author is not responsible for any damage or data loss.

Useful options:

- `--clean-up` (`-c`): cleaner JSON for comparison
- `--new-files` (`-n`): create new JSON files instead of overwriting
- `--export-resources` (`-x`): export embedded images/sounds

## Documentation

Detailed reference lives in [`docs/`](docs/README.md):

- [CLI usage](docs/cli-usage.md) — invocation, input types, flags, output location.
- [Output formats](docs/output-formats.md) — JSON file, resource folder layout, indexed-BMP/palette handling, sounds.
- [JSON specification](docs/json-spec.md) — complete field-by-field schema, block types, and enums.

## Current status

- Parsing/export is the current focus
- JSON -> binary write support is planned
- Few formats/fields may still be incomplete

## Contributing

Contributions are welcome.

## License

MIT License. See `LICENSE`.

## Credits

Developed by Xem85