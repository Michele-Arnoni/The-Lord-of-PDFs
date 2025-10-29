using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;


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

        // Fields for drag-and-drop functionality 
        private TreeViewItem _draggedItem = null;
        private Point _startPoint;

        private TreeViewItem _rightClickedItem; // Field to store the right-clicked item

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

            // Show placeholder text in the PDF viewer initially
            ShowPlaceholder(true);
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


        // Methods for drag-and-drop functionality

        //helper method to find ancestor of a specific type in the visual tree
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject 
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        // helper method to get expanded paths
        private HashSet<string> GetExpandedPaths(ItemsControl parent)
        {
            var paths = new HashSet<string>();
            foreach (object item in parent.Items)
            {
                var tvItem = item as TreeViewItem;
                if (tvItem != null)
                {
                    if (tvItem.IsExpanded)
                    {
                        paths.Add(tvItem.Tag.ToString());
                    }
                    foreach (var path in GetExpandedPaths(tvItem))
                    {
                        paths.Add(path);
                    }
                }
            }
            return paths;
        }

        // helper method to set expanded paths
        private void SetExpandedPaths(ItemsControl parent, HashSet<string> expandedPaths)
        {
            foreach (object item in parent.Items)
            {
                var tvItem = item as TreeViewItem;
                if (tvItem != null)
                {
                    string path = tvItem.Tag.ToString();
                    if (expandedPaths.Contains(path))
                    {
                        tvItem.IsExpanded = true;
                    }
                    SetExpandedPaths(tvItem, expandedPaths);
                }
            }
        }

        // STEP 1 (drag & drop): user starts dragging an item whith mouse left button down
        private void TreeView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // save the start point and the dragged item
            _startPoint = e.GetPosition(null);
            _draggedItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
        }

        // STEP 2 (drag & drop): user moves the mouse with left button pressed
        private void TreeView_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // if left button is not pressed or no item is being dragged, exit
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released || _draggedItem == null)
                return;

            // calculate if the distance of the mouse since the drag started is enough to start a drag-and-drop operation
            Point currentPosition = e.GetPosition(null);
            Vector diff = _startPoint - currentPosition;

            if (Math.Abs(diff.X) > 5.0 || Math.Abs(diff.Y) > 5.0)
            {
                // start the drag-and-drop operation
                // _draggedItem is the source of the drag
                DragDrop.DoDragDrop(_draggedItem, _draggedItem, DragDropEffects.Move);
                _draggedItem = null; // reset the dragged item after the operation
            }
        }

        // STEP 3 (drag & drop): user drags the item over another item
        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            // find the target item (the one under the mouse)
            var targetItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

            // fin the source item (the one being dragged)
            var sourceItem = e.Data.GetData(typeof(TreeViewItem)) as TreeViewItem;

            // Default: no drop allowed
            e.Effects = DragDropEffects.None;

            if (targetItem == null || sourceItem == null || targetItem == sourceItem)
            {
                e.Handled = true;
                return;
            }

            // check if the target item is a directory
            string targetPath = targetItem.Tag.ToString();
            FileAttributes attrs = File.GetAttributes(targetPath);

            if (attrs.HasFlag(FileAttributes.Directory))
            {
                // if target is a directory, allow drop
                e.Effects = DragDropEffects.Move;
            }

            e.Handled = true;
        }


        // STEP 4 (drag & drop): user drops the item
        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            var targetItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            var sourceItem = e.Data.GetData(typeof(TreeViewItem)) as TreeViewItem;

            // Basic validation
            if (targetItem == null || sourceItem == null || targetItem == sourceItem)
                return;

            string sourcePath = sourceItem.Tag.ToString();
            string targetPath = targetItem.Tag.ToString();

            // check if target is a directory
            FileAttributes attrs = File.GetAttributes(targetPath);
            if (!attrs.HasFlag(FileAttributes.Directory))
            {
                // if target is not a directory, set target to its parent directory
                targetPath = Path.GetDirectoryName(targetPath);
            }

            // you cannot move the vault root
            if (sourcePath == vaultPath)
            {
                return;
            }

            try
            {
                // save expanded paths
                HashSet<string> expandedPaths = GetExpandedPaths(fileTreeView);

                // execute the move operation on disk
                string destPath = Path.Combine(targetPath, Path.GetFileName(sourcePath));

                // make sure we are not overwriting existing files/folders
                if (File.Exists(destPath) || Directory.Exists(destPath))
                {
                    MessageBox.Show($"A file or folder with the name: '{Path.GetFileName(sourcePath)}' already exist in this destination.", "Conflict");
                    return;
                }

                // move the file or directory
                if (File.GetAttributes(sourcePath).HasFlag(FileAttributes.Directory))
                {
                    Directory.Move(sourcePath, destPath);
                }
                else
                {
                    File.Move(sourcePath, destPath);
                }

                // update the TreeView
                LoadVaultContents();

                // restore expanded paths
                SetExpandedPaths(fileTreeView, expandedPaths);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"drag & drop error: {ex.Message}", "Error");
                // if error occurs, reload the vault contents to ensure UI consistency
                LoadVaultContents();
            }
        }



        // Helper method to show/hide the placeholder text in the PDF viewer
        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // GET the selected item
            TreeViewItem selectedItem = fileTreeView.SelectedItem as TreeViewItem;

            if (selectedItem == null || selectedItem.Tag == null)
            {
                // no item selected
                ShowPlaceholder(true);
                return;
            }

            string path = selectedItem.Tag.ToString();
            FileAttributes attrs;

            try
            {
                attrs = File.GetAttributes(path);
            }
            catch (Exception)
            {
                // error getting attributes, show placeholder
                ShowPlaceholder(true);
                return;
            }


            // check if it's a file or directory
            if (!attrs.HasFlag(FileAttributes.Directory))
            {
                // selected item is a file so we can try to display it
                ShowPlaceholder(false); // hide the placeholder text

                // load the PDF file in the WebView
                // note: WebView control can display PDF files directly
                pdfWebView.Source = new Uri($"file:///{path}");
            }
            else
            {
                // selected item is a directory
                ShowPlaceholder(true); // show the placeholder text
            }
        }

        /// <summary>
        /// function to show or hide the placeholder text in the PDF viewer 
        /// </summary>
        private void ShowPlaceholder(bool show)
        {
            if (show)
            {
                placeholderText.Visibility = Visibility.Visible;
                pdfWebView.Source = new Uri("about:blank"); // clear the WebView
                pdfWebView.Visibility = Visibility.Collapsed;
            }
            else
            {
                placeholderText.Visibility = Visibility.Collapsed;
                pdfWebView.Visibility = Visibility.Visible;
            }
        }

        // Context Menu: RENAME functionality
        private void TreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // find the item that was right-clicked
            _rightClickedItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

            /* If no item was right-clicked or if the right-clicked item is the VAULT root,
             * we prevent the context menu from opening by setting e.Handled = true
             */
            if (_rightClickedItem == null || _rightClickedItem.Tag.ToString() == vaultPath)
            {
                e.Handled = true; // 'e.Handled = true' prevents the context menu from opening
            }
        }

        // Handler for the "Rename" menu item click
        private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_rightClickedItem == null) return;

            // find the StackPanel that is the Header of the TreeViewItem
            var header = _rightClickedItem.Header as StackPanel;
            if (header == null) return;

            // find the TextBlock inside the StackPanel
            var textBlock = header.Children.OfType<TextBlock>().FirstOrDefault();
            if (textBlock == null) return;

            // create a TextBox to allow renaming
            var editBox = new TextBox
            {
                Text = textBlock.Text, // pre-fill with the current name
                FontSize = 16,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0)
            };

            // hide the original TextBlock...
            header.Visibility = Visibility.Collapsed;

            // ...and set the TextBox as the new Header
            _rightClickedItem.Header = editBox;

            // focus and select all text in the TextBox
            editBox.Focus();
            editBox.SelectAll();
            editBox.LostFocus += EditBox_LostFocus; // in case the user clicks away
            editBox.KeyDown += EditBox_KeyDown;     // handle Enter/Esc keys
        }

        // Handlers for the TextBox events during renaming
        private void EditBox_KeyDown(object sender, KeyEventArgs e)
        {
            var editBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                // confirm the rename
                editBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            else if (e.Key == Key.Escape)
            {
                // cancel the rename
                // find the 'TreeViewItem' that contains this TextBox
                var item = FindAncestor<TreeViewItem>(editBox);
                if (item != null)
                {
                    // the rename is cancelled, so we restore the original header
                    string originalName = Path.GetFileName(item.Tag.ToString());
                    bool isDirectory = File.GetAttributes(item.Tag.ToString()).HasFlag(FileAttributes.Directory);
                    item.Header = CreateHeaderStackPanel(originalName, isDirectory ? "folderICON.png" : "pdfICON.png");
                }
            }
        }

        private void EditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var editBox = sender as TextBox;
            // find the 'TreeViewItem' that contains this TextBox
            var item = FindAncestor<TreeViewItem>(editBox);

            if (item == null)
            {
                LoadVaultContents(); // error case, reload everything
                return;
            }

            string newName = editBox.Text.Trim(); // new name entered by the user
            string oldPath = item.Tag.ToString();
            string oldName = Path.GetFileName(oldPath);
            string iconName;
            bool isDirectory;

            try
            {
                isDirectory = File.GetAttributes(oldPath).HasFlag(FileAttributes.Directory);
                iconName = isDirectory ? "folderICON.png" : "pdfICON.png";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading the attribute: {ex.Message}");
                LoadVaultContents(); // error case, reload everything
                return;
            }

            // case 1: user entered an empty name or the same name
            if (string.IsNullOrWhiteSpace(newName) || newName == oldName)
            {
                // cancel the rename
                item.Header = CreateHeaderStackPanel(oldName, iconName);
                return;
            }

            // case 2: proceed with renaming
            try
            {
                string parentPath = Path.GetDirectoryName(oldPath);
                string newPath = Path.Combine(parentPath, newName);

                // check for invalid characters
                if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    MessageBox.Show("Il nome contiene caratteri non validi.", "Errore");
                    item.Header = CreateHeaderStackPanel(oldName, iconName); // cancel
                    return;
                }

                // check for name conflicts
                if (File.Exists(newPath) || Directory.Exists(newPath))
                {
                    MessageBox.Show("Un file o cartella con questo nome esiste già.", "Conflitto");
                    item.Header = CreateHeaderStackPanel(oldName, iconName); // cancel
                    return;
                }

                // --- ok, rename it ! ---

                // rename on disk
                if (isDirectory)
                    Directory.Move(oldPath, newPath);
                else
                    File.Move(oldPath, newPath);

                // update the TreeViewItem
                item.Tag = newPath; // update the path
                item.Header = CreateHeaderStackPanel(newName, iconName); // update the header
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la rinomina: {ex.Message}");
                // on error, reload the vault contents to ensure UI consistency
                LoadVaultContents();
            }
        }
    }
}