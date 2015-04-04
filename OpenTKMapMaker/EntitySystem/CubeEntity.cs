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
    public class CubeEntity : Entity
    {
        public CubeEntity(Location min, Location max)
        {
            Mins = min;
            Maxes = max;
            Position = min + (max - min) / 2;
            Mass = 0;
            Velocity = Location.Zero;
            Angle = Location.Zero;
            Angular_Velocity = Location.Zero;
        }

        public Location Mins;
        public Location Maxes;
        public string[] Textures = new string[] { "TOP", "BOTTOM", "XP", "XM", "YP", "YM" };

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("mins", Mins.ToString()));
            vars.Add(new KeyValuePair<string, string>("maxes", Mins.ToString()));
            vars.Add(new KeyValuePair<string, string>("textures", GetTextureString()));
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
                case "mins":
                    Mins = Location.FromString(value);
                    Position = ((Maxes - Mins) / 2) + Mins;
                    return true;
                case "maxes":
                    Maxes = Location.FromString(value);
                    Position = ((Maxes - Mins) / 2) + Mins;
                    return true;
                case "textures":
                    string[] texes = value.Split('|');
                    if (texes.Length != 6)
                    {
                        return false;
                    }
                    Textures = texes;
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }
        }

        public string GetTextureString()
        {
            return Textures[0] + "|" + Textures[1] + "|" + Textures[2] + "|" + Textures[3] + "|" + Textures[4] + "|" + Textures[5];
        }

        public override void Render(GLContext context)
        {
            context.Textures.White.Bind();
            if (PrimaryEditor.RenderLines)
            {
                context.Rendering.RenderLineBox(Mins, Maxes);
            }
            else
            {
                Matrix4 mat = Matrix4.CreateScale((Maxes - Mins).ToOVector()) * Matrix4.CreateTranslation(Mins.ToOVector());
                GL.UniformMatrix4(2, false, ref mat);
                context.Models.Cube.Draw();
            }
        }

        public override void Reposition(Location pos)
        {
            Maxes += pos - Position;
            Mins += pos - Position;
            base.Reposition(pos);
        }

        public override string ToString()
        {
            return "CUBENTITY{mins=" + Mins + ";maxes=" + Maxes + ";textures=" + GetTextureString() + ";mass=" + Mass + ";velocity=" + Velocity + ";angle=" + Angle + ";angular_velocity=" + Angular_Velocity + "}";
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
