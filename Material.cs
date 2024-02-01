using Microsoft.Xna.Framework;

namespace ParticleSim;

public static class Materials
{
    public static readonly Material Air = new Material(true, false, false, false, 1, Color.Black);
    public static readonly Material Sand = new Material(false, false, true, true, 3, Color.Yellow);
    public static readonly Material Water = new Material(false, true, false, true, 2, Color.Blue);
    public static readonly Material Concrete = new Material(false, false, true, false, 99, Color.Gray);
    public static readonly Material Steam = new Material(true, false, false, true, 0, Color.AliceBlue);
}


public class Material
{
    public Material(bool isGas, bool isLiquid, bool isSolid, bool isDynamic, int density, Color colour)
    {
        IsGas = isGas;
        IsLiquid = isLiquid;
        IsSolid = isSolid;
        IsDynamic = isDynamic;
        Density = density;
        Colour = colour;
    }

    public bool IsGas { get; set; }
    public bool IsLiquid { get; set; }
    public bool IsSolid { get; set; }
    public bool IsDynamic { get; set; }
    public int Density { get; set; }
    public Color Colour { get; set; }
}