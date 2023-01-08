using System.Security.Cryptography.X509Certificates;
using System.Net.Mime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects;
using ThermalClasses.GameObjects.Particles;
using ThermalClasses.CollisionHandling;
using ThermalClasses.PhysicsLaws;
using ThermalClasses.GameObjects.ObjectCollections;

namespace ThermalClasses.Handlers;

public class SimulationHandler : Handler
{
    #region Fields
    #region Objects
    #region Particles
    private Polygon[] smallParticles, largeParticles; // Total particles present in simulation
    private List<Polygon> activeSmallParticles, activeLargeParticles; // All enabled particles
    private SHG spatialHashGrid;
    #endregion
    private SimulationBox simulationBox;
    #region Buttons
    private List<GameObject> buttonCollection;
    private List<UpDownButton> upDownCollection;
    private CheckButton pauseButton;
    private UpDownButton volumeControl;
    private UpDownButton smallParticleControl;
    private UpDownButton largeParticleControl;
    #endregion
    #endregion
    private int volume, maxVolume; // Measured in metres cubed
    private float temperature, pressure, rmsVelocity; // Measured in Kelvin, Pascals, metres per second
    private bool paused;
    private Color penColour = Color.White;
    #endregion

    #region Properties
    public Color BackgroundColour { get; set; }
    public Color HoverColour { get; set; }
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
        rmsVelocity = 200;
        paused = false;
    }

    #region Initialisation

    #region Particle Initialisation
    private void InitialiseParticles()
    {
        for (var i = 0; i < smallParticles.Length; i++)
        {
            smallParticles[i] = NewSmallCircle(i);
            largeParticles[i] = NewLargeCircle(i);
        }
    }

    private Polygon NewSmallCircle(int identifier)
    {
        return new Polygon(content.Load<Texture2D>("SimulationAssets/YellowParticle"), new Vector2(0, 0), new Vector2(0, 0), 5, 10, Color.White, new Point(10, 10))
        {
            Enabled = false,
            Type = "Small",
            Identifier = identifier,
        };
    }

    private Polygon NewLargeCircle(int identifier)
    {
        return new(content.Load<Texture2D>("SimulationAssets/BlueParticle"), new Vector2(0, 0), new Vector2(0, 0), 6, 10, Color.White, new Point(12, 12))
        {
            Enabled = false,
            Type = "Large",
            Identifier = identifier,
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

        GameObject fixedBox = new GameObject(content.Load<Texture2D>("SimulationAssets/FixedBox"), Color.White, fixedRect);
        GameObject movingBox = new GameObject(content.Load<Texture2D>("SimulationAssets/MovingBox"), Color.White, movingRect);

        simulationBox = new SimulationBox(game, fixedBox, movingBox, (int)(renderRectangle.Width * 0.4), 0);
    }
    #endregion

    public override void Initialize()
    {
        const int listSize = 500;
        smallParticles = new Polygon[listSize];
        largeParticles = new Polygon[listSize];
        activeSmallParticles = new List<Polygon>();
        activeLargeParticles = new List<Polygon>();
        buttonCollection = new List<GameObject>();
        upDownCollection = new List<UpDownButton>();
    }

    public override void LoadContent()
    {
        SpriteFont font = content.Load<SpriteFont>("GeneralAssets/Arial");
        Color unclickedColour = Color.White;
        // Particle Initialisation
        InitialiseParticles();

        // GameObject Initialisation
        InitSimBox();
        // Buttons Initialisation
        Vector2 pausePosition = new Vector2(0, simulationBox.BoxRect.Y - 55);
        pauseButton = new CheckButton(content.Load<Texture2D>("GeneralAssets/PauseButton"), content.Load<Texture2D>("GeneralAssets/PlayButton"), font, pausePosition, unclickedColour, penColour, new Point(40, 40))
        {
            HoverColour = HoverColour,
        };
        pauseButton.Click += PauseSimulation_Click;

        Texture2D upTexture = content.Load<Texture2D>("GeneralAssets/UpButton");
        Texture2D downTexture = content.Load<Texture2D>("GeneralAssets/DownButton");
        Texture2D labelTexture = content.Load<Texture2D>("GeneralAssets/LabelBox1");

        Rectangle volumeButtonSize = new Rectangle(Point.Zero, new Point(150, 40));
        volumeControl = new UpDownButton(upTexture, downTexture, labelTexture, volumeButtonSize, "Volume", font, penColour, unclickedColour, HoverColour);
        volumeControl.DownButton.Click += DecreaseVolume_Click;
        volumeControl.UpButton.Click += IncreaseVolume_Click;

        Point particleControlSize = new Point(200, 40);
        Rectangle largeParticleButtonSize = new Rectangle(new Point(simulationBox.BoxRect.Right - particleControlSize.X, simulationBox.BoxRect.Bottom + 15), particleControlSize);
        Rectangle smallParticleButtonSize = new Rectangle(new Point(largeParticleButtonSize.X, largeParticleButtonSize.Y + particleControlSize.Y + 10), particleControlSize);
        smallParticleControl = new UpDownButton(upTexture, downTexture, labelTexture, smallParticleButtonSize, "Small Particles", font, penColour, unclickedColour, HoverColour);
        smallParticleControl.DownButton.Click += RemoveSmallParticles_Click;
        smallParticleControl.UpButton.Click += AddSmallParticles_Click;

        largeParticleControl = new UpDownButton(upTexture, downTexture, labelTexture, largeParticleButtonSize, "Large Particles", font, penColour, unclickedColour, HoverColour);
        largeParticleControl.DownButton.Click += RemoveLargeParticles_Click;
        largeParticleControl.UpButton.Click += AddLargeParticles_Click;

        // Putting all objects into a list for easier updating and drawing
        buttonCollection.Add(pauseButton);

        upDownCollection.Add(volumeControl);
        upDownCollection.Add(smallParticleControl);
        upDownCollection.Add(largeParticleControl);
    }
    #endregion

    #region Updating & Drawing
    // Calls the update method of all objects that need updating (buttons, particles, sliders etc.)
    public override void Update(GameTime gameTime)
    {
        pauseButton.Update(gameTime);

        for (var i = 0; i < buttonCollection.Count; i++)
        {
            buttonCollection[i].Update(gameTime);
        }
        for (var i = 0; i < upDownCollection.Count; i++)
        {
            upDownCollection[i].Update(gameTime);
        }
        if (!paused)
        {
            UpdateParticles(gameTime);
        }
    }

    // Calls the draw methods of all GameObjects
    public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        simulationBox.Draw(_spriteBatch);
        for (var i = 0; i < buttonCollection.Count; i++)
        {
            buttonCollection[i].Draw(_spriteBatch);
        }
        for (var i = 0; i < upDownCollection.Count; i++)
        {
            upDownCollection[i].Draw(_spriteBatch);
        }
        // Drawing all active particles to screen
        for (var i = 0; i < activeSmallParticles.Count; i++)
        {
            activeSmallParticles[i].Draw(_spriteBatch);
        }
        for (var x = 0; x < activeLargeParticles.Count; x++)
        {
            activeLargeParticles[x].Draw(_spriteBatch);
        }
    }

    private void UpdateParticles(GameTime gameTime)
    {
        for (var i = 0; i < activeSmallParticles.Count; i++)
        {
            activeSmallParticles[i].Update(gameTime);
            activeSmallParticles[i].colliding = false;
        }
        for (var i = 0; i < activeLargeParticles.Count; i++)
        {
            activeLargeParticles[i].Update(gameTime);
            activeLargeParticles[i].colliding = false;
        }

        // Broad phase: generate a spatial hash grid containing all particles
        List<Polygon> allParticles = new();
        allParticles.AddRange(activeSmallParticles);
        allParticles.AddRange(activeLargeParticles);

        spatialHashGrid = new SHG(simulationBox.BoxRect, 15);
        spatialHashGrid.Insert(allParticles);

        // Handling particle-particle collisions
        // Narrow phase: confirm whether pairs of particles are actually colliding
        foreach (var polygonList1 in spatialHashGrid.ReturnParticleCollisions())
        {
            // Check whether each polygon in bucket is colliding with every other polygon
            for (var i = 0; i < polygonList1.Count - 1; i++)
            {
                for (var j = i + 1; j < polygonList1.Count; j++)
                {
                    if (polygonList1[i].Type == "Small")
                    {
                        if (polygonList1[j].Type == "Small")
                        {
                            CollisionParticleUpdates(ref activeSmallParticles, polygonList1[i].Identifier, ref activeSmallParticles, polygonList1[j].Identifier, gameTime);
                        }
                        else
                        {
                            CollisionParticleUpdates(ref activeSmallParticles, polygonList1[i].Identifier, ref activeLargeParticles, polygonList1[j].Identifier, gameTime);
                        }
                    }
                    else
                    {
                        if (polygonList1[j].Type == "Small")
                        {
                            CollisionParticleUpdates(ref activeLargeParticles, polygonList1[i].Identifier, ref activeSmallParticles, polygonList1[j].Identifier, gameTime);
                        }
                        else
                        {
                            CollisionParticleUpdates(ref activeLargeParticles, polygonList1[i].Identifier, ref activeLargeParticles, polygonList1[j].Identifier, gameTime);
                        }
                    }
                }
            }
        }

        // Handling particle-border collisions
        // Finding all particles potentially colliding with the border and updating them
        var polygonList = spatialHashGrid.ReturnBoundaryCollisions();
        for (var i = 0; i < polygonList.Count; i++)
        {
            foreach (var particle in polygonList[i])
            {
                Polygon myParticle = CollisionFunctions.BoundaryCollisionHandling(particle, simulationBox.BoxRect, gameTime);

                // Move particles back into original lists
                if (particle.Type == "Small")
                {
                    activeSmallParticles[myParticle.Identifier] = myParticle;
                }
                else
                {
                    activeLargeParticles[myParticle.Identifier] = myParticle;
                }
            }
        }
    }

    private void CollisionParticleUpdates(ref List<Polygon> a1, int i1, ref List<Polygon> a2, int i2, GameTime gameTime)
    {
        if (CollisionFunctions.SeparatingAxisTheorem(a1[i1], a2[i2]))
        {
            a1[i1].Position = CollisionFunctions.TouchingPosition(a1[i1], a2[i2], gameTime);
            a1[i1].CollisionParticleUpdate(a2[i2], gameTime);

            a2[i2].CollisionParticleUpdate(a1[i1], gameTime);
        }
    }
    #endregion

    #region Events
    #region Adding Particles
    private void AddSmallParticles_Click(object sender, EventArgs e)
    {
        if (!paused)
        {
            AddParticles(10, ref activeSmallParticles, ref smallParticles);
        }
    }

    private void AddLargeParticles_Click(object sender, EventArgs e)
    {
        if (!paused)
        {
            AddParticles(10, ref activeLargeParticles, ref largeParticles);
        }
    }

    // Method to add particles to the list of active particles (called by event)
    private void AddParticles(int amount, ref List<Polygon> activeParticles, ref Polygon[] allParticles)
    {
        if (activeParticles.Count + amount <= allParticles.Length)
        {
            Vector2 insertPosition = new Vector2(simulationBox.BoxRect.Right - 10, simulationBox.BoxRect.Top + 10);
            // Creating the input velocities
            float theta = (float)((Math.PI / (2 * amount)) + 0.1);
            // Enabling particles to allow them to be drawn and updated
            int indexToEnable = activeParticles.Count;
            for (var i = indexToEnable; i < indexToEnable + amount; i++)
            {
                Vector2 insertionVelocity = new Vector2(-1 * (float)Math.Sin(theta) * rmsVelocity, (float)Math.Sin(theta) * rmsVelocity);
                allParticles[i].Enabled = true;
                allParticles[i].Position = insertPosition;
                allParticles[i].ChangeVelocityTo(insertionVelocity);
                activeParticles.Add(allParticles[i]);
                insertPosition.Y += 20; // Inserts next particle into a space below previous particle
                theta += (float)Math.PI / 2 / amount; // Modifies angle at which the magnitude of the velocity acts in
            }
        }
    }

    #endregion

    #region Removing Particles
    private void RemoveSmallParticles_Click(object sender, EventArgs e)
    {
        RemoveParticles(10, ref activeSmallParticles, ref smallParticles);
    }

    private void RemoveLargeParticles_Click(object sender, EventArgs e)
    {
        RemoveParticles(10, ref activeLargeParticles, ref largeParticles);
    }

    // Method to remove particles from the list of active particles (called by event)
    private void RemoveParticles(int amount, ref List<Polygon> activeList, ref Polygon[] allParticles)
    {
        if (activeList.Count >= amount)
        {
            int indexToDisable = activeList.Count - 1;
            for (var i = indexToDisable; i > indexToDisable - amount; i--)
            {
                activeList.RemoveAt(activeList.Count - 1);
                allParticles[i].Enabled = false;
            }
        }
    }
    #endregion

    private void PauseSimulation_Click(object sender, EventArgs e)
    {
        paused = !paused;
    }

    private void IncreaseVolume_Click(object sender, EventArgs e)
    {
        simulationBox.ChangeVolume(20);
    }

    private void DecreaseVolume_Click(object sender, EventArgs e)
    {
        simulationBox.ChangeVolume(-20);
    }
    #endregion
    #endregion
}