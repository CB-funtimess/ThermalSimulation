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
                    if (maxA1 < minA2 || minA1 > maxA2)
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
        public static Vector2 NewCollisionVelocity(Particle p1, Particle p2)
        {
            return NewCollisionVelocity(p1.CurrentVelocity, p2.CurrentVelocity, p1.Mass, p2.Mass, p1.Position, p2.Position);
        }

        public static Vector2 NewCollisionVelocity(Vector2 v1, Vector2 v2, double m1, double m2, Vector2 x1, Vector2 x2)
        {
            return v1 - (Vector2.Dot(v1 - v2, x1 - x2) / (float)(x1 - x2).LengthSquared() * (x1 - x2) * (float)(2 * m2 / (m1 + m2)));
        }

        /// <summary>
        /// Linear search algorithm to find the closest approximate position at which p1 touches p2
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="positionRatio"></param>
        /// <returns></returns>
        public static Vector2 TouchingPosition(Polygon p1, Polygon p2, GameTime gameTime)
        {
            Polygon a1 = p1;
            Polygon a2 = p2;
            if (p1.Type == "Large" && p2.Type == "Small")
            {
                a1 = p2;
                a2 = p1;
            }
            float radiiDistance = a1.YRadius + a2.YRadius;
            int length = (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            float iterateBy = (float)(gameTime.ElapsedGameTime.TotalSeconds / Math.Min(a1.YRadius - 2, a2.YRadius - 2)); // Making iterateBy smaller will increase precision but decrease performance (10 seems to be a number that limits performance only slightly)
            for (float i = 0; i < length; i += iterateBy)
            {
                Vector2 tempPosition = a1.PreviousPosition + (a1.CurrentVelocity * i);
                float distanceBetweenCentres = (tempPosition - a2.Position).Length();
                if (Math.Round(distanceBetweenCentres) >= Math.Round(radiiDistance))
                {
                    return tempPosition;
                }
            }
            return a1.Position;
        }

        /// <summary>
        /// Returns a polygon that may be colliding with the wall with a handled velocity
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="border"></param>
        /// <returns></returns>
        public static Polygon BoundaryCollisionHandling(Polygon polygon, Rectangle border, GameTime gameTime)
        {
            bool borderCollision = false;
            BorderCollisions borderCollisions = new BorderCollisions();
            if (polygon.Position.Y < border.Top)
            {
                borderCollisions.top = true;
                borderCollision = true;
            }
            else if (polygon.Position.Y > border.Bottom)
            {
                borderCollisions.bottom = true;
                borderCollision = true;
            }
            if (polygon.Position.X < border.Left)
            {
                borderCollisions.left = true;
                borderCollision = true;
            }
            else if (polygon.Position.X > border.Right)
            {
                borderCollisions.right = true;
                borderCollision = true;
            }

            if (borderCollision)
            {
                polygon.CollisionBoundaryUpdate(borderCollisions, gameTime);
            }
            return polygon;
        }

        public struct BorderCollisions
        {
            public bool left;
            public bool right;
            public bool top;
            public bool bottom;
            public BorderCollisions(bool left, bool right, bool top, bool bottom)
            {
                this.left = left;
                this.right = right;
                this.top = top;
                this.bottom = bottom;
            }

            public BorderCollisions()
            {
                left = right = top = bottom = false;
            }
        }
    }
}