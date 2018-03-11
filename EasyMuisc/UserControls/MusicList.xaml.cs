using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using static EasyMuisc.ShareStaticResources;
using static EasyMuisc.MusicHelper;
using System.IO;
using static WpfControls.Dialog.DialogHelper;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using WpfControls.Dialog;

namespace EasyMuisc
{
    [Serializable]
    public class MusicInfo
    {
        public string MusicName { get;  set; }
        public string Singer { get;  set; }
        public string Length { get;  set; }
        public string Album { get;  set; }
        public string Path { get;  set; }
    }
    /// <summary>
    /// MusicList.xaml 的交互逻辑
    /// </summary>
    public partial class MusicList : UserControl
    {
     public   UserControls.ToggleButton lastMusicListBtn;
        

        public MusicList()
        {
            InitializeComponent();
            //if(!Directory.Exists(ToAbstractPath()))
            //{
            //    try
            //    {
            //        Directory.CreateDirectory(ToAbstractPath());
            //        File.Create(ToAbstractPath(set.DefautMusicList));
            //    }
            //    catch(Exception ex)
            //    {
            //        ShowException("歌单目录不存在且无法创建", ex);
            //    }

            //    musicDatas = new ObservableCollection<MusicInfo>();
            //}
            //else
            //{
            
                //ReadFileToList(set.DefautMusicList);
                if(!Directory.Exists(ToAbstractPath()))
                {
                try
                {
                    Directory.CreateDirectory(ToAbstractPath());
                }
                catch(Exception ex)
                {
                    ShowException("目录不存在且无法创建", ex);
                    return;
                }
                
            }
                if(Directory.EnumerateFiles(ToAbstractPath()).Count()==0)
            {
                try
                {
                    File.Create(ToAbstractPath("默认"));
                }
                catch (Exception ex)
                {
                    ShowException("不存在任何歌单且无法创建", ex);
                    return;
                }
            }
                foreach (var i in Directory.EnumerateFiles(ToAbstractPath()))
                {
                    UserControls.ToggleButton button = null;
                    FileInfo file = new FileInfo(i);
                button = new UserControls.ToggleButton() { Text = file.Name.Replace(file.Extension, "") };

                if (i == ToAbstractPath(set.LastMusicList))
                    {
                        lastMusicListBtn = button;
                    }
                stkMusiList.Children.Add(button);
                button.Select += BtnSelectEventHandler;
                    button.PreviewMouseRightButtonDown += BtnPreviewMouseRightButtonDownEventHandler;
                
                }
                if(lastMusicListBtn==null)
            {
                // (stkMusiList.Children[1] as UserControls.ToggleButton).IsPressed = true;
                lastMusicListBtn = stkMusiList.Children[1] as UserControls.ToggleButton;
            }
            lastMusicListBtn.RaiseClickEvent();

            ResetItemsSource();
            
        }
        /// <summary>
        /// 歌单按钮右键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPreviewMouseRightButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
          
            var btn = sender as UserControls.ToggleButton;

            MenuItem menuRename = new MenuItem() { Header = "重命名" };
            menuRename.Click += (p1, p2) =>
              {
                  if (!File.Exists(ToAbstractPath(btn.Text)))
                  {
                      if (ShowMessage("歌单文件不存在，是否直接从界面删除？", DialogType.Information, MessageBoxButton.YesNo) == 1)
                      {
                          RemoveButton();
                      }
                  }
                  else
                  {
                      if (GetInput("请输入目标名称（不含后缀名）：", out string name, lvw.Background as SolidColorBrush,btn.Text, @"^[^\/:*\?\”“\<>|,]+$"))
                          {
                          try
                          {
                              RenameMusicListFile(btn.Text, name, false);
                              //if(btn.Text==set.DefautMusicList)
                              //{
                              //    set.DefautMusicList = name;
                              //}
                              btn.Text = name;
                          }
                          catch (Exception ex)
                          {
                              ShowException("重命名歌单文件失败", ex);
                          }
                      }
                  }
              };

            MenuItem menuDelete = new MenuItem() { Header="删除" };
            menuDelete.Click += (p1, p2) =>
              {
                  if(!File.Exists(ToAbstractPath(btn.Text)))
                  {
                      if(ShowMessage("歌单文件不存在，是否直接从界面删除？", DialogType.Warn, MessageBoxButton.YesNo) == 1)
                      {
                          RemoveButton();
                      }
                  }
                  else
                  {
                      try
                      {
                          DeleteMusicListFile(btn.Text, false);
                          RemoveButton();
                      }
                      catch(Exception ex)
                      {
                          ShowException("删除歌单文件失败", ex);
                      }
                  }
              };


            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = this,
                IsOpen = true,
            };
            menu.Items.Add(menuRename);
            if(stkMusiList.Children.Count>2)
            {
                menu.Items.Add(menuDelete);
            }
            void RemoveButton()
            {
                if(btn.IsPressed)
                {
                    lastMusicListBtn = null;
                }
                string temp = btn.Text;
                stkMusiList.Children.Remove(btn);
                //if(temp==set.DefautMusicList)
                //{
                //    ShowPrompt("默认歌单被删除，请重新选择默认歌单。");
                //    new Windows.WinSettings().ShowDialog();
                //}
                (stkMusiList.Children[1] as UserControls.ToggleButton).RaiseClickEvent();
            }
        }
        /// <summary>
        /// 歌单按钮选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelectEventHandler(object sender, EventArgs e)
        {
            UserControls.ToggleButton btn = sender as UserControls.ToggleButton;
            foreach (var i in stkMusiList.Children)
            {
                if(!(i is UserControls.ToggleButton) || i==sender )
                {
                    continue;
                }
                (i as UserControls.ToggleButton).IsPressed = false; 
            }
            if (lastMusicListBtn != null && musicDatas!=null)
            {
                SaveListToFile(lastMusicListBtn.Text, false);
            }
            lastMusicListBtn = btn;

            ReadFileToList(btn.Text);
             
            
            ResetItemsSource();

        }
        /// <summary>
        /// 刷新列表
        /// </summary>
        public void ResetItemsSource() => lvw.ItemsSource = musicDatas;
        /// <summary>
        /// 双击列表项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LvwItemPreviewMouseDoubleClickEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (CurrentHistoryIndex < HistoryCount - 1)
            {
                RemoveHistory(CurrentHistoryIndex + 1, HistoryCount- CurrentHistoryIndex - 1);
            }
            musicIndex = lvw.SelectedIndex;
          mainWindow.  PlayCurrent();
        }
        /// <summary>
        /// 在列表项上按下按钮事件，包括打开、删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvwItemPreviewKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            if(lvw.SelectedIndex==-1)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.Enter:
                    LvwItemPreviewMouseDoubleClickEventHandler(null, null);
                    break;
                case Key.Delete:
                    RemoveAllSelection();
                    break;
            }

        }
        /// <summary>
        /// 在列表项上单击鼠标右键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItemMouseRightButtonUpEvetnHandler(object sender, MouseButtonEventArgs e)
        {
            if (lvw.SelectedIndex != -1)
            {
               mainWindow. BtnListOptionClickEventHanlder(sender, null);

            }
        }
        /// <summary>
        /// 删除所有选中的项
        /// </summary>
        public void RemoveAllSelection()
        {
            MusicInfo[] list = lvw.SelectedItems.Cast<MusicInfo>().ToArray();
            foreach (var i in list)
            {
                RemoveMusic(i);
            }
        }
        /// <summary>
        /// 选择的项的索引
        /// </summary>
        public int SelectedIndex => lvw.SelectedIndex;
        /// <summary>
        /// 选择的项
        /// </summary>
        public MusicInfo SelectedItem => lvw.SelectedItem as MusicInfo;
        /// <summary>
        /// 定位到
        /// </summary>
        /// <param name="index"></param>
        public void SelectAndScroll(int index)
        {
            lvw.SelectedIndex = index;
            lvw.ScrollIntoView(lvw.SelectedItem);
        }
        /// <summary>
        /// 单击增加歌单按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (GetInput("请输入文件名：", out string name, lvw.Background as SolidColorBrush))
            {
              var  button = new UserControls.ToggleButton() { Text = new FileInfo(name).Name };
        
            button.Select += BtnSelectEventHandler;
            button.PreviewMouseRightButtonDown += BtnPreviewMouseRightButtonDownEventHandler;
                stkMusiList.Children.Add(button);
                try
                {
                    using (File.Create(ToAbstractPath(name)))
                    { }
                    button.RaiseClickEvent();
                    scv.ScrollToRightEnd();
                }
                catch(Exception ex)
                {
                    ShowException("创建歌单文件失败", ex);
                }
            }
        }
        /// <summary>
        /// 鼠标滚轮在歌单按钮上滚动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewerPreviewMouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta>0)
            {
                scv.LineLeft();
            }
            else
            {
                scv.LineRight();
            }
        }
        
    }

    public class WidthConverter : IValueConverter
    {
        //当值从绑定源传播给绑定目标时，调用方法Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return (double)value - 64;
        }
        //当值从绑定目标传播给绑定源时，调用此方法ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception();
        }
    }
}
