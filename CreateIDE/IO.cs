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
        public void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            string filePath = "";
            foreach (FileInfo file in files)
            {
                try
                {
                    filePath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(filePath, false);
                } catch (System.IO.IOException)
                {
                    if (System.IO.Directory.Exists(filePath))
                    {
                        DeleteDirectory(filePath);
                        CopyDirectory(sourceDirName, destDirName, true);
                    }
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        public void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        public void OpenProject(Encoding encoding, out string projectPath, out string projectFolderPath, out string projectName, out string version, out string startMethod)
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
                    projectPath = fd.FileName;
                    projectFolderPath = System.IO.Path.GetDirectoryName(projectPath);
                    text = File.ReadAllText(fd.FileName, encoding);
                    data = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    version = data[0];
                    projectName = data[1];
                    startMethod = data[2];
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
    }
}
