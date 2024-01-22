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
            this.btnOpenAsync = new System.Windows.Forms.Button();
            this.btnOpenSync = new System.Windows.Forms.Button();
            this.lblResult = new System.Windows.Forms.Label();
            this.txtConString = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOpenAsync
            // 
            this.btnOpenAsync.Location = new System.Drawing.Point(27, 181);
            this.btnOpenAsync.Name = "btnOpenAsync";
            this.btnOpenAsync.Size = new System.Drawing.Size(119, 23);
            this.btnOpenAsync.TabIndex = 0;
            this.btnOpenAsync.Text = "Open async";
            this.btnOpenAsync.UseVisualStyleBackColor = true;
            this.btnOpenAsync.Click += new System.EventHandler(this.btnOpenAsync_Click);
            // 
            // btnOpenSync
            // 
            this.btnOpenSync.Location = new System.Drawing.Point(161, 181);
            this.btnOpenSync.Name = "btnOpenSync";
            this.btnOpenSync.Size = new System.Drawing.Size(125, 23);
            this.btnOpenSync.TabIndex = 1;
            this.btnOpenSync.Text = "Open sync";
            this.btnOpenSync.UseVisualStyleBackColor = true;
            this.btnOpenSync.Click += new System.EventHandler(this.btnOpenSync_Click);
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(24, 227);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(152, 13);
            this.lblResult.TabIndex = 2;
            this.lblResult.Text = "- results will be displayed here -";
            // 
            // txtConString
            // 
            this.txtConString.Location = new System.Drawing.Point(122, 85);
            this.txtConString.Multiline = true;
            this.txtConString.Name = "txtConString";
            this.txtConString.Size = new System.Drawing.Size(510, 59);
            this.txtConString.TabIndex = 3;
            this.txtConString.Text = "port=5443;Server=localhost;Username=enterprisedb;Password=edb;Database=edb;Timeou" +
    "t=10;Command Timeout=10;SSL Mode=Disable;Multiplexing=False;Pooling=false";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Connection string ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(664, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "This sample must be run befoire each release. Those two buttons runs the EDB samp" +
    "le, THEY SHOULD NOT HANG more than 5 seconds.";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtConString);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.btnOpenSync);
            this.Controls.Add(this.btnOpenAsync);
            this.Name = "Form1";
            this.Text = "EDB hang sample";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOpenAsync;
        private System.Windows.Forms.Button btnOpenSync;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.TextBox txtConString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

