using System;
using System.Collections.Generic;
using System.IO;
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

namespace EasyMusic.Windows
{
    /// <summary>
    /// WinHelp.xaml 的交互逻辑
    /// </summary>
    public partial class WinHelp : Window
    {
        public WinHelp()
        {
            InitializeComponent();
            MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(Properties.Resources.strHelp));
            rtx.Selection.Load(stream, DataFormats.Rtf);
        }
    }
}
