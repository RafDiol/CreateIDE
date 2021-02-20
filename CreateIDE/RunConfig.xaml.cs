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
using Settings = YourIDE.Properties.Settings;
using System.Collections.Specialized;

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
            // Update the arguments with their latest values
            updateArgs();
        }

        // This method assigns the default or last used values to our GUI components
        public void updateArgs()
        {
            // References
            refListBox.Items.Clear();
            for (int i = 0; i < Settings.Default.references.Count; i++)
            {
                refListBox.Items.Add(Settings.Default.references[i]);
            }

            this.sourceFile = Settings.Default.sourceFile;

            warningLvlComboBox.SelectedIndex = Settings.Default.warningLvl;

            outputTypeComboBox.SelectedIndex = Settings.Default.compileOption;

            autoRunCheckBox.IsChecked = Settings.Default.autoRun;

            treatWarningsAsErrorsCheckbox.IsChecked = Settings.Default.treatWarningsAsErrors;

            inculdeDebugInfoCheckbox.IsChecked = Settings.Default.includeDebugInfo;
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
            if (Dialogs.InputBox("Add File", "Enter the filename and extension", "", ref reference) == System.Windows.Forms.DialogResult.OK)
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
            Settings.Default.compileOption = outputTypeComboBox.SelectedIndex;
            
            // Source File
            Settings.Default.sourceFile = sourceFile;

            // Auto-run the executable (only works for the .exe file type)
            // Need to cast from nullable bool (bool?) to bool
            Settings.Default.autoRun = (bool)autoRunCheckBox.IsChecked;

            // References
            Settings.Default.references.Clear();
            for (int i=0; i < refListBox.Items.Count; i++)
            {
                Settings.Default.references.Add(refListBox.Items[i].ToString());
            }

            // Warning Level
            Settings.Default.warningLvl = warningLvlComboBox.SelectedIndex;

            // Treat Warnings as Errors
            // Need to cast bool? to bool
            Settings.Default.treatWarningsAsErrors = (bool)treatWarningsAsErrorsCheckbox.IsChecked;

            // Include Debug Info
            // Need to cast bool? to bool
            Settings.Default.includeDebugInfo = (bool)inculdeDebugInfoCheckbox.IsChecked;

            // Now lets save the settings
            Settings.Default.Save();
        }
    }
}
