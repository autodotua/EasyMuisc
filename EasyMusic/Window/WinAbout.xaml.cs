using System;

namespace EasyMusic.Windows
{
    /// <summary>
    /// WinAbout.xaml 的交互逻辑
    /// </summary>
    public partial class WinAbout : WindowBase
    {
        public WinAbout()
        {
            InitializeComponent();
            var newLine = Environment.NewLine;
            txt1.Text = "制作者：方震" + newLine
                + "邮箱：autodotua@outlook.com" + newLine
                + "编译时间：" + System.IO.File.GetLastWriteTime(GetType().Assembly.Location) + newLine
                + "日志：";
            txt2.Text = Properties.Resources.日志;
        }
    }
}