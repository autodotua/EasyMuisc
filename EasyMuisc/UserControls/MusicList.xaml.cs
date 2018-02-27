using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using static EasyMuisc.ShareStaticResources;
using static EasyMuisc.MusicHelper;
using System.IO;
using static Dialog.DialogHelper;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

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
     public   UserControls.ToggleButton lastBtn;
        

        public MusicList()
        {
            InitializeComponent();
            if(!Directory.Exists(ToAbstractPath()))
            {
                try
                {
                    Directory.CreateDirectory(ToAbstractPath());
                    File.Create(ToAbstractPath(set.DefautMusicList));
                }
                catch(Exception ex)
                {
                    ShowException("歌单目录不存在且无法创建", ex);
                }

                musicDatas = new ObservableCollection<MusicInfo>();
            }
            else
            {
                ReadFileToList(set.DefautMusicList);
                foreach (var i in Directory.EnumerateFiles(ToAbstractPath()))
                {
                    UserControls.ToggleButton button = null;
                    FileInfo file = new FileInfo(i);
                    if (i == ToAbstractPath(set.DefautMusicList))
                    {
                         button = new UserControls.ToggleButton() { Text =file.Name.Replace(file.Extension,""), IsPressed=true};
                        lastBtn = button;
                        stkMusiList.Children.Insert(1, button);
                    }
                    else
                    {
                        button = new UserControls.ToggleButton() { Text = file.Name.Replace(file.Extension, "") };
                        stkMusiList.Children.Add(button);
                    }
                    button.Select += BtnSelectEventHandler;
                    button.PreviewMouseRightButtonDown += BtnPreviewMouseRightButtonDownEventHandler;
                
                }
            }
            ResetItemsSource();
            
        }
        /// <summary>
        /// 歌单按钮右键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPreviewMouseRightButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            if(stkMusiList.Children.Count==2)
            {
                return;
            }
            var btn = sender as UserControls.ToggleButton;

            MenuItem menuRename = new MenuItem() { Header = "重命名" };
            menuRename.Click += (p1, p2) =>
              {
                  if (!File.Exists(ToAbstractPath(btn.Text)))
                  {
                      if (ShowMessage("歌单文件不存在，是否直接从界面删除？", Dialog.DialogType.Information, MessageBoxButton.YesNo) == 1)
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
                              btn.Text = name;
                          }
                          catch (Exception ex)
                          {
                              ShowException("删除歌单文件失败", ex);
                          }
                      }
                  }
              };

            MenuItem menuDelete = new MenuItem() { Header="删除" };
            menuDelete.Click += (p1, p2) =>
              {
                  if(!File.Exists(ToAbstractPath(btn.Text)))
                  {
                      if(ShowMessage("歌单文件不存在，是否直接从界面删除？", Dialog.DialogType.Warn, MessageBoxButton.YesNo) == 1)
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
                Items = {menuRename, menuDelete },
                IsOpen = true,
            };

            void RemoveButton()
            {
                stkMusiList.Children.Remove(btn);
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

            SaveListToFile(lastBtn.Text, false);
            lastBtn = btn;

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
                    File.Create(ToAbstractPath(name));
                }
                catch(Exception ex)
                {
                    ShowException("创建歌单文件失败", ex);
                }
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
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
