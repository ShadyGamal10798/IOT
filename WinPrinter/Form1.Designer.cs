namespace WinPrinter
{
    partial class Form1
    {
        private System.Windows.Forms.TextBox textBoxGreen;
        private System.Windows.Forms.TextBox textBoxBlue;

        // ... other existing code ...
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "WinPrinter";
            //SuspendLayout();
            //// 
            //// Form1
            //// 
            //AutoScaleDimensions = new SizeF(13F, 32F);
            //AutoScaleMode = AutoScaleMode.Font;
            //ClientSize = new Size(800, 450);
            //Name = "Form1";
            //Text = "WinPrinter";
            //Load += Form1_Load;
            //ResumeLayout(false);
        }
    }
}

