using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ThermalClasses.GameObjects.Particles;

public class TextInput : GameObject
{
    #region Fields
    private bool selected;
    private bool isHovering;
    private KeyboardState prevKeyState, keyState;
    private MouseState prevMouseState, mouseState;
    private SpriteFont font;
    private string text;
    #endregion

    #region Properties
    public Color PenColour;
    public Color HoverColour;
    public string Text => text;
    public EventHandler Enter;
    #endregion

    #region Methods
    public TextInput(Texture2D texture, Rectangle position, SpriteFont font, Color colour, Color penColour) : base(texture, colour, position)
    {
        PenColour = penColour;
        text = "";
        this.font = font;
        selected = false;
    }

    public override void Update(GameTime gameTime)
    {
        prevMouseState = mouseState;
        prevKeyState = keyState;
        mouseState = Mouse.GetState();
        keyState = Keyboard.GetState();

        isHovering = false;

        Rectangle mouseRectangle = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

        if (mouseRectangle.Intersects(ObjectRectangle))
        {
            isHovering = true;
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                selected = true;
                if (keyState.GetPressedKeys().Contains(Keys.Enter))
                {
                    Enter?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        else if (!mouseRectangle.Intersects(ObjectRectangle) && mouseState.LeftButton == ButtonState.Pressed)
        {
            selected = false;
        }

        if (selected)
        {
            text += keyState.GetPressedKeys();
        }
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            if (isHovering)
            {
                _spriteBatch.Draw(texture, ObjectRectangle, HoverColour);
            }
            else
            {
                _spriteBatch.Draw(texture, ObjectRectangle, colour);
            }

            if (!String.IsNullOrEmpty(Text))
            {
                var x = ObjectRectangle.X + (ObjectRectangle.Width / 2);
                var y = ObjectRectangle.Y + (ObjectRectangle.Height / 2);

                _spriteBatch.DrawString(font, Text, new Vector2(x,y), PenColour);
            }
        }
    }
    #endregion
}