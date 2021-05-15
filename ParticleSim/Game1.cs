using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;

namespace ParticleSim
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		// Window resolution
		private Vector2 windowSize = new Vector2(1920, 1080) / 1.5f;

		// The size of the simulated area
		private int dataHeight;
		private int dataWidth;

		// The array used to store the simulation state
		private Particle[,] dataArray;

		// How many real pixels a simulation pixel takes up
		private int scaleMod = 4;

		private int prevScrollValue = 0;
		private int addRadius = 1;

		private Random random = new Random();

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);

			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			dataHeight = (int)(windowSize.X / scaleMod);
			dataWidth = (int)(windowSize.Y / scaleMod);
			dataArray = new Particle[dataHeight, dataWidth];
		}

		protected override void Initialize()
		{
			_graphics.PreferredBackBufferHeight = (int)windowSize.Y;
			_graphics.PreferredBackBufferWidth = (int)windowSize.X;
			//_graphics.IsFullScreen = true;
			_graphics.ApplyChanges();

			// Initialise the scene with a concrete border
			for (int i = 0; i < dataHeight; i++)
			{
				for (int j = 0; j < dataWidth; j++)
				{
					if (i == 0 || j == 0) dataArray[i, j] = new Particle(Materials.Concrete);
					else if (i == dataHeight - 1 || j == dataWidth - 1) dataArray[i, j] = new Particle(Materials.Concrete);
					else dataArray[i, j] = new Particle(Materials.Air);
				}
			}

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		protected override void Update(GameTime gameTime)
		{
			// Exit if the user presses escape
			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			// Loop through every particle and update it
			OuterLoop();

			// Reset/update any needed particle properties after each frame
			UpdateAfterSim((float)gameTime.ElapsedGameTime.TotalSeconds);

			// Add new particles based on user's input
			AddFromUserInput();

			base.Update(gameTime);
		}

		private void OuterLoop()
		{
			// Randomise the loop direction to make movement less predictable
			if (random.NextDouble() < 0.5)
			{
				for (int i = 0; i < dataHeight; i++)
				{
					InnerLoop(i);
				}
			}
			else
			{
				for (int i = dataHeight - 1; i > -1; i--)
				{
					InnerLoop(i);
				}
			}
		}

		private void InnerLoop(int i)
		{
			// Randomise the loop direction to make movement less predictable
			if (random.NextDouble() < 0.5)
			{
				for (int j = dataWidth - 1; j > -1; j--)
				{
					UpdateParticle(i, j);
				}
			}
			else
			{
				for (int j = 0; j < dataWidth; j++)
				{
					UpdateParticle(i, j);
				}
			}
		}

		/// <summary>
		/// Update the particle at the given position
		/// </summary>
		private void UpdateParticle(int i, int j)
		{
			Particle p = dataArray[i, j];
			// Only update the particle if it has not already been updated this frame, and the particle is dynamic
			if (!p.HasBeenUpdated && dataArray[i, j].Material.IsDynamic)
			{
				p.HasBeenUpdated = true;

				// If the particle is water, try to evaporate it
				TryEvaporateWater(i, j, p);

				// If the particle has a higher density than the particle directly below it, they should swap places
				if (dataArray[i, j + 1].Material.Density < p.Material.Density)
				{
					dataArray[i, j] = dataArray[i, j + 1];
					dataArray[i, j + 1] = p;
					return;
				}
				// Same as above, except directly above and swap places if lower density (Mostly only for gasses)
				if (dataArray[i, j - 1].Material.IsGas && dataArray[i, j - 1].Material.Density > p.Material.Density)
				{
					dataArray[i, j] = dataArray[i, j - 1];
					dataArray[i, j - 1] = p;
					return;
				}

				// Check below and to the left, and below and to the right, to check if there is space to move into
				// Randomness is used to switch the order in which left/right are checked, to help prevent a bias to either side.
				if (random.NextDouble() < 0.5)
				{
					if (dataArray[i + 1, j + 1].Material.Density < p.Material.Density)
					{
						dataArray[i, j] = dataArray[i + 1, j + 1];
						dataArray[i + 1, j + 1] = p;
					}
					else if (dataArray[i - 1, j + 1].Material.Density < p.Material.Density)
					{
						dataArray[i, j] = dataArray[i - 1, j + 1];
						dataArray[i - 1, j + 1] = p;
					}
				}
				else
				{
					if (dataArray[i - 1, j + 1].Material.Density < p.Material.Density)
					{
						dataArray[i, j] = dataArray[i - 1, j + 1];
						dataArray[i - 1, j + 1] = p;
					}
					else if (dataArray[i + 1, j + 1].Material.Density < p.Material.Density)
					{
						dataArray[i, j] = dataArray[i + 1, j + 1];
						dataArray[i + 1, j + 1] = p;
					}
				}

				// Steam should vanish after a while
				if (p.Material == Materials.Steam)
				{

					if (random.NextDouble() < 0.001)
					{
						p.Material = Materials.Air;
						return;
					}
				}

				if (p.Material.IsLiquid)
				{
					MoveToSides(i, j);
				}
				else if (p.Material.IsGas)
				{
					MoveToSides(i, j);
				}
			}
		}

		/// <summary>
		/// Reset the HasBeenUpdated property for the next frame and update the particle's timer
		/// </summary>
		private void UpdateAfterSim(float elapsedTime)
		{
			for (int i = 0; i < dataHeight; i++)
			{
				for (int j = 0; j < dataWidth; j++)
				{
					dataArray[i, j].HasBeenUpdated = false;
					dataArray[i, j].Timer += elapsedTime;
				}
			}
		}

		/// <summary>
		/// Add new particles based on user input
		/// </summary>
		private void AddFromUserInput()
		{
			MouseState mState = Mouse.GetState();

			int scrollDiff = 0;
			if (prevScrollValue > mState.ScrollWheelValue) scrollDiff--;
			else if (prevScrollValue < mState.ScrollWheelValue) scrollDiff++;
			prevScrollValue = mState.ScrollWheelValue;
			addRadius += scrollDiff;

			addRadius = Math.Clamp(addRadius, 1, 15);

			int x = mState.X / scaleMod;
			int y = mState.Y / scaleMod;
			for (int i = x - addRadius; i <= x + addRadius; i++)
			{
				for (int j = y - addRadius; j <= y + addRadius; j++)
				{
					if (i > 0 && j > 0 && i < dataHeight - 1 && j < dataWidth - 1)
					{
						if (MathF.Sqrt((j-y) * (j-y) + (i-x) * (i-x)) < addRadius)
						{

							try
							{
								if (dataArray[i, j].Material == Materials.Air)
								{
									if (mState.LeftButton == ButtonState.Pressed)
										dataArray[i, j] = new Particle(Materials.Sand);
									else if (mState.RightButton == ButtonState.Pressed)
										dataArray[i, j] = new Particle(Materials.Water);
									else if (mState.MiddleButton == ButtonState.Pressed)
										dataArray[i, j] = new Particle(Materials.Concrete);
								}
							}
							catch { }
						}
					}
				}
			}
			
		}

		/// <summary>
		/// If the particle has gas above and on either side of it, then it can evaporate
		/// </summary>
		private void TryEvaporateWater(int i, int j, Particle p)
		{
			if (p.Material.IsLiquid)
			{
				if (dataArray[i, j - 1].Material.IsGas && dataArray[i - 1, j].Material.IsGas && dataArray[i, j - 1].Material.IsGas)
				{
					if (random.NextDouble() < 0.001)
					{
						p.Material = Materials.Steam;
						return;
					}
				}
			}
		}

		/// <summary>
		/// Move the particle to either side to simulate water/gas flowing
		/// </summary>
		private void MoveToSides(int i, int j)
		{
			Particle p = dataArray[i, j];
			if (random.NextDouble() < 0.5)
			{
				if (dataArray[i + 1, j].Material.Density < p.Material.Density || dataArray[i + 1, j].Material.IsGas)
				{
					LiquidTryMove(1, i, j);
				}
				else if (dataArray[i - 1, j].Material.Density < p.Material.Density || dataArray[i - 1, j].Material.IsGas)
				{
					LiquidTryMove(-1, i, j);
				}
			}
			else
			{
				if (dataArray[i - 1, j].Material.Density < p.Material.Density || dataArray[i - 1, j].Material.IsGas)
				{
					LiquidTryMove(-1, i, j);
				}
				else if (dataArray[i + 1, j].Material.Density < p.Material.Density || dataArray[i + 1, j].Material.IsGas)
				{
					LiquidTryMove(1, i, j);
				}
			}
		}

		private void LiquidTryMove(int moveBy, int i, int j)
		{
			Particle p = dataArray[i, j];
			dataArray[i, j] = dataArray[i + moveBy, j];
			dataArray[i + moveBy, j] = p;
		}

		/// <summary>
		/// Draw the current simulation state to the screen
		/// </summary>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			_spriteBatch.Begin();

			// Draw every particle
			for (int i = 0; i < dataHeight; i++)
			{
				for (int j = 0; j < dataWidth; j++)
				{
					_spriteBatch.DrawPoint(new Vector2(i + 0.5f, j + 0.5f) * scaleMod, dataArray[i, j].Material.Colour, scaleMod);
				}
			}

			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
