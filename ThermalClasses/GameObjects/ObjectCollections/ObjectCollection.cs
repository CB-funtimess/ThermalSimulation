using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace ThermalClasses.GameObjects.ObjectCollections;

public abstract class ObjectCollection
{
    public bool Enabled;
    public abstract void Draw(SpriteBatch _spriteBatch);
    public abstract void Update(GameTime gameTime);
    public abstract void ChangePenColour(Color penColour);
}
