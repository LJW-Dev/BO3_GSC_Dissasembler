namespace GSCDecompiler
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
            this.HeaderDataTextOutput = new System.Windows.Forms.TextBox();
            this.GetHeaderData = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.IncludesTextOutput = new System.Windows.Forms.TextBox();
            this.StringTextOutput = new System.Windows.Forms.TextBox();
            this.GSCName = new System.Windows.Forms.TextBox();
            this.DissasembleButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.FunctionTextOutput = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.RenameBuiltinCheckbox = new System.Windows.Forms.CheckBox();
            this.NameHash = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.CanonicalHashInput = new System.Windows.Forms.TextBox();
            this.CanonicalHashOutput = new System.Windows.Forms.TextBox();
            this.CanonicalHash = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.CurrentOffsetTextOutput = new GSCDecompiler.SyncTextBox();
            this.DecompiledTextOutput = new GSCDecompiler.SyncTextBox();
            this.SuspendLayout();
            // 
            // HeaderDataTextOutput
            // 
            this.HeaderDataTextOutput.Location = new System.Drawing.Point(12, 85);
            this.HeaderDataTextOutput.Multiline = true;
            this.HeaderDataTextOutput.Name = "HeaderDataTextOutput";
            this.HeaderDataTextOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.HeaderDataTextOutput.Size = new System.Drawing.Size(212, 328);
            this.HeaderDataTextOutput.TabIndex = 0;
            // 
            // GetHeaderData
            // 
            this.GetHeaderData.Location = new System.Drawing.Point(15, 22);
            this.GetHeaderData.Name = "GetHeaderData";
            this.GetHeaderData.Size = new System.Drawing.Size(152, 28);
            this.GetHeaderData.TabIndex = 1;
            this.GetHeaderData.Text = "Open GSC File";
            this.GetHeaderData.UseVisualStyleBackColor = true;
            this.GetHeaderData.Click += new System.EventHandler(this.GetHeaderData_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // IncludesTextOutput
            // 
            this.IncludesTextOutput.Location = new System.Drawing.Point(230, 85);
            this.IncludesTextOutput.Multiline = true;
            this.IncludesTextOutput.Name = "IncludesTextOutput";
            this.IncludesTextOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.IncludesTextOutput.Size = new System.Drawing.Size(303, 328);
            this.IncludesTextOutput.TabIndex = 2;
            this.IncludesTextOutput.WordWrap = false;
            // 
            // StringTextOutput
            // 
            this.StringTextOutput.Location = new System.Drawing.Point(539, 85);
            this.StringTextOutput.Multiline = true;
            this.StringTextOutput.Name = "StringTextOutput";
            this.StringTextOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.StringTextOutput.Size = new System.Drawing.Size(368, 328);
            this.StringTextOutput.TabIndex = 3;
            this.StringTextOutput.WordWrap = false;
            // 
            // GSCName
            // 
            this.GSCName.Location = new System.Drawing.Point(199, 25);
            this.GSCName.Multiline = true;
            this.GSCName.Name = "GSCName";
            this.GSCName.Size = new System.Drawing.Size(351, 28);
            this.GSCName.TabIndex = 4;
            // 
            // DissasembleButton
            // 
            this.DissasembleButton.Location = new System.Drawing.Point(105, 432);
            this.DissasembleButton.Name = "DissasembleButton";
            this.DissasembleButton.Size = new System.Drawing.Size(152, 28);
            this.DissasembleButton.TabIndex = 6;
            this.DissasembleButton.Text = "Dissasemble File";
            this.DissasembleButton.UseVisualStyleBackColor = true;
            this.DissasembleButton.Click += new System.EventHandler(this.DissasembleButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(196, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(189, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Original GSC name and path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "Header Information";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(230, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 17);
            this.label3.TabIndex = 12;
            this.label3.Text = "All Includes";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(628, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 17);
            this.label4.TabIndex = 13;
            this.label4.Text = "All Strings";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 446);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 17);
            this.label5.TabIndex = 14;
            this.label5.Text = "Dissasembly";
            // 
            // FunctionTextOutput
            // 
            this.FunctionTextOutput.Location = new System.Drawing.Point(930, 85);
            this.FunctionTextOutput.Multiline = true;
            this.FunctionTextOutput.Name = "FunctionTextOutput";
            this.FunctionTextOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.FunctionTextOutput.Size = new System.Drawing.Size(344, 328);
            this.FunctionTextOutput.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(927, 65);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 17);
            this.label6.TabIndex = 16;
            this.label6.Text = "All Functions";
            // 
            // RenameBuiltinCheckbox
            // 
            this.RenameBuiltinCheckbox.AutoSize = true;
            this.RenameBuiltinCheckbox.Location = new System.Drawing.Point(264, 438);
            this.RenameBuiltinCheckbox.Name = "RenameBuiltinCheckbox";
            this.RenameBuiltinCheckbox.Size = new System.Drawing.Size(363, 21);
            this.RenameBuiltinCheckbox.TabIndex = 17;
            this.RenameBuiltinCheckbox.Text = "Rename Builtin Functions to Hash (for custom GSCs)";
            this.RenameBuiltinCheckbox.UseVisualStyleBackColor = true;
            // 
            // NameHash
            // 
            this.NameHash.Location = new System.Drawing.Point(573, 25);
            this.NameHash.Multiline = true;
            this.NameHash.Name = "NameHash";
            this.NameHash.Size = new System.Drawing.Size(144, 28);
            this.NameHash.TabIndex = 19;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(570, 5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(133, 17);
            this.label7.TabIndex = 20;
            this.label7.Text = "Original name Hash";
            // 
            // CanonicalHashInput
            // 
            this.CanonicalHashInput.Location = new System.Drawing.Point(941, 472);
            this.CanonicalHashInput.Name = "CanonicalHashInput";
            this.CanonicalHashInput.Size = new System.Drawing.Size(333, 22);
            this.CanonicalHashInput.TabIndex = 21;
            // 
            // CanonicalHashOutput
            // 
            this.CanonicalHashOutput.Location = new System.Drawing.Point(941, 534);
            this.CanonicalHashOutput.Multiline = true;
            this.CanonicalHashOutput.Name = "CanonicalHashOutput";
            this.CanonicalHashOutput.Size = new System.Drawing.Size(173, 28);
            this.CanonicalHashOutput.TabIndex = 22;
            // 
            // CanonicalHash
            // 
            this.CanonicalHash.Location = new System.Drawing.Point(941, 500);
            this.CanonicalHash.Name = "CanonicalHash";
            this.CanonicalHash.Size = new System.Drawing.Size(173, 28);
            this.CanonicalHash.TabIndex = 23;
            this.CanonicalHash.Text = "Get Canonical Hash";
            this.CanonicalHash.UseVisualStyleBackColor = true;
            this.CanonicalHash.Click += new System.EventHandler(this.CanonicalHash_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(1133, 962);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(141, 17);
            this.label8.TabIndex = 28;
            this.label8.Text = "Made by CraftyCritter";
            // 
            // CurrentOffsetTextOutput
            // 
            this.CurrentOffsetTextOutput.Buddy = this.DecompiledTextOutput;
            this.CurrentOffsetTextOutput.Location = new System.Drawing.Point(15, 466);
            this.CurrentOffsetTextOutput.Multiline = true;
            this.CurrentOffsetTextOutput.Name = "CurrentOffsetTextOutput";
            this.CurrentOffsetTextOutput.ReadOnly = true;
            this.CurrentOffsetTextOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CurrentOffsetTextOutput.Size = new System.Drawing.Size(71, 513);
            this.CurrentOffsetTextOutput.TabIndex = 27;
            this.CurrentOffsetTextOutput.WordWrap = false;
            // 
            // DecompiledTextOutput
            // 
            this.DecompiledTextOutput.Buddy = this.CurrentOffsetTextOutput;
            this.DecompiledTextOutput.Location = new System.Drawing.Point(92, 466);
            this.DecompiledTextOutput.MaxLength = 65535;
            this.DecompiledTextOutput.Multiline = true;
            this.DecompiledTextOutput.Name = "DecompiledTextOutput";
            this.DecompiledTextOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.DecompiledTextOutput.Size = new System.Drawing.Size(832, 513);
            this.DecompiledTextOutput.TabIndex = 26;
            this.DecompiledTextOutput.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1286, 991);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.CurrentOffsetTextOutput);
            this.Controls.Add(this.DecompiledTextOutput);
            this.Controls.Add(this.CanonicalHash);
            this.Controls.Add(this.CanonicalHashOutput);
            this.Controls.Add(this.CanonicalHashInput);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.NameHash);
            this.Controls.Add(this.RenameBuiltinCheckbox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.FunctionTextOutput);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DissasembleButton);
            this.Controls.Add(this.GSCName);
            this.Controls.Add(this.StringTextOutput);
            this.Controls.Add(this.IncludesTextOutput);
            this.Controls.Add(this.GetHeaderData);
            this.Controls.Add(this.HeaderDataTextOutput);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "BO3 GSC Dissasembler";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox HeaderDataTextOutput;
        private System.Windows.Forms.Button GetHeaderData;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox IncludesTextOutput;
        private System.Windows.Forms.TextBox StringTextOutput;
        private System.Windows.Forms.TextBox GSCName;
        private System.Windows.Forms.Button DissasembleButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox FunctionTextOutput;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox RenameBuiltinCheckbox;
        private System.Windows.Forms.TextBox NameHash;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox CanonicalHashInput;
        private System.Windows.Forms.TextBox CanonicalHashOutput;
        private System.Windows.Forms.Button CanonicalHash;
        private SyncTextBox DecompiledTextOutput;
        public SyncTextBox CurrentOffsetTextOutput;
        private System.Windows.Forms.Label label8;
    }
}

