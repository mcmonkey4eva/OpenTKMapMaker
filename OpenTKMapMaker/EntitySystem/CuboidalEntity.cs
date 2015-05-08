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
    public abstract class CuboidalEntity: Entity
    {
        public CuboidalEntity(Location min, Location max, string tex)
        {
            Mins = min;
            Maxes = max;
            Position = min + (max - min) / 2;
            Mass = 0;
            Velocity = Location.Zero;
            Angle = Location.Zero;
            Angular_Velocity = Location.Zero;
            float f1 = (float)Utilities.UtilRandom.NextDouble();
            ViewColor = new Color4(0f, 1 - f1, f1, 1f);
            Texture = tex;
        }

        public Location Mins;
        public Location Maxes;
        public string Texture = "white";
        public TextureCoordinates[] Coords = new TextureCoordinates[] { new TextureCoordinates(), new TextureCoordinates(),
            new TextureCoordinates(), new TextureCoordinates(), new TextureCoordinates(), new TextureCoordinates() };

        public bool ContainsPoint(Location point)
        {
            return CollisionUtil.BoxContainsPoint(Mins, Maxes, point);
        }

        public void Include(Location point)
        {
            Location pmax = Maxes;
            Location pmin = Mins;
            Location oldsize = pmax - pmin;
            if (PrimaryEditor.stretch_x == 1 && point.X > Mins.X) { Maxes.X = point.X; }
            if (PrimaryEditor.stretch_x == -1 && point.X < Maxes.X) { Mins.X = point.X; }
            if (PrimaryEditor.stretch_y == 1 && point.Y > Mins.Y) { Maxes.Y = point.Y; }
            if (PrimaryEditor.stretch_y == -1 && point.Y < Maxes.Y) { Mins.Y = point.Y; }
            if (PrimaryEditor.stretch_z == 1 && point.Z > Mins.Z) { Maxes.Z = point.Z; }
            if (PrimaryEditor.stretch_z == -1 && point.Z < Maxes.Z) { Mins.Z = point.Z; }
            Position = ((Maxes - Mins) / 2) + Mins;
            Location newsize = Maxes - Mins;
            Location adjust = newsize / oldsize;
        }

        public VBO MyVBO = null;

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("mins", Mins.ToString()));
            vars.Add(new KeyValuePair<string, string>("maxes", Maxes.ToString()));
            return vars;
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "mins":
                    Mins = Location.FromString(value);
                    Position = ((Maxes - Mins) / 2) + Mins;
                    return true;
                case "maxes":
                    Maxes = Location.FromString(value);
                    Position = ((Maxes - Mins) / 2) + Mins;
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }
        }

        public Matrix4 RotMatrix()
        {
            Matrix4 mat = Matrix4.Identity;
            if (!Angle.IsCloseTo(Location.Zero, 0.01f))
            {
                mat *= Matrix4.CreateRotationX((float)(Angle.X * Utilities.PI180))
                    * Matrix4.CreateRotationY((float)(Angle.Y * Utilities.PI180))
                    * Matrix4.CreateRotationZ((float)(Angle.Z * Utilities.PI180));
            }
            return mat;
        }

        public override void Render(GLContext context)
        {
            if (PrimaryEditor.RenderLines)
            {
                context.Textures.White.Bind();
                context.Rendering.RenderLineBox(Mins, Maxes, RotMatrix());
            }
            else
            {
                Location HalfSize = (Maxes - Mins) / 2;
                Matrix4 mat = Matrix4.CreateScale((float)HalfSize.X, (float)HalfSize.Y, (float)HalfSize.Z) * RotMatrix() * Matrix4.CreateTranslation((Mins + HalfSize).ToOVector());
                GL.UniformMatrix4(2, false, ref mat);
                context.Rendering.SetMinimumLight(1.0f);
                MyVBO.Render(PrimaryEditor.RenderTextures);
            }
        }

        public override void Reposition(Location pos)
        {
            Maxes += pos - Position;
            Mins += pos - Position;
            base.Reposition(pos);
        }


        public override void Recalculate()
        {
            Position = ((Maxes - Mins) / 2) + Mins;
            PrimaryEditor.ContextView.Control.MakeCurrent();
            if (MyVBO != null)
            {
                MyVBO.Destroy();
            }
            MyVBO = new VBO();
            MyVBO.Prepare();
            MyVBO.Tex = PrimaryEditor.ContextView.Textures.GetTexture(Texture);
            MyVBO.AddSide(new Location(0, 0, 1), Coords[0]);
            MyVBO.AddSide(new Location(0, 0, -1), Coords[1]);
            MyVBO.AddSide(new Location(1, 0, 0), Coords[2]);
            MyVBO.AddSide(new Location(-1, 0, 0), Coords[3]);
            MyVBO.AddSide(new Location(0, 1, 0), Coords[4]);
            MyVBO.AddSide(new Location(0, -1, 0), Coords[5]);
            MyVBO.GenerateVBO();
        }

        public override string ToString()
        {
            return "CUBOIDALNTITY{mins=" + Mins + ";maxes=" + Maxes + ";mass=" + Mass
                + ";velocity=" + Velocity + ";angle=" + Angle + ";angular_velocity=" + Angular_Velocity + "}";
        }
    }
}
