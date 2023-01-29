using System.Dynamic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.CollisionHandling;
using ThermalClasses.PhysicsLaws;
using ThermalClasses.Handlers;

namespace ThermalClasses.GameObjects.Particles;

// NEED TO ADD ACCOMMODATION FOR PROPERTIES OF SIMULATION TO BE CHANGED

// Higher-level class for a particle that deals with the movement and position on-screen of a shape
public class Particle : GameObject
{
    #region Fields
    protected Vector2 prevPos, currentVelocity; // Prev values are only useful in case a backtracking algorithm is used for advanced collision handling
    protected double mass;
    #endregion

    #region Properties
    public double Mass => mass;
    public Vector2 PreviousPosition => prevPos;
    public Vector2 CurrentVelocity => currentVelocity;
    public ParticleType Type { get; set; } // Type of ball ("Small" and "Large")
    public int Identifier { get; set; } // Unique identifier for the ball type that describes its position in the array
    #endregion

    #region Methods
    public Particle(Texture2D texture, Vector2 position, Vector2 velocity, double Mass, Color colour, Point dimensions) : base(texture, position, colour, dimensions)
    {
        currentVelocity = velocity;
        mass = Mass;
    }

    public override void Update(GameTime gameTime)
    {
        if (Enabled)
        {
            prevPos = position;
            // If there is no collision, the position is the only thing that changes
            // When there is no collision, the velocity of the particle is effectively zero: as such, it does not move on the GUI
            // Collision handling is outside the scope of a single instance of this object: therefore, only the movement of the object can be handled in the Update() function
            position.X += (float)(CurrentVelocity.X * gameTime.ElapsedGameTime.TotalSeconds);
            position.Y += (float)(CurrentVelocity.Y * gameTime.ElapsedGameTime.TotalSeconds);
        }
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        currentVelocity = newVelocity;
    }

    // This function calculates the new velocity of the particle after a collision with another particle
    public void CollisionParticleUpdate(Particle collidingMass)
    {
        currentVelocity = CollisionFunctions.NewCollisionVelocity(this, collidingMass);
    }

    // This function calculates the new velocity of the particle after a collision with a boundary
    public void CollisionBoundaryUpdate(BorderCollisions borderCollisions)
    {
        if ((borderCollisions.left && CurrentVelocity.X < 0) || (borderCollisions.right && CurrentVelocity.X > 0)) // If colliding with the left or right walls
        {
            currentVelocity = new Vector2(CurrentVelocity.X * -1, CurrentVelocity.Y);
        }
        if ((borderCollisions.top && CurrentVelocity.Y < 0) || (borderCollisions.bottom && CurrentVelocity.Y > 0))
        {
            currentVelocity = new Vector2(CurrentVelocity.X, CurrentVelocity.Y * -1);
        }
    }
    #endregion
}
