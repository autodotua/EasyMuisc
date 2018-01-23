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

namespace EasyMuisc.Windows
{
    /// <summary>
    /// WinAbout.xaml 的交互逻辑
    /// </summary>
    public partial class WinAbout : Window
    {
        public WinAbout()
        {
            InitializeComponent();
            var newLine = Environment.NewLine;
            txt1.Text = "制作者：方震" + newLine
                + "邮箱：autodotua@outlook.com" + newLine
                + "编译时间：" + System.IO.File.GetLastWriteTime(GetType().Assembly.Location) + newLine
                + "日志：";
                txt2.Text= Properties.Resources.日志;
        }
    }
}
