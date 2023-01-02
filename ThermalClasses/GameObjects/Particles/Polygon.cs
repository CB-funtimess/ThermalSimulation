using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects.Particles;

// Subclass to deal with the mathematical properties of the particle
public class Polygon : Particle
{
    #region Fields
    protected int sides;
    protected Vector2[] unitPoints; // Vectors of vertices if the centre was at (0,0)
    #endregion

    #region Properties
    public int Sides { get { return sides; } }
    public Vector2[] Points { get { return GeneratePoints(); } } // Vectors of the vertices; Index 0 represents the topmost middle point
    #endregion

    #region Methods
    public Polygon(Texture2D texture, Vector2 centrePosition, Vector2 velocity, float mass, int noSides, Color colour, Point dimensions) : base(texture, centrePosition, velocity, mass, colour, dimensions)
    {
        sides = noSides;
        unitPoints = new Vector2[sides];
        xRadius = dimensions.X / 2;
        yRadius = dimensions.Y / 2;
        position = centrePosition;
        InitialisePoints();
        InitBoundingBox();
    }

    // Method to initialise all points
    private void InitialisePoints()
    {
        for (var i = 0; i < sides; i++)
        {
            unitPoints[i] = new();
            Points[i] = new();
        }

        float theta = (float)(2 * Math.PI / sides);
        unitPoints[0] = new Vector2(0, -yRadius);

        // 2-D Matrix transformation to generate points
        // Clockwise matrix rotation
        for (int i = 1; i < sides; i++)
        {
            float x = (float)((unitPoints[i - 1].X * Math.Cos(theta)) - (unitPoints[i - 1].Y * Math.Sin(theta)));
            float y = (float)((unitPoints[i - 1].X * Math.Sin(theta)) + (unitPoints[i - 1].Y * Math.Cos(theta)));
            unitPoints[i] = new Vector2(x, y);
        }
    }

    private void InitBoundingBox()
    {
        Point bottomLeft = new Point(int.MaxValue);
        Point topRight = new Point(int.MinValue);
        foreach (var point in Points)
        {
            // Find the smallest x/y value to get Left/Bottom, largest for Right/Top
            topRight = new Point((int)Math.Max(topRight.X, point.X), (int)Math.Max(topRight.Y, point.Y));
            bottomLeft = new Point((int)Math.Min(bottomLeft.X, point.X), (int)Math.Min(bottomLeft.Y, point.Y));
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (Enabled && !paused)
        {
            base.Update(gameTime);
        }
    }

    private Vector2[] GeneratePoints()
    {
        Vector2[] points = new Vector2[sides];
        for (var i = 0; i < points.Length; i++)
        {
            points[i] = Vector2.Add(unitPoints[i], position);
        }
        return points;
    }

    // This function calculates the new velocity of the particle after a collision with another particle
    public void CollisionParticleUpdate(Polygon collidingMass, GameTime gameTime)
    {
        CurrentVelocity = CollisionHandling.CollisionFunctions.NewCollisionVelocity(this, collidingMass);
        Update(gameTime);
    }

    // This function calculates the new velocity of the particle after a collision with a boundary
    public void CollisionBoundaryUpdate(bool xCollision, GameTime gameTime)
    {
        if (xCollision) // If colliding with the left or right walls
        {
            CurrentVelocity = new Vector2(CurrentVelocity.X * -1, CurrentVelocity.Y);
            Update(gameTime);
        }
        else
        {
            CurrentVelocity = new Vector2(CurrentVelocity.X, -1 * CurrentVelocity.Y);
            Update(gameTime);
        }
    }
    #endregion
}