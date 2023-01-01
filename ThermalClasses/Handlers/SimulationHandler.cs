using System.Net.Mime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects;
using ThermalClasses.GameObjects.Particles;

namespace ThermalClasses.Handlers;

public class SimulationHandler : Handler
{
    #region Fields
    private Polygon[] particles;
    private SimulationBox simulationBox;
    private int volume, maxVolume; // Measured in metres cubed
    private float temperature; // Measured in Kelvin
    private float pressure; // Measured in Pascals
    #endregion

    #region Methods
    public SimulationHandler(Game game, Rectangle renderRectangle)
    {
        this.renderRectangle = renderRectangle;
        this.game = game;
        content = game.Content;

        // Initialising physical properties
        volume = 100;
        temperature = 273;
        pressure = 100;
        maxVolume = 300;
    }

    #region Particle Initialisation
    private void InitialiseParticles()
    {
        for (var i = 0; i < particles.Length; i++)
        {
            if (i % 500 == 0)
            {
                particles[i] = NewSmallCircle();
            }
            else
            {
                particles[i] = NewLargeCircle();
            }
        }
    }

    private Polygon NewSmallCircle()
    {
        return new(content.Load<Texture2D>("YellowParticle"), new Vector2(0, 0), new Vector2(0, 0), 100, 50, new Color(10, 10, 10), new Point(10, 10))
        {
            Enabled = false,
        };
    }

    private Polygon NewLargeCircle()
    {
        return new(content.Load<Texture2D>("BlueParticle"), new Vector2(0, 0), new Vector2(0, 0), 200, 50, new Color(10, 10, 10), new Point(20, 20))
        {
            Enabled = false,
        };
    }
    #endregion

    #region Game Object Initialisation
    private void InitSimBox()
    {
        Point fixedMovingStart = new(renderRectangle.Left, (int)(renderRectangle.Top + (renderRectangle.Height * 0.2)));
        Point movingSize = new(10, (int)(renderRectangle.Height * 0.6));
        Point fixedSize = new((int)(renderRectangle.Width * 0.6), movingSize.Y);
        Rectangle fixedRect = new(fixedMovingStart, fixedSize);
        Rectangle movingRect = new(fixedMovingStart, movingSize);

        GameObject fixedBox = new GameObject(content.Load<Texture2D>("FixedBox"), Color.White, fixedRect);
        GameObject movingBox = new GameObject(content.Load<Texture2D>("MovingBox"), Color.White, movingRect);

        simulationBox = new SimulationBox(game, fixedBox, movingBox, (int)(renderRectangle.Width * 0.6), (int)(renderRectangle.Width * 0.05));
    }
    #endregion

    public override void Initialize()
    {
    }

    public override void LoadContent()
    {
        // Particle Initialisation
        particles = new Polygon[1000];
        InitialiseParticles();

        // GameObject Initialisation
        InitSimBox();
    }

    // Calls the update method of all objects that need updating (buttons, particles, sliders etc.)
    public override void Update(GameTime gameTime)
    {
    }

    // Calls the draw methods of all GameObjects
    public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        simulationBox.Draw(_spriteBatch);
    }
    #endregion
}