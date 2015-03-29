using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;
using OpenTKMapMaker.GraphicsSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKMapMaker.EntitySystem
{
    class SpawnPointEntity: Entity
    {
        public SpawnPointEntity(Location pos)
        {
            Position = pos;
            Velocity = Location.Zero;
            Angle = Location.Zero;
            Angular_Velocity = Location.Zero;
            Mass = 0;
        }

        public override string GetEntityType()
        {
            return "spawn";
        }

        public override void Render(GLContext context)
        {
            if (PrimaryEditor.RenderEntities)
            {
                Matrix4 mat = Matrix4.CreateTranslation((Position - new Location(0.5)).ToOVector());
                GL.UniformMatrix4(2, false, ref mat);
                context.Models.Cube.Draw();
            }
        }

        public override string ToString()
        {
            return "SPAWNENTITY{location=" + Position + "}";
        }
    }
}
