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

namespace YourIDE
{
    /// <summary>
    /// Interaction logic for Log.xaml
    /// </summary>
    public partial class Log : Window
    {
        public Log()
        {
            InitializeComponent();
        }

        public void Error(string error)
        {
            if (log.Text != "")
            {
                log.Text += $"\n{error}";
            } else
            {
                log.Text += error;
            }
        }

        public void Success(string msg)
        {
            log.Text += msg;
        }
    }
}
