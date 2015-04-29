using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;

namespace OpenTKMapMaker
{
    public class ClipboardEntity
    {
        public string entitytype;
        public List<KeyValuePair<string, string>> variables;
        public Color4? color;
    }
}
