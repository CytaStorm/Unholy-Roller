using Final_Game.Entity;
using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Final_Game.Managers
{
	public enum EnemyType
	{
		Dummy,
		BasicPuncher,
		BombPin,
		PinMech
	}

	public class EnemyManager
	{
		#region Fields
		private Texture2D _dummyImage;
		private Texture2D _gloveImage;
		private Game1 gm;
		private int _koedEnemies;

		private EnemyType[] _enemyTypes;
		private Random _rng;
		#endregion

		#region Properties
		public List<Enemy> Enemies { get; private set; }
		public bool EnemiesInvincible { get; set; }
		#endregion

		// Events
		public delegate void EnemiesKilled();
		public event EnemiesKilled OnLastEnemyKilled;

		// Constructors

		public EnemyManager(Game1 gm)
		{
			this.gm = gm;

			_dummyImage = gm.Content.Load<Texture2D>("Sprites/BasicEnemy");
			_gloveImage = gm.Content.Load<Texture2D>("Sprites/PinPunchSpritesheet");

			Enemies = new List<Enemy>();

			// Create test enemies
			//BasicPuncher testEnemy2 = new BasicPuncher(gm, new Vector2(500f, 200f));
			//Enemies.Add(testEnemy2);
			//BasicPuncher testEnemy3 = new BasicPuncher(gm, new Vector2(300f, 800f));
			//Enemies.Add(testEnemy3);

			_enemyTypes = new EnemyType[]
			{
				EnemyType.BasicPuncher,
				EnemyType.BombPin
			};

			_rng = new Random();

		}

		// Methods
		public void CreateRoomEnemies(Room r)
		{
			for (int i = 0; i < r.Tileset.Spawners.Count; i++)
			{
				Tile curSpawner = r.Tileset.Spawners[i];


				if (Game1.CurrentLevel == Game1.TestLevel)
					// Spawn on spawn tile
					SpawnEnemy(curSpawner.WorldPosition, ChooseRandomEnemy());
				else
				{
					Enemies.Add(new Dummy(gm, curSpawner.WorldPosition, false));
				}
            }
        }
        public void SpawnBoss(Room r)
        {
            Enemies.Add(new PinMech(gm, new Vector2(r.Center.X - 50, r.Center.Y - 50)));
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

				Enemies[i].InfiniteHealth = EnemiesInvincible;
				if (Enemies[i].Alive)
				{
					continue;
				}
				// Remove dead enemies
				Enemies.RemoveAt(i);
				i--;
			 }

			// Remove all enemies if all are currently KO-ed
			if (Enemies.Count > 0 && _koedEnemies == Enemies.Count)
			{
				Enemies.Clear();

                OnLastEnemyKilled();
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

		/// <summary>
		/// Picks a random enemy type excluding the dummy
		/// </summary>
		/// <returns></returns>
		private EnemyType ChooseRandomEnemy()
		{
			return _enemyTypes[_rng.Next(_enemyTypes.Length)];
		}

		/// <summary>
		/// Spawns an enemy of the specified type at the specified position
		/// </summary>
		/// <param name="position"> position to spawn enemy </param>
		/// <param name="enemyType"> type of enemy to spawn </param>
		private void SpawnEnemy(Vector2 position, EnemyType enemyType)
		{
			switch (enemyType)
			{
				case EnemyType.Dummy:
					Enemies.Add(new Dummy(gm, position, false));
					break;


				case EnemyType.BasicPuncher:
					Enemies.Add(new BasicPuncher(gm, position));
					break;

				case EnemyType.BombPin:
					Enemies.Add(new BombPin(gm, position));
					break;
			}
		}

	}
}
