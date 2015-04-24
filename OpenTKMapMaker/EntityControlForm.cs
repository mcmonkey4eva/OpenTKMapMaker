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

namespace OpenTKMapMaker
{
    public partial class EntityControlForm : Form
    {
        Entity ent;

        public EntityControlForm(Entity e)
        {
            ent = e;
            InitializeComponent();
            recalc();
            listBox1.MouseDoubleClick += new MouseEventHandler(listBox1_MouseDoubleClick);
            textBox1.TextChanged += new EventHandler(textBox1_TextChanged);
        }

        public void recalc()
        {
            label1.Text = "Entity: " + ent.GetEntityType();
            textBox2.Text = ent.GetEntityType();
            resetListBoxContents();
            textBox1.ReadOnly = true;
        }

        void resetListBoxContents()
        {
            listBox1.Items.Clear();
            List<KeyValuePair<string, string>> vars = ent.GetVars();
            foreach (KeyValuePair<string, string> kvp in vars)
            {
                listBox1.Items.Add(kvp.Key + ": " + kvp.Value);
            }
        }

        void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0 && !textBox1.ReadOnly)
            {
                ent.ApplyVar(label2.Text, textBox1.Text);
                ent.Recalculate();
                resetListBoxContents();
                PrimaryEditor.PRFMain.invalidateAll();
            }
        }

        void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string res = null;
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                Rectangle r = listBox1.GetItemRectangle(i);
                if (r.Contains(e.X, e.Y))
                {
                    res = listBox1.Items[i].ToString();
                }
            }
            if (res != null)
            {
                SysConsole.Output(OutputType.INFO, res);
                string[] data = res.Split(new char[] { ':' }, 2);
                label2.Text = data[0];
                textBox1.ReadOnly = true;
                textBox1.Text = data[1].Trim();
                textBox1.ReadOnly = false;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.ToLower() != ent.GetEntityType().ToLower())
            {
                PrimaryEditor.PRFMain.SetType(textBox2.Text);
                if (PrimaryEditor.Selected.Count != 1)
                {
                    Close();
                    return;
                }
                if (ent != PrimaryEditor.Selected[0])
                {
                    ent = PrimaryEditor.Selected[0];
                    recalc();
                }
            }
        }
    }
}
