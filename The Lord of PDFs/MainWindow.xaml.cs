using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;


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

        private void BtnNewFolder_Click(object sender, RoutedEventArgs e)
        {
            /* Placeholder for "Create New Folder" functionality
            
             -three cases to handle:
                1) No node selected: create the new folder in the VAULT root
                2) Folder node selected: create the new folder inside the selected folder   
                3) File node selected: create the new folder in the parent folder of the selected file

             -Define a name for the new folder, ensuring it does not conflict with existing folders

             -Create the new folder in the determined location 

             -update UI (TreeView) to reflect the new folder creation, 
                !!! ATTENTION: we can't refresh all TreeView with LoadVaultContents() because it would collapse all folders !!!
             */

            TreeViewItem parentNode = fileTreeView.Items[0] as TreeViewItem;
            if (parentNode == null)
            {
                MessageBox.Show("Error: ROOT NODE NOT FOUND");
                return;
            }

            string parentPath = parentNode.Tag.ToString(); //default to VAULT root path

            TreeViewItem selectedItem = fileTreeView.SelectedItem as TreeViewItem; //get the selected item in the TreeView
            if (selectedItem != null)
            {
                string selectedPath = selectedItem.Tag.ToString(); //get the path of the selected item
                FileAttributes attrs = File.GetAttributes(selectedPath);

                if (attrs.HasFlag(FileAttributes.Directory)) // if item selected is a directory
                {
                    parentPath = selectedPath; //set parentPath to the selected directory
                    parentNode = selectedItem; //update parentNode to the selected directory node
                }
                else
                {
                    // if item selected is a file
                    parentPath = Path.GetDirectoryName(selectedPath); //get the parent directory of the file
                    parentNode = selectedItem.Parent as TreeViewItem; //update parentNode to the parent directory node
                }
            }

            string folderName = "New Folder";
            string newFolderPath = Path.Combine(parentPath, folderName);

            for (int i=0; Directory.Exists(newFolderPath); i++) //ensure unique folder name
            {
                folderName = "New Folder " + i.ToString();
                newFolderPath = Path.Combine(parentPath, folderName); //append number to name and try again
            }

            try
            {
                Directory.CreateDirectory(newFolderPath); //create the new folder

                //update the TreeView to reflect the new folder creation
                TreeViewItem newDirNode = new TreeViewItem
                {
                    Header = CreateHeaderStackPanel(folderName, "folderICON.png"), 
                    Tag = newFolderPath
                };
                parentNode.Items.Add(newDirNode); //adding the new folder node to the parent node
                parentNode.IsExpanded = true; //expand the parent node to show the new folder
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during new folter creation: {ex.Message}");
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for "Import PDF" functionality
            
            OpenFileDialog openDialog = new OpenFileDialog //create an OpenFileDialog to select PDF files
            {
                Title = "Import PDF in your CASTLE",
                Filter = "File PDF (*.pdf)|*.pdf", //show only .pdf
                Multiselect = true // you can select multiple files
            };

            if (openDialog.ShowDialog() == true)
            {
                // Determine the destination directory based on the selected node

                // Default: vault root
                TreeViewItem parentNode = fileTreeView.Items[0] as TreeViewItem;
                string parentPath = parentNode.Tag.ToString();

                TreeViewItem selectedItem = fileTreeView.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    string selectedPath = selectedItem.Tag.ToString();
                    FileAttributes attrs = File.GetAttributes(selectedPath);

                    if (attrs.HasFlag(FileAttributes.Directory)) // destination: folder selected
                    {
                        parentPath = selectedPath;
                        parentNode = selectedItem;
                    }
                    else // destination: parent of file selected
                    {
                        parentPath = Path.GetDirectoryName(selectedPath);
                        parentNode = selectedItem.Parent as TreeViewItem;
                    }
                }

                // Iterate through selected files and copy them to the destination directory
                try
                {
                    foreach (string sourceFilePath in openDialog.FileNames) 
                    {
                        // find a unique name to avoid overwriting existing files
                        string baseName = Path.GetFileNameWithoutExtension(sourceFilePath);
                        string extension = Path.GetExtension(sourceFilePath);
                        string destFileName = Path.GetFileName(sourceFilePath); 
                        string destFilePath = Path.Combine(parentPath, destFileName);
                        int counter = 1;

                        while (File.Exists(destFilePath))
                        {
                            destFileName = $"{baseName} ({counter}){extension}";
                            destFilePath = Path.Combine(parentPath, destFileName);
                            counter++;
                        }

                        // copy the file to the destination
                        File.Copy(sourceFilePath, destFilePath);

                        // update the TreeView to reflect the new file
                        TreeViewItem newFileNode = new TreeViewItem
                        {
                            Header = CreateHeaderStackPanel(destFileName, "pdfICON.png"),
                            Tag = destFilePath
                        };
                        parentNode.Items.Add(newFileNode);
                    }

                    // expand the parent node to show the newly imported files
                    parentNode.IsExpanded = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante l'importazione dei file: {ex.Message}", "Errore di importazione");
                }
            }
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for "Scan PDF" functionality
            MessageBox.Show("Logica 'Scansiona' da implementare.");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for "Delete" functionality

            TreeViewItem selectedItem = fileTreeView.SelectedItem as TreeViewItem; //get the selected item in the TreeView

            if (selectedItem == null || selectedItem.Tag.ToString() == vaultPath) return; //no item selected or item selected is the ROOT path (the vault), exit

            string itemName = " ";
            if (selectedItem.Header is StackPanel headerPanel)
            {
                var textBlock = headerPanel.Children.OfType<TextBlock>().FirstOrDefault();
                if (textBlock != null)
                {
                    itemName = textBlock.Text;
                }
            }
            MessageBoxResult result = MessageBox.Show(
                $"Do you want delete: '{itemName}'?\nThis operation cannot be undone.",
                "Confirm? ",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                return; // user cancelled the deletion
            }

            /*
             * Proceed with deletion:
             * 
             * We need to find the parent of the selected item to remove it from the TreeView after deletion.
             * 
             * the TreeView structure is a hierarchy of TreeViewItems
             */
            try
            {
                FileAttributes attrs = File.GetAttributes(selectedItem.Tag.ToString());
                if (attrs.HasFlag(FileAttributes.Directory))
                {
                    //selected item is a directory
                    Directory.Delete(selectedItem.Tag.ToString(), true); //delete the directory and its contents
                }
                else
                {
                    File.Delete(selectedItem.Tag.ToString()); //delete the file
                }

                /*
                 * ItemsControl is the base class for controls that contain a collection of items, TreeViewItem derives from ItemsControl
                 * 
                 * why? Because the Parent of selectedItem could be another TreeViewItem (if it's inside a folder) or the TreeView itself (if it's in the root)
                 * for this reason we use ItemsControl as the type for parent, is more generic
                 * 
                 */
                ItemsControl parent = selectedItem.Parent as ItemsControl; //get the parent of the selected item
                if (parent != null) {
                    parent.Items.Remove(selectedItem); //remove the selected item from its parent
                    return;
                } 
                else
                {
                    fileTreeView.Items.Remove(selectedItem); //fallback, should not happen because root is not deletable
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during deletion: {ex.Message}");
                return;
            }
        }
    }
}