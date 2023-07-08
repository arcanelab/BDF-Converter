#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

public class BDFConverter
{
    byte characterWidth = 0;
    byte characterHeight = 0;
    uint characterIndex = 0;
    Bitmap bitmap = null;
    List<byte> tileData = new List<byte>();

    public BDFConverter()
    {
    }

    public BDFConverter(byte width, byte height)
    {
        this.characterWidth = width;
        this.characterHeight = height;

        Console.WriteLine($"Overriding character size to {width}x{height}.");
    }

    public void ProcessLines(in string[] lines, in string path)
    {
        bool readBitmapData = false;
        int i = 0;
        List<string> bitmapLines = new List<string>();
        foreach (string line in lines)
        {
            if (i < 50 && (characterWidth == 0 | characterHeight == 0)) // check only the top 50 lines for FONTBOUNDINGBOX
            {
                var boundingBoxRegex = new Regex(@"^FONTBOUNDINGBOX\s+(\d+)\s+(\d+)\s+(-?\d+)");
                if (boundingBoxRegex.IsMatch(line))
                {
                    characterWidth = byte.Parse(boundingBoxRegex.Match(line).Groups[1].Value);
                    characterHeight = byte.Parse(boundingBoxRegex.Match(line).Groups[2].Value);
                    Console.WriteLine($"Character size: {characterWidth}x{characterHeight}");
                    continue;
                }
            }
            else if (i == 50)
            {
                if (characterWidth == 0 || characterHeight == 0)
                {
                    Console.WriteLine("Error: Could not find FONTBOUNDINGBOX or invalid size overrides.");
                    return;
                }
            }

            if (line.Trim() == "BITMAP")
            {
                readBitmapData = true;
                continue;
            }
            if (line.Trim() == "ENDCHAR")
            {
                readBitmapData = false;
                ProcessBitmapData(bitmapLines, characterWidth, characterHeight);
                bitmapLines.Clear();
                continue;
            }

            if (readBitmapData)
            {
                bitmapLines.Add(line.Trim());
            }

            if (characterIndex > 255)
            {
                break;
            }
        }

        if (bitmap != null && tileData.Count > 0)
        {
            Console.WriteLine($"Processed {characterIndex} characters.");
            string outputFilename = Path.ChangeExtension(path, ".png");
            Console.WriteLine($"Writing file {outputFilename}");
            bitmap.Save(outputFilename, ImageFormat.Png);

            string tileDataFilename = Path.ChangeExtension(path, ".tiledata");
            Console.WriteLine($"Writing file {tileDataFilename}");
            File.WriteAllBytes(tileDataFilename, tileData.ToArray());
        }
        else
        {
            Console.WriteLine("Error: Could not process font file.");
            Console.WriteLine($"bitmap = {bitmap != null}");
            Console.WriteLine($"tileData.Count = {tileData.Count}");
        }
    }

    void ProcessBitmapData(List<string> bitmapLines, int width, int height)
    {
        if (bitmap == null)
        {
            bitmap = new Bitmap(width * 16, height * 16, PixelFormat.Format32bppArgb);
            Console.WriteLine($"Bitmap size: {bitmap.Width}x{bitmap.Height}");
        }

        byte x = (byte)(((characterIndex) % 16) * width);
        byte y = (byte)(((characterIndex) / 16) * height);

        int maxLines = Math.Min(bitmapLines.Count, height);
        int maxColumns = Math.Min(width, 8);
        for (int i = 0; i < maxLines; i++)
        {
            string line = bitmapLines[i];
            int number = int.Parse(line, System.Globalization.NumberStyles.HexNumber);
            for (int j = 0; j < maxColumns; j++)
            {
                int mask = 1 << (maxColumns - 1 - j);
                if ((number & (mask)) != 0)
                {
                    try
                    {
                        bitmap.SetPixel(x + j, y + i, Color.White);
                        tileData.Add(35);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"number = {number}");
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        bitmap.SetPixel(x + j, y + i, Color.Black);
                        tileData.Add(0);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\nError at {x + j},{y + i}");
                        // Console.WriteLine($"bitmapLines.Count = {bitmapLines.Count}");
                        // Console.WriteLine($"maxLines = {maxLines}");
                        // Console.WriteLine($"width = {width}, height = {height}");
                        // Console.WriteLine($"bitmap.Width = {bitmap.Width}, bitmap.Height = {bitmap.Height}");
                        // Console.WriteLine($"x = {x}, y = {y}, i = {i}, j = {j}");
                        // Console.WriteLine($"characterIndex = {characterIndex}\n");
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
            }
        }
        characterIndex++;
    }
}