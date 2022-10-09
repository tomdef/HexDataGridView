namespace WinFormsTest
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.bytesEditControl1 = new WinFormsTest.HexDataGridViewControl();
            this.SuspendLayout();
            // 
            // bytesEditControl1
            // 
            this.bytesEditControl1.DefaultEncoding = ((System.Text.Encoding)(resources.GetObject("bytesEditControl1.DefaultEncoding")));
            this.bytesEditControl1.Location = new System.Drawing.Point(22, 21);
            this.bytesEditControl1.Name = "bytesEditControl1";
            this.bytesEditControl1.Size = new System.Drawing.Size(580, 320);
            this.bytesEditControl1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 364);
            this.Controls.Add(this.bytesEditControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private HexDataGridViewControl bytesEditControl1;
    }
}

