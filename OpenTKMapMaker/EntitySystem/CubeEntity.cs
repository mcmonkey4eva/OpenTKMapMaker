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
    public class CubeEntity : CuboidalEntity
    {
        public CubeEntity(Location min, Location max)
            : base(min, max, "")
        {
            Mins = min;
            Maxes = max;
            Position = min + (max - min) / 2;
            Mass = 0;
            Velocity = Location.Zero;
            Angle = Location.Zero;
            Angular_Velocity = Location.Zero;
            float f1 = (float)Utilities.UtilRandom.NextDouble();
            ViewColor = new Color4(f1, 0f, 1 - f1, 1f);
        }

        public string[] Textures = new string[] { "top", "bottom", "xp", "xm", "yp", "ym" };

        public override void Include(Location point)
        {
            Location pmax = Maxes;
            Location pmin = Mins;
            Location oldsize = pmax - pmin;
            base.Include(point);
            Location newsize = Maxes - Mins;
            Location adjust = newsize / oldsize;
            if (PrimaryEditor.autoStretch)
            {
                Coords[0].xscale *= (float)adjust.X;
                Coords[0].yscale *= (float)adjust.Y;
                Coords[1].xscale *= (float)adjust.X;
                Coords[1].yscale *= (float)adjust.Y;
                Coords[2].xscale *= (float)adjust.Y;
                Coords[2].yscale *= (float)adjust.Z;
                Coords[3].xscale *= (float)adjust.Y;
                Coords[3].yscale *= (float)adjust.Z;
                Coords[4].xscale *= (float)adjust.X;
                Coords[4].yscale *= (float)adjust.Z;
                Coords[5].xscale *= (float)adjust.X;
                Coords[5].yscale *= (float)adjust.Z;
                Recalculate();
            }
        }

        public List<VBO> VBOs = new List<VBO>();

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("textures", GetTextureString()));
            vars.Add(new KeyValuePair<string,string>("coords", GetCoordString()));
            return vars;
        }

        public override string GetEntityType()
        {
            return "cube";
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "textures":
                    string[] texes = value.Split('|');
                    if (texes.Length != 6)
                    {
                        return false;
                    }
                    Textures = texes;
                    return true;
                case "coords":
                    string[] coords = value.Split('|');
                    if (coords.Length != 6)
                    {
                        return false;
                    }
                    Coords = new TextureCoordinates[] { TextureCoordinates.FromString(coords[0]), TextureCoordinates.FromString(coords[1]),
                        TextureCoordinates.FromString(coords[2]), TextureCoordinates.FromString(coords[3]),
                        TextureCoordinates.FromString(coords[4]), TextureCoordinates.FromString(coords[5]) };
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }
        }

        public string GetTextureString()
        {
            return Textures[0] + "|" + Textures[1] + "|" + Textures[2] + "|" + Textures[3] + "|" + Textures[4] + "|" + Textures[5];
        }

        public string GetCoordString()
        {
            return Coords[0] + "|" + Coords[1] + "|" + Coords[2] + "|" + Coords[3] + "|" + Coords[4] + "|" + Coords[5];
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
                context.Rendering.SetMinimumLight(0.0f);
                for (int i = 0; i < VBOs.Count; i++)
                {
                    VBOs[i].Render(PrimaryEditor.RenderTextures);
                }
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
            for (int i = 0; i < VBOs.Count; i++)
            {
                VBOs[i].Destroy();
            }
            VBOs.Clear();
            GetVBOFor(PrimaryEditor.ContextView.Textures.GetTexture(Textures[0])).AddSide(new Location(0, 0, 1), Coords[0]);
            GetVBOFor(PrimaryEditor.ContextView.Textures.GetTexture(Textures[1])).AddSide(new Location(0, 0, -1), Coords[1]);
            GetVBOFor(PrimaryEditor.ContextView.Textures.GetTexture(Textures[2])).AddSide(new Location(1, 0, 0), Coords[2]);
            GetVBOFor(PrimaryEditor.ContextView.Textures.GetTexture(Textures[3])).AddSide(new Location(-1, 0, 0), Coords[3]);
            GetVBOFor(PrimaryEditor.ContextView.Textures.GetTexture(Textures[4])).AddSide(new Location(0, 1, 0), Coords[4]);
            GetVBOFor(PrimaryEditor.ContextView.Textures.GetTexture(Textures[5])).AddSide(new Location(0, -1, 0), Coords[5]);
            for (int i = 0; i < VBOs.Count; i++)
            {
                VBOs[i].GenerateVBO();
            }
        }

        VBO GetVBOFor(Texture tex)
        {
            for (int i = 0; i < VBOs.Count; i++)
            {
                if (VBOs[i].Tex.Original_InternalID == tex.Original_InternalID)
                {
                    return VBOs[i];
                }
            }
            VBO vbo = new VBO();
            vbo.Tex = tex;
            vbo.Prepare();
            VBOs.Add(vbo);
            return vbo;
        }

        public override string ToString()
        {
            return "CUBENTITY{mins=" + Mins + ";maxes=" + Maxes + ";textures=" + GetTextureString() + ";coords=" + GetCoordString() + ";mass=" + Mass
                + ";velocity=" + Velocity + ";angle=" + Angle + ";angular_velocity=" + Angular_Velocity + "}";
        }

        public override Entity CreateInstance()
        {
            return new CubeEntity(Mins, Maxes);
        }
    }

    public enum CubeTexturePos
    {
        TOP = 0,
        BOTTOM = 1,
        XP = 2,
        XM = 3,
        YP = 4,
        YM = 5
    }
}
