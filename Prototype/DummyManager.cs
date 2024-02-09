using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prototype.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    internal class DummyManager : IGameObject
    {
        // Fields
        private Texture2D _dummyImage;

        // Properties

        public List<Dummy> Dummies { get; private set; }
        

        public DummyManager(Game1 gManager)
        {
            _dummyImage = gManager.Content.Load<Texture2D>("Cthulu_Muggles");


            Dummies = new List<Dummy>();
            Dummy d1 = new Dummy(_dummyImage, new Vector2(100f, 100f), gManager.Graphics);
            d1.Image.TintColor = Color.Red;
            Dummies.Add(d1);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < Dummies.Count; i++)
            {
                Dummies[i].Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (int i = 0; i < Dummies.Count; i++)
            {
                Dummies[i].Draw(spriteBatch, gameTime);
            }
        }
    }
}
