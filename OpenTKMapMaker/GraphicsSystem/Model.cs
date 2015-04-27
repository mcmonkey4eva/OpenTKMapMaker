using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTKMapMaker.Utility;
using Assimp;

namespace OpenTKMapMaker.GraphicsSystem
{
    public class ModelEngine
    {
        /// <summary>
        /// All currently loaded models.
        /// </summary>
        public List<Model> LoadedModels;

        public ModelHandler Handler;

        public Model Cube;

        /// <summary>
        /// Prepares the model system.
        /// </summary>
        public void Init()
        {
            Handler = new ModelHandler();
            LoadedModels = new List<Model>();
            Cube = FromBytes("cube", FileHandler.encoding.GetBytes(CubeData));
            LoadedModels.Add(Cube);
        }

        static string CubeData = "o Cube\nv 1.000000 0.000000 0.000000\nv 1.000000 0.000000 1.000000\nv 0.000000 0.000000 1.000000\n" +
            "v 0.000000 0.000000 0.000000\nv 1.000000 1.000000 0.000000\nv 1.000000 1.000000 1.000000\nv 0.000000 1.000000 1.000000\n" +
            "v 0.000000 1.000000 0.000000\nvt 1.000000 0.000000\nvt 1.000000 1.000000\nvt 0.000000 1.000000\nvt 0.000000 0.000000\n" +
            "f 2/1 3/2 4/3\nf 8/1 7/2 6/3\nf 1/4 5/1 6/2\nf 2/4 6/1 7/2\nf 7/1 8/2 4/3\nf 1/1 4/2 8/3\nf 1/4 2/1 4/3\nf 5/4 8/1 6/3\n" +
            "f 2/3 1/4 6/2\nf 3/3 2/4 7/2\nf 3/4 7/1 4/3\nf 5/4 1/1 8/3\n"; // TODO: Normals!

        public Model LoadModel(string filename)
        {
            try
            {
                filename = FileHandler.CleanFileName(filename);
                if (!FileHandler.Exists("models/" + filename))
                {
                    SysConsole.Output(OutputType.WARNING, "Cannot load model, file '" +
                        TextStyle.Color_Standout + "models/" + filename + TextStyle.Color_Warning +
                        "' does not exist.");
                    return null;
                }
                return FromBytes(filename, FileHandler.ReadBytes("models/" + filename));
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to load model from filename '" +
                    TextStyle.Color_Standout + "models/" + filename + TextStyle.Color_Error + "'" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gets the texture object for a specific texture name.
        /// </summary>
        /// <param name="texturename">The name of the texture</param>
        /// <returns>A valid texture object</returns>
        public Model GetModel(string modelname)
        {
            modelname = FileHandler.CleanFileName(modelname);
            for (int i = 0; i < LoadedModels.Count; i++)
            {
                if (LoadedModels[i].Name == modelname)
                {
                    return LoadedModels[i];
                }
            }
            Model Loaded = LoadModel(modelname);
            if (Loaded == null)
            {
                Loaded = FromBytes(modelname, FileHandler.encoding.GetBytes(CubeData));
            }
            LoadedModels.Add(Loaded);
            return Loaded;
        }

        /// <summary>
        /// loads a model from a file byte array.
        /// </summary>
        /// <param name="name">The name of the model</param>
        /// <param name="data">The .obj file string</param>
        /// <returns>A valid model</returns>
        public Model FromBytes(string name, byte[] data)
        {
            Scene scene = Handler.LoadModel(data, name.Substring(name.LastIndexOf('.') + 1));
            if (!scene.HasMeshes)
            {
                throw new Exception("Scene has no meshes!");
            }
            Model model = new Model(name);
            model.OriginalModel = scene;
            foreach (Mesh mesh in scene.Meshes)
            {
                ModelMesh modmesh = new ModelMesh(mesh.Name);
                modmesh.vbo.Prepare();
                bool hastc = mesh.HasTextureCoords(0);
                bool hasn = mesh.HasNormals;
                if (!hasn)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has no normals!");
                }
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    Vector3D vertex = mesh.Vertices[i];
                    modmesh.vbo.Vertices.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));
                    if (!hastc)
                    {
                        modmesh.vbo.TexCoords.Add(new Vector3(0, 0, 0));
                    }
                    else
                    {
                        Vector3D texCoord = mesh.TextureCoordinateChannels[0][i];
                        modmesh.vbo.TexCoords.Add(new Vector3(texCoord.X, texCoord.Y, texCoord.Z));
                    }
                    if (!hasn)
                    {
                        modmesh.vbo.Normals.Add(new Vector3(0, 0, 1));
                    }
                    else
                    {
                        modmesh.vbo.Normals.Add(new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z));
                    }
                    modmesh.vbo.Colors.Add(new Vector4(1, 1, 1, 1)); // TODO: From the mesh?
                }
                foreach (Face face in mesh.Faces)
                {
                    if (face.Indices.Count == 3)
                    {
                        for (int i = 2; i >= 0; i--)
                        {
                            modmesh.vbo.Indices.Add((uint)face.Indices[i]);
                        }
                    }
                    else
                    {
                        SysConsole.Output(OutputType.WARNING, "Mesh has face with " + face.Indices.Count + " faces!!");
                    }
                }
                model.Meshes.Add(modmesh);
                modmesh.GenerateVBO();
            }
            return model;
        }
    }

    /// <summary>
    /// Represents a 3D model.
    /// </summary>
    public class Model
    {
        public Scene OriginalModel;

        public Model(string _name)
        {
            Name = _name;
            Meshes = new List<ModelMesh>();
        }

        /// <summary>
        /// The name of  this model.
        /// </summary>
        public string Name;

        /// <summary>
        /// All the meshes this model has.
        /// </summary>
        public List<ModelMesh> Meshes;

        public ModelMesh MeshFor(string name)
        {
            name = name.ToLower();
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (Meshes[i].Name.StartsWith(name))
                {
                    return Meshes[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Draws the model.
        /// </summary>
        public void Draw()
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].Draw();
            }
        }
    }

    public class ModelMesh
    {
        /// <summary>
        /// The name of this mesh.
        /// </summary>
        public string Name;

        public ModelMesh(string _name)
        {
            Name = _name.ToLower();
            Faces = new List<ModelFace>();
            vbo = new VBO();
        }

        /// <summary>
        /// All the mesh's faces.
        /// </summary>
        public List<ModelFace> Faces;

        /// <summary>
        /// The VBO for this mesh.
        /// </summary>
        public VBO vbo;

        public void DestroyVBO()
        {
            vbo.Destroy();
        }

        public void GenerateVBO()
        {
            vbo.GenerateVBO();
        }

        /// <summary>
        /// Renders the mesh.
        /// </summary>
        public void Draw()
        {
            vbo.Render(false);
        }
    }

    public class ModelFace
    {
        public ModelFace(int _l1, int _l2, int _l3, int _t1, int _t2, int _t3, Location _normal)
        {
            L1 = _l1;
            L2 = _l2;
            L3 = _l3;
            T1 = _t1;
            T2 = _t2;
            T3 = _t3;
            Normal = _normal;
        }

        public Location Normal;

        public int L1;
        public int L2;
        public int L3;

        public int T1;
        public int T2;
        public int T3;
    }
}
