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
        public static Location mins = new Location(-0.3f, -0.3f, 0f);
        public static Location maxes = new Location(0.3f, 0.3f, 2f);

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
                if (PrimaryEditor.RenderLines)
                {
                    context.Rendering.RenderLineBox(Position + mins, Position + maxes);
                }
                else
                {
                    context.Textures.White.Bind();
                    Matrix4 mat = Matrix4.CreateScale(0.6f, 0.6f, 2f) + Matrix4.CreateTranslation((Position - new Location(0.3f, 0.3f, 0f)).ToOVector());
                    GL.UniformMatrix4(2, false, ref mat);
                    context.Rendering.SetMinimumLight(1.0f);
                    context.Models.Cube.Draw();
                }
            }
        }

        public override string ToString()
        {
            return "SPAWNENTITY{location=" + Position + "}";
        }
    }
}
