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

## Usage

Parse a project starting from `.kgt`:

```bash
Fm2ndParser game.kgt
```

Useful options:

- `--clean-up` (`-c`): cleaner JSON for comparison
- `--new-files` (`-n`): create new JSON files instead of overwriting
- `--export-resources` (`-x`): export embedded images/sounds

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