namespace TileEditorHW
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
            labelTileHeight = new Label();
            labelTileWidth = new Label();
            textTileHeight = new TextBox();
            textTileWidth = new TextBox();
            groupCreate.SuspendLayout();
            SuspendLayout();
            // 
            // buttonLoad
            // 
            buttonLoad.Location = new Point(64, 25);
            buttonLoad.Name = "buttonLoad";
            buttonLoad.Size = new Size(268, 114);
            buttonLoad.TabIndex = 0;
            buttonLoad.Text = "Load Map";
            buttonLoad.UseVisualStyleBackColor = true;
            buttonLoad.Click += buttonLoad_Click;
            // 
            // groupCreate
            // 
            groupCreate.Controls.Add(buttonCreate);
            groupCreate.Controls.Add(labelTileHeight);
            groupCreate.Controls.Add(labelTileWidth);
            groupCreate.Controls.Add(textTileHeight);
            groupCreate.Controls.Add(textTileWidth);
            groupCreate.Location = new Point(38, 198);
            groupCreate.Name = "groupCreate";
            groupCreate.Size = new Size(319, 252);
            groupCreate.TabIndex = 1;
            groupCreate.TabStop = false;
            groupCreate.Text = "Create New Map";
            // 
            // buttonCreate
            // 
            buttonCreate.Location = new Point(50, 163);
            buttonCreate.Name = "buttonCreate";
            buttonCreate.Size = new Size(221, 83);
            buttonCreate.TabIndex = 2;
            buttonCreate.Text = "Create Map";
            buttonCreate.UseVisualStyleBackColor = true;
            buttonCreate.Click += buttonCreate_Click;
            // 
            // labelTileHeight
            // 
            labelTileHeight.AutoSize = true;
            labelTileHeight.Location = new Point(13, 108);
            labelTileHeight.Name = "labelTileHeight";
            labelTileHeight.Size = new Size(130, 25);
            labelTileHeight.TabIndex = 3;
            labelTileHeight.Text = "Height (in tiles)";
            // 
            // labelTileWidth
            // 
            labelTileWidth.AutoSize = true;
            labelTileWidth.Location = new Point(13, 50);
            labelTileWidth.Name = "labelTileWidth";
            labelTileWidth.Size = new Size(125, 25);
            labelTileWidth.TabIndex = 2;
            labelTileWidth.Text = "Width (in tiles)";
            // 
            // textTileHeight
            // 
            textTileHeight.Location = new Point(144, 108);
            textTileHeight.Name = "textTileHeight";
            textTileHeight.Size = new Size(150, 31);
            textTileHeight.TabIndex = 1;
            // 
            // textTileWidth
            // 
            textTileWidth.Location = new Point(144, 50);
            textTileWidth.Name = "textTileWidth";
            textTileWidth.Size = new Size(150, 31);
            textTileWidth.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(396, 484);
            Controls.Add(groupCreate);
            Controls.Add(buttonLoad);
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
        private TextBox textTileHeight;
        private TextBox textTileWidth;
        private Button buttonCreate;
        private Label labelTileHeight;
    }
}