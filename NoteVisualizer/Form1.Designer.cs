namespace NoteVisualizer
{
    partial class SoundVisualizer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.inputSampleTextBox = new System.Windows.Forms.TextBox();
            this.inputGroupBox = new System.Windows.Forms.GroupBox();
            this.orLabel = new System.Windows.Forms.Label();
            this.inputLabel = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.processButton = new System.Windows.Forms.Button();
            this.inputGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // inputSampleTextBox
            // 
            this.inputSampleTextBox.Location = new System.Drawing.Point(167, 22);
            this.inputSampleTextBox.Name = "inputSampleTextBox";
            this.inputSampleTextBox.Size = new System.Drawing.Size(139, 23);
            this.inputSampleTextBox.TabIndex = 0;
            // 
            // inputGroupBox
            // 
            this.inputGroupBox.Controls.Add(this.orLabel);
            this.inputGroupBox.Controls.Add(this.inputLabel);
            this.inputGroupBox.Controls.Add(this.browseButton);
            this.inputGroupBox.Controls.Add(this.inputSampleTextBox);
            this.inputGroupBox.Location = new System.Drawing.Point(12, 12);
            this.inputGroupBox.Name = "inputGroupBox";
            this.inputGroupBox.Size = new System.Drawing.Size(490, 67);
            this.inputGroupBox.TabIndex = 1;
            this.inputGroupBox.TabStop = false;
            this.inputGroupBox.Text = "Input";
            // 
            // orLabel
            // 
            this.orLabel.AutoSize = true;
            this.orLabel.Location = new System.Drawing.Point(343, 25);
            this.orLabel.Name = "orLabel";
            this.orLabel.Size = new System.Drawing.Size(20, 15);
            this.orLabel.TabIndex = 3;
            this.orLabel.Text = "Or";
            // 
            // inputLabel
            // 
            this.inputLabel.AutoSize = true;
            this.inputLabel.Location = new System.Drawing.Point(6, 26);
            this.inputLabel.Name = "inputLabel";
            this.inputLabel.Size = new System.Drawing.Size(141, 15);
            this.inputLabel.TabIndex = 2;
            this.inputLabel.Text = "Insert path to the sample:";
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(394, 22);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(85, 24);
            this.browseButton.TabIndex = 1;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButtonClick);
            // 
            // processButton
            // 
            this.processButton.Location = new System.Drawing.Point(12, 106);
            this.processButton.Name = "processButton";
            this.processButton.Size = new System.Drawing.Size(101, 54);
            this.processButton.TabIndex = 4;
            this.processButton.Text = "Process";
            this.processButton.UseVisualStyleBackColor = true;
            this.processButton.Click += new System.EventHandler(this.processButtonClicked);
            // 
            // SoundVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 470);
            this.Controls.Add(this.processButton);
            this.Controls.Add(this.inputGroupBox);
            this.Name = "SoundVisualizer";
            this.Text = "SoundVisualizer";
            this.inputGroupBox.ResumeLayout(false);
            this.inputGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox inputSampleTextBox;
        private System.Windows.Forms.GroupBox inputGroupBox;
        private System.Windows.Forms.Label orLabel;
        private System.Windows.Forms.Label inputLabel;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button processButton;
    }
}

