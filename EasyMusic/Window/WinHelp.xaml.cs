using System.IO;
using System.Text;
using System.Windows;

namespace EasyMusic.Windows
{
    /// <summary>
    /// WinHelp.xaml 的交互逻辑
    /// </summary>
    public partial class WinHelp : WindowBase
    {
        public WinHelp()
        {
            InitializeComponent();
            MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(Properties.Resources.strHelp));
            rtx.Selection.Load(stream, DataFormats.Rtf);
        }
    }
}