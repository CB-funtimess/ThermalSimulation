using System.Net.Mime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects;
using ThermalClasses.GameObjects.Particles;
using ThermalClasses.CollisionHandling;

namespace ThermalClasses.Handlers;

public class SimulationHandler : Handler
{
    #region Fields
    #region Particles
    private Polygon[] smallParticles, largeParticles; // Total particles present in simulation
    private List<Polygon> activeSmallParticles, activeLargeParticles; // All enabled particles
    private SHG spatialHashGrid;
    #endregion
    private SimulationBox simulationBox;
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
        return new Polygon(content.Load<Texture2D>("YellowParticle"), new Vector2(0, 0), new Vector2(0, 0), 100, 50, new Color(10, 10, 10), new Point(10, 10))
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
    }

    // Calls the update method of all objects that need updating (buttons, particles, sliders etc.)
    public override void Update(GameTime gameTime)
    {
        UpdateParticles(gameTime);
    }

    // Calls the draw methods of all GameObjects
    public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        // Drawing the simulation box to screen
        simulationBox.Draw(_spriteBatch);
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
        // Broad phase: generate a spatial hash grid containing all particles
        List<Polygon> allParticles = new List<Polygon>();
        allParticles.AddRange(activeSmallParticles);
        allParticles.AddRange(activeLargeParticles);

        // Reset all particles so that colliding = false
        foreach (var particle in allParticles)
        {
            particle.colliding = false;
        }

        spatialHashGrid = new SHG(renderRectangle.Height, renderRectangle.Width, 15);
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
        AddParticles(amount, activeSmallParticles, smallParticles);
    }

    private void AddLargeParticles(int amount)
    {
        AddParticles(amount, activeLargeParticles, largeParticles);
    }

    // Method to add particles to the list of active particles (called by event)
    private void AddParticles(int amount, List<Polygon> activeParticles, Polygon[] allParticles)
    {
        Vector2 insertPosition = new Vector2(renderRectangle.Left + 5, renderRectangle.Top + 5);
        // Creating the input velocities
        float theta = 90 / (float)amount;
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
        RemoveParticles(amount, activeSmallParticles, smallParticles);
    }

    private void RemoveLargeParticles(int amount)
    {
        RemoveParticles(amount, activeLargeParticles, largeParticles);
    }

    // Method to remove particles from the list of active particles (called by event)
    private void RemoveParticles(int amount, List<Polygon> activeList, Polygon[] allParticles)
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