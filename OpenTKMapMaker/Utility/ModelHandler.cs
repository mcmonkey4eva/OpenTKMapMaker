using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assimp;
using Assimp.Configs;

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
    }
}
