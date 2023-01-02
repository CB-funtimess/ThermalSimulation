using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects.Particles;

namespace ThermalClasses.CollisionHandling
{
    public static class CollisionFunctions
    {
        // The separating axis theorem function
        /// <summary>
        /// Finds whether two polygons are colliding; returns true if they collide
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool SeparatingAxisTheorem(Polygon p1, Polygon p2)
        {
            Polygon a1 = p1;
            Polygon a2 = p2;

            // For each shape
            for (int shape = 0; shape < 2; shape++)
            {
                // Swap shapes for correct comparison
                if (shape == 1)
                {
                    a1 = p2;
                    a2 = p1;
                }

                // For each side
                for (int side = 0; side < a1.Sides; side++)
                {
                    // Find normal projection to side
                    int b = (side + 1) % a1.Sides;
                    Vector2 axisProj = new(-(a1.Points[b].Y - a1.Points[side].Y), a1.Points[b].X - a1.Points[side].X);

                    // Mapping points on a1 to find the 'shadow' of the object in 1d
                    float minA1 = float.PositiveInfinity;
                    float maxA1 = float.NegativeInfinity;
                    for (int p = 0; p < a1.Sides; p++)
                    {
                        float dotProd = Vector2.Dot(a1.Points[p], axisProj);
                        minA1 = Math.Min(minA1, dotProd);
                        maxA1 = Math.Max(maxA1, dotProd);
                    }

                    // Mapping points on a2 to find the 'shadow' of the object in 1d
                    float minA2 = float.PositiveInfinity;
                    float maxA2 = float.NegativeInfinity;
                    for (int p = 0; p < a2.Sides; p++)
                    {
                        float dotProd = Vector2.Dot(a2.Points[p], axisProj);
                        minA2 = Math.Min(minA2, dotProd);
                        maxA2 = Math.Max(maxA2, dotProd);
                    }

                    // Comparing 1D shadows to find whether they intersect
                    if (!(minA1 <= minA2 && maxA1 >= minA2))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // Method for handling the velocities of colliding objects
        /// <summary>
        /// Returns the updated velocity for particle 1 after a collision with another particle; only for use in the particle class
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Vector2 NewCollisionVelocities(Particle p1, Particle p2)
        {
            Vector2 updatedVelocity = new();
            float massConst = 2 * p2.Mass / (p1.Mass + p2.Mass);
            Vector2 postionDiff = Vector2.Subtract(p1.Position, p2.Position);
            float velocityDiff = Vector2.Dot(Vector2.Subtract(p1.CurrentVelocity, p2.CurrentVelocity), Vector2.Subtract(p1.Position, p2.Position)) / Vector2.Subtract(p1.Position, p2.Position).LengthSquared();
            updatedVelocity.X = p1.CurrentVelocity.X - (massConst * velocityDiff * postionDiff.X);
            updatedVelocity.Y = p1.CurrentVelocity.Y - (massConst * velocityDiff * postionDiff.Y);

            return updatedVelocity;
        }

        /// <summary>
        /// Returns a polygon that may be colliding with the wall with a handled velocity
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="border"></param>
        /// <returns></returns>
        public static Polygon BoundaryCollisionHandling(Polygon polygon, Rectangle border)
        {
            if (polygon.Position.Y < border.Top || polygon.Position.Y > border.Bottom)
            {
                Console.WriteLine($"Y Collision");
                polygon.CollisionBoundaryUpdate(false);
            }
            else if (polygon.Position.X < border.Left || polygon.Position.X > border.Right)
            {
                Console.WriteLine($"X Collision");
                polygon.CollisionBoundaryUpdate(true);
            }
            return polygon;
        }
    }
}