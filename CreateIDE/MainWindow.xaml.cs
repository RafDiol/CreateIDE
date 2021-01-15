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
using YourIDE;

namespace CreateIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constants
        const string VERSION = "1.0.0";
        public enum CompileOption
        {
            EXE = 0,
            DLL = 1
        }

        CompletionWindow completionWindow;
        IList<ICompletionData> completionListData;
        IO_Handler ioHandler;
        Compiler compiler;
        string projectName, projectVersion, projectPath, projectFolderPath, filepath, filename;
        string text = "";
        Encoding encoding = Encoding.UTF8;
        private object dummyNode = null;
        bool isAutocompleteActive = false;
        private List<TabItem> tabItems;
        private TextEditor textEditor = null;
        // Compiler Parameters
        /*
         * The Compiler parameters are static because they need to be referenced by the RunConfig.cs file without
         * the instantiation of the MainWindow object.
         */

        // The default references
        public string[] references = { "System", "System.Linq", "System.IO"};
        public int WarningLvl = 3;
        public bool TreatWarningsAsErrors = false, IncludeDebugInfo = true, autoRunExe = true;
        public string compilerOptions = "", startMethod = "Main", sourceFile;
        public CompileOption CompOpt = CompileOption.EXE;
        string Namespace;

        public MainWindow()
        {
            InitializeComponent();
            // Initialize some components
            ioHandler = new IO_Handler();
            compiler = new Compiler();
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
            tabEditor.Name = $"tabEditor";
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
            DynamicTab.SelectedIndex = tabItems.Count - 1; // We subtract 1 because we need to start counting from 0
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
            TabItem tab = null;
            try
            {
                Image img = (Image)sender;
                StackPanel stkpanel = (StackPanel)img.Parent;
                tab = (TabItem)stkpanel.Parent;
                
                if (tab.Tag.ToString() != "welcome") // Just to prevent crushes and exceptions
                {
                    if (File.ReadAllText(filepath) != textEditor.Text && MessageBox.Show("Do you want to save the changes made?", "Save Changes", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        // Save the file and the continue on closing the file
                        SaveFile();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // Do nothing
            }
            finally
            {
                tabItems.Remove(tab);
                DynamicTab.DataContext = null;
                DynamicTab.DataContext = tabItems;
                DynamicTab.SelectedIndex = tabItems.Count - 1; // We subtract 1 because we need to start counting from index 0
                                                               // Add the welcome tab if no tabs are open
                if (tabItems.Count() == 0)
                {
                    AddWelcomeTab();
                }

            }
            
        }

        private void ForceCloseTab(TabItem tab)
        {
            tabItems.Remove(tab);
            DynamicTab.DataContext = null;
            DynamicTab.DataContext = tabItems;
            DynamicTab.SelectedIndex = tabItems.Count - 1; // We subtract 1 because we need to start counting from index 0
                                                           // Add the welcome tab if no tabs are open
            if (tabItems.Count() == 0)
            {
                AddWelcomeTab();
            }
        }

        private void AddWelcomeTab(bool hasFocus = true)
        {
            // add a tabItem
            TabItem tabItem = new TabItem();
            tabItem.Header = getItemHeader("Welcome");
            tabItem.Tag = "welcome";
            // Set the welcome message
            tabItem.Content = "\nWelcome to CreateIDE";
            tabItems.Add(tabItem);
            // Update the UI
            DynamicTab.DataContext = null;
            DynamicTab.DataContext = tabItems;
            if (hasFocus)
            {
                // Set it to have focus
                // We subtract 1 since we need to start counting from 0
                DynamicTab.SelectedIndex = tabItems.Count - 1;
            }
        }


        private void TabSelectionChanged(object sender, RoutedEventArgs e)
        {
            TabItem tempTabItem = (TabItem)DynamicTab.SelectedItem;
            if (tempTabItem != null && tempTabItem.Tag.ToString() != "welcome")
            {
                filepath = tempTabItem.Tag.ToString();
                filename = Path.GetFileName(filepath);
                textEditor = (TextEditor)tempTabItem.Content;
            }
            if (filename != null && textEditor != null) // If i remove this line i will get an error on initialization
            {
                if (filename.EndsWith(".cs"))
                {
                    isAutocompleteActive = true;
                    textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
                }
                else
                {
                    isAutocompleteActive = false;
                    textEditor.SyntaxHighlighting = null;
                }
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

        // Auto-complete Methods

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // Autoclose Brackets, parentheses etc...
            if (isAutocompleteActive)
            {
                if (e.Text == "(") { textEditor.Document.Insert(textEditor.TextArea.Caret.Offset, ")"); textEditor.TextArea.Caret.Offset--; }
                if (e.Text == "[") { textEditor.Document.Insert(textEditor.TextArea.Caret.Offset, "]"); textEditor.TextArea.Caret.Offset--; }
                if (e.Text == "{") { textEditor.Document.Insert(textEditor.TextArea.Caret.Offset ,"}"); textEditor.TextArea.Caret.Offset--; }
            }
            // When to open the autocompletion window
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
                CreatePrj window = new CreatePrj();
                window.ShowDialog();

                Namespace = window.getProjectNamespace();
                projectFolderPath = window.getProjectPath();
                projectName = window.getProjectName();
                projectPath = Path.Combine(window.getProjectPath(), projectName+".prj");
                File.WriteAllText(projectPath, VERSION);
                // Now lets dispose the window
                window = null;

                this.Title = $"{projectName} - CreateIDE";
                CreateFileView();
            }
            catch
            {

            }
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
            string senderTag = senderItem.Tag.ToString();
            bool isDirecotry = File.GetAttributes(senderItem.Tag.ToString()).HasFlag(FileAttributes.Directory);
            if (isDirecotry)
            {
                return; // return because you cannot read a directory
            }
            if (senderItem.IsSelected)
            {
                for (int i=0; i < tabItems.Count; i++)
                {
                    if (tabItems[i].Tag.ToString() == senderTag)
                    {
                        // Set the already open folder tab to have focus
                        DynamicTab.SelectedItem = tabItems[i];
                        // Return
                        return;
                    }
                }
                StackPanel stackPanel = (StackPanel)senderItem.Header;
                TextBlock textBlock = (TextBlock)stackPanel.Children[1];
                AddTabItem(textBlock.Text, senderTag);
                if (filepath != null && File.ReadAllText(filepath) == textEditor.Text)
                {
                    SaveFile();
                }
                filepath = senderItem.Tag.ToString();
                filename = Path.GetFileName(filepath);
                textEditor.Text = File.ReadAllText(filepath);
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
                else if (tempfilename.EndsWith(".xml") || tempfilename.EndsWith(".json"))
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
                else if (tempfilename.EndsWith(".exe"))
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/exe.png");
                }
                else if (tempfilename.EndsWith(".html") || tempfilename.EndsWith(".htm"))
                {
                    item.Header = CustomizeTreeViewItem(tempfilename, "/Icons/html.png");
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

        // Edit Menu Commands
        private void Undo(object sender, RoutedEventArgs e)
        {
            textEditor.Undo();
        }

        private void Redo(object sender, RoutedEventArgs e)
        {
            textEditor.Redo();
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            textEditor.SelectAll();
        }

        private void Copy(object sender, RoutedEventArgs e)
        {
            textEditor.Copy();
        }

        private void Paste(object sender, RoutedEventArgs e)
        {
            textEditor.Paste();
        }

        private void Cut(object sender, RoutedEventArgs e)
        {
            textEditor.Cut();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            textEditor.Delete();
        }

        // Context Menu Commands

        private void AddFile(object sender, RoutedEventArgs e)
        {
            try
            {
                TreeViewItem SelectedItem = fileViewer.SelectedItem as TreeViewItem;

                if (SelectedItem == null)
                {
                    return;
                } // Sanity Check

                string tempfilename = "";
                FileStream fileStream;
                if (Dialogs.InputBox("Add File", "Enter the filename and extension", "Untitled.cs", ref tempfilename) == System.Windows.Forms.DialogResult.OK)
                {
                    if (File.GetAttributes(SelectedItem.Tag.ToString()).HasFlag(FileAttributes.Directory))
                    {
                        if (File.Exists(Path.Combine(SelectedItem.Tag.ToString(), tempfilename)) && MessageBox.Show($"A file with the name '{tempfilename}' already exists do you wish to override it?", "File Exists", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            // If a file with the same name exists and the user wishes to override it do it
                            fileStream = File.Create(Path.Combine(SelectedItem.Tag.ToString(), tempfilename));
                        }
                        else
                        {
                            // If no file with the same name exists then create the file
                            fileStream = File.Create(Path.Combine(SelectedItem.Tag.ToString(), tempfilename));
                        }

                        // We need to close the assigned file stream. The reason that we create the file stream in the
                        // first place is because if we don't the method File.Create will return an unassigned fileStream which
                        // will cause an exception
                        fileStream.Close();
                        addDefautFileContent(Path.Combine(SelectedItem.Tag.ToString(), tempfilename));
                    }
                    else
                    {
                        if (File.Exists(Path.Combine(SelectedItem.Tag.ToString(), tempfilename)) && MessageBox.Show($"A file with the name '{tempfilename}' already exists do you wish to override it?", "File Exists", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            // If a file with the same name exists and the user wishes to override it do it
                            fileStream = File.Create(Path.Combine(Path.GetDirectoryName(SelectedItem.Tag.ToString()), tempfilename));
                        }
                        else
                        {
                            // If no file with the same name exists then create the file
                            fileStream = File.Create(Path.Combine(Path.GetDirectoryName(SelectedItem.Tag.ToString()), tempfilename));
                        }
                        // We need to close the assigned file stream. The reason that we create the file stream in the
                        // first place is because if we don't the method File.Create will return an unassigned fileStream which
                        // will cause an exception
                        fileStream.Close();
                        addDefautFileContent(Path.Combine(Path.GetDirectoryName(SelectedItem.Tag.ToString()), tempfilename));
                    }
                    CreateFileView();
                }

            } catch (System.NullReferenceException)
            {

            }
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            TreeViewItem SelectedItem = fileViewer.SelectedItem as TreeViewItem;
            if (SelectedItem == null)
            {
                return;
            } // Sanity Check

            string tempfilename = "";
            if (Dialogs.InputBox("Add Folder", "Enter the folder name", "New Folder", ref tempfilename) == System.Windows.Forms.DialogResult.OK)
            {
                if (File.GetAttributes(SelectedItem.Tag.ToString()).HasFlag(FileAttributes.Directory))
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
                        Directory.CreateDirectory(Path.Combine(SelectedItem.Tag.ToString(), tempfilename));
                    }
                 } 
                else
                 {
                    if (Directory.Exists(Path.Combine(Path.GetDirectoryName(SelectedItem.Tag.ToString()), tempfilename)))
                    {
                        if (MessageBox.Show($"A folder with the name {tempfilename} already exists do you wish to overwrite it?", "Folder Already Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                        {
                            ioHandler.DeleteDirectory(Path.Combine(Path.GetDirectoryName(SelectedItem.Tag.ToString()), tempfilename));
                            Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(SelectedItem.Tag.ToString()), tempfilename));
                        }
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
            if (SelectedItem == null) { return; }
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
            if (MessageBox.Show($"'{fileInfo.Name}' will be deleted permanently.", "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(tempfilepath))
                {
                    File.Delete(tempfilepath);
                } else if (Directory.Exists(tempfilepath))
                {
                    ioHandler.DeleteDirectory(tempfilepath);
                }
                CreateFileView();
                for (int i=0; i < tabItems.Count; i++)
                {
                    if (tabItems[i].Tag.ToString() == tempfilepath)
                    {
                        ForceCloseTab(tabItems[i]);
                        break;
                    }
                }
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

        private void addDefautFileContent(string path)
        {
            // Check whether default content exists for this kind of file
            if (path.Split('.').Last() == "html" || path.Split('.').Last() == "htm")
            {
                File.WriteAllText(path, File.ReadAllText("Presets/HTML.txt"));
            }
            else if (path.Split('.').Last() == "cs")
            {
                File.WriteAllText(path, File.ReadAllText("Presets/CSharp.txt"));
            }
        }
        
        // This method compiles the app and runs it
        private void Run(object sender, RoutedEventArgs e)
        {
            if (sourceFile == null)
            {
                MessageBox.Show("You must selected the file from which the execution will begin", "Source File", System.Windows.Forms.MessageBoxButtons.OK);
                OpenFileDialog fd = new OpenFileDialog();
                fd.FilterIndex = 1;
                fd.Multiselect = false;
                fd.Filter = "C# File(*.cs)|*.cs| All Files (*.*)|*.*";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK) // Successfully selected a file
                {
                    sourceFile = Path.Combine(projectFolderPath, fd.FileName);
                } else
                {
                    return;
                }
            }
            if (CompOpt == CompileOption.EXE)
            {
                // Compile
                compiler.CompileToExe(Path.Combine(projectFolderPath, "bin"), projectFolderPath, references, projectName.Split('.')[0], sourceFile,
                    WarningLvl, TreatWarningsAsErrors, compilerOptions, IncludeDebugInfo, startMethod);
            } else
            {
                compiler.CompileToDLL(Path.Combine(projectFolderPath, "bin"), projectFolderPath, references, projectName.Split('.')[0], sourceFile,
                    WarningLvl, TreatWarningsAsErrors, compilerOptions, IncludeDebugInfo);
            }
            // Refresh the File View Window
            CreateFileView();
        }

        private void ConfigRunSettings(object sender, RoutedEventArgs e)
        {
            ConfigRunSettings();
        } // Just Calls the overload

        private void ConfigRunSettings()
        {
            RunConfig window = new RunConfig();
            window.Show();
        }

        // The class for the auto-completion items
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
