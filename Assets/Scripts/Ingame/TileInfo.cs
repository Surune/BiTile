public class TileInfo
{
    public char Type { get; set; }
    public char Color { get; set; }

    public TileInfo(char type, char color)
    {
        Type = type;
        Color = color;
    }
}