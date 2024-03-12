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
        private Texture2D _gloveImage;
        private Random rng;
        private Game1 gm;
        private int _koedEnemies;

        // Properties
        public List<Enemy> Dummies { get; private set; }

        // Events
        public event EnemiesKilled OnLastEnemyKilled;
        
        // Constructors

        public EnemyManager(Game1 gm)
        {
            this.gm = gm;

            _dummyImage = gm.Content.Load<Texture2D>("BasicEnemy");
            _gloveImage = gm.Content.Load<Texture2D>("PinPunchSpritesheet");

            // Create a few enemies in the scene
            Dummies = new List<Enemy>();
            rng = new Random();
        }

        // Methods

        /// <summary>
        /// Destroys all enemies in the scene
        /// </summary>
        public void Clear()
        {
            Dummies.Clear();
        }

        public void ResetRoomEnemies(Room r)
        {
            // Remove all enemies
            Clear();

            CreateRoomEnemies(r);
        }

        public void CreateRoomEnemies(Room r)
        {
            for (int i = 0; i < r.Floor.Spawners.Count; i++)
            {
                Tile curSpawner = r.Floor.Spawners[i];

                // Spawn on spawn tile
                Enemy addition = new Enemy(
                    _dummyImage, 
                    _gloveImage, 
                    curSpawner.WorldPosition,
                    gm.Graphics);

                Dummies.Add(addition);
            }
        }

        public void Update(GameTime gameTime)
        {
            _koedEnemies = 0;

            for (int i = 0; i < Dummies.Count; i++)
            {
                Dummies[i].Update(gameTime);

                // Keep track of all KO-ed enemies
                if (Dummies[i].IsKO)
                {
                    _koedEnemies++;
                }

                // Remove dead enemies
                if (!Dummies[i].Alive)
                {
                    Dummies.RemoveAt(i);
                    i--;

                    if (Dummies.Count == 0)
                        OnLastEnemyKilled();
                }
            }

            // Remove all enemies if all are currently KO-ed
            if (Dummies.Count > 0 && _koedEnemies == Dummies.Count)
            {
                Dummies.Clear();

                OnLastEnemyKilled();
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (int i = 0; i < Dummies.Count; i++)
            {
                Dummies[i].Draw(spriteBatch, gameTime);
            }
        }
        
        public void DrawGizmos(GameTime gameTime)
        {
            for (int i = 0; i < Dummies.Count; i++)
            {
                Dummies[i].DrawGizmos();
            }
        }
    }
}
