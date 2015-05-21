using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKMapMaker.EntitySystem
{
    public abstract class TargetEntity : Entity
    {
        public string TargetName = "";

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("targetname", TargetName));
            return vars;
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "targetname":
                    TargetName = value;
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }
        }

        public override void Render(GLContext context)
        {
            if (PrimaryEditor.RenderEntities)
            {
                if (PrimaryEditor.RenderLines)
                {
                    context.Rendering.RenderLineBox(Position - new Location(0.5f), Position + new Location(0.5f));
                }
                else
                {
                    context.Textures.White.Bind();
                    Matrix4 mat = Matrix4.CreateTranslation((Position - new Location(0.5f)).ToOVector());
                    GL.UniformMatrix4(2, false, ref mat);
                    context.Rendering.SetMinimumLight(1.0f);
                    context.Models.Cube.Draw(0);
                }
            }
        }
    }
}
