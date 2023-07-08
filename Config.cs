using System.Text.Json;

[Serializable]
public class Config
{
    public string WhatIsThisFile { get; set; }
    public UInt16 maxCharacters { get; set; }

    public byte foregroundColorIndex { get; set; }
    public byte backgroundColorIndex { get; set; }

    public string[] bitmapForegroundColor { get; set; }
    public string[] bitmapBackgroundColor { get; set; }

    // non-serialized properties
    public byte characterWidth;
    public byte characterHeight;
    public string filePath;
    public int bitmapCharacterDimension;

    public Config()
    {
        WhatIsThisFile = "BDFConverter config file";
        characterWidth = 0;
        characterHeight = 0;
        maxCharacters = 256;

        foregroundColorIndex = 35; // '#'
        backgroundColorIndex = 0;

        bitmapForegroundColor = new string[3];
        bitmapForegroundColor.SetValue("200", 0);
        bitmapForegroundColor.SetValue("200", 1);
        bitmapForegroundColor.SetValue("200", 2);

        bitmapBackgroundColor = new string[3];
        bitmapBackgroundColor.SetValue("38", 0);
        bitmapBackgroundColor.SetValue("38", 1);
        bitmapBackgroundColor.SetValue("38", 2);
    }

    // Reads the config.json file if it exists.
    // Creates a default one if it doesn't.
    public static Config GetConfigFile()
    {
        Config config = new Config();
        if (System.IO.File.Exists("config.json"))
        {
            Console.WriteLine("Reading config.json...");
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
        else
        {
            Console.WriteLine("No config.json found, a default one will be created if conversion succeeds.");
        }

        return config;
    }

    public static void WriteConfigFile(Config config)
    {
        string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine($"Creating config.json");
        File.WriteAllText("config.json", jsonString);
    }
}
