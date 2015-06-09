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

        public Model Cylinder;

        /// <summary>
        /// Prepares the model system.
        /// </summary>
        public void Init(AnimationEngine engine)
        {
            AnimEngine = engine;
            Handler = new ModelHandler();
            LoadedModels = new List<Model>();
            Cube = FromBytes("cube", FileHandler.encoding.GetBytes(ModelHandler.CubeData));
            Cylinder = FromBytes("cylinder", FileHandler.encoding.GetBytes(ModelHandler.CylinderData));
            LoadedModels.Add(Cube);
            LoadedModels.Add(Cylinder);
        }

        public Model LoadModel(string filename)
        {
            try
            {
                filename = FileHandler.CleanFileName(filename);
                if (!Program.Files.Exists("models/" + filename))
                {
                    SysConsole.Output(OutputType.WARNING, "Cannot load model, file '" +
                        TextStyle.Color_Standout + "models/" + filename + TextStyle.Color_Warning +
                        "' does not exist.");
                    return null;
                }
                return FromBytes(filename, Program.Files.ReadBytes("models/" + filename));
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
            Model Loaded = null;
            try
            {
                Loaded = LoadModel(modelname);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, ex.ToString());
            }
            if (Loaded == null)
            {
                Loaded = FromBytes("model.obj", FileHandler.encoding.GetBytes(ModelHandler.CubeData));
                Loaded.Name = modelname;
            }
            LoadedModels.Add(Loaded);
            return Loaded;
        }

        public AnimationEngine AnimEngine;

        /// <summary>
        /// loads a model from a file byte array.
        /// </summary>
        /// <param name="name">The name of the model</param>
        /// <param name="data">The .obj file string</param>
        /// <returns>A valid model</returns>
        public Model FromBytes(string name, byte[] data)
        {
            Scene scene = Handler.LoadModel(data, name.Substring(name.LastIndexOf('.') + 1));
            return FromScene(scene, name, AnimEngine);
        }

        public Model FromScene(Scene scene, string name, AnimationEngine engine)
        {
            if (!scene.HasMeshes)
            {
                throw new Exception("Scene has no meshes!");
            }
            Model model = new Model(name);
            model.OriginalModel = scene;
            model.Root = convert(scene.RootNode.Transform);
            foreach (Mesh mesh in scene.Meshes)
            {
                if (mesh.Name.ToLower().Contains("collision"))
                {
                    continue;
                }
                ModelMesh modmesh = new ModelMesh(mesh.Name, mesh);
                modmesh.Base = scene;
                modmesh.vbo.Prepare();
                bool hastc = mesh.HasTextureCoords(0);
                bool hasn = mesh.HasNormals;
                if (!hasn)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has no normals!");
                }
                if (!hastc)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has no texcoords!");
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
                        modmesh.vbo.TexCoords.Add(new Vector3(texCoord.X, 1 - texCoord.Y, texCoord.Z));
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
                        SysConsole.Output(OutputType.WARNING, "Mesh has face with " + face.Indices.Count + " vertices!");
                    }
                }
                int bc = mesh.Bones.Count;
                if (bc > 70)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has " + bc + " bones!");
                    bc = 70;
                }
                modmesh.vbo.BoneIDs = new Vector4[modmesh.vbo.Vertices.Count].ToList();
                modmesh.vbo.BoneWeights = new Vector4[modmesh.vbo.Vertices.Count].ToList();
                int[] pos = new int[modmesh.vbo.Vertices.Count];
                for (int i = 0; i < bc; i++)
                {
                    for (int x = 0; x < mesh.Bones[i].VertexWeights.Count; x++)
                    {
                        VertexWeight vw = mesh.Bones[i].VertexWeights[x];
                        int spot = pos[vw.VertexID]++;
                        if (spot > 3)
                        {
                            //SysConsole.Output(OutputType.WARNING, "Too many bones influencing " + vw.VertexID + "!");
                            ForceSet(modmesh.vbo.BoneWeights, vw.VertexID, 3, modmesh.vbo.BoneWeights[vw.VertexID][3] + vw.Weight);
                        }
                        else
                        {
                            ForceSet(modmesh.vbo.BoneIDs, vw.VertexID, spot, i);
                            ForceSet(modmesh.vbo.BoneWeights, vw.VertexID, spot, vw.Weight);
                        }
                    }
                }
                model.Meshes.Add(modmesh);
                modmesh.GenerateVBO();
            }
            model.RootNode = new ModelNode() { Internal = scene.RootNode, Parent = null, Name = scene.RootNode.Name.ToLower() };
            List<ModelNode> allNodes = new List<ModelNode>();
            PopulateChildren(model.RootNode, scene, model, engine, allNodes);
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                for (int x = 0; x < model.Meshes[i].Original.Bones.Count; x++)
                {
                    ModelNode nodet = null;
                    string nl = model.Meshes[i].Original.Bones[x].Name.ToLower();
                    for (int n = 0; n < allNodes.Count; n++)
                    {
                        if (allNodes[n].Name == nl)
                        {
                            nodet = allNodes[n];
                            break;
                        }
                    }
                    ModelBone mb = new ModelBone() { Internal = model.Meshes[i].Original.Bones[x], Offset = convert(model.Meshes[i].Original.Bones[x].OffsetMatrix) };
                    nodet.Bones.Add(mb);
                    model.Meshes[i].Bones.Add(mb);
                }
            }
            return model;
        }

        void PopulateChildren(ModelNode node, Scene original, Model model, AnimationEngine engine, List<ModelNode> allNodes)
        {
            allNodes.Add(node);
            if (engine.HeadBones.Contains(node.Name))
            {
                node.Mode = 0;
            }
            else if (engine.LegBones.Contains(node.Name))
            {
                node.Mode = 2;
            }
            else
            {
                node.Mode = 1;
            }
            for (int i = 0; i < node.Internal.Children.Count; i++)
            {
                ModelNode child = new ModelNode() { Internal = node.Internal.Children[i], Parent = node, Name = node.Internal.Children[i].Name.ToLower() };
                PopulateChildren(child, original, model, engine, allNodes);
                node.Children.Add(child);
            }
        }

        void ForceSet(List<Vector4> vecs, int ind, int subind, float val)
        {
            Vector4 vec = vecs[ind];
            vec[subind] = val;
            vecs[ind] = vec;
        }

        Matrix4 convert(Matrix4x4 mat)
        {
            return new Matrix4(mat.A1, mat.A2, mat.A3, mat.A4,
                mat.B1, mat.B2, mat.B3, mat.B4,
                mat.C1, mat.C2, mat.C3, mat.C4,
                mat.D1, mat.D2, mat.D3, mat.D4);
            /*return new Matrix4(mat.A1, mat.B1, mat.C1, mat.D1,
                mat.A2, mat.B2, mat.C2, mat.D2,
                mat.A3, mat.B3, mat.C3, mat.D3,
                mat.A4, mat.C4, mat.C4, mat.D4);*/
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

        public Matrix4 Root;

        /// <summary>
        /// The name of  this model.
        /// </summary>
        public string Name;

        /// <summary>
        /// All the meshes this model has.
        /// </summary>
        public List<ModelMesh> Meshes;

        public ModelNode RootNode;

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

        void SetBones(Matrix4[] mats)
        {
            int bones = 70;
            float[] set = new float[bones * 16];
            for (int i = 0; i < mats.Length; i++)
            {
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        set[i * 16 + x * 4 + y] = mats[i][x, y];
                    }
                }
            }
            for (int i = mats.Length; i < bones; i++)
            {
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        set[i * 16 + x * 4 + y] = Matrix4.Identity[x, y];
                    }
                }
            }
            GL.UniformMatrix4(7, bones, false, set);
        }

        Matrix4 convert(Matrix4x4 mat)
        {
            return new Matrix4(mat.A1, mat.A2, mat.A3, mat.A4,
                mat.B1, mat.B2, mat.B3, mat.B4,
                mat.C1, mat.C2, mat.C3, mat.C4,
                mat.D1, mat.D2, mat.D3, mat.D4);
        }

        Matrix4 globalInverse = Matrix4.Identity;

        public void UpdateTransforms(ModelNode pNode, Matrix4 transf)
        {
            string nodename = pNode.Name;
            Matrix4 nodeTransf = Matrix4.Identity;
            double time;
            SingleAnimationNode pNodeAnim = FindNodeAnim(nodename, pNode.Mode, out time);
            if (pNodeAnim != null)
            {
                BEPUutilities.Vector3 vec = pNodeAnim.lerpPos(time);
                BEPUutilities.Quaternion quat = pNodeAnim.lerpRotate(time);
                OpenTK.Quaternion oquat = new OpenTK.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
                Matrix4 trans;
                Matrix4.CreateTranslation(vec.X, vec.Y, vec.Z, out trans);
                trans.Transpose();
                Matrix4 rot;
                Matrix4.CreateFromQuaternion(ref oquat, out rot);
                rot.Transpose();
                Matrix4.Mult(ref trans, ref rot, out nodeTransf);
            }
            Matrix4 global;
            Matrix4.Mult(ref transf, ref nodeTransf, out global);
            for (int i = 0; i < pNode.Bones.Count; i++)
            {
                //Matrix4 modded;
                //Matrix4.Mult(ref globalInverse, ref global, out modded);
                Matrix4.Mult(ref global, ref pNode.Bones[i].Offset, out pNode.Bones[i].Transform);
            }
            for (int i = 0; i < pNode.Children.Count; i++)
            {
                UpdateTransforms(pNode.Children[i], global);
            }
        }

        AnimationEngine Engine = null;

        SingleAnimationNode FindNodeAnim(string nodeName, int mode, out double time)
        {
            SingleAnimation nodes;
            if (mode == 0)
            {
                nodes = hAnim;
                time = aTHead;
            }
            else if (mode == 1)
            {
                nodes = tAnim;
                time = aTTorso;
            }
            else
            {
                nodes = lAnim;
                time = aTLegs;
            }
            if (nodes == null)
            {
                return null;
            }
            return nodes.GetNode(nodeName);
        }

        SingleAnimation hAnim;
        SingleAnimation tAnim;
        SingleAnimation lAnim;
        double aTHead;
        double aTTorso;
        double aTLegs;

        /// <summary>
        /// Draws the model.
        /// </summary>
        public void Draw(double aTimeHead = 0, SingleAnimation headanim = null, double aTimeTorso = 0, SingleAnimation torsoanim = null, double aTimeLegs = 0, SingleAnimation legsanim = null)
        {
            hAnim = headanim;
            tAnim = torsoanim;
            lAnim = legsanim;
            bool any = hAnim != null || tAnim != null || lAnim != null;
            if (any)
            {
                if (hAnim != null)
                {
                    Engine = hAnim.Engine;
                }
                else if (tAnim != null)
                {
                    Engine = tAnim.Engine;
                }
                else
                {
                    Engine = lAnim.Engine;
                }
                globalInverse = Root.Inverted();
                aTHead = aTimeHead;
                aTTorso = aTimeTorso;
                aTLegs = aTimeLegs;
                UpdateTransforms(RootNode, Matrix4.Identity);
            }
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (Meshes[i].Bones.Count > 0)
                {
                    Matrix4[] mats = new Matrix4[Meshes[i].Bones.Count];
                    for (int x = 0; x < Meshes[i].Bones.Count; x++)
                    {
                        mats[x] = Meshes[i].Bones[x].Transform;
                    }
                    SetBones(mats);
                }
                Meshes[i].Draw();
            }
            if (any)
            {
                VBO.BonesIdentity();
            }
        }

        public bool Skinned = false;

        public void LoadSkin(TextureEngine texs)
        {
            if (Skinned)
            {
                return;
            }
            Skinned = true;
            if (Program.Files.Exists("models/" + Name + ".skin"))
            {
                string[] data = Program.Files.ReadText("models/" + Name + ".skin").Split('\n');
                foreach (string datum in data)
                {
                    if (datum.Length > 0)
                    {
                        string[] datums = datum.Split('=');
                        if (datums.Length == 2)
                        {
                            Texture tex = texs.GetTexture(datums[1]);
                            bool success = false;
                            for (int i = 0; i < Meshes.Count; i++)
                            {
                                if (Meshes[i].Name == datums[0])
                                {
                                    Meshes[i].vbo.Tex = tex;
                                    success = true;
                                }
                            }
                            if (!success)
                            {
                                SysConsole.Output(OutputType.WARNING, "Unknown skin entry " + datums[0]);
                                StringBuilder all = new StringBuilder(Meshes.Count * 100);
                                for (int i = 0; i < Meshes.Count; i++)
                                {
                                    all.Append(Meshes[i].Name + ", ");
                                }
                                SysConsole.Output(OutputType.WARNING, "Available: " + all.ToString());
                            }
                        }
                    }
                }
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "Can't find models/" + Name + ".skin!");
            }
        }
    }

    public class ModelBone
    {
        public Bone Internal = null;
        public Matrix4 Transform = Matrix4.Identity;
        public Matrix4 Offset;
    }

    public class ModelNode
    {
        public Node Internal = null;
        public ModelNode Parent = null;
        public List<ModelNode> Children = new List<ModelNode>();
        public List<ModelBone> Bones = new List<ModelBone>();
        public byte Mode;
        public string Name;
    }

    public class ModelMesh
    {
        /// <summary>
        /// The name of this mesh.
        /// </summary>
        public string Name;

        public Scene Base;

        public Mesh Original;

        public List<ModelBone> Bones = new List<ModelBone>();

        public ModelMesh(string _name, Mesh orig)
        {
            Original = orig;
            Name = _name.ToLower();
            if (Name.EndsWith(".001"))
            {
                Name = Name.Substring(0, Name.Length - ".001".Length);
            }
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
            vbo.Render(true);
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
