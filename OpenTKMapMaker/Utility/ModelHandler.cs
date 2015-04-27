using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assimp;
using Assimp.Configs;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;

namespace OpenTKMapMaker.Utility
{
    public class ModelHandler
    {
        public Scene LoadModel(byte[] data, string ext)
        {
            if (ext == null || ext == "")
            {
                ext = "obj";
            }
            using (AssimpContext ACont = new AssimpContext())
            {
                ACont.SetConfig(new NormalSmoothingAngleConfig(66f));
                return ACont.ImportFileFromStream(new DataStream(data), PostProcessSteps.Triangulate | PostProcessSteps.SortByPrimitiveType, ext);
            }
        }

        public MobileMesh MeshToBepu(Scene input)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            foreach (Mesh mesh in input.Meshes)
            {
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    Vector3D vert = mesh.Vertices[i];
                    vertices.Add(new Vector3(vert.X, vert.Y, vert.Z));
                }
                foreach (Face face in mesh.Faces)
                {
                    if (face.Indices.Count == 3)
                    {
                        for (int i = 2; i >= 0; i--)
                        {
                            indices.Add(face.Indices[i]);
                        }
                    }
                    else
                    {
                        SysConsole.Output(OutputType.WARNING, "Mesh has face with " + face.Indices.Count + " faces!!");
                    }
                }
            }
            SysConsole.Output(OutputType.INFO, "Sweeping against a model with " + vertices.Count + " verts and " + indices.Count + " indices!");
            return new MobileMesh(vertices.ToArray(), indices.ToArray(), AffineTransform.Identity, MobileMeshSolidity.DoubleSided);
        }
    }
}
