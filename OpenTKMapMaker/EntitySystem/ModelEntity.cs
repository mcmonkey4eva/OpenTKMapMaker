using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.GraphicsSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTKMapMaker.Utility;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;

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

        public ModelCollisionMode mode = ModelCollisionMode.AABB;

        public BEPUphysics.Entities.Entity mesh;

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
                case "collisionmode":
                    ModelCollisionMode newmode;
                    if (Enum.TryParse(value.ToUpper(), out newmode))
                    {
                        mode = newmode;
                    }
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
            vars.Add(new KeyValuePair<string, string>("collisionmode", mode.ToString().ToLower()));
            return vars;
        }

        public Matrix4 RotMatrix()
        {
            BEPUutilities.Matrix mat = BEPUutilities.Matrix.CreateFromQuaternion(Angle);
            return new Matrix4(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23, mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }

        string pmodel = null;

        Model internalModel;

        Matrix4 offsetmat = Matrix4.Identity;

        public void refreshModel(GLContext context)
        {
            if (model != pmodel)
            {
                pmodel = model;
                internalModel = context.Models.GetModel(model);
                if (mode == ModelCollisionMode.PRECISE)
                {
                    mesh = context.Models.Handler.MeshToBepu(internalModel.OriginalModel);
                    offset = -Location.FromBVector(mesh.Position);
                }
                else
                {
                    List<BEPUutilities.Vector3> vecs = context.Models.Handler.GetCollisionVertices(internalModel.OriginalModel);
                    Location zero = new Location(vecs[0].X, vecs[0].Y, vecs[0].Z);
                    AABB abox = new AABB() { Min = zero, Max = zero };
                    for (int v = 1; v < vecs.Count; v++)
                    {
                        abox.Include(new Location(vecs[v].X, vecs[v].Y, vecs[v].Z));
                    }
                    Location size = abox.Max - abox.Min;
                    offset = abox.Max - size / 2;
                    mesh = new BEPUphysics.Entities.Prefabs.Box(
                        new BEPUphysics.EntityStateManagement.MotionState() { Position = Position.ToBVector(), Orientation = Angle },
                        (float)size.X, (float)size.Y, (float)size.Z);
                }
                offsetmat = Matrix4.CreateTranslation(offset.ToOVector());
            }
            mesh.Position = Position.ToBVector();
            mesh.Orientation = Angle;
        }

        public Location offset;

        public override void Render(GLContext context)
        {
            refreshModel(context);
            Matrix4 mat = offsetmat * Matrix4.CreateScale(scale.ToOVector()) * RotMatrix() * Matrix4.CreateTranslation(Position.ToOVector());
            GL.UniformMatrix4(2, false, ref mat);
            context.Rendering.SetMinimumLight(0.0f);
            Model rmodel = context.Models.GetModel(model); // TODO: Handle more efficiently. Recalculate()?
            rmodel.LoadSkin(context.Textures);
            rmodel.Draw(0);
        }

        public override Entity CreateInstance()
        {
            return new ModelEntity(model);
        }
    }

    public enum ModelCollisionMode : byte
    {
        PRECISE = 1,
        AABB = 2
    }
}
