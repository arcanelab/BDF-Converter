#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text.RegularExpressions;

public class BDFConverter
{
    byte characterWidth = 0;
    byte characterHeight = 0;
    uint characterIndex = 0;
    Bitmap bitmap = null;
    List<byte> tileData = new List<byte>();
    Config config;

    public BDFConverter(Config config)
    {
        this.config = config;
        this.characterWidth = config.characterWidth;
        this.characterHeight = config.characterHeight;

        Console.WriteLine($"Overriding character size to {characterWidth}x{characterHeight}.");
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
                if (CheckForBoundingBox(line)) continue;
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

            if (characterIndex > config.maxCharacters - 1)
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

            if (!File.Exists("config.json"))
            {
                string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"Writing file config.json");
                File.WriteAllText("config.json", jsonString);
            }
        }
        else
        {
            Console.WriteLine("Error: Could not process font file.");
            Console.WriteLine($"bitmap = {bitmap != null}");
            Console.WriteLine($"tileData.Count = {tileData.Count}");
        }
    }

    bool CheckForBoundingBox(string line)
    {
        var boundingBoxRegex = new Regex(@"^FONTBOUNDINGBOX\s+(\d+)\s+(\d+)\s+(-?\d+)");
        if (boundingBoxRegex.IsMatch(line))
        {
            characterWidth = byte.Parse(boundingBoxRegex.Match(line).Groups[1].Value);
            characterHeight = byte.Parse(boundingBoxRegex.Match(line).Groups[2].Value);
            Console.WriteLine($"Character size: {characterWidth}x{characterHeight}");
            return true;
        }
        return false;
    }

    void ProcessBitmapData(List<string> bitmapLines, int width, int height)
    {
        if (bitmap == null)
        {
            Console.WriteLine($"maxcharacters = {config.maxCharacters}");
            int dimension = (int)Math.Ceiling(Math.Sqrt(config.maxCharacters));
            Console.WriteLine($"dimension = {dimension}");
            if (dimension < 1)
            {
                Console.WriteLine($"Error: Invalid maxCharacters value {config.maxCharacters}");
                Environment.Exit(0);
            }
            bitmap = new Bitmap(width * dimension, height * dimension, PixelFormat.Format32bppRgb);
            FillBitmapToBackgroundColor();
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
                if (((number << (j + 1)) & 0x100) != 0) // check 9th bit from the right
                {
                    StoreColor(config.bitmapForegroundColor, config.foregroundColorIndex, x + j, y + i, ref tileData);
                }
                else
                {
                    StoreColor(config.bitmapBackgroundColor, config.backgroundColorIndex, x + j, y + i, ref tileData);
                }
            }
        }
        characterIndex++;
    }

    private void FillBitmapToBackgroundColor()
    {
        for (int i = 0; i < bitmap.Width; i++)
        {
            for (int j = 0; j < bitmap.Height; j++)
            {
                bitmap.SetPixel(i, j, TryGetColorsFromStrings(config.bitmapBackgroundColor, Color.Black));
            }
        }
    }

    void StoreColor(string[] colorArray, byte colorIndex, int x, int y, ref List<byte> tileData)
    {
        try
        {
            Color color = TryGetColorsFromStrings(colorArray, Color.Black);
            bitmap.SetPixel(x, y, color);
            tileData.Add(colorIndex);
        }
        catch (Exception e)
        {
            Console.WriteLine($"\nError at {x},{y}");
            Console.WriteLine(e.Message);
        }
    }

    Color TryGetColorsFromStrings(string[] colorComponents, Color fallbackColor)
    {
        Color color;

        if (colorComponents == null || colorComponents.Length != 3)
        {
            return fallbackColor;
        }

        try
        {
            color = Color.FromArgb(config.foregroundColorIndex,
                                   int.Parse(colorComponents[0]),
                                   int.Parse(colorComponents[1]),
                                   int.Parse(colorComponents[2]));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            color = fallbackColor;
        }

        return color;
    }
}