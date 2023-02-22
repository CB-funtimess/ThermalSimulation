using System.Data;
using System.Runtime.InteropServices;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GraphClasses;
using ThermalClasses.GameObjects;
namespace ThermalClasses.Handlers;

public class GraphHandler : Handler
{
    #region Fields
    private Graph pressureTemp, pressureInvVolume, volumeTemp;
    private List<Graph> graphList;
    private Button addPointButton, resetButton;
    private GameObject surroundBox;
    private double minPressure, maxPressure, minVolume, maxVolume, minTemperature, maxTemperature;
    #endregion Fields

    #region Properties
    public double Pressure, Volume, Temperature;
    #endregion Properties

    #region Methods
    public GraphHandler(Game game, Rectangle renderRect, double minPressure, double maxPressure, double minVolume, double maxVolume, double minTemperature, double maxTemperature)
    {
        Enabled = true;

        this.game = game;
        renderRectangle = renderRect;
        content = game.Content;

        this.minPressure = minPressure;
        this.maxPressure = maxPressure;
        this.minTemperature = minTemperature;
        this.maxTemperature = maxTemperature;
        this.minVolume = minVolume;
        this.maxVolume = maxVolume;
    }

    public override void Initialize()
    {
        graphList = new List<Graph>();
    }

    public override void LoadContent()
    {
        SpriteFont font = content.Load<SpriteFont>("GeneralAssets/Arial");

        Texture2D graphFrameTexture = content.Load<Texture2D>("GraphAssets/GraphFrame");
        Texture2D crossTexture = content.Load<Texture2D>("GraphAssets/GraphCross");
        Texture2D plusButtonTexture = content.Load<Texture2D>("GraphAssets/PlusButton");
        Texture2D surroundTexture = content.Load<Texture2D>("GeneralAssets/DataBox");
        Texture2D resetTexture = content.Load<Texture2D>("GeneralAssets/ResetButton");

        // Surrounding box
        Rectangle surroundRect = new Rectangle(new Point((int)(renderRectangle.Width * 0.73), 0), new Point((int)(renderRectangle.Width * 0.27), (int)(renderRectangle.Height * 0.6)));
        surroundBox = new GameObject(surroundTexture, UnclickedColour, surroundRect);

        // Initialising graphs
        Point graphSize = new Point((int)(surroundRect.Width * 0.45));
        Rectangle pressureTempRect = new Rectangle(new Point(surroundRect.Left + 20, surroundRect.Top + 100), graphSize);
        Rectangle pressureVolRect = new Rectangle(new Point(surroundRect.Right - graphSize.X - 10, pressureTempRect.Y), graphSize);
        Rectangle volTempRect = new Rectangle(new Point(surroundRect.Center.X - (graphSize.X / 2), pressureVolRect.Bottom + 20), graphSize);

        // Pressure on the y axis, temperature on the x axis
        pressureTemp = new Graph(graphFrameTexture, crossTexture, pressureTempRect, UnclickedColour, new Vector2((float)maxTemperature, (float)maxPressure), new Vector2((float)minTemperature, (float)minPressure), "Temperature/K", "Pressure/Pa", font)
        {
            PenColour = PenColour
        };
        // Pressure on the y axis, volume on the x axis
        pressureInvVolume = new Graph(graphFrameTexture, crossTexture, pressureVolRect, UnclickedColour, new Vector2((float)(1 / minVolume), (float)maxPressure), new Vector2((float)(1 / maxVolume), (float)minPressure), "1/Volume / m^-3", "Pressure/Pa", font)
        {
            PenColour = PenColour
        };
        // Volume on the y axis, temperature on the x axis
        volumeTemp = new Graph(graphFrameTexture, crossTexture, volTempRect, UnclickedColour, new Vector2((float)maxTemperature, (float)maxVolume), new Vector2((float)minTemperature, (float)minVolume), "Temperature/K", "Volume/m^3", font)
        {
            PenColour = PenColour
        };

        // Initialising the add point button
        Point addSize = new Point(40, 40);
        Rectangle addButtonRect = new Rectangle(new Point(surroundRect.Right - addSize.X - 20, surroundRect.Bottom - addSize.Y - 20), addSize);
        addPointButton = new Button(plusButtonTexture, font, addButtonRect, UnclickedColour, PenColour)
        {
            HoverColour = HoverColour
        };
        addPointButton.Click += AddPoint_Click;

        // Initialising the reset button
        Rectangle resetRect = new Rectangle(new Point(addButtonRect.Left - 20 - addSize.X, addButtonRect.Y), addSize);
        resetButton = new Button(resetTexture, font, resetRect, UnclickedColour, PenColour)
        {
            HoverColour = HoverColour,
        };
        resetButton.Click += ResetGraphs_Click;

        graphList.Add(pressureInvVolume);
        graphList.Add(pressureTemp);
        graphList.Add(volumeTemp);
    }

    public override void Update(GameTime gameTime)
    {
        if (Enabled)
        {
            for (int i = 0; i < graphList.Count; i++)
            {
                graphList[i].Update(gameTime);
            }
            addPointButton.Update(gameTime);
            resetButton.Update(gameTime);
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            for (int i = 0; i < graphList.Count; i++)
            {
                graphList[i].Draw(_spriteBatch);
            }
            addPointButton.Draw(_spriteBatch);
            surroundBox.Draw(_spriteBatch);
            resetButton.Draw(_spriteBatch);
        }
    }

    public override void ChangePenColour()
    {
        for (int i = 0; i < graphList.Count; i++)
        {
            graphList[i].PenColour = PenColour;
        }
    }

    public void Dispose()
    {
        // Since the program creates new textures, they need manually disposing
        for (int i = 0; i < graphList.Count; i++)
        {
            graphList[i].Dispose();
        }
    }

    #region Events
    private void AddPoint_Click(object sender, EventArgs e)
    {
        pressureTemp.AddPoint(new Vector2((float)Temperature, (float)Pressure));
        pressureInvVolume.AddPoint(new Vector2((float)(1/Volume), (float)Pressure));
        volumeTemp.AddPoint(new Vector2((float)Temperature, (float)Volume));
    }

    private void ResetGraphs_Click(object sender, EventArgs e)
    {
        for (int i = 0; i < graphList.Count; i++)
        {
            graphList[i].ClearPoints();
        }
    }
    #endregion Events
    #endregion Methods
}