#pragma warning disable CA1416

using System.Text.Json;

class Application
{
    static void Main(string[] args)
    {
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            Console.WriteLine("This application only runs on Windows.");
            return;
        }

        PrintCommandLineArgumentsIfNeeded(args);

        Config config = GetConfig();

        config.filePath = args[0];
        string[] lines = ReadLines(config.filePath);

        BDFConverter converter;

        if (args.Length == 3)
        {
            config.characterWidth = byte.Parse(args[1]);
            config.characterHeight = byte.Parse(args[2]);
        }

        converter = new BDFConverter(config);
        converter.ProcessLines(lines, config.filePath);
    }

    static void PrintCommandLineArgumentsIfNeeded(string[] args)
    {
        var executableName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        if (args.Length == 0)
        {
            Console.WriteLine($"{executableName} <bdf-font-file> [width-override] [height-override]");
            Console.WriteLine("Example: " + executableName + " font.bdf 8 8");
            Console.WriteLine("\nOutput:");
            Console.WriteLine(" - the font atlas in png format");
            Console.WriteLine(" - the tile data in binary format");
            Console.WriteLine("\nOnly fixed-width fonts are supported.");
            Console.WriteLine("The font atlas will be 16x16 characters.");
            Console.WriteLine("Only up to 256 characters will be processed.");
            Environment.Exit(0);
        }
    }

    static string[] ReadLines(string filename)
    {
        string[] lines;

        try
        {
            lines = System.IO.File.ReadAllLines(filename);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file {filename}: {ex.Message}");
            return null;
        }

        return lines;
    }

    public static Config GetConfig()
    {
        Config config = new Config();
        if (System.IO.File.Exists("config.json"))
        {
            try
            {
                config = JsonSerializer.Deserialize<Config>(System.IO.File.ReadAllText("config.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading config.json: {ex.Message}");
                return null;
            }
        }

        return config;
    }
}
