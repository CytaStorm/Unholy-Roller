using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    public interface ICollidable
    {
        // Properties
        Rectangle Hitbox { get; protected set; }

        // Methods
        void OnHitTile();

        void OnHitEntity();
    }
}
