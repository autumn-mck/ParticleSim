namespace ParticleSim
{
	class Particle
	{
		public Material Material { get; set; }
		public float Timer { get; set; }
		public bool HasBeenUpdated { get; set; }

		public Particle(Material _material)
		{
			Material = _material;
			Timer = 0f;
			HasBeenUpdated = false;
		}
	}
}
