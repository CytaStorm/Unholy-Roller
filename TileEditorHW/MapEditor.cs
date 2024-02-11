using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

// Ramon Miland
// 2/7/24
// Create a window for tilemap editing

namespace TileEditorHW
{
    public partial class MapEditor : Form
    {
        // Fields
        private PictureBox[,]? tiles;
        private Color curColor;

        private int mapCols;
        private int mapRows;

        private bool changesSaved = true;

        // Constructors

        /// <summary>
        /// Creates an empty map editor window whose editor has the
        /// specified number of rows and columns
        /// </summary>
        /// <param name="cols"> columns in editor </param>
        /// <param name="rows"> rows in editor </param>
        public MapEditor(int cols, int rows)
        {
            InitializeComponent();

            this.mapRows = rows;
            this.mapCols = cols;

            ResizeEditor(cols, rows);

            // Set color choices
            buttonChoice1.BackColor = Color.Gray;
            buttonChoice2.BackColor = Color.Green;
            buttonChoice3.BackColor = Color.Blue;
            buttonChoice4.BackColor = Color.Red;
            buttonChoice5.BackColor = Color.Orange;
            buttonChoice6.BackColor = Color.Purple;

            //buttonChoice1.Image = Image.FromFile("../../../SpikeTile.png");

            // Set default color
            curColor = buttonChoice1.BackColor;
            pictureCurTile.BackColor = curColor;
        }

        /// <summary>
        /// Creates a map editor window with a loaded map
        /// </summary>
        /// <param name="filename"> the map file to load </param>
        public MapEditor(string filename)
        {
            InitializeComponent();

            LoadFile(filename);

            // Set color choices
            buttonChoice1.BackColor = Color.Gray;
            buttonChoice2.BackColor = Color.Green;
            buttonChoice3.BackColor = Color.Blue;
            buttonChoice4.BackColor = Color.Red;
            buttonChoice5.BackColor = Color.Orange;
            buttonChoice6.BackColor = Color.Purple;

            // Set default color
            curColor = buttonChoice1.BackColor;
            pictureCurTile.BackColor = curColor;
        }

        /// <summary>
        /// Changes selected color to that of 
        /// the color choice button clicked in editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseColor(object sender, EventArgs e)
        {
            Button b = (Button)sender;

            // Change color to match color choice
            curColor = b.BackColor;

            // Display currently chosen color
            pictureCurTile.BackColor = curColor;
        }

        /// <summary>
        /// Sets the clicked tile to the currently selected color
        /// </summary>
        /// <param name="sender"> tile to change </param>
        /// <param name="e"></param>
        private void SetTileColor(object? sender, EventArgs e)
        {
            PictureBox tile = (PictureBox)sender!;

            // Change tile color
            tile.BackColor = curColor;

            // User has made changes
            if (changesSaved) this.Text += " *";
            changesSaved = false;

        }

        /// <summary>
        /// Prompts user to save current map, then writes map data
        /// to specified file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveWindow = new SaveFileDialog();

            saveWindow.Title = "Save a level file.";

            // Only show files with '.level' extension
            saveWindow.Filter = "Level Files|*.level";

            // Display save window
            DialogResult result = saveWindow.ShowDialog();

            // User chooses a valid file
            if (result == DialogResult.OK && saveWindow.FileName.Trim() != "")
            {
                // Save current map data to file
                SaveFile(saveWindow.FileName);

                // Report success to user
                MessageBox.Show("File saved successfully", "File saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // User saved their changes
                changesSaved = true;
                this.Text = Text.Substring(0, Text.Length - 2); // Exclude asterisk
            }
        }

        /// <summary>
        /// Writes current map data to the specified file
        /// </summary>
        /// <param name="filename"> file to write to </param>
        private void SaveFile(string filename)
        {
            // Open the file
            StreamWriter output = new StreamWriter(filename);

            // Save map width and height
            output.WriteLine($"{mapCols},{mapRows}");

            // Write all tile data to the file
            for (int y = 0; y < tiles!.GetLength(0); y++)
            {
                for (int x = 0; x < tiles.GetLength(1); x++)
                {
                    output.Write(tiles[y, x].BackColor.ToArgb() + " ");
                }
                output.WriteLine();
            }

            // Close the file
            output.Close();
        }

        /// <summary>
        /// Prompts user to load a map file, then loads that map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog loadDialog = new OpenFileDialog();

            loadDialog.Title = "Open a level file.";

            // Only show '.level' files
            loadDialog.Filter = "Level Files|*.level";

            // Show file chooser window
            DialogResult result = loadDialog.ShowDialog();

            // User chooses a valid file
            if (result == DialogResult.OK && loadDialog.FileName.Trim() != "")
            {
                // Load specified file
                LoadFile(loadDialog.FileName);

                // Display name of loaded file
                string[] dirs = loadDialog.FileName.Split('\\');
                this.Text = $"Level Editor - {dirs[dirs.Length - 1]}";

                // Report success to user
                MessageBox.Show("File loaded successfully", "File loaded",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // User hasn't made changes yet
                changesSaved = true;
            }
        }

        /// <summary>
        /// Loads the specified map file to the editor
        /// </summary>
        /// <param name="filename"> map file to load </param>
        public void LoadFile(string filename)
        {
            StreamReader input = null;
            try
            {
                // Open the file
                input = new StreamReader(filename);

                // Load tile data
                bool resized = false;
                int y = 0;
                string line = "";
                while ((line = input.ReadLine()!) != null)
                {
                    if (!resized)
                    {
                        // Resize editor and window
                        string[] mapDimensions = line.Split(",");

                        mapCols = int.Parse(mapDimensions[0]);
                        mapRows = int.Parse(mapDimensions[1]);

                        ResizeEditor(mapCols, mapRows);

                        resized = true;

                        continue;
                    }

                    // Load tile colors
                    string[] tileLine = line.Split(" ");
                    for (int x = 0; x < tiles!.GetLength(1); x++)
                    {
                        tiles[y, x].BackColor = Color.FromArgb(int.Parse(tileLine[x]));
                    }

                    y++;
                }

            }
            finally
            {
                // If a file was read close it
                if (input != null)
                    input.Close();
            }
        }

        /// <summary>
        /// Resizes the editor and window to fit the specified number of square tiles,
        /// and fills editor with blank tiles
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="rows"></param>
        private void ResizeEditor(int cols, int rows)
        {
            // Create tile storage
            tiles = new PictureBox[rows, cols];

            // Determine tile dimensions
            int boxHeight = (groupMapEditor.Height - 30) / rows;

            // Resize editing space
            groupMapEditor.Width = boxHeight * cols;

            // Resize window
            Width += (groupMapEditor.Left + groupMapEditor.Width) - Width + 50;

            // Add blank tiles
            groupMapEditor.Controls.Clear();
            for (int y = 0; y < tiles.GetLength(0); y++)
            {
                for (int x = 0; x < tiles.GetLength(1); x++)
                {
                    // Create tile
                    tiles[y, x] = new PictureBox();

                    tiles[y, x].SetBounds(
                        x * boxHeight,
                        30 + y * boxHeight,
                        boxHeight,
                        boxHeight);

                    tiles[y, x].Click += SetTileColor;

                    // Add to editor
                    groupMapEditor.Controls.Add(tiles[y, x]);
                }
            }
        }

        /// <summary>
        /// Closes the window, alerting the user if they
        /// have unsaved changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!changesSaved)
            {
                // Warn user about unsaved changes
                DialogResult choice = 
                    MessageBox.Show(
                    "There are unsaved changes. Are you sure you want to quit?",
                    "Unsaved changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (choice == DialogResult.No)
                {
                    // Stop window from closing
                    e.Cancel = true;
                }
            }
        }
    }
}
