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
    class PointLightEntity: Entity
    {
        public PointLightEntity(Location pos, float rad, Location col)
        {
            Position = pos;
            Radius = rad;
            Color = col;
            Velocity = Location.Zero;
            Angle = Location.Zero;
            Angular_Velocity = Location.Zero;
            Mass = 0;
        }

        public float Radius = 0;
        public Location Color;

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("radius", Radius.ToString()));
            vars.Add(new KeyValuePair<string, string>("color", Color.ToString()));
            return vars;
        }

        public override void Render(GLContext context)
        {
            Matrix4 mat = Matrix4.CreateTranslation((Position - new Location(0.5)).ToOVector());
            GL.UniformMatrix4(2, false, ref mat);
            context.Models.Cube.Draw();
        }
    }
}
