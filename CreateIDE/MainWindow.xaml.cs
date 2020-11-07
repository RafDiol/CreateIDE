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
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration.Xml;

namespace YourIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CompletionWindow completionWindow;
        IList<ICompletionData> completionListData;
        public MainWindow()
        {
            InitializeComponent();
            textEditor.ShowLineNumbers = true;
            // For code completion
            textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
        }

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
                completionWindow.Closed += delegate {
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
    }

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
}
