using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
    public enum CollisionType
    {
        Horizontal, 
        Vertical
    }

    public interface ICollidable
    {
        // Properties
        Rectangle Hitbox { get; }
        Vector2 WorldPosition { get; }
        bool CollisionOn { get; }

        // Methods
        void OnHitSomething(ICollidable other, CollisionType collision);
    }
}
