using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace ParticleSim;

public class Game : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Window resolution
    private readonly Vector2 _windowSize = new Vector2(1920, 1080) / 1.5f;

    // The size of the simulated area
    private readonly int _dataHeight;
    private readonly int _dataWidth;

    // The array used to store the simulation state
    private readonly Particle[,] _dataArray;

    // How many real pixels a simulation pixel takes up
    private const int ScaleMod = 4;

    private int _prevScrollValue = 0;
    private int _addRadius = 1;

    private readonly Random _random = new();

    public Game()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _dataHeight = (int)(_windowSize.X / ScaleMod);
        _dataWidth = (int)(_windowSize.Y / ScaleMod);
        _dataArray = new Particle[_dataHeight, _dataWidth];
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferHeight = (int)_windowSize.Y;
        _graphics.PreferredBackBufferWidth = (int)_windowSize.X;
        //_graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

        // Initialise the scene with a concrete border
        for (var i = 0; i < _dataHeight; i++)
        {
            for (var j = 0; j < _dataWidth; j++)
            {
                if (i == 0 || j == 0) _dataArray[i, j] = new Particle(Materials.Concrete);
                else if (i == _dataHeight - 1 || j == _dataWidth - 1) _dataArray[i, j] = new Particle(Materials.Concrete);
                else _dataArray[i, j] = new Particle(Materials.Air);
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
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Loop through every particle and update it
        OuterLoop();

        // Reset/update any needed particle properties after each frame
        UpdateAfterSim();

        // Add new particles based on user's input
        AddFromUserInput();

        base.Update(gameTime);

        base.Update(gameTime);
    }

    private void OuterLoop()
    {
        // Randomise the loop direction to make movement less predictable
        if (_random.NextDouble() < 0.5)
        {
            for (var i = 0; i < _dataHeight; i++)
            {
                InnerLoop(i);
            }
        }
        else
        {
            for (var i = _dataHeight - 1; i > -1; i--)
            {
                InnerLoop(i);
            }
        }
    }

    private void InnerLoop(int i)
    {
        // Randomise the loop direction to make movement less predictable
        if (_random.NextDouble() < 0.5)
        {
            for (var j = _dataWidth - 1; j > -1; j--)
            {
                UpdateParticle(i, j);
            }
        }
        else
        {
            for (var j = 0; j < _dataWidth; j++)
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
        var p = _dataArray[i, j];
        // Only update the particle if it has not already been updated this frame, and the particle is dynamic
        if (p.HasBeenUpdated ||
            (_dataArray[i, j].Material is Solid && ((Solid)_dataArray[i, j].Material).IsStatic))
            return;

        p.HasBeenUpdated = true;

        // If the particle is water, try to evaporate it
        TryEvaporateWater(i, j, p);

        // If the particle has a higher density than the particle directly below it, they should swap places
        if (_dataArray[i, j + 1].Material.Density < p.Material.Density)
        {
            _dataArray[i, j] = _dataArray[i, j + 1];
            _dataArray[i, j + 1] = p;
            return;
        }
        // Same as above, except directly above and swap places if lower density (Mostly only for gasses)
        if (_dataArray[i, j - 1].Material is Gas && _dataArray[i, j - 1].Material.Density > p.Material.Density)
        {
            _dataArray[i, j] = _dataArray[i, j - 1];
            _dataArray[i, j - 1] = p;
            return;
        }

        // Check below and to the left, and below and to the right, to check if there is space to move into
        // Randomness is used to switch the order in which left/right are checked, to help prevent a bias to either side.
        if (_random.NextDouble() < 0.5)
        {
            if (_dataArray[i + 1, j + 1].Material.Density < p.Material.Density)
            {
                _dataArray[i, j] = _dataArray[i + 1, j + 1];
                _dataArray[i + 1, j + 1] = p;
            }
            else if (_dataArray[i - 1, j + 1].Material.Density < p.Material.Density)
            {
                _dataArray[i, j] = _dataArray[i - 1, j + 1];
                _dataArray[i - 1, j + 1] = p;
            }
        }
        else
        {
            if (_dataArray[i - 1, j + 1].Material.Density < p.Material.Density)
            {
                _dataArray[i, j] = _dataArray[i - 1, j + 1];
                _dataArray[i - 1, j + 1] = p;
            }
            else if (_dataArray[i + 1, j + 1].Material.Density < p.Material.Density)
            {
                _dataArray[i, j] = _dataArray[i + 1, j + 1];
                _dataArray[i + 1, j + 1] = p;
            }
        }

        // Steam should vanish after a while
        if (p.Material is Steam)
        {

            if (_random.NextDouble() < 0.001)
            {
                p.Material = Materials.Air;
                return;
            }
        }

        switch (p.Material)
        {
            case Liquid:
            case Gas:
                MoveToSides(i, j);
                break;
        }
    }

    /// <summary>
    /// Reset the HasBeenUpdated property for the next frame and update the particle's timer
    /// </summary>
    private void UpdateAfterSim()
    {
        for (var i = 0; i < _dataHeight; i++)
        {
            for (var j = 0; j < _dataWidth; j++)
            {
                _dataArray[i, j].HasBeenUpdated = false;
            }
        }
    }

    /// <summary>
    /// Add new particles based on user input
    /// </summary>
    private void AddFromUserInput()
    {
        var mState = Mouse.GetState();

        var scrollDiff = 0;
        if (_prevScrollValue > mState.ScrollWheelValue) scrollDiff--;
        else if (_prevScrollValue < mState.ScrollWheelValue) scrollDiff++;
        _prevScrollValue = mState.ScrollWheelValue;
        _addRadius += scrollDiff;

        _addRadius = Math.Clamp(_addRadius, 1, 15);

        int x = mState.X / ScaleMod;
        int y = mState.Y / ScaleMod;
        for (var i = x - _addRadius; i <= x + _addRadius; i++)
        {
            for (var j = y - _addRadius; j <= y + _addRadius; j++)
            {
                if (i <= 0 || j <= 0 || i >= _dataHeight - 1 || j >= _dataWidth - 1) continue;
                if (!(MathF.Sqrt((j - y) * (j - y) + (i - x) * (i - x)) < _addRadius)) continue;
                try
                {
                    if (_dataArray[i, j].Material is Air)
                    {
                        if (mState.LeftButton == ButtonState.Pressed)
                            _dataArray[i, j] = new Particle(Materials.Sand);
                        else if (mState.RightButton == ButtonState.Pressed)
                            _dataArray[i, j] = new Particle(Materials.Water);
                        else if (mState.MiddleButton == ButtonState.Pressed)
                            _dataArray[i, j] = new Particle(Materials.Concrete);
                    }
                }
                catch { }
            }
        }

    }

    /// <summary>
    /// If the particle has gas above and on either side of it, then it can evaporate
    /// </summary>
    private void TryEvaporateWater(int i, int j, Particle p)
    {
        if (p.Material is not Liquid) return;
        if (_dataArray[i, j - 1].Material is not Gas || _dataArray[i - 1, j].Material is not Gas ||
            _dataArray[i, j - 1].Material is not Gas) return;
        if (!(_random.NextDouble() < 0.001)) return;
        p.Material = Materials.Steam;
    }

    /// <summary>
    /// Move the particle to either side to simulate water/gas flowing
    /// </summary>
    private void MoveToSides(int i, int j)
    {
        var p = _dataArray[i, j];
        if (_random.NextDouble() < 0.5)
        {
            if (_dataArray[i + 1, j].Material.Density < p.Material.Density)
            {
                LiquidTryMove(1, i, j);
            }
            else if (_dataArray[i - 1, j].Material.Density < p.Material.Density)
            {
                LiquidTryMove(-1, i, j);
            }
        }
        else
        {
            if (_dataArray[i - 1, j].Material.Density < p.Material.Density)
            {
                LiquidTryMove(-1, i, j);
            }
            else if (_dataArray[i + 1, j].Material.Density < p.Material.Density)
            {
                LiquidTryMove(1, i, j);
            }
        }
    }

    private void LiquidTryMove(int moveBy, int i, int j)
    {
        (_dataArray[i, j], _dataArray[i + moveBy, j]) = (_dataArray[i + moveBy, j], _dataArray[i, j]);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // Draw every particle
        for (int i = 0; i < _dataHeight; i++)
        {
            for (int j = 0; j < _dataWidth; j++)
            {
                _spriteBatch.DrawPoint(new Vector2(i + 0.5f, j + 0.5f) * ScaleMod, _dataArray[i, j].Material.Colour, ScaleMod);
            }
        }

        _spriteBatch.End();

        base.Draw(gameTime);

        base.Draw(gameTime);
    }
}
