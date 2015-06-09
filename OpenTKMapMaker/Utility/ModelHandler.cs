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
        public static string CubeData = "o Cube_Cube.001\nv 0.000000 0.000000 0.000000\nv -1.000000 0.000000 0.000000\n" +
            "v -1.000000 1.000000 0.000000\nv 0.000000 1.000000 0.000000\nv 0.000000 0.000000 1.000000\nv -1.000000 0.000000 1.000000\n" +
			"v -1.000000 1.000000 1.000000\nv 0.000000 1.000000 1.000000\nvt 0.333333 0.000000\nvt 0.666667 0.000000\nvt 0.666667 0.333333\nvt 0.000000 0.333333\nvt 0.333333 0.333333\nvt 0.333333 0.666667\n" +
			"vt 0.000000 0.000000\nvt 0.666667 0.666667\nvt 1.000000 0.333333\nvt 0.333333 1.000000\nvt 0.000000 1.000000\nvt 0.000000 0.666667\nvt 1.000000 0.000000\nvn 0.000000 -1.000000 0.000000\n" +
			"vn -1.000000 0.000000 0.000000\nvn 0.000000 1.000000 0.000000\nvn 1.000000 0.000000 0.000000\nvn 0.000000 0.000000 -1.000000\nvn 0.000000 0.000000 1.000000\nusemtl None\ns off\n" +
            "f 6/1/1 2/2/1 1/3/1\nf 7/4/2 3/5/2 2/6/2\nf 8/7/3 4/1/3 3/5/3\nf 5/3/4 1/8/4 4/6/4\nf 2/9/5 3/3/5 4/2/5\nf 7/6/6 6/10/6 5/11/6\nf 5/5/1 6/1/1 1/3/1\nf 6/12/2 7/4/2 2/6/2\n" +
			"f 7/4/3 8/7/3 3/5/3\nf 8/5/4 5/3/4 4/6/4\nf 1/13/5 2/9/5 4/2/5\nf 8/12/6 7/6/6 5/11/6\n";

        public static string CylinderData = "o Cylinder\nv -1.000000 0.000000 0.000000\nv -1.000000 0.000000 1.000000\nv -0.707107 0.707107 0.000000\nv -0.707107 0.707107 1.000000\nv 0.000000 1.000000 0.000000\n" +
            "v 0.000000 1.000000 1.000000\nv 0.707107 0.707107 0.000000\nv 0.707107 0.707107 1.000000\nv 1.000000 -0.000000 0.000000\nv 1.000000 -0.000000 1.000000\n" +
            "v 0.707107 -0.707107 0.000000\nv 0.707107 -0.707107 1.000000\nv -0.000000 -1.000000 0.000000\nv -0.000000 -1.000000 1.000000\nv -0.707107 -0.707107 0.000000\n" +
            "v -0.707107 -0.707107 1.000000\nvt 1.000000 0.000000\nvt 1.000000 1.000000\nvt 0.000000 1.000000\nvt 0.500000 1.000000\nvt 1.000000 0.500000\nvt 0.853553 0.146447\n" +
            "vt 0.500000 0.000000\nvt 0.000000 0.000000\nvt 0.000000 0.500000\nvt 0.146447 0.853553\nvt 0.146447 0.146447\nvt 0.853553 0.853553\nvn -0.923900 0.382700 0.000000\n" +
            "vn -0.382700 0.923900 0.000000\nvn 0.382700 0.923900 0.000000\nvn 0.923900 0.382700 0.000000\nvn 0.923900 -0.382700 0.000000\nvn 0.382700 -0.923900 0.000000\n" +
            "vn -0.000000 0.000000 1.000000\nvn -0.923900 -0.382700 0.000000\nvn -0.382700 -0.923900 0.000000\nvn 0.000000 0.000000 -1.000000\nusemtl None\ns off\n" +
            "f 2/1/1 4/2/1 3/3/1\nf 4/1/2 6/2/2 5/3/2\nf 6/1/3 8/2/3 7/3/3\nf 8/1/4 10/2/4 9/3/4\nf 10/1/5 12/2/5 11/3/5\nf 12/1/6 14/2/6 13/3/6\nf 4/4/7 16/5/7 14/6/7\n" +
            "f 16/1/8 2/2/8 1/3/8\nf 14/1/9 16/2/9 15/3/9\nf 1/4/10 5/5/10 9/7/10\nf 1/8/1 2/1/1 3/3/1\nf 3/8/2 4/1/2 5/3/2\nf 5/8/3 6/1/3 7/3/3\nf 7/8/4 8/1/4 9/3/4\n" +
            "f 9/8/5 10/1/5 11/3/5\nf 11/8/6 12/1/6 13/3/6\nf 8/9/7 6/10/7 4/4/7\nf 12/7/7 10/11/7 8/9/7\nf 8/9/7 14/6/7 12/7/7\nf 4/4/7 2/12/7 16/5/7\nf 14/6/7 8/9/7 4/4/7\n" +
            "f 15/8/8 16/1/8 1/3/8\nf 13/8/9 14/1/9 15/3/9\nf 13/9/10 15/10/10 1/4/10\nf 1/4/10 11/11/10 13/9/10\nf 5/5/10 7/6/10 9/7/10\nf 1/4/10 3/12/10 5/5/10\nf 9/7/10 11/11/10 1/4/10\n";

        public Scene LoadModel(byte[] data, string ext)
        {
            if (ext == null || ext == "")
            {
                ext = "obj";
            }
            using (AssimpContext ACont = new AssimpContext())
            {
                ACont.SetConfig(new NormalSmoothingAngleConfig(66f));
                return ACont.ImportFileFromStream(new DataStream(data), PostProcessSteps.Triangulate, ext);
            }
        }

        public List<Vector3> GetCollisionVertices(Scene input)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            bool colOnly = false;
            foreach (Mesh mesh in input.Meshes)
            {
                if (mesh.Name.ToLower().Contains("collision"))
                {
                    colOnly = true;
                    break;
                }
            }
            foreach (Mesh mesh in input.Meshes)
            {
                if (!colOnly || mesh.Name.ToLower().Contains("collision"))
                {
                    AddMesh(mesh, vertices, indices);
                }
            }
            return vertices;
        }

        public MobileMesh MeshToBepu(Scene input)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            bool colOnly = false;
            foreach (Mesh mesh in input.Meshes)
            {
                if (mesh.Name.ToLower().Contains("collision"))
                {
                    colOnly = true;
                    break;
                }
            }
            foreach (Mesh mesh in input.Meshes)
            {
                if (!colOnly || mesh.Name.ToLower().Contains("collision"))
                {
                    AddMesh(mesh, vertices, indices);
                }
            }
            return new MobileMesh(vertices.ToArray(), indices.ToArray(), AffineTransform.Identity, MobileMeshSolidity.DoubleSided);
        }

        void AddMesh(Mesh mesh, List<Vector3> vertices, List<int> indices)
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
    }
}
