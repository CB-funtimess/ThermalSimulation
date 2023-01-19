using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace ThermalClasses.GameObjects.ObjectCollections;

public class RadioButtons
{
    #region Fields
    private List<Label> labels;
    private Label surround;
    #endregion

    #region Properties
    public List<CheckButton> Buttons;
    #endregion

    #region Methods
    public RadioButtons(Texture2D buttonTexture, Texture2D checkedTexture, Texture2D labelTexture, Texture2D surroundTexture, Rectangle rect, List<string> text, SpriteFont font, Color baseColour, Color penColour)
    {
        surround = new Label(surroundTexture, baseColour, rect, font, penColour);
        int numButtons = text.Count;
        Point buttonSize = new Point(40, 40);

        for (var i = 0; i < numButtons; i++)
        {
            Vector2 buttonPos = new Vector2(rect.X + 20, rect.Y + (rect.Height * ((i+1) / (numButtons + 1))));
            Rectangle labelRect = new Rectangle(new Point((int)(buttonPos.X + 60), (int)(buttonPos.Y - (buttonSize.Y / 2))), new Point(rect.Width / (3/4), buttonSize.Y));
            labels[i] = new Label(labelTexture, baseColour, labelRect, font, penColour)
            {
                Text = text[i],
            };
            Buttons[i] = new CheckButton(buttonTexture, checkedTexture, font, buttonPos, baseColour, penColour, buttonSize);
        }
    }
    #endregion
}