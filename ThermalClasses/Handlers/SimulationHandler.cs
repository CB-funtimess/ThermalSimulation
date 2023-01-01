using System.Net.Mime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects;
using ThermalClasses.GameObjects.Particles;
using ThermalClasses.CollisionHandling;
using ThermalClasses.PhysicsLaws;

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
    private Button testButton;
    #endregion
    private int volume, maxVolume; // Measured in metres cubed
    private float temperature, pressure, rmsVelocity; // Measured in Kelvin, Pascals, metres per second

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
        rmsVelocity = 100;
    }

    #region Particle Initialisation
    private void InitialiseParticles()
    {
        for (var i = 0; i < smallParticles.Length; i++)
        {
            smallParticles[i] = NewSmallCircle();
            largeParticles[i] = NewLargeCircle();
        }
    }

    private Polygon NewSmallCircle()
    {
        return new Polygon(content.Load<Texture2D>("SimulationAssets/YellowParticle"), new Vector2(0, 0), new Vector2(0, 0), 100, 50, Color.White, new Point(10, 10))
        {
            Enabled = false,
        };
    }

    private Polygon NewLargeCircle()
    {
        return new(content.Load<Texture2D>("SimulationAssets/BlueParticle"), new Vector2(0, 0), new Vector2(0, 0), 200, 50, Color.White, new Point(20, 20))
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

        GameObject fixedBox = new GameObject(content.Load<Texture2D>("SimulationAssets/FixedBox"), Color.White, fixedRect);
        GameObject movingBox = new GameObject(content.Load<Texture2D>("SimulationAssets/MovingBox"), Color.White, movingRect);

        simulationBox = new SimulationBox(game, fixedBox, movingBox, (int)(renderRectangle.Width * 0.6), (int)(renderRectangle.Width * 0.05));
    }
    #endregion

    public override void Initialize()
    {
        smallParticles = new Polygon[500];
        largeParticles = new Polygon[500];
        activeSmallParticles = new List<Polygon>();
        activeLargeParticles = new List<Polygon>();
    }

    public override void LoadContent()
    {
        // Particle Initialisation
        InitialiseParticles();

        // GameObject Initialisation
        InitSimBox();
        testButton = new Button(content.Load<Texture2D>("GeneralAssets/Button"), content.Load<SpriteFont>("GeneralAssets/Arial"), new Vector2(0,0), Color.White, Color.Black, new Point(200, 50))
        {
            Text = "Add small particles",
            HoverColour = Color.Gray,
        };
        testButton.Click += TestButton_Click;

        AddSmallParticles(2);
    }

    private void TestButton_Click(object sender, EventArgs e)
    {
        AddSmallParticles(1);
    }

    // Calls the update method of all objects that need updating (buttons, particles, sliders etc.)
    public override void Update(GameTime gameTime)
    {
        UpdateParticles(gameTime);
        testButton.Update(gameTime);
    }

    // Calls the draw methods of all GameObjects
    public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        Console.WriteLine($"Particle 1:\nPosition: {activeSmallParticles[0].Position}");
        Console.WriteLine($"Particle 2:\nPosition: {activeSmallParticles[1].Position}");
        simulationBox.Draw(_spriteBatch);
        testButton.Draw(_spriteBatch);
        // Drawing all active particles to screen
        for (var i = 0; i < activeSmallParticles.Count; i++)
        {
            activeSmallParticles[i].ScaleDraw(_spriteBatch);
        }
        for (var x = 0; x < activeLargeParticles.Count; x++)
        {
            activeLargeParticles[x].ScaleDraw(_spriteBatch);
        }
    }

    private void UpdateParticles(GameTime gameTime)
    {
        // Broad phase: generate a spatial hash grid containing all particles
        List<Polygon> allParticles = new List<Polygon>();
        allParticles.AddRange(activeSmallParticles);
        allParticles.AddRange(activeLargeParticles);

        // Reset all particles so that colliding = false
        foreach (var particle in allParticles)
        {
            particle.colliding = false;
        }

        spatialHashGrid = new SHG(renderRectangle.Height, simulationBox.BoxRect.Width, 15);
        spatialHashGrid.Insert(allParticles);

        // Narrow phase: confirm whether pairs of particles are actually colliding
        foreach (var polygonList in spatialHashGrid.ReturnParticleCollisions())
        {
            // Check whether each polygon in bucket is colliding with every other polygon
            for (var i = 0; i < polygonList.Count - 1; i++)
            {
                for (var j = i+1; j < polygonList.Count; j++)
                {
                    if (CollisionFunctions.SeparatingAxisTheorem(polygonList[i], polygonList[j]))
                    {
                        // Collision handling: adjust the particles' velocities
                        polygonList[i].CollisionParticleUpdate(polygonList[j]);
                        polygonList[i].colliding = true;
                        polygonList[j].CollisionParticleUpdate(polygonList[i]);
                        polygonList[j].colliding = true;
                    }
                }
            }
        }

        // Broad phase pt2: find all particles potentially colliding with the border
        foreach (var polygonList in spatialHashGrid.ReturnBoundaryCollisions())
        {
            // Check whether each polygon in bucket is colliding with the wall
            foreach (var particle in polygonList)
            {
                if (CollisionFunctions.IsBoundaryXCollision(particle, renderRectangle))
                {
                    particle.CollisionBoundaryUpdate(true);
                }
                else if (CollisionFunctions.IsBoundaryYCollision(particle, renderRectangle))
                {
                    particle.CollisionBoundaryUpdate(false);
                }
            }
        }

        for (var i = 0; i < allParticles.Count; i++)
        {
            allParticles[i].Update(gameTime);
        }
    }

    #region Adding Particles
    private void AddSmallParticles(int amount)
    {
        AddParticles(amount, ref activeSmallParticles, ref smallParticles);
        rmsVelocity = PhysicsEquations.CalcVelocityRMS(pressure, volume, activeSmallParticles.Count + activeLargeParticles.Count);
    }

    private void AddLargeParticles(int amount)
    {
        AddParticles(amount, ref activeLargeParticles, ref largeParticles);
        rmsVelocity = PhysicsEquations.CalcVelocityRMS(pressure, volume, activeSmallParticles.Count + activeLargeParticles.Count);
    }

    // Method to add particles to the list of active particles (called by event)
    private void AddParticles(int amount, ref List<Polygon> activeParticles, ref Polygon[] allParticles)
    {
        Vector2 insertPosition = new Vector2(simulationBox.BoxRect.Right - 10, simulationBox.BoxRect.Top - 10);
        // Creating the input velocities
        float theta = (90 / (float)amount) - 1;
        // Enabling particles to allow them to be drawn and updated
        int indexToEnable = activeParticles.Count;
        for (var i = indexToEnable; i < indexToEnable + amount; i++)
        {
            Vector2 insertionVelocity = new Vector2((float)Math.Sin(theta) * rmsVelocity, (float)Math.Cos(theta) * rmsVelocity);
            allParticles[i].Enabled = true;
            allParticles[i].Position = insertPosition;
            allParticles[i].ChangeVelocityTo(insertionVelocity);
            activeParticles.Add(allParticles[i]);
            insertPosition.Y += 10; // Inserts next particle into a space below previous particle
            theta += 90 / (float)amount; // Modifies angle at which the magnitude of the velocity acts in
        }
    }
    #endregion

    #region Removing Particles
    private void RemoveSmallParticles(int amount)
    {
        RemoveParticles(amount, ref activeSmallParticles, ref smallParticles);
        rmsVelocity = PhysicsEquations.CalcVelocityRMS(pressure, volume, activeSmallParticles.Count + activeLargeParticles.Count);
    }

    private void RemoveLargeParticles(int amount)
    {
        RemoveParticles(amount, ref activeLargeParticles, ref largeParticles);
        rmsVelocity = PhysicsEquations.CalcVelocityRMS(pressure, volume, activeSmallParticles.Count + activeLargeParticles.Count);
    }

    // Method to remove particles from the list of active particles (called by event)
    private void RemoveParticles(int amount, ref List<Polygon> activeList, ref Polygon[] allParticles)
    {
        int indexToDisable = activeList.Count - 1;
        for (var i = indexToDisable; i > indexToDisable - amount ; i--)
        {
            activeList.RemoveAt(activeList.Count - 1);
            allParticles[i].Enabled = false;
        }
    }
    #endregion
    #endregion
}