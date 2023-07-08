[Serializable]
public class Config
{
    public string WhatIsThisFile { get; set; }
    public byte characterWidth { get; set; }
    public byte characterHeight { get; set; }
    public UInt16 maxCharacters { get; set; }

    public byte foregroundColorIndex { get; set; }
    public byte backgroundColorIndex { get; set; }

    public string[] bitmapForegroundColor { get; set; }
    public string[] bitmapBackgroundColor { get; set; }

    public string filePath;

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
}
