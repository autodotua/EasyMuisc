using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static FzLib.Control.Dialog.DialogHelper;
using EasyMusic.Helper;
using EasyMusic.UserControls;

namespace EasyMusic.Windows
{
    /// <summary>
    /// winAlbumPicture.xaml 的交互逻辑
    /// </summary>
    public partial class WinAlbumPicture : Window
    {
        bool winMainTopMost = false;
        public WinAlbumPicture(Header header)
        {
            InitializeComponent();
            Left = MainWindow.Current.Left ; // + 12;
            Top = MainWindow.Current.Top;// + win.ActualHeight - win.imgAlbum.Height - 16;
            Width = header.imgAlbum.ActualWidth;
            Height = header.imgAlbum.ActualHeight;
            if(MainWindow.Current.Topmost)
            {
                winMainTopMost = true;
            }
        }
        
        private void ImgPreviewMouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void WindowLoadedEventHandler(object sender, RoutedEventArgs e)
        { 

            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;
            double targetHeight = 500;
            double targetWidth = 500;
            DoubleAnimation aniTop = new DoubleAnimation()
            {
                To = 0.5 * (screenHeight - targetHeight),
            Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                DecelerationRatio = 0.3,
            };
            Storyboard.SetTargetName(aniTop, Name);
            Storyboard.SetTargetProperty(aniTop, new PropertyPath(TopProperty));

            DoubleAnimation aniLeft = new DoubleAnimation()
            {
                To = 0.5 * (screenWidth - targetWidth),
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                DecelerationRatio = 0.3,
            };
            Storyboard.SetTargetName(aniLeft, Name);
            Storyboard.SetTargetProperty(aniLeft, new PropertyPath(LeftProperty));

            DoubleAnimation aniWidth = new DoubleAnimation()
            {
                To = targetWidth,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                DecelerationRatio = 0.3,
            };
            Storyboard.SetTargetName(aniWidth, Name);
            Storyboard.SetTargetProperty(aniWidth, new PropertyPath(WidthProperty));

            DoubleAnimation aniHeigth = new DoubleAnimation()
            {
                To = targetHeight,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                DecelerationRatio = 0.3,
            };
            Storyboard.SetTargetName(aniHeigth, Name);
            Storyboard.SetTargetProperty(aniHeigth, new PropertyPath(HeightProperty));
             
            Storyboard story1 = new Storyboard();
            story1.Children.Add(aniTop);
            story1.Children.Add(aniLeft);
            story1.Completed += (p1,p2)=>
              {
                  Storyboard story2 = new Storyboard();
                  story2.Children.Add(aniHeigth);
                  story2.Children.Add(aniWidth);
                  story2.Begin(this);
              };
            story1.Begin(this);
        }

        private void MenuCloseClickEventHandler(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuSaveClickEventHandler(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = "jpg",
                Title = "保存专辑图",
                Filter = "JPG图片(*.jpg)|*.jpg",
                FileName = MusicControlHelper.Music.Name,
            };
            sfd.FileOk += delegate
              {
                  try
                  {
                      File.Copy(new Uri(img.Source.ToString()).AbsolutePath, sfd.FileName, true);
                  }
                  catch(Exception ex)
                  {
                      ShowException("保存文件失败" , ex);
                  }
              };
            sfd.ShowDialog();
        }

        private void ImgPreviewKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Escape)
            {
                Close();
            }
        }

        private void WinAlbumPicKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void WinAlbumPicClosingEventHandler(object sender, EventArgs e)
        {
            MainWindow.Current.Topmost = winMainTopMost;
        }
    }
}
