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
    private List<Vector2> crossPositions;
    private double minX, maxX, minY, maxY;
    private Texture2D lineTexture, crossTexture;
    private Color unclickedColour;
    private string xName, yName;
    private Vector2 xNameStart, yNameStart;
    private SpriteFont font;
    #endregion Fields

    #region Properties
    public Color PenColour;
    #endregion Properties

    #region Methods
    public Graph(Texture2D graphTexture, Texture2D crossTexture, Rectangle graphRect, Color unclickedColour, Vector2 maxCrossPos, Vector2 minCrossPos, string xName, string yName, SpriteFont font)
    {
        Enabled = true;
        graphFrame = new GameObject(graphTexture, unclickedColour, graphRect);

        this.crossTexture = crossTexture;
        this.unclickedColour = unclickedColour;

        minX = minCrossPos.X;
        minY = minCrossPos.Y;
        maxX = maxCrossPos.X;
        maxY = maxCrossPos.Y;

        this.xName = xName;
        this.yName = yName;

        this.font = font;

        crosses = new List<GameObject>();
        crossPositions = new List<Vector2>();

        xNameStart = new Vector2(graphRect.Center.X - (font.MeasureString(xName).X / 2), graphRect.Bottom);
        yNameStart = new Vector2(graphRect.Left - 10, graphRect.Center.Y - (font.MeasureString(yName).Y / 2));
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            _spriteBatch.DrawString(font, xName, xNameStart, PenColour);
            Vector2 yOrigin = new Vector2(font.MeasureString(yName).X / 2, font.MeasureString(yName).Y / 2);//yNameStart.X, yNameStart.Y + (font.MeasureString(yName).Y / 2));
            _spriteBatch.DrawString(font, yName, yNameStart, PenColour, (float)(Math.PI / 2), yOrigin, 1.0f, SpriteEffects.None, 1.0f);

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
    }

    public override void Update(GameTime gameTime)
    {
        if (Enabled)
        {
            graphFrame.Update(gameTime);
            for (int i = 0; i < crosses.Count; i++)
            {
                crosses[i].Update(gameTime);
            }
        }
    }

    public override void ChangePenColour(Color penColour) { }

    public void AddPoint(Vector2 value)
    {
        // Add value to relative list provided it is within the conditions
        if (value.X >= minX && value.X <= maxX && value.Y >= minY && value.Y <= maxY && !crossPositions.Contains(value))
        {
            // Scale value appropriate to size of frame
            Vector2 scaledValue = new Vector2((float)(graphFrame.ObjectRectangle.Width / (maxX - minX) * (value.X - minX)), (float)(graphFrame.ObjectRectangle.Height / (maxY - minY) * (value.Y - minY)));

            // Add leftbottom point to value to make sure it's drawn in the correct position
            Vector2 actualPosition = new Vector2(scaledValue.X + graphFrame.ObjectRectangle.Left, graphFrame.ObjectRectangle.Bottom - scaledValue.Y);

            // Creating new cross
            GameObject cross = new GameObject(crossTexture, actualPosition, Color.Red, new Point(10, 10));
            crosses.Add(cross);
            crossPositions.Add(actualPosition);
        }
    }

    // Calculates the line of best fit using the least square method
    public Vector2[] CalcLOBF()
    {
        // Calculate means
        double xMean, yMean;
        xMean = yMean = 0;
        for (int i = 0; i < crosses.Count; i++)
        {
            xMean += crossPositions[i].X;
            yMean += crossPositions[i].Y;
        }
        xMean /= crosses.Count;
        yMean /= crosses.Count;

        // Calculate gradient and y-intercept
        double topSum = 0;
        double bottomSum = 0;
        for (int i = 0; i < crosses.Count; i++)
        {
            topSum += (crossPositions[i].X - xMean) * (crossPositions[i].Y - yMean);
            bottomSum += Math.Pow(crossPositions[i].X - xMean, 2);
        }
        double gradient = topSum / bottomSum;
        float yIntercept = (float)(yMean - (gradient * xMean));

        // Finding minimum and maximum x points
        float pointMinX = float.MaxValue;
        float pointMaxX = float.MinValue;
        for (int i = 0; i < crosses.Count; i++)
        {
            if (crossPositions[i].X > pointMaxX)
            {
                pointMaxX = crossPositions[i].X;
            }
            else if (crossPositions[i].X < pointMinX)
            {
                pointMinX = crossPositions[i].X;
            }
        }

        // Finding corresponding y positions
        Vector2 minCrossPos = new Vector2(pointMinX, (float)((gradient * pointMinX) + yIntercept));
        Vector2 maxCrossPos = new Vector2(pointMaxX, (float)((gradient * pointMaxX) + yIntercept));

        return new Vector2[] { minCrossPos, maxCrossPos };
    }

    public void DrawLOBF(SpriteBatch _spriteBatch, Vector2 startPoint, Vector2 endPoint)
    {
        const int thickness = 2;

        int distance = (int)Vector2.Distance(startPoint, endPoint);
        if (distance > 0)
        {
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
    }

    public void ClearPoints()
    {
        crosses.Clear();
        crossPositions.Clear();
    }

    public void Dispose()
    {
        lineTexture?.Dispose();
    }
    #endregion Methods
}
