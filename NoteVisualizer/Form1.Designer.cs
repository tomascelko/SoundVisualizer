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
            this.loadingLabel = new System.Windows.Forms.Label();
            this.chooseTuningBox = new System.Windows.Forms.ComboBox();
            this.inputGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // inputSampleTextBox
            // 
            this.inputSampleTextBox.Location = new System.Drawing.Point(197, 34);
            this.inputSampleTextBox.Name = "inputSampleTextBox";
            this.inputSampleTextBox.Size = new System.Drawing.Size(191, 27);
            this.inputSampleTextBox.TabIndex = 0;
            // 
            // inputGroupBox
            // 
            this.inputGroupBox.Controls.Add(this.orLabel);
            this.inputGroupBox.Controls.Add(this.inputLabel);
            this.inputGroupBox.Controls.Add(this.browseButton);
            this.inputGroupBox.Controls.Add(this.inputSampleTextBox);
            this.inputGroupBox.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.inputGroupBox.Location = new System.Drawing.Point(12, 12);
            this.inputGroupBox.Name = "inputGroupBox";
            this.inputGroupBox.Size = new System.Drawing.Size(581, 88);
            this.inputGroupBox.TabIndex = 1;
            this.inputGroupBox.TabStop = false;
            this.inputGroupBox.Text = "Input";
            // 
            // orLabel
            // 
            this.orLabel.AutoSize = true;
            this.orLabel.Location = new System.Drawing.Point(410, 37);
            this.orLabel.Name = "orLabel";
            this.orLabel.Size = new System.Drawing.Size(25, 20);
            this.orLabel.TabIndex = 3;
            this.orLabel.Text = "Or";
            // 
            // inputLabel
            // 
            this.inputLabel.AutoSize = true;
            this.inputLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.inputLabel.Location = new System.Drawing.Point(6, 34);
            this.inputLabel.Name = "inputLabel";
            this.inputLabel.Size = new System.Drawing.Size(185, 21);
            this.inputLabel.TabIndex = 2;
            this.inputLabel.Text = "Insert path to the sample:";
            // 
            // browseButton
            // 
            this.browseButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.browseButton.Location = new System.Drawing.Point(468, 26);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(85, 39);
            this.browseButton.TabIndex = 1;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButtonClick);
            // 
            // processButton
            // 
            this.processButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.processButton.Location = new System.Drawing.Point(12, 114);
            this.processButton.Name = "processButton";
            this.processButton.Size = new System.Drawing.Size(191, 73);
            this.processButton.TabIndex = 4;
            this.processButton.Text = "Process";
            this.processButton.UseVisualStyleBackColor = true;
            this.processButton.Click += new System.EventHandler(this.processButtonClicked);
            // 
            // loadingLabel
            // 
            this.loadingLabel.AutoSize = true;
            this.loadingLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.loadingLabel.Location = new System.Drawing.Point(382, 140);
            this.loadingLabel.Name = "loadingLabel";
            this.loadingLabel.Size = new System.Drawing.Size(88, 20);
            this.loadingLabel.TabIndex = 6;
            this.loadingLabel.Text = "Processing...";
            // 
            // chooseTuningBox
            // 
            this.chooseTuningBox.FormattingEnabled = true;
            this.chooseTuningBox.Items.AddRange(new object[] {
            "C(0)",
            "F(-1)",
            "Bes(-2)",
            "Es(-3)",
            "As(-4)",
            "Des(-5)",
            "Ges(-6)",
            "Ces(-7)",
            "G(1)",
            "D(2)",
            "A(3)",
            "E(4)",
            "B(5)",
            "Fis(6)",
            "Cis(7)"});
            this.chooseTuningBox.Location = new System.Drawing.Point(620, 50);
            this.chooseTuningBox.Name = "chooseTuningBox";
            this.chooseTuningBox.Size = new System.Drawing.Size(141, 23);
            this.chooseTuningBox.TabIndex = 7;
            // 
            // SoundVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 470);
            this.Controls.Add(this.chooseTuningBox);
            this.Controls.Add(this.loadingLabel);
            this.Controls.Add(this.processButton);
            this.Controls.Add(this.inputGroupBox);
            this.Name = "SoundVisualizer";
            this.Text = "SoundVisualizer";
            this.inputGroupBox.ResumeLayout(false);
            this.inputGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox inputSampleTextBox;
        private System.Windows.Forms.GroupBox inputGroupBox;
        private System.Windows.Forms.Label orLabel;
        private System.Windows.Forms.Label inputLabel;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button processButton;
        private System.Windows.Forms.Label loadingLabel;
        private System.Windows.Forms.ComboBox chooseTuningBox;
    }
}

