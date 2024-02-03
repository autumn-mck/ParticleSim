using Microsoft.Xna.Framework;

namespace ParticleSim;

public abstract class Material
{
    public abstract float Density { get; }
    public abstract Color Colour { get; }
}

public abstract class Solid : Material
{
    public abstract bool IsStatic { get; }
}

public abstract class Liquid : Material
{

}

public abstract class Gas : Material
{

}

public class Air : Gas
{
    public override float Density { get; } = 1;
    public override Color Colour { get; } = Color.Black;
}

public class Sand : Solid
{
    public override float Density { get; } = 3;
    public override Color Colour { get; } = Color.Yellow;
    public override bool IsStatic { get; } = true;
}

public class Water : Liquid
{
    public override float Density { get; } = 2;
    public override Color Colour { get; } = Color.Blue;
}

public class Concrete : Solid
{
    public override float Density { get; } = 99;
    public override Color Colour { get; } = Color.Gray;
    public override bool IsStatic { get; } = false;
}

public class Steam : Gas
{
    public override float Density { get; } = 0;
    public override Color Colour { get; } = Color.AliceBlue;
}

public static class Materials
{
    public static readonly Air Air = new();
    public static readonly Sand Sand = new();
    public static readonly Water Water = new();
    public static readonly Concrete Concrete = new();
    public static readonly Steam Steam = new();
}
