using System.Dynamic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects.Particles;

// NEED TO ADD ACCOMMODATION FOR PROPERTIES OF SIMULATION TO BE CHANGED

// Higher-level class for a particle that deals with the movement and position on-screen of a shape
public class Particle : GameObject
{
    #region Fields
    protected Vector2 prevPos, prevVelocity; // Prev values are only useful in case a backtracking algorithm is used for advanced collision handling
    #endregion

    #region Properties
    public bool colliding;
    public bool paused;
    public float Mass { get; protected set; }
    public Vector2 PreviousVelocity { get { return prevVelocity; } }
    public Vector2 PreviousPosition { get { return prevPos; } }
    public Vector2 CurrentVelocity { get; protected set; }
    public string Type { get; set; } // Type of ball ("Small" and "Large")
    public int Identifier { get; set; } // Unique identifier for the ball type that describes its position in the array
    #endregion

    #region Methods
    public Particle(Texture2D texture, Vector2 position, Vector2 velocity, float Mass, Color colour, Point dimensions) : base(texture, position, colour, dimensions)
    {
        colliding = false;
        paused = false;
        CurrentVelocity = velocity;
        this.Mass = Mass;
    }

    public override void Update(GameTime gameTime)
    {
        if (Enabled && !paused)
        {
            prevPos = position;
            // If there is no collision, the position is the only thing that changes
            // When there is no collision, the velocity of the particle is effectively zero: as such, it does not move on the GUI
            // Collision handling is outside the scope of a single instance of this object: therefore, only the movement of the object can be handled in the Update() function
            if (!colliding)
            {
                position.X += (float)(CurrentVelocity.X * gameTime.ElapsedGameTime.TotalSeconds);
                position.Y += (float)(CurrentVelocity.Y * gameTime.ElapsedGameTime.TotalSeconds);
            }
        }
    }

    public void ChangeVelocityTo(Vector2 newVelocity)
    {
        CurrentVelocity = newVelocity;
    }
    #endregion
}
