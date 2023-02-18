using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
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
    private QuestionHandler questionHandler;
    private GraphHandler graphHandler;
    private Button whiteButton, greenButton, orangeButton, cyanButton, yellowButton;
    private List<GameObject> objectList;
    private List<Color> colourList;
    private Label penColourLabel, colourLabel, graphToggleLabel, questionsToggleLabel;
    private CheckButton graphQuestionsToggle;
    Color unclickedColour = Color.White;
    #endregion

    #region Methods
    public ThermalSim()
    {
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        Window.Title = "Thermal Simulation";

        InitializeRenderTarget();
        objectList = new List<GameObject>();
        colourList = new List<Color>();
        // Initialising and calling the Init() methods of my handlers
        simulationHandler = new(this, renderRectangle)
        {
            BackgroundColour = backgroundColour,
            HoverColour = Color.Gray,
            PenColour = Color.White,
            UnclickedColour = Color.White
        };
        questionHandler = new QuestionHandler(this, renderRectangle)
        {
            BackgroundColour = backgroundColour,
            HoverColour = Color.Gray,
            PenColour = Color.White,
            UnclickedColour = Color.White
        };
        graphHandler = new GraphHandler(this, renderRectangle, 0, 9.9E-21, 50, 300, 0, 600)
        {
            BackgroundColour = backgroundColour,
            HoverColour = Color.Gray,
            PenColour = Color.White,
            UnclickedColour = Color.White
        };
        graphHandler.Enabled = false;
        simulationHandler.Initialize();
        questionHandler.Initialize();
        graphHandler.Initialize();

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
        if (windowWidth / (double)windowHeight < 16 / (double)9)
        {
            // Output is taller than it is wide (relative to screen ratio)
            int outputHeight = (int)((windowWidth / (16 / (double)9)) + 0.5);
            int barHeight = (windowHeight - outputHeight) / 2;
            renderRectangle = new Rectangle(0, barHeight, windowWidth, outputHeight);
        }
        else if (windowWidth / (double)windowHeight > 16 / (double)9)
        {
            // Output is wider than it is tall (relative to screen ratio)
            int outputWidth = (int)((windowHeight * (16 / (double)9)) + 0.5);
            int barWidth = (windowWidth - outputWidth) / 2;
            renderRectangle = new Rectangle(barWidth, 0, outputWidth, windowHeight);
        }
        else
        {
            renderRectangle = new Rectangle(0, 0, windowWidth, windowHeight);
        }
        window = new RenderTarget2D(GraphicsDevice, renderRectangle.Width, renderRectangle.Height, false, SurfaceFormat.Color, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);

        // Resizing the touch panel to handle input correctly
        TouchPanel.DisplayHeight = renderRectangle.Height;
        TouchPanel.DisplayWidth = renderRectangle.Width;
        TouchPanel.EnableMouseTouchPoint = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        LoadColours();
        LoadToggle();
        // Calling the LoadContent() methods of my handlers
        simulationHandler.LoadContent();
        questionHandler.LoadContent();
        graphHandler.LoadContent();
    }

    private void LoadToggle()
    {
        SpriteFont font = Content.Load<SpriteFont>("GeneralAssets/Arial");
        Texture2D switchLeft = Content.Load<Texture2D>("GeneralAssets/Switch_Left");
        Texture2D switchRight = Content.Load<Texture2D>("GeneralAssets/Switch_Right");
        Texture2D labelTexture = Content.Load<Texture2D>("GeneralAssets/LabelBox1");

        Point labelSize = new Point(101, 20);
        Point toggleSize = new Point(40, 20);
        Rectangle questionsRect = new Rectangle(new Point((int)((renderRectangle.Width * 0.73) - 10 - labelSize.X), 0), labelSize);
        Rectangle toggleRect = new Rectangle(new Point(questionsRect.Left - toggleSize.X - 5, 0), toggleSize);
        Rectangle graphsRect = new Rectangle(new Point(toggleRect.Left - labelSize.X - 5, 0), labelSize);

        graphToggleLabel = new Label(labelTexture, unclickedColour, graphsRect, font, penColour)
        {
            Text = "Graphs"
        };
        questionsToggleLabel = new Label(labelTexture, unclickedColour, questionsRect, font, penColour)
        {
            Text = "Questions"
        };
        graphQuestionsToggle = new CheckButton(switchRight, switchLeft, font, toggleRect, unclickedColour, penColour)
        {
            HoverColour = hoverColour
        };
        graphQuestionsToggle.Click += ToggleGraphs_Click;
        objectList.Add(graphQuestionsToggle);
        objectList.Add(questionsToggleLabel);
        objectList.Add(graphToggleLabel);
    }

    private void ToggleGraphs_Click(object sender, EventArgs e)
    {
        graphHandler.Enabled = !graphHandler.Enabled;
        questionHandler.Enabled = !questionHandler.Enabled;
    }

    private void LoadColours()
    {
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
        Point colourSize = new Point(20, 20);
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

        objectList.Add(whiteButton);
        colourList.Add(Color.White);
        objectList.Add(greenButton);
        colourList.Add(Color.Green);
        objectList.Add(yellowButton);
        colourList.Add(Color.Yellow);
        objectList.Add(orangeButton);
        colourList.Add(Color.Orange);
        objectList.Add(cyanButton);
        colourList.Add(Color.Cyan);
    }

    #region Changing Pen Colour
    // Matches the index of the button to the index of the colour
    private void ColourButton_Click(object sender, EventArgs e)
    {
        for (var i = 0; i < objectList.Count; i++)
        {
            if (objectList[i] == sender)
            {
                penColour = colourList[i];
            }
        }
        ChangeAllPenColour();
    }

    private void ChangeAllPenColour()
    {
        simulationHandler.PenColour = penColour;
        questionHandler.PenColour = penColour;
        graphHandler.PenColour = penColour;
        simulationHandler.ChangePenColour();
        questionHandler.ChangePenColour();
        graphHandler.ChangePenColour();
        penColourLabel.PenColour = penColour;
        graphToggleLabel.PenColour = penColour;
        questionsToggleLabel.PenColour = penColour;
    }
    #endregion

    protected override void Update(GameTime gameTime)
    {
        // Calling the Update methods of my handlers
        graphHandler.Pressure = simulationHandler.Pressure;
        graphHandler.Temperature = simulationHandler.Temperature;
        graphHandler.Volume = simulationHandler.Volume;

        simulationHandler.Update(gameTime);
        questionHandler.Update(gameTime);
        graphHandler.Update(gameTime);

        for (var i = 0; i < objectList.Count; i++)
        {
            objectList[i].Update(gameTime);
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
        for (var i = 0; i < objectList.Count; i++)
        {
            objectList[i].Draw(_spriteBatch);
        }
        penColourLabel.Draw(_spriteBatch);
        colourLabel.Draw(_spriteBatch);
        // Calling the Draw methods of my handlers
        simulationHandler.Draw(gameTime, _spriteBatch);
        questionHandler.Draw(gameTime, _spriteBatch);
        graphHandler.Draw(gameTime, _spriteBatch);

        _spriteBatch.End();
        _graphics.GraphicsDevice.SetRenderTarget(null);

        GraphicsDevice.Clear(ClearOptions.Target, backgroundColour, 1.0f, 0);

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
        graphHandler.Dispose();
        base.Dispose(disposing);
    }
    #endregion
}
