using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ThermalClasses.GameObjects;

public class GameObject
{
    #region Fields
    protected Texture2D texture;
    protected Vector2 position; // The central position of the object
    protected Color colour;
    protected float xRadius, yRadius;
    protected Point dimensions;
    #endregion

    #region Properties
    public bool Enabled;
    public Vector2 Position
    {
        get { return position; }
    }
    public Vector2 TopLeftPoint { get { return new Vector2(position.X - xRadius, position.Y - yRadius); } }
    public Rectangle ObjectRectangle { get { return new Rectangle((int)TopLeftPoint.X, (int)TopLeftPoint.Y, dimensions.X, dimensions.Y); } }
    public int XRadius { get { return dimensions.X / 2; }}
    public int YRadius { get { return dimensions.Y / 2; }}
    #endregion

    #region Methods
    public GameObject(Texture2D texture, Vector2 position, Color colour, Point dimensions)
    {
        this.dimensions = dimensions;
        this.texture = texture;
        this.position = position;
        this.colour = colour;
        Enabled = true;
    }

    public GameObject(Texture2D texture, Color colour, Rectangle size)
    {
        dimensions = size.Size;
        this.texture = texture;
        this.colour = colour;
        xRadius = dimensions.X / 2;
        yRadius = dimensions.Y / 2;
        Enabled = true;
        position = new Vector2(size.Left + xRadius, size.Top + yRadius);
    }

    // Initialises a basic GameObject
    protected GameObject(Texture2D texture) : this(texture, new Vector2(0, 0), new Color(0, 0, 0), new Point()) { }

    // Draws objects with the scale of the texture
    public virtual void TextureScaleDraw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            _spriteBatch.Draw(texture, TopLeftPoint, colour);
        }
    }

    // Method to draw objects using a rectangle
    public virtual void Draw(SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            Rectangle scaleDraw = new((int)TopLeftPoint.X, (int)TopLeftPoint.Y, dimensions.X, dimensions.Y);
            _spriteBatch.Draw(texture, scaleDraw, colour);
        }
    }

    // Method to draw objects to scale
    public virtual void ScaleDraw(SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            Rectangle scaleDraw = new((int)TopLeftPoint.X, (int)TopLeftPoint.Y, dimensions.X, dimensions.Y);
            _spriteBatch.Draw(texture, TopLeftPoint, scaleDraw, colour);
        }
    }

    public virtual void Update(GameTime gameTime) { }

    public void ChangePositionBy(Vector2 changeBy)
    {
        position = Vector2.Add(position, changeBy);
    }

    public void SetPosition(Vector2 newPosition)
    {
        position = newPosition;
    }
    #endregion
}
