using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace The_Lord_of_PDFs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /* Defining the VAULT path in the user's Documents folder
         * 
         * The Lord has to work in a dedicated folder called "my Castle" to manage PDF files.
         */
        private readonly string vaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "My CASTLE"
            );

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) //this event is triggered when the window is loaded
        {
            // Ensure the VAULT directory exists
            if (!Directory.Exists(vaultPath))
            {
                //if does not exist, create it
                Directory.CreateDirectory(vaultPath);
            }
            // Load existing PDF files into the TreeView
            LoadVaultContents();
        }

        // Method to load PDF files from the VAULT directory into the TreeView
        private void LoadVaultContents()
        {
            // Clear existing items from the TreeView
            fileTreeView.Items.Clear();

            // Create the root node for the VAULT
            TreeViewItem rootNode = new TreeViewItem
            {
                Header = CreateHeaderStackPanel(vaultPath, "castleICON.png"), //root node with castelICON
                Tag = vaultPath,
                IsExpanded = true
            };
            fileTreeView.Items.Add(rootNode); //adding the root node to the TreeView

            LoadDirectoryRecursive(vaultPath, rootNode); // Load files and directories recursively
        }

        /* Recursive method to load PDF files and directories
         * 
         * For each directory, it creates a TreeViewItem and adds it to the parent node.
         */
        private void LoadDirectoryRecursive(string directoryPath, TreeViewItem parentNode)
        {
            // Load all PDF files in the current directory
            foreach (var filePath in Directory.GetFiles(directoryPath, "*.pdf"))
            {
                TreeViewItem fileNode = new TreeViewItem
                {
                    Header = CreateHeaderStackPanel(Path.GetFileName(filePath), "pdfICON.png"), // Display only the file name and a PDF icon
                    Tag = filePath
                };
                parentNode.Items.Add(fileNode); //adding the file node to the parent node
            }
            // Recursively load subdirectories
            try
            {
                foreach (var dirPath in Directory.GetDirectories(directoryPath))
                {
                    TreeViewItem dirNode = new TreeViewItem
                    {
                        Header = CreateHeaderStackPanel(Path.GetFileName(dirPath), "folderICON.png"), // Display only the directory name and a folder icon
                        Tag = dirPath,
                        IsExpanded = false
                    };
                    parentNode.Items.Add(dirNode); //adding the directory node to the parent node
                    LoadDirectoryRecursive(dirPath, dirNode); // Recursive call for subdirectory
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle the case where access to a directory is denied
            }
            catch (Exception ex) //generic exception handler
            {
                MessageBox.Show($"Errore durante la lettura della cartella: {ex.Message}");
            }
        }

        private StackPanel CreateHeaderStackPanel(string text, string iconName)
        {
            // Create a panel to hold the icon and text
            StackPanel stack = new StackPanel
            {
                Orientation = Orientation.Horizontal 
            };

            // Create the icon (Image)
            Image icon = new Image
            {
                Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/{iconName}")), //all icons are stored in Assets folder
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 5, 0) // spacing between icon and text
            };
            RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.HighQuality); //best quality for the icon

            // Create the text block
            TextBlock textBlock = new TextBlock
            {
                Text = text,
                FontSize = 16
            };

            // Add icon and text to the stack panel
            stack.Children.Add(icon);
            stack.Children.Add(textBlock);

            return stack;
        }
    }
}