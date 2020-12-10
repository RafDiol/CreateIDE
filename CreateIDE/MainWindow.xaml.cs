using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Size = System.Drawing.Size;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration.Xml;
using System.Buffers;
using Path = System.IO.Path;
using System.Globalization;
using Clipboard = System.Windows.Clipboard;
using Orientation = System.Windows.Controls.Orientation;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Collections.Specialized;
using System.Threading;
using System.Runtime.CompilerServices;
using Application = System.Windows.Application;
using System.Diagnostics.CodeAnalysis;
using TextBox = System.Windows.Forms.TextBox;
using Button = System.Windows.Controls.Button;

namespace CreateIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constants
        const string VERSION = "1.0.0";

        CompletionWindow completionWindow;
        IList<ICompletionData> completionListData;
        IO_Handler ioHandler;
        string projectName, projectVersion, projectPath, projectFolderPath, filepath, filename;
        string text = "";
        Encoding encoding = Encoding.UTF8;
        private object dummyNode = null;
        bool isAutocompleteActive = false;
        private List<TabItem> tabItems;
        private TextEditor textEditor = null;

        public MainWindow()
        {
            InitializeComponent();
            // Initialize some components
            ioHandler = new IO_Handler();
            // Create Tab Layout
            CreateTabMenu();
            // File Viewer
            CreateFileView();
            fileViewer.ContextMenuClosing += fileViewerContextMenuClosing;

        }

        // Tab Methods
        private void CreateTabMenu()
        {
            // initialize tabItem array
            tabItems = new List<TabItem>();

            AddWelcomeTab();

            // bind tab control
            DynamicTab.DataContext = tabItems;
            DynamicTab.SelectedIndex = 0;
        }

        private void AddTabItem(string header, string tag=null)
        {
            // add a tabItem
            TabItem tabItem = new TabItem();
            tabItem.Header = getItemHeader(header);
            tabItem.Tag = tag;
            // Content
            TextEditor tabEditor = new TextEditor();
            tabEditor.Name = "tabEditor";
            tabEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
            tabEditor.FontSize = 15;
            tabEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            tabEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            tabItem.Content = tabEditor;
            tabEditor.ShowLineNumbers = true;
            textEditor = tabEditor;
            tabItems.Add(tabItem);
            // Set it up
            DynamicTab.DataContext = null;
            DynamicTab.DataContext = tabItems;
            // Set it to have focus
            DynamicTab.SelectedIndex = tabItems.Count - 1; // We subtruct 1 because we need to start counting from 0
        }


        private StackPanel getItemHeader(object itemObj)
        {
            string iconPath = "/AppIcons/close.png"; 
            // Create Stack Panel
            StackPanel stkPanel = new StackPanel();
            stkPanel.Orientation = Orientation.Horizontal;

            // Create Image
            Image img = new Image();
            img.MouseLeftButtonDown += CloseTab;
            img.Source = new BitmapImage(new Uri(iconPath, UriKind.Relative));
            img.Width = 22;
            img.Height = 22;

            // Create TextBlock
            TextBlock lbl = new TextBlock();
            lbl.Text = itemObj.ToString();

            // Add to stack
            stkPanel.Children.Add(lbl);
            stkPanel.Children.Add(img);

            return stkPanel;
        }

        private void CloseTab(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            StackPanel stkpanel = (StackPanel)img.Parent;
            TabItem tab = (TabItem)stkpanel.Parent;
            Console.WriteLine(tab.Tag);
            if (tab.Tag.ToString() != "welcome") // Just to prevent crushes and exceptions
            {
                if (File.ReadAllText(filepath) != textEditor.Text && MessageBox.Show("Do you want to save the changes made?", "Save Changes", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    // Save the file and the continue on closing the file
                    SaveFile();
                }
            }
            tabItems.Remove(tab);
            DynamicTab.DataContext = null;
            DynamicTab.DataContext = tabItems;
            DynamicTab.SelectedIndex = tabItems.Count - 1; // We subtract 1 because we need to start counting from index 0
        }

        private void AddWelcomeTab()
        {
            // add a tabItem
            TabItem tabItem = new TabItem();
            tabItem.Header = getItemHeader("Welcome");
            tabItem.Tag = "welcome";
            // Set the welcome message
            tabItem.Content = "\nWelcome to CreateIDE";
            tabItems.Add(tabItem);
            // Set it to have focus
            DynamicTab.SelectedIndex = tabItems.Count;
        }


        private void TabSelectionChanged(object sender, RoutedEventArgs e)
        {
            TabItem tempTabItem = (TabItem)DynamicTab.SelectedItem;
            if (tempTabItem != null && tempTabItem.Tag.ToString() != "welcome")
            {
                textEditor = (TextEditor)tempTabItem.Content;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string tabName = (sender as System.Windows.Controls.Button).CommandParameter.ToString();

            var item = DynamicTab.Items.Cast<TabItem>().Where(i => i.Name.Equals(tabName)).SingleOrDefault();

            TabItem tab = item;

            if (tab != null)
            {
                if (tabItems.Count < 3)
                {
                    MessageBox.Show("Cannot remove last tab.");
                }
                else if (System.Windows.MessageBox.Show(string.Format("Are you sure you want to remove the tab '{0}'?", tab.Header.ToString()), "Remove Tab", System.Windows.MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // get selected tab
                    TabItem selectedTab = DynamicTab.SelectedItem as TabItem;

                    // clear tab control binding
                    DynamicTab.DataContext = null;

                    tabItems.Remove(tab);

                    // bind tab control
                    DynamicTab.DataContext = tabItems;

                    // select previously selected tab. if that is removed then select first tab
                    if (selectedTab == null || selectedTab.Equals(tab))
                    {
                        selectedTab = tabItems[0];
                    }
                    DynamicTab.SelectedItem = selectedTab;
                }
            }
        }

        // Autocomplete Methods

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "." || e.Text == ";" || e.Text == "(" || e.Text == "{" || e.Text == " ")
            {
                if (isAutocompleteActive)
                {
                    // Open code completion after the user has pressed dot:
                    completionWindow = new CompletionWindow(textEditor.TextArea);
                    completionListData = completionWindow.CompletionList.CompletionData;
                    AssignCompletionDataToWindow();
                    completionWindow.Show();
                    completionWindow.Closed += delegate
                    {
                        completionWindow = null;
                    }; 
                }
            }
        }

        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null && isAutocompleteActive)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void AssignCompletionDataToWindow()
        {
            // integers signed 
            completionListData.Add(new CompletionData("int", "signed int"));
            completionListData.Add(new CompletionData("int?", "nullable int"));
            completionListData.Add(new CompletionData("int[]", "int[] keyword"));
            completionListData.Add(new CompletionData("Int16", "Represents a 16bit signed integer"));
            completionListData.Add(new CompletionData("Int32", "Represents a 32bit signed integer"));
            completionListData.Add(new CompletionData("Int64", "Represents a 64bit signed integer"));
            // Unssigned
            completionListData.Add(new CompletionData("uint", "unsigned int"));
            completionListData.Add(new CompletionData("uint?", "nullable uint"));
            completionListData.Add(new CompletionData("uint[]", "uint[] keyword"));
            completionListData.Add(new CompletionData("UInt16", "Represents a 16bit unsigned integer"));
            completionListData.Add(new CompletionData("UInt32", "Represents a 32bit unsigned integer"));
            completionListData.Add(new CompletionData("UInt64", "Represents a 64bit unsigned integer"));
            // Double
            completionListData.Add(new CompletionData("double", "Represents a double-precision floating point number"));
            completionListData.Add(new CompletionData("double[]", "double[] keyword"));
            completionListData.Add(new CompletionData("double?", "nullable double"));
            // Long
            completionListData.Add(new CompletionData("long", "Represents a signed 64bit integer"));
            completionListData.Add(new CompletionData("long[]", "long[] keyword"));
            completionListData.Add(new CompletionData("long?", "nullable long"));
            // ulong
            completionListData.Add(new CompletionData("ulong", "Represents a unsigned 64bit integer"));
            completionListData.Add(new CompletionData("long[]", "ulong keyword"));
            completionListData.Add(new CompletionData("long?", "nullable ulong"));
            // Short
            completionListData.Add(new CompletionData("short", "Represents a signed 16bit integer"));
            completionListData.Add(new CompletionData("short[]", "short[] keyword"));
            completionListData.Add(new CompletionData("short?", "nullable short"));
            // ushort
            completionListData.Add(new CompletionData("ushort", "Represents a unsigned 16bit integer"));
            completionListData.Add(new CompletionData("ushort[]", "ushort[] keyword"));
            completionListData.Add(new CompletionData("ushort?", "nullable ushort"));
            // String
            completionListData.Add(new CompletionData("string", "Represents text as a sequence of UTF-16 code units"));
            completionListData.Add(new CompletionData("string[]", "string[] keyword"));
            // Literals
            completionListData.Add(new CompletionData("null", "null keyword"));
            completionListData.Add(new CompletionData("true", "true keyword"));
            completionListData.Add(new CompletionData("false", "false keyword"));
            completionListData.Add(new CompletionData("default", "default keyword"));
            // logic statements
            completionListData.Add(new CompletionData("case", "case keyword"));
            completionListData.Add(new CompletionData("switch", "switch keyword"));
            completionListData.Add(new CompletionData("if", "if keyword"));
            completionListData.Add(new CompletionData("else", "else keyword"));
            // Namespace-Method keywords
            completionListData.Add(new CompletionData("private", "Access is limited to the containing type"));
            completionListData.Add(new CompletionData("public", "Not restricted access"));
            completionListData.Add(new CompletionData("protected", "Access is limited to the containing class or types derived from the containing class"));
            completionListData.Add(new CompletionData("internal", "Access is limited to the current assembly"));
            completionListData.Add(new CompletionData("void", "The method does not return a value"));
            completionListData.Add(new CompletionData("static", "A static member which belongs to the type itself rather than to a specific object"));
            completionListData.Add(new CompletionData("class", "class keyword"));
            completionListData.Add(new CompletionData("unsafe", "Denotes an unsafe context, which is required for any operation involving pointers"));
            completionListData.Add(new CompletionData("virtual", "Used to modify a method, property, indexer, or event declaration and allow for it to be overridden in a derived class"));
            completionListData.Add(new CompletionData("volatile", "Indicates that a field might be modified by multiple threads that are executing at the same time"));
            completionListData.Add(new CompletionData("namespace", "namespace keyword"));
            // Iteration Statements
            completionListData.Add(new CompletionData("do", "do keyword"));
            completionListData.Add(new CompletionData("for", "for keyword"));
            completionListData.Add(new CompletionData("foreach", "foreach keyword"));
            completionListData.Add(new CompletionData("while", "while keyword"));
            // Jump statements
            completionListData.Add(new CompletionData("break", "break keyword"));
            completionListData.Add(new CompletionData("return", "Terminates execution of the method in which it appears and returns control to the calling method. It can also return an optional value"));
            completionListData.Add(new CompletionData("goto", "goto keyword"));
            completionListData.Add(new CompletionData("continue", "continue keyword"));
            // Exception
            completionListData.Add(new CompletionData("throw", "throw keyword"));
            // Non Categorized
            completionListData.Add(new CompletionData("using", "using keyword"));
        }

        // The following methods have to do with the file menu
        private void OpenProject(object sender, RoutedEventArgs e)
        {
            OpenProject();
        } // Just calls the overload
        
        private void OpenProject()
        {
            try
            {
                ioHandler.OpenProject(encoding, out projectPath, out projectFolderPath, out projectName, out projectVersion);
            }
            catch (IOException)
            {
                // The user did not select a file so break
                return;
            }
            catch (IndexOutOfRangeException)
            {
                System.Windows.MessageBox.Show("The project file is either corrupted or out of date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Configure accordingly
            if (textEditor != null  && text != textEditor.Text)
            {
                // Save
            }
            CreateFileView();
            this.Title = $"{projectName} - CreateIDE";
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            SaveFile();
        } // Just calls the overload

        private void SaveFile(bool executedFromCode = false)
        {
            if (filepath != null && !executedFromCode)
            {
                filename = Path.GetFileName(filepath);
                File.WriteAllText(filepath, textEditor.Text);
            }
        }

        private void NewProject(object sender, RoutedEventArgs e)
        {
            NewProject();
        } // Just calls the overload
        private void NewProject()
        {
            try
            {
                ioHandler.NewProject(VERSION, out projectPath, out projectFolderPath, out projectName);
            }
            catch (IOException)
            {
                // The user did not select a file
                return;
            }
            // Configure accordingly
            if (textEditor != null) // Sanity Check
            {
                if (text != textEditor.Text)
                {
                    // Save
                }
            }
            this.Title = $"{projectName} - CreateIDE";
            CreateFileView();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            // Check  if there are unsaved changes
            System.Windows.Application.Current.Shutdown();
        }

        // The following methods have to do with the file viewer
        private void CreateFileView()
        {
            fileViewer.Items.Clear();
            if (projectPath == null)
            {
                return;
            } // So no Exception occurs during runtime


            foreach (string s in Directory.GetFileSystemEntries(projectFolderPath, "*", SearchOption.TopDirectoryOnly))
            {
                TreeViewItem item = new TreeViewItem();
                StylizeTreeViewItem(s, item);
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                item.MouseRightButtonUp += FileViewElementRightClicked;
                item.MouseDoubleClick += FileViewElementDoubleClick;
                fileViewer.Items.Add(item);
            }
        }

        private void FileViewElementDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem senderItem = sender as TreeViewItem;
            bool isDirecotry = File.GetAttributes(senderItem.Tag.ToString()).HasFlag(FileAttributes.Directory);
            if (isDirecotry)
            {
                return; // return because you cannot read a directory
            }
            if (senderItem.IsSelected)
            {
                StackPanel stackPanel = (StackPanel)senderItem.Header;
                TextBlock textBlock = (TextBlock)stackPanel.Children[1];
                AddTabItem(textBlock.Text, senderItem.Tag.ToString());
                if (filepath != null && File.ReadAllText(filepath) == textEditor.Text)
                {
                    SaveFile();
                }
                filepath = senderItem.Tag.ToString();
                filename = Path.GetFileName(filepath);
                textEditor.Text = File.ReadAllText(filepath);
                if (filename.EndsWith(".cs"))
                {
                    isAutocompleteActive = true;
                    textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
                } else
                {
                    isAutocompleteActive = false;
                    textEditor.SyntaxHighlighting = null;
                }
            }
        }

        private void CreateFileView(object sender, RoutedEventArgs e)
        {
            CreateFileView();
        } // Just calls the overload

        private void FileViewElementRightClicked(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem SelectedItem = sender as TreeViewItem;

            fileViewer.ContextMenu = fileViewer.Resources["ContextMenu"] as System.Windows.Controls.ContextMenu;
        }
        private void fileViewerContextMenuClosing(object sender, RoutedEventArgs e)
        {
            fileViewer.ContextMenu = null;
        }
        private void StylizeTreeViewItem(string s, TreeViewItem item)
        {
            string tempfilename;
            FileAttributes attr = File.GetAttributes(s);
            tempfilename = Path.GetFileName(s);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                item.Header = CustomizeTreeViewItem(Path.GetFileName(s), "Icons/folder.png");
                item.Items.Add(dummyNode);
            }
            else
            {
                if (tempfilename.EndsWith(".cs")) // C#
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/cs.png");
                }
                else if (tempfilename.EndsWith(".py")) // Python
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/python.png");
                }
                else if (tempfilename.EndsWith(".sql") || tempfilename.EndsWith(".awtb") || tempfilename.EndsWith(".db") || tempfilename.EndsWith(".dat") || tempfilename.EndsWith(".php") || tempfilename.EndsWith(".php4") || tempfilename.EndsWith(".php5")) // Database
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/db.png");
                }
                else if (tempfilename.EndsWith(".xml"))
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/xml.png");
                }
                else if (tempfilename.EndsWith(".txt"))
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/txt.png");
                }
                else if (tempfilename.EndsWith(".png") || tempfilename.EndsWith(".jpg") || tempfilename.EndsWith(".jpeg") || tempfilename.EndsWith(".bmp") || tempfilename.EndsWith(".svg"))
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/img.png");
                }
                else if (tempfilename.EndsWith(".prj"))
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/project.png");
                }
                else if (tempfilename.EndsWith(".dll"))
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/dll.png");
                }
                else if (tempfilename.EndsWith(".zip") || tempfilename.EndsWith(".rar") || tempfilename.EndsWith(".7z"))
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/zip.png");
                }
                else if (tempfilename.EndsWith(".pdf"))
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/pdf.png");
                }
                else
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/unknown.png");
                }
            }
        }
        private StackPanel CustomizeTreeViewItem(object itemObj, string path)
        {
            // Add Icon
            // Create Stack Panel
            StackPanel stkPanel = new StackPanel();
            stkPanel.Orientation = Orientation.Horizontal;

            // Create Image
            Image img = new Image();
            img.Source = new BitmapImage(new Uri(path, UriKind.Relative));
            img.Width = 20;
            img.Height = 20;

            // Create TextBlock
            TextBlock lbl = new TextBlock();
            lbl.Text = itemObj.ToString();

            // Add to stack
            stkPanel.Children.Add(img);
            stkPanel.Children.Add(lbl);

            return stkPanel;
        }
        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetFileSystemEntries(item.Tag.ToString(), "*", SearchOption.TopDirectoryOnly))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        StylizeTreeViewItem(s, subitem);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        subitem.MouseRightButtonUp += FileViewElementRightClicked;
                        subitem.MouseDoubleClick += FileViewElementDoubleClick;
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }

        // Context Menu Commands

        private void AddFile(object sender, RoutedEventArgs e)
        {
            TreeViewItem SelectedItem = fileViewer.SelectedItem as TreeViewItem;
            string tempfilename = "";
            if (Dialogs.InputBox("Add File", "Enter the filename and extension", "Untitled.cs", ref tempfilename) == System.Windows.Forms.DialogResult.OK)
            {
                if (File.GetAttributes(SelectedItem.Tag.ToString()).HasFlag(FileAttributes.Directory))
                {
                    File.Create(Path.Combine(SelectedItem.Tag.ToString(), tempfilename));
                } else
                {
                    File.Create(Path.Combine(Path.GetDirectoryName(SelectedItem.Tag.ToString()), tempfilename));
                }
                CreateFileView();
            }
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            TreeViewItem SelectedItem = fileViewer.SelectedItem as TreeViewItem;
            string tempfilename = "";
            if (Dialogs.InputBox("Add Folder", "Enter the folder name", "New Folder", ref tempfilename) == System.Windows.Forms.DialogResult.OK)
            {
                if (Directory.Exists(Path.Combine(SelectedItem.Tag.ToString(), tempfilename)))
                {
                    if (MessageBox.Show($"A folder with the name {tempfilename} already exists do you wish to overwrite it?", "Folder Already Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                    {
                        ioHandler.DeleteDirectory(Path.Combine(SelectedItem.Tag.ToString(), tempfilename));
                        Directory.CreateDirectory(Path.Combine(SelectedItem.Tag.ToString(), tempfilename));
                    }
                } 
                else
                {
                    if (File.GetAttributes(SelectedItem.Tag.ToString()).HasFlag(FileAttributes.Directory))
                    {
                        Directory.CreateDirectory(Path.Combine(SelectedItem.Tag.ToString(), tempfilename));
                    } else
                    {
                        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(SelectedItem.Tag.ToString()), tempfilename));
                    }
                }
                CreateFileView();
            }
        }

        private void CopyFullPath(object sender, RoutedEventArgs e)
        {
            TreeViewItem SelectedItem = fileViewer.SelectedItem as TreeViewItem;
            Clipboard.Clear();
            Clipboard.SetText(SelectedItem.Tag.ToString());
        }

        private void RenameFile(object sender, RoutedEventArgs e)
        {
            TreeViewItem SelectedItem = fileViewer.SelectedItem as TreeViewItem;
            string newName = "";
            FileInfo fileInfo = new FileInfo(SelectedItem.Tag.ToString());
            if (Dialogs.InputBox("Rename", "New name", fileInfo.Name, ref newName) == System.Windows.Forms.DialogResult.OK)
            {
                fileInfo.MoveTo(Path.Combine(fileInfo.Directory.FullName, newName));
                CreateFileView();
            }
        }

        private void DeleteFile(object sender, RoutedEventArgs e)
        {
            TreeViewItem SelectedItem = fileViewer.SelectedItem as TreeViewItem;
            string tempfilepath = SelectedItem.Tag.ToString();
            FileInfo fileInfo = new FileInfo(SelectedItem.Tag.ToString());
            if (MessageBox.Show($"'{fileInfo.Name}' will be deleted permanantly.", "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(tempfilepath))
                {
                    File.Delete(tempfilepath);
                } else if (Directory.Exists(tempfilepath))
                {
                    ioHandler.DeleteDirectory(tempfilepath);
                }
                CreateFileView();
            }
        }

        private void CopyFiles(object sender, RoutedEventArgs e)
        {
            TreeViewItem SelectedItem = fileViewer.SelectedItem as TreeViewItem;
            StringCollection paths = new StringCollection();
            paths.Add(SelectedItem.Tag.ToString());
            Clipboard.SetFileDropList(paths);
        }

        private void PasteFiles(object sender, RoutedEventArgs e)
        {
            TreeViewItem SelectedItem = fileViewer.SelectedItem as TreeViewItem;
            FileAttributes attr = File.GetAttributes(SelectedItem.Tag.ToString());
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                foreach (string filepath in Clipboard.GetFileDropList())
                {
                    if (File.Exists(filepath)) // So we don't get any FileNotFound exceptions if the user has moved/deleted the file
                    {
                        File.Copy(filepath, Path.Combine(SelectedItem.Tag.ToString(), Path.GetFileName(filepath)));
                    } else if (Directory.Exists(filepath))
                    {
                        ioHandler.CopyDirectory(filepath, Path.Combine(SelectedItem.Tag.ToString(), Path.GetFileName(filepath)), true);
                    }
                }
                CreateFileView();
            }
        }

        // The class for the autocompletion items
        public class CompletionData : ICompletionData
        {
            public CompletionData(string text, string description)
            {
                Text = text;
                DescriptionText = description;
            }

            public System.Windows.Media.ImageSource Image
            {
                get { return null; }
            }

            public string Text { get; private set; }
            public string DescriptionText { get; private set; }

            // Use this property if you want to show a fancy UIElement in the list.
            public object Content
            {
                get { return this.Text; }
            }

            public object Description
            {
                get { return "" + DescriptionText; }
            }

            public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, Text);
            }
            public double Priority
            {
                get { return 1; }
            }
        }

        // A GUI Class
        public class Dialogs
        {
            public static DialogResult InputBox(string title, string promptText, string defaultvalue, ref string value)
            {
                Form form = new Form();
                System.Windows.Forms.Label label = new System.Windows.Forms.Label();
                System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Button buttonOk = new System.Windows.Forms.Button();
                System.Windows.Forms.Button buttonCancel = new System.Windows.Forms.Button();

                form.Text = title;
                label.Text = promptText;
                textBox.Text = defaultvalue;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
                buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                textBox.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new System.Windows.Forms.Control[] { label, textBox, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                DialogResult dialogResult = form.ShowDialog();
                value = textBox.Text;
                return dialogResult;
            }
        }
    }
}
