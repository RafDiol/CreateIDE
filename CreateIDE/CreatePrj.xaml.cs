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

namespace YourIDE
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CreatePrj : Window
    {
        public CreatePrj()
        {
            InitializeComponent();
        }

        public string getProjectPath()
        {
            return projectPathTextBox.Text;
        }

        public string getProjectNamespace()
        {
            return namespaceTextbox.Text;
        }

        public string getProjectName()
        {
            return projectNameTextBox.Text;
        }

        private void CreatePrj_Btn(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChoosePath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                projectPathTextBox.Text = fd.SelectedPath;
            }
        }
    }
}
