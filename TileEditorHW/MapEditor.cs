using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

public enum TileType
{
	Grass,
	Wall = 'w',
	Spike = 's',
	Placeholder,
	ClosedDoor,
	OpenDoor
}

namespace MapEditorTool
{
	public partial class MapEditor : Form
	{

		#region Fields
		private Tile[,]? tiles;
		private List<Tuple<int, string>> loadedMaps;
		private int curMapIndex;

		private Color curColor;
		private Image curImage;

		private int mapCols;
		private int mapRows;

		private bool curChangesSaved = true;

		private int boxHeight;
		private int tileYOffset = 30;

		// Image Presets
		private Image leftBumperWall;
		private Image spikes;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an empty map editor window whose editor has the
		/// specified number of rows and columns
		/// </summary>
		/// <param name="cols"> columns in editor </param>
		/// <param name="rows"> rows in editor </param>
		public MapEditor(int cols, int rows)
		{
			InitializeComponent();

			loadedMaps = new List<Tuple<int, string>>();
			loadedMaps.Add(new Tuple<int, string>(default, default));

			SetImagePresets();

			this.mapRows = rows;
			this.mapCols = cols;

			ResizeEditor(cols, rows);

			SetTileChoices();

			//buttonChoice1.Image = Image.FromFile("../../../SpikeTile.png");

			// Set default color
			curImage = buttonChoice1.Image;
			pictureCurTile.Image = curImage;
		}

		/// <summary>
		/// Creates a map editor window with a loaded map
		/// </summary>
		/// <param name="filename"> the map file to load </param>
		public MapEditor(string filename)
		{
			InitializeComponent();

			loadedMaps = new List<Tuple<int, string>>();

			SetImagePresets();

			LoadFile(filename);

			SetTileChoices();

			// Set default color
			curImage = buttonChoice1.Image;
			pictureCurTile.Image = curImage;
		}

		#endregion

		#region Methods

		#region Saving / Loading Methods
		/// <summary>
		/// Writes the all of the tile data to a .level file
		/// Stores TileType, Row, & Col
		/// </summary>
		/// <param name="filename"> where to save the file </param>
		private void SaveFile(string filename)
		{
			// Open the file
			StreamWriter output = new StreamWriter(filename);

			// Loop through each map in the loaded maps
			// and write its data to the save file
			foreach(Tuple<int, string> mapData in loadedMaps)
			{
				// Save map dimensions
				output.WriteLine($"//{mapData.Item1}");

				// Save obstacle position data
				output.WriteLine(mapData.Item2);
			}

			// Close the file
			output.Close();
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

				// Get rid of currently stored map files
				loadedMaps.Clear();

				// Fill list with map data
				string line = "";
				int mapDimension = - 1;
				string obsData = "";
				while ((line = input.ReadLine()!) != null)
				{
					// Lines that lack the '|' delimeter hold
					// map dimension data
					if (line.IndexOf('|') == -1)
					{
						mapDimension = int.Parse(line.Substring(2, line.Length - 2));
					}
					// Lines with the delimeter hold obstacle
					// position data
					else
					{
						obsData = line;
					}

                    // Check if have enough information to
                    // make a list addition
                    if (mapDimension != -1 && !string.IsNullOrEmpty(obsData))
                    {
                        // Add map data to list
                        loadedMaps.Add(new Tuple<int, string>(mapDimension, obsData));

                        // Reset info tracker variables
                        mapDimension = -1;
                        obsData = "";
                    }
                }
			}
			catch (Exception e)
			{
				Debug.WriteLine("Error: " + e.Message);
			}
			finally
			{
				// If a file was read 
				if (input != null)
				{
					// Load the first map in the storage
					curMapIndex = 0;
					LoadMap(curMapIndex);

					int test = loadedMaps.Count;

                    input.Close();
				}
			}
		}

		public void SaveMap(int index)
		{
			if (index < 0 || index >= loadedMaps.Count)
				throw new IndexOutOfRangeException
					($"Cannot save at invalid list index {index}");

			// Collect obstacle position data
			string obstaclePositions = "";
			for (int y = 1; y < tiles!.GetLength(0) - 1; y++)
			{
				for (int x = 1; x < tiles.GetLength(1) - 1; x++)
				{
					if (tiles[y, x].Image != null)
					{
						// Format: "TileType,Row,Col|"
						obstaclePositions +=
							$"{GetTileChar(tiles[y, x].Image)}," +
							$"{(tiles[y, x].Top - tileYOffset) / boxHeight}," +
							$"{tiles[y, x].Left / boxHeight}|";
					}
				}
			}

			// Overwrite information at index in list
			loadedMaps[index] = new Tuple<int, string>(
					mapCols,
					obstaclePositions.Substring(0, obstaclePositions.Length - 1));
		}

		public bool LoadMap(int index)
		{
			if (index < 0 || index >= loadedMaps.Count)
			{
				MessageBox.Show("Invalid Map Index");
				return false;
			}

			// Resize Editor to fit map
			ResizeEditor(loadedMaps[index].Item1, loadedMaps[index].Item1);

			// Load obstacles
			string[] obstacleData = loadedMaps[index].Item2.Split("|");

            for (int i = 0; i < obstacleData.Length; i++)
            {
                string[] obsCoords = obstacleData[i].Split(",");

                char type = char.Parse(obsCoords[0]);
                int row = int.Parse(obsCoords[1]);
                int col = int.Parse(obsCoords[2]);

                tiles[row, col].Image = GetTileImage(type);
            }

			return true;
		}

		#endregion

		#region Conversion Methods

		/// <summary>
		/// Gets the image corresponding to the 
		/// specified tile type
		/// </summary>
		/// <param name="tileType"></param>
		/// <returns></returns>
		private Image GetTileImage(char tileType)
		{
			switch (tileType)
			{
				case 'w':
					return leftBumperWall;

				case 's':
					return spikes;

				default:
					return null;
			}
		}

		/// <summary>
		/// Gets the character that represents this tile
		/// </summary>
		/// <param name="tileImage"></param>
		/// <returns></returns>
		private char GetTileChar(Image tileImage)
		{
			if (tileImage == leftBumperWall) return 'w';
			else if (tileImage == spikes) return 's';
			else return ' ';
		}
		#endregion

		#region Editor Setup Methods
		private void SetImagePresets()
		{
			leftBumperWall = Image.FromFile("../../../LeftBumperWall.png");
			spikes = Image.FromFile("../../../SpikeTile.png");
		}
		private void SetTileChoices()
		{
			// Set color choices
			buttonChoice1.Image = leftBumperWall;
			buttonChoice2.Image = spikes;
			buttonChoice3.BackColor = Color.Blue;
			buttonChoice4.BackColor = Color.Red;
			buttonChoice5.BackColor = Color.Orange;
			buttonChoice6.BackColor = Color.Purple;
		}

		#endregion

		#region Component Methods
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
				SaveMap(curMapIndex);
				SaveFile(saveWindow.FileName);

				// Report success to user
				MessageBox.Show("File saved successfully", "File saved",
					MessageBoxButtons.OK, MessageBoxIcon.Information);

				// User saved their changes
				curChangesSaved = true;
				this.Text = Text.Substring(0, Text.Length - 2); // Exclude asterisk
			}

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
				curChangesSaved = true;
			}
		}

		private void buttonPrev_Click(object sender, EventArgs e)
		{
            // Warn user about unsaved changes
            if (!curChangesSaved)
            {
                DialogResult choice =
                    MessageBox.Show(
                    "There are unsaved changes. Are you sure you want to switch?",
                    "Unsaved changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (choice == DialogResult.Yes)
                    UpdateSaveStatus(true);
                else
                    return;
            }

			// Try to load previous map
            if (!LoadMap(curMapIndex - 1)) return;

			// Keep track of map index
            curMapIndex--;
		}

		private void buttonNext_Click(object sender, EventArgs e)
		{
            // Warn user about unsaved changes
            if (!curChangesSaved)
            {
                DialogResult choice =
                    MessageBox.Show(
                    "There are unsaved changes. Are you sure you want to switch?",
                    "Unsaved changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);


				if (choice == DialogResult.Yes) 
					UpdateSaveStatus(true);
				else 
					return;
            }

			// Try to load next map
            if (!LoadMap(curMapIndex + 1)) return;

			// Keep track of map index
            curMapIndex++;
		}

		#endregion

		#region Component Subscriber Methods
		/// <summary>
		/// Changes selected image to that of 
		/// the image choice button clicked in editor
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ChooseImage(object sender, EventArgs e)
		{
			Button b = (Button)sender;

			// Change color to match color choice
			curImage = b.Image;

			// Display currently chosen color
			pictureCurTile.Image = curImage;
		}

		/// <summary>
		/// Sets the clicked tile to the currently selected image
		/// </summary>
		/// <param name="sender"> tile to change </param>
		/// <param name="e"></param>
		private void SetTileImage(object? sender, EventArgs e)
		{
			Tile tile = (Tile)sender!;

			int col = tile.Left / boxHeight;
			int row = (tile.Top - tileYOffset) / boxHeight;

			// Make edge tiles un-editable
			// Make tiles directly in front
			// of each door un-editable
			bool inFrontOfLeftDoor =
				col == 1 &&
				row == mapRows / 2;
			bool behindRightDoor =
				col == mapCols - 2 &&
				row == mapRows / 2;
			bool belowTopDoor =
				col == mapCols / 2 &&
				row == 1;
			bool aboveBottomDoor =
				col == mapCols / 2 &&
				row == mapRows - 2;

			if (col == 0 ||
				col == mapCols - 1 ||
				row == 0 ||
				row == mapRows - 1 ||
				inFrontOfLeftDoor || behindRightDoor ||
				belowTopDoor || aboveBottomDoor)
			{
				tileReportText.Text = "Invalid Tile!";
				return;
			}

			// Change tile color
			tile.Image = curImage;

			// User has made changes
			UpdateSaveStatus(false);

			// User has touched a valid tile
			tileReportText.Text = "";
		}
		#endregion

		/// <summary>
		/// Resizes the editor and window to fit the specified number of square tiles,
		/// and fills editor with blank tiles
		/// </summary>
		/// <param name="cols"></param>
		/// <param name="rows"></param>
		private void ResizeEditor(int cols, int rows)
		{
			// Create tile storage
			tiles = new Tile[rows, cols];

			mapRows = rows;
			mapCols = cols;

			// Determine tile dimensions
			boxHeight = (groupMapEditor.Height - tileYOffset) / rows;

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
					tiles[y, x] = new Tile();

					tiles[y, x].SetBounds(
						x * boxHeight,
						30 + y * boxHeight,
						boxHeight,
						boxHeight);

					// Edge tiles are walls
					if (tiles[y, x].Left == 0 ||
						tiles[y, x].Left / boxHeight == mapCols - 1 ||
						tiles[y, x].Top - tileYOffset == 0 ||
						tiles[y, x].Top / boxHeight == mapRows - 1)
					{
						tiles[y, x].BackgroundImage = Image.FromFile("../../../LeftBumperWall.png");
					}
					else
					{
						// Otherwise use floor image
						tiles[y, x].BackgroundImage = Image.FromFile("../../../PurpleTile.png");
					}

					tiles[y, x].Click += SetTileImage;

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
			if (!curChangesSaved)
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

		private void UpdateSaveStatus(bool saved)
		{
			if (!saved && curChangesSaved)
			{
                // User has made changes
                this.Text += " *";
                curChangesSaved = false;
            }
			else if (saved && !curChangesSaved)
			{
                // User saved their changes
                curChangesSaved = true;
                this.Text = Text.Substring(0, Text.Length - 2); // Exclude asterisk
            }
		}
		#endregion

	}
}
