using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects.Particles;
using ThermalClasses.PhysicsLaws;

namespace ThermalClasses.CollisionHandling
{
    public static class CollisionFunctions
    {
        /// <summary>
        /// Checks whether two polygons are colliding
        /// </summary>
        /// <param name="p1">First polygon</param>
        /// <param name="p2">Second polygon</param>
        /// <returns>True if the particles are colliding</returns>
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
        /// Returns the updated velocity for two particles after a collision with another particle
        /// <param name="p1">First particle</param>
        /// <param name="p2">Second particle</param>
        /// <returns>The handled velocity of the first particle</returns>
        public static Vector2[] NewCollisionVelocity(Particle p1, Particle p2)
        {
            return NewCollisionVelocity(p1.CurrentVelocity, p2.CurrentVelocity, p1.Mass, p2.Mass, p1.Position, p2.Position);
        }

        public static Vector2[] NewCollisionVelocity(Vector2 v1, Vector2 v2, double m1, double m2, Vector2 x1, Vector2 x2)
        {
            Vector2 N = Vector2.Normalize(x1 - x2);
            Vector2 relativeVelocity = Vector2.Dot(v2 - v1, N) * N;
            Vector2[] newVelocities = new Vector2[]
            {
                v1 + (relativeVelocity * (float)(2*m2 / (m1 + m2))),
                v2 - (relativeVelocity * (float)(2*m2 / (m1 + m2)))
            };
            return newVelocities;
        }

        /// <summary>
        /// Root finding algorithm to determine the time at which particles are just colliding
        /// </summary>
        /// <param name="p1">Particle 1</param>
        /// <param name="p2">Particle 2</param>
        /// <param name="gameTime">GameTime object</param>
        /// <returns>The minimum time of collision</returns>
        public static double TimeOfCollision(Polygon p1, Polygon p2, GameTime gameTime)
        {
            Polygon a1 = p1;
            Polygon a2 = p2;
            double a = Math.Pow(a2.CurrentVelocity.Y + a2.CurrentVelocity.Y - (a1.CurrentVelocity.Y + a1.CurrentVelocity.X), 2);
            double b = 2*(a2.PreviousPosition.X + a2.PreviousPosition.Y - (a1.PreviousPosition.X + a1.PreviousPosition.Y))*(a2.CurrentVelocity.Y + a2.CurrentVelocity.Y - (a1.CurrentVelocity.Y + a1.CurrentVelocity.X));
            double c = Math.Pow(a2.PreviousPosition.X + a2.PreviousPosition.Y - (a1.PreviousPosition.X + a1.PreviousPosition.Y), 2) - Math.Pow(a1.YRadius + a2.YRadius, 2);

            double discriminant = Math.Pow(b,2) - (4*a*c);
            if (discriminant >= 0) // if there are roots
            {
                double[] roots = CalcRoots(a, b, c).Where(c => c >= 0).ToArray(); // find all positive roots
                if (roots.Length > 0)
                {
                    return roots.Min();
                }
            }
            return gameTime.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// Finds the roots of an equation in the form y = ax^2 + bx + c
        /// </summary>
        /// <param name="a">Coefficient of x^2</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant</param>
        /// <returns>Array containing all roots</returns>
        private static double[] CalcRoots(double a, double b, double c)
        {
            return new double[]
            {
                (-b + Math.Sqrt(Math.Pow(b,2) - (4*a*c))) / 2*a,
                (-b - Math.Sqrt(Math.Pow(b,2) - (4*a*c))) / 2*a,
            };
        }

        /// <summary>
        /// Returns a polygon that may be colliding with the wall with a handled velocity
        /// </summary>
        /// <param name="polygon">The polygon potentially colliding with the border</param>
        /// <param name="border">The border of the simulation</param>
        /// <returns>The polygon with its velocity handled.</returns>
        public static Polygon BoundaryCollisionHandling(Polygon polygon, Rectangle border)
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
                polygon.CollisionBoundaryUpdate(borderCollisions);
            }
            return polygon;
        }
    }
}