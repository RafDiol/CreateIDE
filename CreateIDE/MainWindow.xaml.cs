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
<<<<<<< HEAD
using System.Threading;
using System.Runtime.CompilerServices;
using Application = System.Windows.Application;
using System.Diagnostics.CodeAnalysis;
using TabControl = System.Windows.Controls.TabControl;
=======
>>>>>>> parent of 90aec28... Before Tabs

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
        string projectName, projectVersion, projectPath;
        string text = "";
        Encoding encoding = Encoding.UTF8;
        private object dummyNode = null;
        TextEditor textEditor = new TextEditor();
        public MainWindow()
        {
            InitializeComponent();
            // Initialize some components
            ioHandler = new IO_Handler();
            // Do some visual stuff
            textEditor.ShowLineNumbers = true;
            // For code completion
            textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            // File Viewer
            CreateFileView();
            fileViewer.ContextMenuClosing += fileViewerContextMenuClosing;
<<<<<<< HEAD
            // Create Tab System
            CreateTabs();

=======
>>>>>>> parent of 90aec28... Before Tabs
        }
        // Tab System
        private void CreateTabs()
        {
            TabControl tc = new TabControl();
            tc.Margin = new Thickness(0, 0, 0, 0);
            tc.Name = "DynamicTabControl";
            tc.Items.Add("This is awesome");
            TabItem tabItem = tc.Items[0] as TabItem;
            tabItem.Content = textEditor;
            
        }

        // Completion Window

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            Console.WriteLine(e.Text);
            if (e.Text == "." || e.Text == ";" || e.Text == "(" || e.Text == "{" || e.Text == " ")
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

        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
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


        private void OpenProject(object sender, RoutedEventArgs e)
        {
            OpenProject();
        } // Just calls the overload
        private void OpenProject()
        {
            try
            {
                ioHandler.OpenProject(encoding, out projectPath, out projectName, out projectVersion);
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
            if (text != textEditor.Text)
            {
                // Save
            }
            CreateFileView();
            this.Title = $"{projectName} - CreateIDE";
        }


        private void SaveAsProject(object sender, RoutedEventArgs e)
        {
            SaveAsProject();
        } // Just calls the overload
        private void SaveAsProject()
        {
            try
            {
                ioHandler.SaveAsProject(VERSION, out projectPath, out projectName);
            }
            catch (IOException)
            {
                // The user did not select a file
                return;
            }
            // Configure accordingly
            if (text != textEditor.Text)
            {
                // Save
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


            foreach (string s in Directory.GetFileSystemEntries(projectPath, "*", SearchOption.TopDirectoryOnly))
            {
                TreeViewItem item = new TreeViewItem();
                StylizeTreeViewItem(s, item);
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                item.MouseRightButtonUp += FileViewElementRightClicked;
                fileViewer.Items.Add(item);
            }
        }
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
            string filename;
            FileAttributes attr = File.GetAttributes(s);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                item.Header = CustomizeTreeViewItem(Path.GetFileName(s), "Icons/folder.png");
                item.Items.Add(dummyNode);
            }
            else
            {
                filename = Path.GetFileName(s);
                if (filename.EndsWith(".cs")) // C#
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/cs.png");
                }
                else if (filename.EndsWith(".py")) // Python
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/python.png");
                }
                else if (filename.EndsWith(".sql") || filename.EndsWith(".awtb") || filename.EndsWith(".db") || filename.EndsWith(".dat") || filename.EndsWith(".php") || filename.EndsWith(".php4") || filename.EndsWith(".php5")) // Database
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/db.png");
                }
                else if (filename.EndsWith(".xml"))
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/xml.png");
                }
                else if (filename.EndsWith(".txt"))
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/txt.png");
                }
                else if (filename.EndsWith(".png") || filename.EndsWith(".jpg") || filename.EndsWith(".jpeg") || filename.EndsWith(".bmp") || filename.EndsWith(".svg"))
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/img.png");
                }
                else if (filename.EndsWith(".prj"))
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/project.png");
                }
                else if (filename.EndsWith(".dll"))
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/dll.png");
                }
                else if (filename.EndsWith(".zip") || filename.EndsWith(".rar") || filename.EndsWith(".7z"))
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/zip.png");
                }
                else if (filename.EndsWith(".pdf"))
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/pdf.png");
                }
                else
                {
                    item.Header = CustomizeTreeViewItem(filename, "/Icons/unknown.png");
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
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }

        // Context Menu Commands

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
            FileInfo fileInfo = new FileInfo(SelectedItem.Tag.ToString());
            if (MessageBox.Show($"'{fileInfo.Name}' will be deleted permanantly.", "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
            {
                File.Delete(SelectedItem.Tag.ToString());
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
