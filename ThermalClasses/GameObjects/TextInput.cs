using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ThermalClasses.GameObjects.Particles;

public class TextInput : GameObject
{
    #region Fields
    #endregion

    #region Properties
    #endregion
    
    #region Methods
    public TextInput(Texture2D texture, Rectangle position, Color colour) : base(texture, colour, position)
    {

    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        base.Draw(_spriteBatch);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }
    #endregion
}