using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.CollisionHandling;
using ThermalClasses.GameObjects;
using ThermalClasses.GameObjects.Particles;
using ThermalClasses.Handlers;

namespace ThermalSimulation;

public class ThermalSim : Game
{
    #region Fields
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderTarget2D window; // This render target is used to draw everything correct to the game window

    private int windowHeight, windowWidth;
    private Color backgroundColour = Color.Black;
    private Color penColour = Color.White;
    private Color hoverColour = Color.Gray;
    private Rectangle renderRectangle;
    private SimulationHandler simulationHandler;
    private Button whiteButton, greenButton, orangeButton, cyanButton, yellowButton;
    private List<Button> colourButtonList;
    private List<Color> colourList;
    private Label penColourLabel, colourLabel;
    #endregion

    #region Methods
    public ThermalSim()
    {
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        InitializeRenderTarget();
        colourButtonList = new List<Button>();
        colourList = new List<Color>();
        // Initialising and calling the Init() methods of my handlers
        simulationHandler = new(this, renderRectangle)
        {
            BackgroundColour = backgroundColour,
            HoverColour = Color.Gray,
        };
        simulationHandler.Initialize();

        // Other basic statements used in the Init() function
        IsMouseVisible = true;
        _graphics.PreferMultiSampling = false;
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        base.Initialize();
    }

    // Code modified from http://www.infinitespace-studios.co.uk/general/monogame-scaling-your-game-using-rendertargets-and-touchpanel/
    private void InitializeRenderTarget()
    {
        // My preferred screen ratio: 16:9
        windowWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        windowHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.PreferredBackBufferHeight = windowHeight; // Values laptop screen: 1200, monitor: 1080
        _graphics.PreferredBackBufferWidth = windowWidth; // Values laptop screen: 1920, monitor: 1920
        _graphics.ApplyChanges();
        renderRectangle = new(0, 0, windowWidth, windowHeight);
        window = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        LoadColours();
        // Calling the LoadContent() methods of my handlers
        simulationHandler.LoadContent();
    }

    private void LoadColours()
    {
        Color unclickedColour = Color.White;
        SpriteFont font = Content.Load<SpriteFont>("GeneralAssets/Arial");
        Texture2D upDownLabelTexture = Content.Load<Texture2D>("GeneralAssets/LabelBox1");
        Texture2D whiteTexture = Content.Load<Texture2D>("Colours/WhiteColour");
        Texture2D greenTexture = Content.Load<Texture2D>("Colours/GreenColour");
        Texture2D yellowTexture = Content.Load<Texture2D>("Colours/YellowColour");
        Texture2D orangeTexture = Content.Load<Texture2D>("Colours/OrangeColour");
        Texture2D cyanTexture = Content.Load<Texture2D>("Colours/CyanColour");

        Rectangle penLabelRect = new Rectangle(10, renderRectangle.Top + 10, 200, 20);
        Rectangle coloursRect = new Rectangle(10, renderRectangle.Top + 10, 200, 70);
        colourLabel = new Label(upDownLabelTexture, unclickedColour, coloursRect, font, penColour);
        penColourLabel = new Label(upDownLabelTexture, unclickedColour, penLabelRect, font, penColour)
        {
            Text = "Change Text Colour"
        };
        Point colourSize = new Point(20,20);
        Vector2 colourPos = new Vector2(penLabelRect.Left + colourSize.X, penLabelRect.Bottom + colourSize.Y + 5);
        whiteButton = new Button(whiteTexture, font, colourPos, unclickedColour, penColour, colourSize)
        {
            HoverColour = hoverColour
        };
        whiteButton.Click += ColourButton_Click;
        colourPos.X += colourSize.X * 2;
        greenButton = new Button(greenTexture, font, colourPos, unclickedColour, penColour, colourSize)
        {
            HoverColour = hoverColour
        };
        greenButton.Click += ColourButton_Click;
        colourPos.X += colourSize.X * 2;
        yellowButton = new Button(yellowTexture, font, colourPos, unclickedColour, penColour, colourSize)
        {
            HoverColour = hoverColour
        };
        yellowButton.Click += ColourButton_Click;
        colourPos.X += colourSize.X * 2;
        orangeButton = new Button(orangeTexture, font, colourPos, unclickedColour, penColour, colourSize)
        {
            HoverColour = hoverColour
        };
        orangeButton.Click += ColourButton_Click;
        colourPos.X += colourSize.X * 2;
        cyanButton = new Button(cyanTexture, font, colourPos, unclickedColour, penColour, colourSize)
        {
            HoverColour = hoverColour
        };
        cyanButton.Click += ColourButton_Click;

        colourButtonList.Add(whiteButton);
        colourList.Add(Color.White);
        colourButtonList.Add(greenButton);
        colourList.Add(Color.Green);
        colourButtonList.Add(yellowButton);
        colourList.Add(Color.Yellow);
        colourButtonList.Add(orangeButton);
        colourList.Add(Color.Orange);
        colourButtonList.Add(cyanButton);
        colourList.Add(Color.Cyan);
    }

    #region Changing Pen Colour
    // Matches the index of the button to the index of the colour
    private void ColourButton_Click(object sender, EventArgs e)
    {
        for (var i = 0; i < colourButtonList.Count; i++)
        {
            if (colourButtonList[i] == sender)
            {
                penColour = colourList[i];
            }
        }
        ChangeAllPenColour();
    }

    private void ChangeAllPenColour()
    {
        simulationHandler.ChangePenColour(penColour);
        penColourLabel.PenColour = penColour;
    }
    #endregion

    protected override void Update(GameTime gameTime)
    {
        // Calling the Update methods of my handlers
        simulationHandler.Update(gameTime);

        for (var i = 0; i < colourButtonList.Count; i++)
        {
            colourButtonList[i].Update(gameTime);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(backgroundColour);

        _graphics.GraphicsDevice.SetRenderTarget(window);
        _spriteBatch.Begin(SpriteSortMode.Deferred,
        BlendState.AlphaBlend,
        SamplerState.PointClamp,
        null, null, null, null);

        // Draw everything onto the render target
        for (var i = 0; i < colourButtonList.Count; i++)
        {
            colourButtonList[i].Draw(_spriteBatch);
        }
        penColourLabel.Draw(_spriteBatch);
        colourLabel.Draw(_spriteBatch);
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

    protected override void Dispose(bool disposing)
    {
        // Render targets need to be disposed of manually
        window.Dispose();
        base.Dispose(disposing);
    }
    #endregion
}
