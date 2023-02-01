using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects.ObjectCollections;

public class CollisionCounter : ObjectCollection
{
    #region Fields
    private Label containerLabel;
    private Label textLabel;
    private CheckButton startCount;
    private Button resetButton;
    private bool isCounting;
    #endregion

    #region Properties
    public int NoCollisions;
    #endregion

    #region Methods
    public CollisionCounter(Rectangle position, Texture2D containerTexture, Texture2D textLabelTexture, Texture2D uncheckedStartTexture, Texture2D checkedStartTexture, Texture2D resetTexture, Color backgroundColour, Color hoverColour, Color penColour, SpriteFont font)
    {
        NoCollisions = 0;
        isCounting = true;
        containerLabel = new Label(containerTexture, backgroundColour, position, font, penColour);
        Point textDimensions = new Point(position.Width * 3/4, (int)(position.Height * 0.35));
        Point buttonDimensions = new Point(36,36);
        Rectangle textRect = new Rectangle(new Point(position.Center.X - (textDimensions.X / 2), position.Y * 1/4), textDimensions);
        textLabel = new Label(textLabelTexture, backgroundColour, textRect, font, penColour)
        {
            Text = $"{NoCollisions}",
        };
        Rectangle startRect = new Rectangle(new Point(textRect.X, textRect.Bottom + 10), buttonDimensions);
        startCount = new CheckButton(uncheckedStartTexture, checkedStartTexture, font, startRect, backgroundColour, penColour)
        {
            HoverColour = hoverColour
        };
        startCount.Click += StartStopCount_Click;
        Rectangle resetRect = new Rectangle(new Point(textRect.Right - buttonDimensions.X, textRect.Bottom + 10), buttonDimensions);
        resetButton = new Button(resetTexture, font, resetRect, backgroundColour, penColour)
        {
            HoverColour = hoverColour,
        };
        resetButton.Click += ResetCount_Click;
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        containerLabel.Draw(_spriteBatch);
        textLabel.Draw(_spriteBatch);
        startCount.Draw(_spriteBatch);
        resetButton.Draw(_spriteBatch);
    }

    public override void Update(GameTime gameTime)
    {
        if (isCounting)
        {
            textLabel.Text = $"{NoCollisions}";
        }
        resetButton.Update(gameTime);
        startCount.Update(gameTime);
    }

    public override void ChangePenColour(Color penColour)
    {
        containerLabel.PenColour = penColour;
        textLabel.PenColour = penColour;
    }

    public void ResetCount_Click(object sender, EventArgs e)
    {
        NoCollisions = 0;
        textLabel.Text = $"{NoCollisions}";
        isCounting = false;
        startCount.Check();
    }

    public void StartStopCount_Click(object sender, EventArgs e)
    {
        if (!isCounting)
        {
            NoCollisions = 0;
        }
        isCounting = !isCounting;
    }
    #endregion
}