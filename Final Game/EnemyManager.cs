﻿using Final_Game.Entity;
using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
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

        // Events
        //public event EnemiesKilled OnLastEnemyKilled;

        // Constructors

        public EnemyManager(Game1 gm)
        {
            this.gm = gm;

            _dummyImage = gm.Content.Load<Texture2D>("Sprites/BasicEnemy");
            _gloveImage = gm.Content.Load<Texture2D>("Sprites/PinPunchSpritesheet");

            Enemies = new List<Enemy>();
        }

        // Methods

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

        public void CreateRoomEnemies(Room r)
        {
            for (int i = 0; i < r.RoomFloor.Spawners.Count; i++)
            {
                Tile curSpawner = r.RoomFloor.Spawners[i];

                // Spawn on spawn tile
                Enemy addition = new BasicPuncher(gm, curSpawner.WorldPosition);

                Enemies.Add(addition);
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
            }

            // Remove all enemies if all are currently KO-ed
            if (Enemies.Count > 0 && _koedEnemies == Enemies.Count)
            {
                Enemies.Clear();

                //OnLastEnemyKilled();
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].Draw(spriteBatch);
            }
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
