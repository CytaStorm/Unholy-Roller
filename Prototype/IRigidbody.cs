using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    public interface IRigidbody
    {
        // Properties
        Vector2 WorldPosition { get; protected set; }
        Vector2 Velocity { get; protected set; }

        // Methods
        void Move();

        void ApplyGravity();
    }
}
