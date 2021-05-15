using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParticleSim
{
	public class Materials
	{
		public static Material Air = new Material(true, false, false, false, 1, Color.Black);
		public static Material Sand = new Material(false, false, true, true, 3, Color.Yellow);
		public static Material Water = new Material(false, true, false, true, 2, Color.Blue);
		public static Material Concrete = new Material(false, false, true, false, 99, Color.Gray);
		public static Material Steam = new Material(true, false, false, true, 0, Color.AliceBlue);

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
}
