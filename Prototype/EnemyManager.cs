using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prototype.GameEntity;
using Prototype.MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    public class EnemyManager : IGameObject
    {
        // Fields
        private Texture2D _dummyImage;
        private Random rng;

        // Properties
        public List<Enemy> Dummies { get; private set; }
        
        public EnemyManager(Game1 gm)
        {
            _dummyImage = gm.Content.Load<Texture2D>("Cthulu_Muggles");
            

            // Create a few enemies in the scene
            Dummies = new List<Enemy>();
            rng = new Random();

            for (int i = 0; i < Game1.TEST_ROOM.Floor.Spawners.Count; i++)
            {
                Tile curSpawner = Game1.TEST_ROOM.Floor.Spawners[i];

                /*
                Enemy addition = new Enemy(
                    _dummyImage,
                    new Vector2(
                        // X position within tilemap
                        rng.Next(Game1.TEST_ROOM.Origin.X + Game1.TILESIZE,
                        Game1.TEST_ROOM.Origin.X + Game1.TEST_ROOM.Floor.Width - Enemy.DEFAULT_SPRITE_WIDTH - Game1.TILESIZE),

                        // Y position within tilemap
                        rng.Next(Game1.TEST_ROOM.Origin.Y + Game1.TILESIZE,
                        Game1.TEST_ROOM.Origin.Y + Game1.TEST_ROOM.Floor.Height - Enemy.DEFAULT_SPRITE_HEIGHT - Game1.TILESIZE)),
                        
                        gm.Graphics);
                */

                // Spawn on spawn tile
                Enemy addition = new Enemy(_dummyImage, curSpawner.WorldPosition, gm.Graphics);

                Dummies.Add(addition);
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < Dummies.Count; i++)
            {
                Dummies[i].Update(gameTime);

                // Remove dead enemies
                if (!Dummies[i].Alive)
                {
                    Dummies.RemoveAt(i);
                    i--;
                }
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
