using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUutilities;

namespace OpenTKMapMaker.Utility
{
    public class AnimationEngine
    {
        public AnimationEngine()
        {
            Animations = new Dictionary<string, SingleAnimation>();
            /*
            string[] TBones = new string[] { "hips", "spine", "chest", "chest1", "shoulder.l", "upper_arm.l", "forearm.l", "hand.l",
                "thumb.01.l", "thumb.02.l", "thumb.03.l", "f_index.01.l", "f_index.02.l", "f_index.03.l", "f_middle.01.l", "f_middle.02.l", "f_middle.03.l",
            "f_pinky.01.l", "f_pinky.02.l", "f_pinky.03.l", "f_ring.01.l", "f_ring.02.l", "f_ringy.03.l",
            "shoulder.r", "upper_arm.r", "forearm.r", "hand.r", "thumb.01.r", "thumb.02.r", "thumb.03.r", "f_index.01.r", "f_index.02.r", "f_index.03.r",
            "f_middle.01.r", "f_middle.02.r", "f_middle.03.r", "f_pinky.01.r", "f_pinky.02.r", "f_pinky.03.r", "f_ring.01.r", "f_ring.02.r", "f_ringy.03.r" };*/
            string[] HBones = new string[] { "neck", "head", "jaw", "tongue_base", "tongue_mod", "tongue_tip", "lolid.l", "lolid.r", "uplid.l", "uplid.r", "eye.l", "eye.r" };
            string[] LBones = new string[] { "thigh.l", "shin.l", "foot.l", "toe.l", "thigh.r", "shin.r", "foot.r", "toe.r" };
            foreach (string str in HBones)
            {
                HeadBones.Add(str);
            }
            foreach (string str in LBones)
            {
                LegBones.Add(str);
            }
        }

        public HashSet<string> HeadBones = new HashSet<string>();
        //public HashSet<string> TorsoBones = new HashSet<string>();
        public HashSet<string> LegBones = new HashSet<string>();

        public Dictionary<string, SingleAnimation> Animations;

        public SingleAnimation GetAnimation(string name)
        {
            string namelow = name.ToLower();
            SingleAnimation sa;
            if (Animations.TryGetValue(namelow, out sa))
            {
                return sa;
            }
            try
            {
                sa = LoadAnimation(namelow);
                Animations.Add(sa.Name, sa);
                return sa;
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Loading an animation: " + ex.ToString());
                sa = new SingleAnimation() { Name = namelow, Length = 1, Engine = this };
                Animations.Add(sa.Name, sa);
                return sa;
            }
        }


        SingleAnimation LoadAnimation(string name)
        {
            if (Program.Files.Exists("animations/" + name + ".anim"))
            {
                SingleAnimation created = new SingleAnimation();
                created.Name = name;
                string[] data = Program.Files.ReadText("animations/" + name + ".anim").Split('\n');
                int entr = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i].StartsWith("//"))
                    {
                        continue;
                    }
                    string type = data[i];
                    if (data.Length <= i + 1 || data[i + 1] != "{")
                    {
                        break;
                    }
                    List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
                    for (i += 2; i < data.Length; i++)
                    {
                        if (data[i].Trim().StartsWith("//"))
                        {
                            continue;
                        }
                        if (data[i] == "}")
                        {
                            break;
                        }
                        string[] dat = data[i].Split(':');
                        if (dat.Length <= 1)
                        {
                            SysConsole.Output(OutputType.WARNING, "Invalid key dat: " + dat[0]);
                        }
                        else
                        {
                            string key = dat[0].Trim();
                            string value = dat[1].Substring(0, dat[1].Length - 1).Trim();
                            entries.Add(new KeyValuePair<string, string>(key, value));
                        }
                    }
                    bool isgeneral = type == "general" && entr == 0;
                    SingleAnimationNode node = null;
                    if (!isgeneral)
                    {
                        node = new SingleAnimationNode();
                        node.Name = type.ToLower();
                    }
                    foreach (KeyValuePair<string, string> entry in entries)
                    {
                        if (isgeneral)
                        {
                            if (entry.Key == "length")
                            {
                                created.Length = Utilities.StringToDouble(entry.Value);
                            }
                            else
                            {
                                SysConsole.Output(OutputType.WARNING, "Unknown GENERAL key: " + entry.Key);
                            }
                        }
                        else
                        {
                            if (entry.Key == "positions")
                            {
                                string[] poses = entry.Value.Split(' ');
                                for (int x = 0; x < poses.Length; x++)
                                {
                                    if (poses[x].Length > 0)
                                    {
                                        string[] posdata = poses[x].Split('=');
                                        node.PosTimes.Add(Utilities.StringToDouble(posdata[0]));
                                        node.Positions.Add(new Location(Utilities.StringToFloat(posdata[1]),
                                            Utilities.StringToFloat(posdata[2]), Utilities.StringToFloat(posdata[3])));
                                    }
                                }
                            }
                            else if (entry.Key == "rotations")
                            {
                                string[] rots = entry.Value.Split(' ');
                                for (int x = 0; x < rots.Length; x++)
                                {
                                    if (rots[x].Length > 0)
                                    {
                                        string[] posdata = rots[x].Split('=');
                                        node.RotTimes.Add(Utilities.StringToDouble(posdata[0]));
                                        node.Rotations.Add(new Quaternion(Utilities.StringToFloat(posdata[1]), Utilities.StringToFloat(posdata[2]),
                                            Utilities.StringToFloat(posdata[3]), Utilities.StringToFloat(posdata[4])));
                                    }
                                }
                            }
                            else if (entry.Key == "parent")
                            {
                                node.ParentName = entry.Value.ToLower();
                            }
                            else if (entry.Key == "offset")
                            {
                                string[] posdata = entry.Value.Split('=');
                                node.Offset = new Location(Utilities.StringToFloat(posdata[0]),
                                    Utilities.StringToFloat(posdata[1]), Utilities.StringToFloat(posdata[2]));
                            }
                            else
                            {
                                SysConsole.Output(OutputType.WARNING, "Unknown NODE key: " + entry.Key);
                            }
                        }
                    }
                    if (!isgeneral)
                    {
                        created.Nodes.Add(node);
                        created.node_map.Add(node.Name, node);
                    }
                    entr++;
                }
                foreach (SingleAnimationNode node in created.Nodes)
                {
                    for (int i = 0; i < created.Nodes.Count; i++)
                    {
                        if (created.Nodes[i].Name == node.ParentName)
                        {
                            node.Parent = created.Nodes[i];
                            break;
                        }
                    }
                }
                created.Engine = this;
                return created;
            }
            else
            {
                throw new Exception("Invalid animation file - file not found: animations/" + name + ".anim");
            }
        }
    }

    public class SingleAnimation
    {
        public string Name;

        public double Length;

        public AnimationEngine Engine;

        public List<SingleAnimationNode> Nodes = new List<SingleAnimationNode>();

        public Dictionary<string, SingleAnimationNode> node_map = new Dictionary<string, SingleAnimationNode>();

        public SingleAnimationNode GetNode(string name)
        {
            SingleAnimationNode node;
            if (node_map.TryGetValue(name, out node))
            {
                return node;
            }
            return null;
        }
    }

    public class SingleAnimationNode
    {
        public string Name;

        public SingleAnimationNode Parent = null;

        public string ParentName;

        public Location Offset;

        public List<double> PosTimes = new List<double>();

        public List<Location> Positions = new List<Location>();

        public List<double> RotTimes = new List<double>();

        public List<Quaternion> Rotations = new List<Quaternion>();

        int findPos(double time)
        {
            for (int i = 0; i < Positions.Count - 1; i++)
            {
                if (time >= PosTimes[i] && time < PosTimes[i + 1])
                {
                    return i;
                }
            }
            return 0;
        }

        public Vector3 lerpPos(double aTime)
        {
            if (Positions.Count == 0)
            {
                return new Vector3(0, 0, 0);
            }
            if (Positions.Count == 1)
            {
                Location pos = Positions[0];
                return new Vector3((float)pos.X, (float)pos.Y, (float)pos.Z);
            }
            int index = findPos(aTime);
            int nextIndex = index + 1;
            if (nextIndex >= Positions.Count)
            {
                Location pos = Positions[0];
                return new Vector3((float)pos.X, (float)pos.Y, (float)pos.Z);
            }
            double deltaT = PosTimes[nextIndex] - PosTimes[index];
            double factor = (aTime - PosTimes[index]) / deltaT;
            if (factor < 0 || factor > 1)
            {
                Location pos = Positions[0];
                return new Vector3((float)pos.X, (float)pos.Y, (float)pos.Z);
            }
            Location start = Positions[index];
            Location end = Positions[nextIndex];
            Location deltaV = end - start;
            Location npos = start + (float)factor * deltaV;
            return new Vector3((float)npos.X, (float)npos.Y, (float)npos.Z);
        }

        int findRotate(double time)
        {
            for (int i = 0; i < Rotations.Count; i++)
            {
                if (time >= RotTimes[i] && time < RotTimes[i + 1])
                {
                    return i;
                }
            }
            return 0;
        }

        public Quaternion lerpRotate(double aTime)
        {
            if (Rotations.Count == 0)
            {
                return Quaternion.Identity;
            }
            if (Rotations.Count == 1)
            {
                return Rotations[0];
            }
            int index = findRotate(aTime);
            int nextIndex = index + 1;
            if (nextIndex >= Rotations.Count)
            {
                return Rotations[0];
            }
            double deltaT = RotTimes[nextIndex] - RotTimes[index];
            double factor = (aTime - RotTimes[index]) / deltaT;
            if (factor < 0 || factor > 1)
            {
                return Rotations[0];
            }
            Quaternion start = Rotations[index];
            Quaternion end = Rotations[nextIndex];
            Quaternion res = Quaternion.Slerp(start, end, (float)factor);
            res.Normalize();
            return res;
        }

        public Matrix GetBoneTotalMatrix(double aTime)
        {
            Matrix pos = Matrix.CreateTranslation(lerpPos(aTime));
            Matrix rot = Matrix.CreateFromQuaternion(lerpRotate(aTime));
            pos.Transpose();
            rot.Transpose();
            Matrix combined = pos * rot;
            if (Parent != null)
            {
                combined = Parent.GetBoneTotalMatrix(aTime) * combined;
                //combined *= Parent.GetBoneTotalMatrix(aTime);
            }
            return combined;
        }

        Matrix Convert(Assimp.Matrix4x4 mat)
        {
            return new Matrix(mat.A1, mat.A2, mat.A3, mat.A4, mat.B1, mat.B2, mat.B3, mat.B4,
                mat.C1, mat.C2, mat.C3, mat.C4, mat.D1, mat.D2, mat.D3, mat.D4);
        }
    }
}
