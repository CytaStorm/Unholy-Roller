namespace MapEditorTool
{
    partial class Form1
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
            buttonLoad = new Button();
            groupCreate = new GroupBox();
            buttonCreate = new Button();
            labelTileWidth = new Label();
            textTileDimensions = new TextBox();
            groupCreate.SuspendLayout();
            SuspendLayout();
            // 
            // buttonLoad
            // 
            buttonLoad.Location = new Point(45, 15);
            buttonLoad.Margin = new Padding(2);
            buttonLoad.Name = "buttonLoad";
            buttonLoad.Size = new Size(188, 68);
            buttonLoad.TabIndex = 0;
            buttonLoad.Text = "Load Map";
            buttonLoad.UseVisualStyleBackColor = true;
            buttonLoad.Click += buttonLoad_Click;
            // 
            // groupCreate
            // 
            groupCreate.Controls.Add(buttonCreate);
            groupCreate.Controls.Add(labelTileWidth);
            groupCreate.Controls.Add(textTileDimensions);
            groupCreate.Location = new Point(27, 119);
            groupCreate.Margin = new Padding(2);
            groupCreate.Name = "groupCreate";
            groupCreate.Padding = new Padding(2);
            groupCreate.Size = new Size(223, 151);
            groupCreate.TabIndex = 1;
            groupCreate.TabStop = false;
            groupCreate.Text = "Create New Map";
            // 
            // buttonCreate
            // 
            buttonCreate.Location = new Point(35, 98);
            buttonCreate.Margin = new Padding(2);
            buttonCreate.Name = "buttonCreate";
            buttonCreate.Size = new Size(155, 50);
            buttonCreate.TabIndex = 2;
            buttonCreate.Text = "Create Map";
            buttonCreate.UseVisualStyleBackColor = true;
            buttonCreate.Click += buttonCreate_Click;
            // 
            // labelTileWidth
            // 
            labelTileWidth.AutoSize = true;
            labelTileWidth.Location = new Point(18, 49);
            labelTileWidth.Margin = new Padding(2, 0, 2, 0);
            labelTileWidth.Name = "labelTileWidth";
            labelTileWidth.Size = new Size(69, 15);
            labelTileWidth.TabIndex = 2;
            labelTileWidth.Text = "Dimensions";
            // 
            // textTileDimensions
            // 
            textTileDimensions.Location = new Point(101, 46);
            textTileDimensions.Margin = new Padding(2);
            textTileDimensions.Name = "textTileDimensions";
            textTileDimensions.Size = new Size(106, 23);
            textTileDimensions.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(277, 290);
            Controls.Add(groupCreate);
            Controls.Add(buttonLoad);
            Margin = new Padding(2);
            Name = "Form1";
            Text = "Level Editor";
            groupCreate.ResumeLayout(false);
            groupCreate.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button buttonLoad;
        private GroupBox groupCreate;
        private Label labelTileWidth;
        private TextBox textTileDimensions;
        private Button buttonCreate;
    }
}