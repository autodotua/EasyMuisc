using EasyMusic.Info;
using System;
using System.IO;

namespace EasyMusic.Windows
{
    /// <summary>
    /// winMusicInfo.xaml 的交互逻辑
    /// </summary>
    public partial class WinMusicInfo : WindowBase
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
                      FzLib.UI.Dialog.MessageBox.ShowException("加载失败", ex);
                      Close();
                  }
              };
        }
    }
}