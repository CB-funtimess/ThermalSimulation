using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects;

public class Button : GameObject
{
    #region Fields
    private MouseState currentState;
    private MouseState previousState;
    private readonly SpriteFont font;
    private bool isHovering;
    #endregion

    #region Properties
    public EventHandler Click;
    public bool Clicked {get; private set;}
    // Rectangle to mathematically represent the space the button takes up
    public Rectangle Rectangle
    {
        get {
            return new Rectangle((int)position.X, (int)position.Y, dimensions.X, dimensions.Y);
        }
    }
    public string Text{get;set;}
    public Color PenColor{get;set;}
    #endregion

    #region Methods
    public Button(Texture2D texture, SpriteFont font, Vector2 position, Color colour, Color penColour, Point dimensions) : base(texture, position, colour, dimensions)
    {
        this.font = font;

        PenColor = penColour;
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        if(Enabled)
        {
            // If the mouse is hovering over the button
            if (isHovering)
            {
                colour = Color.Black;
            }

            _spriteBatch.Draw(texture, Rectangle, colour);

            if (!String.IsNullOrEmpty(Text))
            {
                var x = Rectangle.X + (Rectangle.Width / 2) - (font.MeasureString(Text).X / 2);
                var y = Rectangle.Y + (Rectangle.Height / 2) - (font.MeasureString(Text).Y / 2);

                _spriteBatch.DrawString(font, Text, new Vector2(x,y), PenColor);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        previousState = currentState;
        currentState = Mouse.GetState();
        Clicked = false;

        var mouseRectangle = new Rectangle(currentState.X, currentState.Y, 1, 1);

        isHovering = false;

        // If the button is pressed, trigger a new event
        if (mouseRectangle.Intersects(Rectangle))
        {
            isHovering = true;

            if (currentState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed)
            {
                Click?.Invoke(this, new EventArgs());
                Clicked = true;
            }
        }
    }

    #endregion
}