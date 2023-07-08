# BDF-Converter

BDF Converter is a command-line tool written in C# that can be used to convert font files in the BDF (Glyph Bitmap Distribution Format) format to PNG images and tile data. The tool generates a PNG image of the font atlas and a binary file containing the tile data.

# Usage
To use the BDF Converter tool, run the executable from the command line and pass in the path to the BDF font file as the first argument. Optionally, you can also specify the width and height of each character in the font as the second and third arguments, respectively.

`BDFConverter.exe <bdf-font-file> [width-override] [height-override]`

For example:

`BDFConverter.exe font.bdf 8 8`

This will generate a PNG image of the font atlas and a binary file containing the tile data.
