using EasyMuisc.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using static EasyMuisc.ShareStaticResources;
using static EasyMuisc.Tools.Tools;
using static EasyMuisc.MusicHelper;
using System.Windows.Shell;
using Shell32;

namespace EasyMuisc
{
    [Serializable]
    public class MusicInfo
    {
        public string MusicName { get; internal set; }
        public string Singer { get; internal set; }
        public string Length { get; internal set; }
        public string Album { get; internal set; }
        public string Path { get; internal set; }
        public bool Enable { get; internal set; }
    }
    /// <summary>
    /// MusicList.xaml 的交互逻辑
    /// </summary>
    public partial class MusicList : UserControl
    {  
        /// <summary>
        /// 歌单二进制文件名
        /// </summary>
        
        public MusicList()
        {
            InitializeComponent();
            if (File.Exists(programDirectory + "\\" + MusicListName))
            {
                try
                {
                    musicDatas = DeserializeObject(File.ReadAllBytes(programDirectory + "\\" + MusicListName)) as ObservableCollection<MusicInfo>;
                }
                catch
                {
                    ShowAlert("歌单存档已损坏，将重置歌单。");
                    musicDatas = new ObservableCollection<MusicInfo>();
                }
            }
            else
            {
                musicDatas = new ObservableCollection<MusicInfo>();
            }
            lvw.DataContext = musicDatas;
        }

        private void MusicListLoadedEventHandler(object sender, RoutedEventArgs e)
        {
          
        }

        
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

            switch (e.Key)
            {
                case Key.Enter:
                    LvwItemPreviewMouseDoubleClickEventHandler(null, null);
                    break;
                case Key.Delete:
                    musicDatas.RemoveAt(lvw.SelectedIndex);
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
        /// 在列表项上按下按钮事件（Enter、Del）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItemKeyUpEventHandler(object sender, KeyEventArgs e)
        {
            if (lvw.SelectedIndex != 1)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        LvwItemPreviewMouseDoubleClickEventHandler(null, null);
                        break;
                    case Key.Delete:
                        RemoveMusic(SelectedIndex);
                        break;
                }
            }

        }


        public int SelectedIndex => lvw.SelectedIndex;
        public MusicInfo SelectedItem => lvw.SelectedItem as MusicInfo;
        public void SelectAndScroll(int index)
        {
            lvw.SelectedIndex = index;
            lvw.ScrollIntoView(lvw.SelectedItem);
        }
    }
    public static class MusicHelper

    {  
        /// <summary>
        /// 音乐信息，与列表绑定
        /// </summary>
        public static ObservableCollection<MusicInfo> musicDatas;
        /// <summary>
        /// 增加新的歌曲到列表中
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static MusicInfo AddMusic(string path)
        {

            try
            {
                foreach (var i in musicDatas)
                {
                    if (path == i.Path)
                    {
                        return i;
                    }
                }
                bool info = GetMusicInfo(path, out string name, out string singer, out string length, out string album);
                musicDatas.Add(new MusicInfo
                {
                    Enable = info,
                    MusicName = name,
                    Singer = singer,
                    Length = length.StartsWith("00:") ? length.Remove(0, 3) : length,
                    Path = path,
                    Album = album,
                });
                DoEvents();
                return musicDatas[musicDatas.Count - 1];
            }
            catch
            {
                return null;
            }

        }
      public static  string MusicListName = "EasyMusicList.bin";
        /// <summary>
        /// 历史记录
        /// </summary>
        private static List<MusicInfo> historyList = new List<MusicInfo>();
        /// <summary>
        /// 当前播放历史索引
        /// </summary>
       private static int historyIndex = -1;
        /// <summary>
        /// 当前音乐在列表中的索引
        /// </summary>
        public static int musicIndex = 0;
        /// <summary>
        /// 获取音乐的信息
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="name">音乐的标题，若没有则返回无扩展名的文件名</param>
        /// <param name="singer">歌手</param>
        /// <param name="length">时长</param>
        public static bool GetMusicInfo(string path, out string name, out string singer, out string length, out string album)
        {
            if (!File.Exists(path))
            {
                name = path;
                singer = "";
                length = "";
                album = "";
                return false;
            }
            ShellClass sh = new ShellClass();
            Folder dir = sh.NameSpace(Path.GetDirectoryName(path));
            FolderItem item = dir.ParseName(Path.GetFileName(path));
            name = dir.GetDetailsOf(item, 21);
            if (name == "")
            {
                name = new FileInfo(path).Name.Replace(new FileInfo(path).Extension, "");
            }
            singer = dir.GetDetailsOf(item, 13);
            length = dir.GetDetailsOf(item, 27);
            album = dir.GetDetailsOf(item, 14);
            return true;
        }
        /// <summary>
        /// 增加一组歌曲到列表中
        /// </summary>
        /// <param name="musics"></param>
        /// <param name="firstLoad"></param>
        /// <returns></returns>
        public static void AddMusic(string[] musics)
        {
            var taskBar = mainWindow.taskBar;
            taskBar.ProgressValue = 0;
            taskBar.ProgressState = TaskbarItemProgressState.Normal;
          mainWindow.  LoadingSpinner = true;

            //if (cfa.AppSettings.Settings["MusicList"] != null)
            //{
            int n = 1;
            foreach (var i in musics)
            {
                AddMusic(i);
                taskBar.ProgressValue = 1.0 * (n++) / musics.Length;
            }
            //}
            taskBar.ProgressState = TaskbarItemProgressState.None;
            mainWindow.LoadingSpinner = false;

        }

        public static void SaveListToFile()
        {
            File.WriteAllBytes(programDirectory + "\\" + MusicListName, SerializeObject(musicDatas));
        }

        public static int MusicCount => musicDatas.Count;

        public static MusicInfo CurrentMusic => musicDatas[musicIndex];
        public static int CurrentMusicIndex => musicIndex;
        public static void RemoveMusic()
        {
            musicDatas.Clear();
        }
        public static void RemoveMusic(int index)
        {
            musicDatas.RemoveAt(index);
        }
        public static MusicInfo GetMusic(int index) => musicDatas[index];
        public static int GetMusic(MusicInfo music) => musicDatas.IndexOf(music);
        public static void SetCurrent(int index)
        {
            musicIndex = index;
        }

        public static int HistoryCount => historyList.Count;
        public static int CurrentHistoryIndex
        {
            get => historyIndex;
            set => historyIndex = value;
        }
        public static MusicInfo CurrentHistory=>historyList[ historyIndex];
        public static void AddHistory(MusicInfo music)
        {
            historyList.Add(music);
            historyIndex++;
        }
        public static void RemoveHistory(int index,int count)
        {
            historyList.RemoveRange(index, count);
            historyIndex = index - 1;
        }
        public static MusicInfo GetHistory(int index)
        {
            return historyList[index];
        }

    }
}
