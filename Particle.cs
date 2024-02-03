namespace ParticleSim;

public class Particle
{
    public Material Material { get; set; }
    public bool HasBeenUpdated { get; set; }

    public Particle(Material material)
    {
        Material = material;
        HasBeenUpdated = false;
    }
}