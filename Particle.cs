namespace ParticleSim;

public class Particle
{
    public Material Material { get; set; }
    public bool HasBeenUpdated { get; set; }

    public Particle(Material _material)
    {
        Material = _material;
        HasBeenUpdated = false;
    }
}