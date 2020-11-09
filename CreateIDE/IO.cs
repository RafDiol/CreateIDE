using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using System.IO.Packaging;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace CreateIDE
{
    class IO_Handler
    {
        public void OpenProject(Encoding encoding, out string projectPath, out string projectName, out string version)
        {
            try
            {
                // Get the creation file (.crt)
                string text;
                string[] data;
                OpenFileDialog fd = new OpenFileDialog();
                fd.FilterIndex = 1;
                fd.Filter = "Project Creation (*.prj)|*.prj";
                if (fd.ShowDialog() == DialogResult.OK) // Successfully selected a file
                {
                    projectPath = Path.GetDirectoryName(fd.FileName);
                    text = File.ReadAllText(fd.FileName, encoding);
                    data = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    version = data[0];
                    projectName = data[1];
                }
                else
                {
                    throw new IOException("The file dialog was closed by the user");
                }
            } catch (System.IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void SaveAsProject(string version ,out string projectPath, out string projectName)
        {
            SaveFileDialog fd = new SaveFileDialog();
            fd.FilterIndex = 1;
            fd.Filter = "Project Creation (*.prj)|*.prj";
            if (fd.ShowDialog() == DialogResult.OK) // Successfully selected a file
            {
                // Create some basic variables neededlater down the line
                projectPath = fd.FileName;
                projectName = System.IO.Path.GetFileName(projectPath);
                // Now lets save this project
                File.WriteAllText(projectPath, $"{version}{Environment.NewLine}{projectName}");
            }
            else
            {
                throw new IOException("The file dialog was closed by the user");
            }
        }
    }
}
