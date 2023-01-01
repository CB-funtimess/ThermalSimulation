using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects.Particles;
namespace ThermalClasses
{
    public class SHG
    {
        #region Attributes
        private Dictionary<Microsoft.Xna.Framework.Vector2, List<Polygon>> spatialHashGrid;
        private int cellHeight, cellWidth;
        #endregion

        #region Properties
        #endregion

        #region Methods
        // Constructor: Create dictionary (a new dictionary is created every frame)
        public SHG(int simHeight, int simWidth, double avgParticleSize)
        {
            // Bucket dimensions should be roughly double the size of a particle
            cellHeight = Convert.ToInt32(Math.Ceiling(avgParticleSize * 2));
            cellWidth = Convert.ToInt32(Math.Ceiling(avgParticleSize * 2));

            spatialHashGrid = new Dictionary<Vector2, List<Polygon>>();
            // Initialising all buckets in the grid
            for (int i = 0; i < Hash(new Vector2(simHeight, simWidth)).X; i++)
            {
                for (int j = 0; j < Hash(new Vector2(simHeight, simWidth)).Y; j++)
                {
                    spatialHashGrid.Add(new Vector2(i, j), new List<Polygon>());
                }
            }
        }

        // Hash function will return the bucket's vector index for hashing a value
        private Vector2 Hash(Vector2 point)
        {
            // Maps point position to a bucket value
            return new Vector2(Convert.ToInt32(Math.Ceiling(point.X / cellWidth)), Convert.ToInt32(Math.Ceiling(point.Y / cellHeight)));
        }

        // This function inserts a particle into the dictionary depending on its position
        public void Insert(Polygon particle)
        {
            // Create a bounding box - largest possible area of the shape
            Rectangle boundingBox = particle.BoundingBox;
            Vector2[] points = {
                new Vector2(boundingBox.Left, boundingBox.Top),
                new Vector2(boundingBox.Right, boundingBox.Top),
                new Vector2(boundingBox.Right, boundingBox.Bottom),
                new Vector2(boundingBox.Left, boundingBox.Bottom)
            };

            // Assign each corner of the rectangle to a bucket (while this is an approximation, it is an overestimate so it is guaranteed that all collisions will be accommodated)
            foreach (var point in points)
            {
                Vector2 key = Hash(point);
                if (!spatialHashGrid[key].Contains(particle))
                {
                    spatialHashGrid[key].Add(particle);
                }
            }
        }

        // This function reads in a set of particles and inserts all of them into the grid
        public void Insert(Polygon[] particles)
        {
            foreach (var polygon in particles)
            {
                Insert(polygon);
            }
        }

        // This function returns all buckets with potentially colliding buckets to end the broad phase
        public List<List<Polygon>> ReturnCollisions()
        {
            List<List<Polygon>> collidingBuckets = new();
            foreach (var bucket in spatialHashGrid)
            {
                if (bucket.Value.Count > 1)
                {
                    collidingBuckets.Add(bucket.Value);
                }
            }

            return collidingBuckets;
        }
        #endregion
    }
}