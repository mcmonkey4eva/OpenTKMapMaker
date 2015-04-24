namespace OpenTKMapMaker
{
    partial class FaceEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.repeatBox1 = new System.Windows.Forms.TextBox();
            this.repeatBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.shiftBox1 = new System.Windows.Forms.TextBox();
            this.shiftBox2 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(256, 256);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(275, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Texture Scale";
            // 
            // repeatBox1
            // 
            this.repeatBox1.Location = new System.Drawing.Point(278, 29);
            this.repeatBox1.Name = "repeatBox1";
            this.repeatBox1.Size = new System.Drawing.Size(78, 20);
            this.repeatBox1.TabIndex = 2;
            this.repeatBox1.TextChanged += new System.EventHandler(this.repeatBox1_TextChanged);
            // 
            // repeatBox2
            // 
            this.repeatBox2.Location = new System.Drawing.Point(363, 29);
            this.repeatBox2.Name = "repeatBox2";
            this.repeatBox2.Size = new System.Drawing.Size(78, 20);
            this.repeatBox2.TabIndex = 3;
            this.repeatBox2.TextChanged += new System.EventHandler(this.repeatBox2_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(278, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Texture Shift";
            // 
            // shiftBox1
            // 
            this.shiftBox1.Location = new System.Drawing.Point(278, 73);
            this.shiftBox1.Name = "shiftBox1";
            this.shiftBox1.Size = new System.Drawing.Size(78, 20);
            this.shiftBox1.TabIndex = 5;
            this.shiftBox1.TextChanged += new System.EventHandler(this.shiftBox1_TextChanged);
            // 
            // shiftBox2
            // 
            this.shiftBox2.Location = new System.Drawing.Point(363, 72);
            this.shiftBox2.Name = "shiftBox2";
            this.shiftBox2.Size = new System.Drawing.Size(78, 20);
            this.shiftBox2.TabIndex = 6;
            this.shiftBox2.TextChanged += new System.EventHandler(this.shiftBox2_TextChanged);
            // 
            // FaceEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 281);
            this.Controls.Add(this.shiftBox2);
            this.Controls.Add(this.shiftBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.repeatBox2);
            this.Controls.Add(this.repeatBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "FaceEditor";
            this.Text = "FaceEditor";
            this.Load += new System.EventHandler(this.FaceEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox repeatBox1;
        private System.Windows.Forms.TextBox repeatBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox shiftBox1;
        private System.Windows.Forms.TextBox shiftBox2;
    }
}