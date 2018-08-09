using EasyMusic.Info;
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
    /// winMusicInfo.xaml 的交互逻辑
    /// </summary>
    public partial class WinMusicInfo : Window
    {
        public WinMusicInfo(MusicInfo music)
        {
            InitializeComponent();
            Loaded += (p1, p2) =>
              {
                  try
                  {

                      FileInfo fileInfo = new FileInfo(music.Path);
                      Title = fileInfo.Name + "-音乐信息";
                      string l = Environment.NewLine;
                      string info = fileInfo.Name + l
                      + music.Path + l
                      + Math.Round(fileInfo.Length / 1024d) + "KB" + l
                      + music.Name + l
                      + music.Length + l
                      + music.Singer + l
                      + music.Album;
                      txt.Text = info;
                  }
                  catch (Exception ex)
                  {
                      WpfControls.Dialog.DialogHelper.ShowException("加载失败", ex);
                      Close();
                  }
              };
        }
    }
}
