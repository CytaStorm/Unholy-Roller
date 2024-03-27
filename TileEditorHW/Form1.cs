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

            // Check value entered for Dimensions is an integer
            int mapDimensions;
            bool isInt = int.TryParse(textTileDimensions.Text, out mapDimensions);
            if (isInt)
            {
                // Check Dimensions is within bounds
                if (mapDimensions > maxRows)
                {
                    errors.Add($"Dimensions too large. Maximum is {maxRows}");
                }
                else if (mapDimensions < minRows)
                {
                    errors.Add($"Dimensions too small. Minimum is {minRows}");
                }
            }
            else
            {
                errors.Add($"{textTileDimensions.Text} is not an integer");
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
                editor = new MapEditor(mapDimensions, mapDimensions);

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