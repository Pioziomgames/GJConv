# GJConv
A .NET 6.0 converter for console texture formats.
# Currently supports import and export of the following formats:
- TMX
- GIM
- TIM2
- TGA
- PNG/BMP/JPG/GIF/TIFF
# Usage:
`GJConv.exe {args} {inputfile} {outputfile}(optional) {outputformat}(optional)`
### Arguments:
- `-so`             Solidify (on by default)
- `-rb`             Swap Red and Blue
- `-id`             Convert to indexed
- `-fc`             Convert to full color
- `-ui {short}`     Tmx user id
- `-uc {string}`    Tmx user comment
### Explanations:
- Default arguments can be set in GJConv.cfg
- Order of arguments does not matter.
- First argument given to the program that isn't in args will be used as the input path.
- Second argument given to the program that isn't in args will be used as the output path.
- If no output path is specified the program will default to: `{InputFile}.{ExportFormat}`
- Output extension can be set by simply providing it as an argument.
- Inputting Yes/No args will set their value to the oposite of the one in config.
- Arguments are applied on top of the config in their order
### Examples:
- `GJConv.exe example.tga -ui 1 example.tmx`
  - Convert example.tga to example.tmx and set the user id to 1
- `GJConv.exe -fc example.gim png`
  - Convert example.gim to example.png and force the output to be full colors
- `GJConv.exe -example.tm2 -so`
  - Convert example.tm2 to example.tmx and turn off solidify (decided by the default config)
- `GJConv.exe -example.png tmx -uc bustup`
  - Convert example.png to example.tmx and set the user comment to "bustup"
## Limitations:
- RLE TGA can only be imported and not exported.
- GIM files using DXT are not supported.
- GIM files using the fast pixel order are not supported.
- Mip maps are not supported.
- If the input file has animation data, all of it besides the first frame will be discarded(even when exporting to a format that supports animation).
- Output pixel formats are mostly dictated by input files(only way to overwrite that is through `-id` and `-fc`).
## Future Development:
- Implementing a better color limiting algorithm
- Adding rw ps2 texture support (rmd)
- DXT support (gim and dds)
- GIM fast pixel order support (PSP tiling)
- TGA RLE export
