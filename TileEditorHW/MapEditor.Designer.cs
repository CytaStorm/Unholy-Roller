namespace MapEditorTool
{
    partial class MapEditor
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
            groupMapEditor = new GroupBox();
            groupCurTile = new GroupBox();
            pictureCurTile = new PictureBox();
            buttonLoad = new Button();
            buttonSave = new Button();
            groupColorChoice = new GroupBox();
            buttonChoice6 = new Button();
            buttonChoice5 = new Button();
            buttonChoice4 = new Button();
            buttonChoice3 = new Button();
            buttonChoice2 = new Button();
            buttonChoice1 = new Button();
            label1 = new Label();
            tileReportText = new TextBox();
            buttonPrev = new Button();
            buttonNext = new Button();
            buttonAddRoom = new Button();
            textRoomSize = new TextBox();
            labelRoomDimensions = new Label();
            groupCurTile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureCurTile).BeginInit();
            groupColorChoice.SuspendLayout();
            SuspendLayout();
            // 
            // groupMapEditor
            // 
            groupMapEditor.Location = new Point(309, 58);
            groupMapEditor.Name = "groupMapEditor";
            groupMapEditor.Size = new Size(727, 624);
            groupMapEditor.TabIndex = 9;
            groupMapEditor.TabStop = false;
            groupMapEditor.Text = "Map";
            // 
            // groupCurTile
            // 
            groupCurTile.Controls.Add(pictureCurTile);
            groupCurTile.Location = new Point(68, 394);
            groupCurTile.Name = "groupCurTile";
            groupCurTile.Size = new Size(180, 142);
            groupCurTile.TabIndex = 8;
            groupCurTile.TabStop = false;
            groupCurTile.Text = "Current Tile";
            // 
            // pictureCurTile
            // 
            pictureCurTile.Location = new Point(37, 30);
            pictureCurTile.Name = "pictureCurTile";
            pictureCurTile.Size = new Size(103, 93);
            pictureCurTile.TabIndex = 0;
            pictureCurTile.TabStop = false;
            // 
            // buttonLoad
            // 
            buttonLoad.Location = new Point(68, 699);
            buttonLoad.Name = "buttonLoad";
            buttonLoad.Size = new Size(180, 124);
            buttonLoad.TabIndex = 7;
            buttonLoad.Text = "Load";
            buttonLoad.UseVisualStyleBackColor = true;
            buttonLoad.Click += buttonLoad_Click;
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(68, 558);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(180, 124);
            buttonSave.TabIndex = 6;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // groupColorChoice
            // 
            groupColorChoice.Controls.Add(buttonChoice6);
            groupColorChoice.Controls.Add(buttonChoice5);
            groupColorChoice.Controls.Add(buttonChoice4);
            groupColorChoice.Controls.Add(buttonChoice3);
            groupColorChoice.Controls.Add(buttonChoice2);
            groupColorChoice.Controls.Add(buttonChoice1);
            groupColorChoice.Location = new Point(61, 58);
            groupColorChoice.Name = "groupColorChoice";
            groupColorChoice.Size = new Size(200, 317);
            groupColorChoice.TabIndex = 5;
            groupColorChoice.TabStop = false;
            groupColorChoice.Text = "Tile Selector";
            // 
            // buttonChoice6
            // 
            buttonChoice6.Location = new Point(100, 217);
            buttonChoice6.Name = "buttonChoice6";
            buttonChoice6.Size = new Size(87, 84);
            buttonChoice6.TabIndex = 5;
            buttonChoice6.UseVisualStyleBackColor = true;
            buttonChoice6.Click += ChooseImage;
            // 
            // buttonChoice5
            // 
            buttonChoice5.Location = new Point(7, 217);
            buttonChoice5.Name = "buttonChoice5";
            buttonChoice5.Size = new Size(87, 84);
            buttonChoice5.TabIndex = 4;
            buttonChoice5.UseVisualStyleBackColor = true;
            buttonChoice5.Click += ChooseImage;
            // 
            // buttonChoice4
            // 
            buttonChoice4.Location = new Point(100, 127);
            buttonChoice4.Name = "buttonChoice4";
            buttonChoice4.Size = new Size(87, 84);
            buttonChoice4.TabIndex = 3;
            buttonChoice4.UseVisualStyleBackColor = true;
            buttonChoice4.Click += ChooseImage;
            // 
            // buttonChoice3
            // 
            buttonChoice3.Location = new Point(7, 127);
            buttonChoice3.Name = "buttonChoice3";
            buttonChoice3.Size = new Size(87, 84);
            buttonChoice3.TabIndex = 2;
            buttonChoice3.UseVisualStyleBackColor = true;
            buttonChoice3.Click += ChooseImage;
            // 
            // buttonChoice2
            // 
            buttonChoice2.Location = new Point(100, 37);
            buttonChoice2.Name = "buttonChoice2";
            buttonChoice2.Size = new Size(87, 84);
            buttonChoice2.TabIndex = 1;
            buttonChoice2.UseVisualStyleBackColor = true;
            buttonChoice2.Click += ChooseImage;
            // 
            // buttonChoice1
            // 
            buttonChoice1.Location = new Point(7, 37);
            buttonChoice1.Name = "buttonChoice1";
            buttonChoice1.Size = new Size(87, 84);
            buttonChoice1.TabIndex = 0;
            buttonChoice1.UseVisualStyleBackColor = true;
            buttonChoice1.Click += ChooseImage;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(59, 25);
            label1.TabIndex = 10;
            label1.Text = "label1";
            // 
            // tileReportText
            // 
            tileReportText.Location = new Point(488, 699);
            tileReportText.Name = "tileReportText";
            tileReportText.ReadOnly = true;
            tileReportText.Size = new Size(342, 31);
            tileReportText.TabIndex = 11;
            // 
            // buttonPrev
            // 
            buttonPrev.Location = new Point(488, 751);
            buttonPrev.Name = "buttonPrev";
            buttonPrev.Size = new Size(131, 53);
            buttonPrev.TabIndex = 12;
            buttonPrev.Text = "Prev";
            buttonPrev.UseVisualStyleBackColor = true;
            buttonPrev.Click += buttonPrev_Click;
            // 
            // buttonNext
            // 
            buttonNext.Location = new Point(659, 751);
            buttonNext.Name = "buttonNext";
            buttonNext.Size = new Size(131, 53);
            buttonNext.TabIndex = 13;
            buttonNext.Text = "Next";
            buttonNext.UseVisualStyleBackColor = true;
            buttonNext.Click += buttonNext_Click;
            // 
            // buttonAddRoom
            // 
            buttonAddRoom.Location = new Point(488, 837);
            buttonAddRoom.Name = "buttonAddRoom";
            buttonAddRoom.Size = new Size(140, 69);
            buttonAddRoom.TabIndex = 14;
            buttonAddRoom.Text = "Add Map";
            buttonAddRoom.UseVisualStyleBackColor = true;
            buttonAddRoom.Click += buttonAddRoom_Click;
            // 
            // textRoomSize
            // 
            textRoomSize.Location = new Point(659, 875);
            textRoomSize.Name = "textRoomSize";
            textRoomSize.Size = new Size(150, 31);
            textRoomSize.TabIndex = 15;
            // 
            // labelRoomDimensions
            // 
            labelRoomDimensions.AutoSize = true;
            labelRoomDimensions.Location = new Point(685, 837);
            labelRoomDimensions.Name = "labelRoomDimensions";
            labelRoomDimensions.Size = new Size(109, 25);
            labelRoomDimensions.TabIndex = 16;
            labelRoomDimensions.Text = "Dimensions:";
            // 
            // MapEditor
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1097, 933);
            Controls.Add(labelRoomDimensions);
            Controls.Add(textRoomSize);
            Controls.Add(buttonAddRoom);
            Controls.Add(buttonNext);
            Controls.Add(buttonPrev);
            Controls.Add(tileReportText);
            Controls.Add(label1);
            Controls.Add(groupMapEditor);
            Controls.Add(groupCurTile);
            Controls.Add(buttonLoad);
            Controls.Add(buttonSave);
            Controls.Add(groupColorChoice);
            Name = "MapEditor";
            Text = "Level Editor";
            FormClosing += MapEditor_FormClosing;
            groupCurTile.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureCurTile).EndInit();
            groupColorChoice.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupMapEditor;
        private GroupBox groupCurTile;
        private PictureBox pictureCurTile;
        private Button buttonLoad;
        private Button buttonSave;
        private GroupBox groupColorChoice;
        private Button buttonChoice6;
        private Button buttonChoice5;
        private Button buttonChoice4;
        private Button buttonChoice3;
        private Button buttonChoice2;
        private Button buttonChoice1;
        private Label label1;
        private TextBox tileReportText;
        private Button buttonPrev;
        private Button buttonNext;
        private Button buttonAddRoom;
        private TextBox textRoomSize;
        private Label labelRoomDimensions;
    }
}