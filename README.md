# GJConv
A .NET 6.0 converter for console texture formats.<br>
# GJView
A texture viewer supporting all of the formats that GJConv supports.
# Currently supports import and export of the following formats:
- TMX
- GIM
- TIM2
- TGA
- TXN/RWTEX (Renderware PS2 TextureNative)
- DDS (limited support)
- PNG/BMP/JPG/GIF/TIFF
# Usage:
`GJConv.exe {args} {inputfile} {outputfile}(optional) {outputformat}(optional)`
### Arguments:
- `-so`             Solidify (on by default)
- `-rb`             Swap Red and Blue channels
- `-id`             Convert image to indexed
- `-fc`             Convert image to full color
- `-ls`             Use linear filtering for scaling (on by default)
- `-f2`             Force image size to be a power of 2
- `-fw {int}`       Force image width
- `-fh {int}`       Force image height
- `-ui {short}`     Tmx user id
- `-uc {string}`    Tmx user comment (turns off TmxUseFileName if on)
- `-uf`             Use filename for user comment (on by default)
- `-po`             Gim use PSP pixel order
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
- GIM files using DXT cannot be exported.
- DDS files support importing of only Uncompressed, DXT1, DXT3 and DXT5 files.
- DDS files can be exported only as uncompressed files.
- Mip maps are ignored.
- If the input file has animation data, all of it besides the first frame will be discarded(even when exporting to a format that supports animation).
- Output pixel formats are mostly dictated by input files(only way to overwrite that is through `-id` and `-fc`).
## Future Development:
- Implementing more texture formats
- Better DDS support
- TGA RLE export
