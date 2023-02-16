using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects;
using ThermalClasses.GameObjects.ObjectCollections;
namespace ThermalClasses.GraphClasses;

public class Graph : ObjectCollection
{
    #region Fields
    private GameObject graphFrame;
    private List<GameObject> crosses;
    private List<Vector2> relativeCrossPositions;
    private double minX, maxX, minY, maxY;
    private Texture2D lineTexture, crossTexture;
    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods
    public Graph(Texture2D graphTexture, Texture2D crossTexture, Rectangle graphRect, Color unclickedColour)
    {
        graphFrame = new GameObject(graphTexture, unclickedColour, graphRect);

        this.crossTexture = crossTexture;

        crosses = new List<GameObject>();
        relativeCrossPositions = new List<Vector2>();
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        graphFrame.Draw(_spriteBatch);
        for (int i = 0; i < crosses.Count; i++)
        {
            crosses[i].Draw(_spriteBatch);
        }
        if (crosses.Count > 1)
        {
            Vector2[] minMax = CalcLOBF();
            DrawLOBF(_spriteBatch, minMax[0], minMax[1]);
        }
    }

    public override void Update(GameTime gameTime)
    {
        graphFrame.Update(gameTime);
        for (int i = 0; i < crosses.Count; i++)
        {
            crosses[i].Update(gameTime);
        }
    }

    public override void ChangePenColour(Color penColour)
    {
        throw new NotImplementedException();
    }

    public void AddPoint(Vector2 values)
    {
        // Scale values appropriate to size of frame

        // Add leftbottom point to value to make sure it's 
    }

    // Calculates the line of best fit using the least square method
    public Vector2[] CalcLOBF()
    {
        // Calculate means
        double xMean, yMean;
        xMean = yMean = 0;
        for (int i = 0; i < crosses.Count; i++)
        {
            xMean += relativeCrossPositions[i].X;
            yMean += relativeCrossPositions[i].Y;
        }
        xMean /= crosses.Count;
        yMean /= crosses.Count;

        // Calculate gradient and y-intercept
        double topSum = 0;
        double bottomSum = 0;
        for (int i = 0; i < crosses.Count; i++)
        {
            topSum += (relativeCrossPositions[i].X - xMean) * (relativeCrossPositions[i].Y - yMean);
            bottomSum += Math.Pow((relativeCrossPositions[i].X - xMean), 2);
        }
        double gradient = topSum / bottomSum;
        Vector2 yIntercept = new Vector2(0, (float)(yMean - (gradient * xMean)));

        // Finding minimum and maximum x points
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        for (int i = 0; i < crosses.Count; i++)
        {
            if (relativeCrossPositions[i].X > maxX)
            {
                maxX = relativeCrossPositions[i].X;
            }
            if (relativeCrossPositions[i].X < minX)
            {
                minX = relativeCrossPositions[i].X;
            }
        }

        // Finding corresponding y positions
        Vector2 minPosition = new Vector2(minX, (float)((gradient * minX) + yIntercept.Y));
        Vector2 maxPosition = new Vector2(minX, (float)((gradient * maxX) + yIntercept.Y));

        return new Vector2[]{minPosition, maxPosition};
    }

    public void DrawLOBF(SpriteBatch _spriteBatch, Vector2 startPoint, Vector2 endPoint)
    {
        const int thickness = 5;

        int distance = (int)Vector2.Distance(startPoint, endPoint);
        lineTexture = new Texture2D(_spriteBatch.GraphicsDevice, distance, thickness);

        var data = new Color[distance * thickness];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Color.White;
        }
        lineTexture.SetData(data);

        // Rotate about middle of line
        float rotation = (float)Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
        Vector2 origin = new Vector2(0, thickness / 2);

        _spriteBatch.Draw(lineTexture, startPoint, null, Color.White, rotation, origin, 1.0f, SpriteEffects.None, 1.0f);
    }

    public void Dispose()
    {
        lineTexture.Dispose();
    }
    #endregion Methods
}
