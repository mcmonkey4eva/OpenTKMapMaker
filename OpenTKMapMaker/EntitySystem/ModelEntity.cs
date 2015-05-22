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
            BEPUutilities.Matrix mat = BEPUutilities.Matrix.CreateFromQuaternion(Angle);
            return new Matrix4(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23, mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }

        public override void Render(GLContext context)
        {
            Matrix4 mat = Matrix4.CreateScale(scale.ToOVector()) * RotMatrix() * Matrix4.CreateTranslation(Position.ToOVector());
            GL.UniformMatrix4(2, false, ref mat);
            context.Rendering.SetMinimumLight(0.0f);
            context.Models.GetModel(model).Draw(0); // TODO: Handle more efficiently. Recalculate()?
        }

        public override Entity CreateInstance()
        {
            return new ModelEntity(model);
        }
    }
}
