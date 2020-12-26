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
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using CreateIDE;
using CompileOption = CreateIDE.MainWindow.CompileOption;
using Dialogs = CreateIDE.MainWindow.Dialogs;
using MainWindow = CreateIDE.MainWindow;

namespace CreateIDE
{
    /// <summary>
    /// Interaction logic for RunConfig.xaml
    /// </summary>
    public partial class RunConfig : Window
    {
        // These are the compiler options saved locally
        // There is no need to save the other parameters locally since there are already referenced 
        // in the MainWindow.cs file
        string sourceFile;



        public RunConfig()
        {
            InitializeComponent();
        }

        // This method assigns the default or last used values to our GUI components
        public void supplyArgs(string[] references, string sourceFile, int WarningLevel, bool TreatWarningsAsErrors, string compilerOptions, bool IncludeDebugInfo,
            string startMethod)
        {
            // References
            for (int i = 0; i < references.Count(); i++)
            {
                // Add a reference if necessary
                if (!refListBox.Items.Contains(references[i]))
                {
                    refListBox.Items.Add((MainWindow.references[i]));
                    Console.WriteLine(MainWindow.references[i]);
                }
            }

            this.sourceFile = sourceFile;

            // Need to subtract one since we need to start counting from 0
            warningLvlComboBox.SelectedIndex = WarningLevel--;
        }

        private void ChangeSourceFile(object sender, RoutedEventArgs e) 
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.FilterIndex = 1;
            fd.Filter = "C# File (*.cs)|*.cs |All Files (*.*)|*.*";
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK) // Successfully selected a file
            {
                sourceFile = fd.FileName;
            }
        }

        // The following functions have to do with reference ListBox
        private void AddRef(object sender, RoutedEventArgs e)
        {
            string reference = ""; // needs to not be equal to null in order to use the keyword ref in the following line
            if (Dialogs.InputBox("Add File", "Enter the filename and extension", "Untitled.cs", ref reference) == System.Windows.Forms.DialogResult.OK)
            {
                refListBox.Items.Add(reference);
            }
        }

        private void RemoveRef(object sender, RoutedEventArgs e)
        {
            if (refListBox.SelectedItem != null) // A sanity check. I ain't want no NullReferenceException's popping up
            {
                refListBox.Items.Remove(refListBox.SelectedItem);
            }
        }

        // The apply button event listener
        // here we update all the compiler parameters
        private void Apply(object sender, RoutedEventArgs e)
        {
            /*
             * We will update the variables on the MainWindow.cs file with the latest
             * assigned values
             */

            // Compiler Output Option
            MainWindow.CompOpt = (MainWindow.CompileOption)outputTypeComboBox.SelectedIndex;
            
            // Source File
            MainWindow.sourceFile = sourceFile;

            // Auto-run the executable (only works for the .exe file type)
            // Need to cast bool? to bool
            MainWindow.autoRunExe = (bool)autoRunCheckBox.IsChecked;

            // References
            /*
            * The reason that we do not just clear the references and then just re-append them one by one is because
            * if a project has a lot of references then we do not want to be re-creating the reference list from scratch
            * every time due to performance concerns
            */
            for (int i=0; i < refListBox.Items.Count - 1; i++)
            {
                // Add a reference if necessary
                if (!MainWindow.references.Contains(refListBox.Items[i]))
                {
                    MainWindow.references.Append((MainWindow.references[i]));
                }
            }

            // Start Method
            MainWindow.startMethod = startMethodTxtBox.Text;

            // Warning Level
            MainWindow.WarningLvl = int.Parse(warningLvlComboBox.SelectedItem.ToString());

            // Treat Warnings as Errors
            // Need to cast bool? to bool
            MainWindow.TreatWarningsAsErrors = (bool)treatWarningsAsErrorsCheckbox.IsChecked;

            // Include Debug Info
            // Need to cast bool? to bool
            MainWindow.IncludeDebugInfo = (bool)inculdeDebugInfo.IsChecked;
        }
    }
}
