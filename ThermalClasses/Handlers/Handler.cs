using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.Handlers;

public abstract class Handler
{
    #region Fields
    protected Game game;
    protected Rectangle renderRectangle;
    protected ContentManager content;
    #endregion

    #region Properties
    public Color BackgroundColour { get; set; }
    public Color HoverColour { get; set; }
    public Color PenColour { get; set;}
    public bool Enabled;
    #endregion

    #region Methods
    public abstract void Initialize();
    public abstract void LoadContent();
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime, SpriteBatch _spriteBatch);
    public abstract void ChangePenColour();
    #endregion
}