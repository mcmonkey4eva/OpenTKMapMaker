using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTKMapMaker.GraphicsSystem;

namespace OpenTKMapMaker
{
    public class GLContext
    {
        public GLControl Control;

        public TextureEngine Textures;

        public ShaderEngine Shaders;

        public GLFontEngine Fonts;

        public FontSetEngine FontSets;

        public ModelEngine Models;

        public Renderer Rendering;
    }
}
