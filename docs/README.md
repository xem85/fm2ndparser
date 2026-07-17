# Documentation

`Fm2ndParser` reads **Fighter Maker 2nd** binary files (`.kgt`, `.player`, `.stage`,
`.demo`) and exports their contents as JSON, and optionally extracts embedded images
and sounds.

This folder is a self-contained reference for using the tool and consuming its output.

| Document | Covers |
|----------|--------|
| [cli-usage.md](cli-usage.md) | Building, invoking the CLI, input types, the `.kgt` cascade, all flags, output location, and error handling. |
| [output-formats.md](output-formats.md) | The JSON file and the `--export-resources` folder layout; indexed-BMP image format and palette (color) handling; sound export. |
| [json-spec.md](json-spec.md) | Full field-by-field JSON reference: envelope, every file type, all skill block types, nested objects, `--clean-up` vs default modes, and every enum. |
