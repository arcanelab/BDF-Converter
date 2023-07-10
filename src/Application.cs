#pragma warning disable CA1416

class Application
{
    static void Main(string[] args)
    {
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            Console.WriteLine("This application only runs on Windows.");
            return;
        }

        Config config = SetupConfiguration(args);
        if (config == null)
            return;

        BDFConverter converter = new BDFConverter(config);

        string[] lines = ReadLines(config.filePath);
        converter.ProcessLines(lines, config.filePath);
    }

    static Config SetupConfiguration(string[] args)
    {
        if (args.Length != 1 && args.Length != 3)
        {
            PrintCommandLineHelp(args);
            return null;
        }

        // Loads the config file if it exists.
        // Creates a default one if it doesn't.
        Config config = Config.GetConfigFile();

        // Set file path.
        config.filePath = args[0];

        // Set character size overrides if provided.
        if (args.Length == 3)
        {
            config.characterWidth = byte.Parse(args[1]);
            config.characterHeight = byte.Parse(args[2]);
        }

        return config;
    }

    static void PrintCommandLineHelp(string[] args)
    {
        var executableName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        Console.WriteLine($"{executableName} <bdf-font-file> [width-override] [height-override]");
        Console.WriteLine("Example: " + executableName + " font.bdf 8 8");
        Console.WriteLine("\nOutput:");
        Console.WriteLine(" - the font atlas in png format");
        Console.WriteLine(" - the tile data in binary format");
        Console.WriteLine("\nOnly fixed-width fonts are supported.");
    }

    static string[] ReadLines(string filename)
    {
        string[] lines = null;

        try
        {
            lines = System.IO.File.ReadAllLines(filename);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file {filename}:\n{ex.Message}");
            Environment.Exit(1);
        }

        return lines;
    }
}
