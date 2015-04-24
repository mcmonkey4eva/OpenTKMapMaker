using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTKMapMaker.EntitySystem;
using OpenTKMapMaker.Utility;
using OpenTKMapMaker.GraphicsSystem;

namespace OpenTKMapMaker
{
    public partial class FaceEditor : Form
    {
        CubeEntity ent;

        Location Normal;

        public string TextureName;

        public static int CoordIndex = 0;

        public FaceEditor(CubeEntity e, Location norm)
        {
            ent = e;
            Normal = norm;
            if (Normal.Z == 1) { TextureName = e.Textures[0]; CoordIndex = 0; }
            else if (Normal.Z == -1) { TextureName = e.Textures[1]; CoordIndex = 1; }
            else if (Normal.X == 1) { TextureName = e.Textures[2]; CoordIndex = 2; }
            else if (Normal.X == -1) { TextureName = e.Textures[3]; CoordIndex = 3; }
            else if (Normal.Y == 1) { TextureName = e.Textures[4]; CoordIndex = 4; }
            else if (Normal.Y == -1) { TextureName = e.Textures[5]; CoordIndex = 5; }
            InitializeComponent();
        }

        private void FaceEditor_Load(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Size = new Size(256, 256);
                pictureBox1.Image = TextureEngine.BitmapForFile(TextureName);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.WARNING, ex.GetType().Name + ": " + ex.Message);
            }
            repeatBox1.Text = ent.Coords[CoordIndex].xscale.ToString();
            repeatBox2.Text = ent.Coords[CoordIndex].yscale.ToString();
            shiftBox1.Text = ent.Coords[CoordIndex].xshift.ToString();
            shiftBox2.Text = ent.Coords[CoordIndex].yshift.ToString();
        }

        private void repeatBox1_TextChanged(object sender, EventArgs e)
        {
            ent.Coords[CoordIndex].xscale = Utilities.StringToFloat(repeatBox1.Text);
            ent.Recalculate();
            PrimaryEditor.PRFMain.invalidateAll();
        }

        private void repeatBox2_TextChanged(object sender, EventArgs e)
        {
            ent.Coords[CoordIndex].yscale = Utilities.StringToFloat(repeatBox2.Text);
            ent.Recalculate();
            PrimaryEditor.PRFMain.invalidateAll();
        }

        private void shiftBox1_TextChanged(object sender, EventArgs e)
        {
            ent.Coords[CoordIndex].xshift = Utilities.StringToFloat(shiftBox1.Text);
            ent.Recalculate();
            PrimaryEditor.PRFMain.invalidateAll();
        }

        private void shiftBox2_TextChanged(object sender, EventArgs e)
        {
            ent.Coords[CoordIndex].yshift = Utilities.StringToFloat(shiftBox2.Text);
            ent.Recalculate();
            PrimaryEditor.PRFMain.invalidateAll();
        }
    }
}
