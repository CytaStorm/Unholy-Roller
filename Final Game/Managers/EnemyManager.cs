using Final_Game.Entity;
using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Final_Game.Managers
{
	public class EnemyManager
    {
        // Fields
        private Texture2D _dummyImage;
        private Texture2D _gloveImage;
        private Game1 gm;
        private int _koedEnemies;

        // Properties
        public List<Enemy> Enemies { get; private set; }
        public bool EnemiesInvincible { get; set; }

        // Events
        //public event EnemiesKilled OnLastEnemyKilled;

        // Constructors

        public EnemyManager(Game1 gm)
        {
            this.gm = gm;

            _dummyImage = gm.Content.Load<Texture2D>("Sprites/BasicEnemy");
            _gloveImage = gm.Content.Load<Texture2D>("Sprites/PinPunchSpritesheet");

            Enemies = new List<Enemy>();

            // Create test enemies
             PinMech testEnemy = new PinMech(gm, new Vector2(750f, 750f));
             Enemies.Add(testEnemy);
             //BasicPuncher testEnemy2 = new BasicPuncher(gm, new Vector2(500f, 200f));
             //Enemies.Add(testEnemy2);
             //BasicPuncher testEnemy3 = new BasicPuncher(gm, new Vector2(300f, 800f));
             //Enemies.Add(testEnemy3);

        }

        // Methods
        public void CreateRoomEnemies(Room r)
        {
            for (int i = 0; i < r.Tileset.Spawners.Count; i++)
            {
                Tile curSpawner = r.Tileset.Spawners[i];

                // Spawn on spawn tile
                Enemies.Add(new BasicPuncher(gm, curSpawner.WorldPosition));
            }
        }

        public void Update(GameTime gameTime)
        {
            _koedEnemies = 0;

            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].Update(gameTime);

                // Keep track of all KO-ed enemies
                if (Enemies[i].IsKO)
                {
                    _koedEnemies++;
                }

                // Remove dead enemies
                if (!Enemies[i].Alive)
                {
                    Enemies.RemoveAt(i);
                    i--;

                    if (Enemies.Count == 0) { }
                        //OnLastEnemyKilled();
                }

                Enemies[i].InfiniteHealth = EnemiesInvincible;
            }

            // Remove all enemies if all are currently KO-ed
            if (Enemies.Count > 0 && _koedEnemies == Enemies.Count)
            {
                Enemies.Clear();
                Game1.IManager.Clear();

                //OnLastEnemyKilled();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].Draw(spriteBatch);
            }
        }


        /// <summary>
        /// Destroys all enemies in the scene
        /// </summary>
        public void Clear()
        {
            Enemies.Clear();
        }

        public void ResetRoomEnemies(Room r)
        {
            // Remove all enemies
            Clear();

            CreateRoomEnemies(r);
        }

        public void DrawGizmos()
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].DrawGizmos();
            }
        }

    }
}
