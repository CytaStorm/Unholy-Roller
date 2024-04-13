using Final_Game.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Managers
{
	public class ParticleTrailManager
	{
		#region Fields
		private List<TrailParticle> _particles = new List<TrailParticle>();
		
		private double _particleCooldown;
		private double _particleCooldownTimer;

        private Color _nextParticleColor;
        #endregion

        #region Constructor(s)
        public ParticleTrailManager(double particleCooldown)
		{
			this._particleCooldown = particleCooldown;
			this._particleCooldownTimer = particleCooldown;

			//_nextParticleColor = new Color(1f, 0f, 0f);
		}
		#endregion

		#region Methods
		public void Update(GameTime gameTime)
		{
			// During bullet time create particles after a set cooldown
			if (Player.BulletTimeMultiplier < 1f)
				SpawnParticles(gameTime);
			else
				ClearParticles();

			// Manage active particles
			for (int i = 0; i < _particles.Count; i++)
			{
				// Update particles
				_particles[i].Update(gameTime);

				// Remove dead particles
				if (!_particles[i].Alive)
				{
					_particles.RemoveAt(i);
					i--;
				}
			}
		}

		/// <summary>
		/// Spawns a new particle after a cooldown
		/// </summary>
		/// <param name="gameTime"></param>
		private void SpawnParticles(GameTime gameTime)
		{
			// Tick cooldown timer
			_particleCooldownTimer -= gameTime.ElapsedGameTime.TotalSeconds;

			if (_particleCooldownTimer <= 0)
			{
                // Add a particle to the trail
                _particles.Add(new TrailParticle(
                    Game1.Player.Image,
                    Color.White * 0.7f,
                    Game1.Player.WorldPosition,
					2));

				//GetNextParticleColor();

                // Reset cooldown timer
                _particleCooldownTimer += _particleCooldown;
            }
            
        }

		/// <summary>
		/// Removes all particles if there are any
		/// </summary>
		private void ClearParticles()
		{
			if (_particles.Count == 0) return;

			// Remove all particles
            _particles.Clear();

			// Reset cooldown timer
            _particleCooldownTimer = _particleCooldown;

			// Reset first particle color
            _nextParticleColor = new Color(1f, 0f, 0f);
        }

		/// <summary>
		/// Cycles the color for the next particle from 
		/// R-> G-> B-> R...
		/// </summary>
		private void GetNextParticleColor()
		{
			if (_nextParticleColor.R == 255)
			{
				_nextParticleColor = new Color(0f, 1f, 0f);
			}
			else if (_nextParticleColor.G == 255)
			{
				_nextParticleColor = new Color(0f, 0f, 1f);
			}
			else
			{
				_nextParticleColor = new Color(1f, 0f, 0f);
			}
		}

		public void Draw(SpriteBatch sb)
		{
			foreach (TrailParticle fx in _particles)
			{
				fx.Draw(sb);
			}
		}
		#endregion
	}
}
