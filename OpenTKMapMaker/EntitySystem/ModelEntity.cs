using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.GraphicsSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker.EntitySystem
{
    class ModelEntity: Entity
    {
        public string model;

        public Location scale = Location.One;

        public ModelEntity(string mod)
        {
            model = mod;
        }

        public override string GetEntityType()
        {
            return "model";
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "model":
                    model = value;
                    return true;
                case "scale":
                    scale = Location.FromString(value);
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }
        }

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("model", model));
            vars.Add(new KeyValuePair<string, string>("scale", scale.ToString()));
            return vars;
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
            Matrix4 mat = Matrix4.CreateScale(scale.ToOVector()) * RotMatrix() * Matrix4.CreateTranslation(Position.ToOVector());
            GL.UniformMatrix4(2, false, ref mat);
            context.Rendering.SetMinimumLight(0.0f);
            context.Models.GetModel(model).Draw(); // TODO: Handle more efficiently. Recalculate()?
        }

        public override Entity CreateInstance()
        {
            return new ModelEntity(model);
        }
    }
}
