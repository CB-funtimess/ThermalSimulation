using System.Runtime.InteropServices;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.GameObjects;
using ThermalClasses.PhysicsLaws;
using ThermalClasses.GameObjects.ObjectCollections;
namespace ThermalClasses.Handlers;

public class QuestionHandler : Handler
{
    #region Fields
    #endregion

    #region Properties
    #endregion

    #region Methods
    public QuestionHandler(Game game, Rectangle renderRectangle)
    {
        this.game = game;
        this.renderRectangle = renderRectangle;
    }

    #region Initialisation
    public override void Initialize()
    {
        throw new NotImplementedException();
    }

    public override void LoadContent()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Drawing and Updating
    public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        throw new NotImplementedException();
    }

    public override void Update(GameTime gameTime)
    {
        throw new NotImplementedException();
    }
    #endregion

    public override void ChangePenColour(Color colour)
    {
        throw new NotImplementedException();
    }
    #endregion
}
