using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.CollisionHandling;
using ThermalClasses.GameObjects;
using ThermalClasses.GameObjects.Particles;
using ThermalClasses.Handlers;

namespace ThermalSimulation;

public class Game1 : Game
{
    #region Fields
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderTarget2D window; // This render target is used to draw everything correct to the game window

    private int windowHeight, windowWidth;
    private Color backgroundColor = Color.Black;
    private Rectangle renderRectangle;
    private SimulationHandler simulationHandler;
    #endregion

    #region Methods
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        InitRenderTarget();

        // Initialising and calling the Init() methods of my handlers
        simulationHandler = new(this, renderRectangle)
        {
            BackgroundColour = backgroundColor,
            HoverColour = Color.Gray,
        };
        simulationHandler.Initialize();

        // Other basic statements used in the Init() function
        IsMouseVisible = true;
        _graphics.PreferMultiSampling = false;
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Calling the LoadContent() methods of my handlers
        simulationHandler.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        // Calling the Update methods of my handlers
        simulationHandler.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(backgroundColor);

        _graphics.GraphicsDevice.SetRenderTarget(window);
        _spriteBatch.Begin(SpriteSortMode.Deferred,
        BlendState.AlphaBlend,
        SamplerState.PointClamp,
        null, null, null, null);

        // Draw everything onto the render target
        // Calling the Draw methods of my handlers
        simulationHandler.Draw(gameTime, _spriteBatch);

        _spriteBatch.End();
        _graphics.GraphicsDevice.SetRenderTarget(null);

        // Draw render target to back buffer
        _spriteBatch.Begin(SpriteSortMode.Deferred,
        BlendState.AlphaBlend,
        SamplerState.PointClamp,
        null, null, null, null); // Sets SamplerState to PointClamp - no colour interpolation, just pixel scaling
        _spriteBatch.Draw(window, renderRectangle, Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    // Code modified from http://www.infinitespace-studios.co.uk/general/monogame-scaling-your-game-using-rendertargets-and-touchpanel/
    private void InitRenderTarget()
    {
        windowWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        windowHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.PreferredBackBufferHeight = windowHeight; // Values laptop screen: 1200, monitor: 1080
        _graphics.PreferredBackBufferWidth = windowWidth; // Values laptop screen: 1920, monitor: 1920
        _graphics.ApplyChanges();
        renderRectangle = new(0, 0, windowWidth, windowHeight);
        window = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
    }

    protected override void Dispose(bool disposing)
    {
        // Render targets need to be disposed of manually
        window.Dispose();
        base.Dispose(disposing);
    }
    #endregion
}
