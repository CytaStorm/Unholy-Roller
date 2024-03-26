// Ramon Miland
// 2/7/24
// Create a window for choosing map editor parameters

namespace MapEditorTool
{
    public partial class Form1 : Form
    {
        // Fields
        private const int maxCols = 30;
        private const int minCols = 10;

        private const int maxRows = 30;
        private const int minRows = 10;

        private MapEditor? editor;

        // Constructors
        public Form1()
        {
            InitializeComponent();
        }

        // Methods

        /// <summary>
        /// Creates an empty map with the user specified dimensions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCreate_Click(object sender, EventArgs e)
        {
            // Check for errors
            List<string> errors = new List<string>();

            // Check value entered for width is an integer
            int mapWidth;
            bool isInt = int.TryParse(textTileWidth.Text, out mapWidth);
            if (isInt)
            {
                // Check Width is within bounds
                if (mapWidth > maxCols)
                {
                    errors.Add($"Width too large. Maximum is {maxCols}");
                }
                else if (mapWidth < minCols)
                {
                    errors.Add($"Width too small. Minimum is {minCols}");
                }
            }
            else
            {
                errors.Add("Width.Text is not an integer");
            }

            // Check value entered for height is an integer
            int mapHeight;
            isInt = int.TryParse(textTileHeight.Text, out mapHeight);
            if (isInt)
            {

                // Check Height is within bounds
                if (mapHeight > maxRows)
                {
                    errors.Add($"Height too large. Maximum is {maxRows}");
                }
                else if (mapHeight < minRows)
                {
                    errors.Add($"Height too small. Minimum is {minRows}");
                }
            }
            else
            {
                errors.Add("Height.Text is not an integer");
            }

            // Report Errors
            if (errors.Count > 0)
            {
                string errorString = "Errors:\n";

                foreach (string error in errors)
                {
                    errorString += $"-{error}\n";
                }

                MessageBox.Show(errorString);
            }
            else
            {
                // Create an empty map
                editor = new MapEditor(mapWidth, mapHeight);

                // Display map editor
                editor.ShowDialog();
            }
        }

        /// <summary>
        /// Prompts user to load a map file, 
        /// then loads that map in the editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog loadDialog = new OpenFileDialog();

            loadDialog.Title = "Open a level file.";

            // Only show '.level' files
            loadDialog.Filter = "Level Files|*.level";

            // Display file chooser window
            DialogResult result = loadDialog.ShowDialog();

            // User chooses a valid file
            if (result == DialogResult.OK && loadDialog.FileName.Trim() != "")
            {
                // Load specified file
                editor = new MapEditor(loadDialog.FileName);

                // Display name of loaded file
                string[] dirs = loadDialog.FileName.Split('\\');
                editor.Text = $"Level Editor - {dirs[dirs.Length - 1]}";

                // Display map editor
                editor.ShowDialog();
            }
        }
    }
}