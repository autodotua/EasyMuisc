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
using static EasyMuisc.GlobalDatas;
using static EasyMuisc.Tools.Tools;
using System.Windows.Shell;
using Shell32;
using static WpfControls.Dialog.DialogHelper;

namespace EasyMuisc
{
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
        public async static Task<MusicInfo> AddMusic(string path)
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
                string name = "";
                string singer = ""; ;
                string length = "";
                string album = "";
                await   Task.Run(() => GetMusicInfo(path, out name, out singer, out length, out album));
                var item = new MusicInfo()
                {
                    Name = name,
                    Singer = singer,
                    Length = GetIntLength(length),
                   // Length = length.StartsWith("00:") ? length.Remove(0, 3) : length,
                    Path = path,
                    Album = album,
                };
                musicDatas.Add(item);
               // DoEvents();
                return musicDatas[musicDatas.Count - 1];
            }
            catch
            {
                return null;
            }

        }
        private static int GetIntLength(string length)
        {
            int[] lengthArray = length.Split(':').Select(p => int.Parse(p)).ToArray();
            return lengthArray[0] * 3600 + lengthArray[1] * 60 + lengthArray[2];
        }
        public static string GetStringLength(int length)
        {
            int hour = length / 3600;
            length -= hour * 3600;
            int minute = length / 60;
            int second = length - minute * 60;
            if (hour != 0)
            {
                return hour.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00");
            }
            return minute.ToString("00") + ":" + second.ToString("00");
        }
        /// <summary>
        /// 历史记录
        /// </summary>
        public static List<MusicInfo> historyList = new List<MusicInfo>();
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
        public async static void AddMusic(string[] musics)
        {
            var taskBar = WinMain.taskBar;
            taskBar.ProgressValue = 0;
            taskBar.ProgressState = TaskbarItemProgressState.Normal;
            WinMain.LoadingSpinner = true;

            //if (cfa.AppSettings.Settings["MusicList"] != null)
            //{
            int n = 1;
            //await Task.Run(() =>
            //{
                foreach (var i in musics)
                {
                  await  AddMusic(i);
                /* mainWindow.Dispatcher.Invoke(() =>*/
                taskBar.ProgressValue = 1.0 * (n++) / musics.Length;//);
                }
            //}); 
           
            //}
            taskBar.ProgressState = TaskbarItemProgressState.None;
            WinMain.LoadingSpinner = false;

        }
        /// <summary>
        /// 保存歌曲列表到程序目录
        /// </summary>
        public static void SaveListToFile(string name)
        {
            try
            {
                SaveListToFile(name,false);
            }
            catch (Exception ex)
            {
                ShowException("保存歌单失败", ex);
            }
        }
        /// <summary>
        /// 保存歌曲列表到指定位置
        /// </summary>
        /// <param name="path"></param>
        public static void SaveListToFile(string path, bool abstractPath)
        {
            if(!abstractPath)
            {
                path = ToAbstractPath(path);
            }
            //if(!File.Exists(path))
            //{
            //    File.Create(path);
            //}
            //File.WriteAllBytes(path, SerializeObject(musicDatas));
            try
            {

                using (StreamWriter stream = new StreamWriter(File.Open(path, FileMode.Create), new UTF8Encoding(true)))
                {
                    CsvHelper.CsvWriter writer = new CsvHelper.CsvWriter(stream);
                    writer.WriteRecords(musicDatas);
                }
            }
            catch(Exception ex)
            {
                ShowException("保存歌单文件" + new FileInfo(path).Name + "失败：", ex);
            }

        }
        /// <summary>
        /// 读取音乐列表
        /// </summary>
        /// <param name="path"></param>
        public static void ReadFileToList(string path)
        {
           if(File.Exists(ToAbstractPath(path)))
            { 
                
                try
                {
                    ReadFileToList(path,false);
                }
                catch(Exception ex)
                {
                    ShowException($"歌单文件“{path}”已损坏，将重置歌单",ex);
                    musicDatas = new ObservableCollection<MusicInfo>();
                }
            }
            else
            {
                ShowPrompt($"歌单文件“{path}”不存在，将新建歌单");
                try
                {
                    using (File.Create(ToAbstractPath(path))) { }
                }
                catch(Exception ex)
                {
                    ShowException($"歌单文件“{path}”创建失败。", ex);
                }
                musicDatas = new ObservableCollection<MusicInfo>();
            }
         

        }
        /// <summary>
        /// 读取音乐列表
        /// </summary>
        /// <param name="path"></param>
        /// <param name="abstractPath"></param>
        public static void ReadFileToList(string path,bool abstractPath)
        {
            if(!abstractPath)
            {
                path = ToAbstractPath(path);
            }
            using (StreamReader stream = new StreamReader(File.Open(path, FileMode.Open),new UTF8Encoding(true)))
            {
                CsvHelper.CsvReader reader = new CsvHelper.CsvReader(stream);
                musicDatas = new ObservableCollection<MusicInfo>(reader.GetRecords<MusicInfo>());
            }
            

            //byte[] bytes;
            //try
            //{
            //    bytes = File.ReadAllBytes(path);
            //}
            //catch
            //{
            //    throw new Exception("文件读取失败。");
            //}
            //try
            //{
            //    musicDatas = DeserializeObject(bytes) as ObservableCollection<MusicInfo>;
            //}
            //catch
            //{
            //    throw new Exception("二进制文件存在错误。");
            //}



        }
        /// <summary>
        /// 删除音乐列表
        /// </summary>
        /// <param name="path"></param>
        /// <param name="abstractPath"></param>
        public static void DeleteMusicListFile(string path, bool abstractPath)
        {
            if(!abstractPath)
            {
                path = ToAbstractPath(path);
            }
            File.Delete(path);
        }
        /// <summary>
        /// 重命名歌单
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <param name="abstractPath"></param>
        public static void RenameMusicListFile(string oldPath,string newPath, bool abstractPath)
        {
            if (!abstractPath)
            {
                oldPath = ToAbstractPath(oldPath);
                newPath = ToAbstractPath(newPath);
            }
            File.Move(oldPath, newPath);
        }
        /// <summary>
        /// 转换到绝对地址
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ToAbstractPath(string path)
        {
            return (set.MusicListPath + "\\" + path+ (path.EndsWith(".csv")?"":".csv")).Replace("%APPDATA%",Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).Replace("\\\\", "");
        }
        /// <summary>
        /// 获取根目录
        /// </summary>
        /// <returns></returns>
        public static string ToAbstractPath()
        {
            return (set.MusicListPath).Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        }
        /// <summary>
        /// 歌曲数量
        /// </summary>
        public static int MusicCount => musicDatas.Count;
        /// <summary>
        /// 当前的歌曲实例
        /// </summary>
        public static MusicInfo CurrentMusic => musicDatas[musicIndex];
        /// <summary>
        /// 当前歌曲索引
        /// </summary>
        public static int CurrentMusicIndex => musicIndex;
        /// <summary>
        /// 删除所有歌曲
        /// </summary>
        public static void RemoveMusic()
        {
            musicDatas.Clear();
            WinMain.AfterClearList();
        }
        /// <summary>
        /// 删除指定歌曲
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveMusic(int index)
        {
            musicDatas.RemoveAt(index);
            if (musicDatas.Count == 0)
            {
                WinMain.AfterClearList();
            }
        }
        public static void RemoveMusic(MusicInfo music)
        {
            musicDatas.Remove(music);
            if (musicDatas.Count == 0)
            {
                WinMain.AfterClearList();
            }
        }


        /// <summary>
        /// 获取指定索引的歌曲
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static MusicInfo GetMusic(int index) => musicDatas[index];
        /// <summary>
        /// 根据歌曲实例获取歌曲的索引
        /// </summary>
        /// <param name="music"></param>
        /// <returns></returns>
        public static int GetMusic(MusicInfo music) => musicDatas.IndexOf(music);
        /// <summary>
        /// 设置当前歌曲索引
        /// </summary>
        /// <param name="index"></param>
        public static void SetCurrent(int index)
        {
            musicIndex = index;
        }
        /// <summary>
        /// 歌曲历史总数
        /// </summary>
        public static int HistoryCount => historyList.Count;
        /// <summary>
        /// 当前歌曲历史索引
        /// </summary>
        public static int CurrentHistoryIndex
        {
            get => historyIndex;
            set => historyIndex = value;
        }
        /// <summary>
        /// 获取当前歌曲历史实例
        /// </summary>
        public static MusicInfo CurrentHistory => historyList[historyIndex];
        /// <summary>
        /// 新增歌曲历史
        /// </summary>
        /// <param name="music"></param>
        public static void AddHistory(MusicInfo music,bool indexPlusOne=true)
        {
            historyList.Add(music);
            if (indexPlusOne)
            {
                historyIndex++;
            }
        }
        /// <summary>
        /// 移除一定的歌曲历史
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public static void RemoveHistory(int index, int count)
        {
            historyList.RemoveRange(index, count);
            historyIndex = index - 1;
        }
        /// <summary>
        /// 获取历史
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static MusicInfo GetHistory(int index)
        {
            return historyList[index];
        }
             public static void RandomizeList()
        {
            //Copy to a array
            MusicInfo[] copyArray = new MusicInfo[musicDatas.Count];
            musicDatas.CopyTo(copyArray,0);

            //Add range
            List<MusicInfo> copyList = new List<MusicInfo>();
            copyList.AddRange(copyArray);

            //Set outputList and random
            //List<MusicInfo> outputList = new List<MusicInfo>();
            Random rd = new Random(DateTime.Now.Millisecond);
            musicDatas.Clear();
            while (copyList.Count > 0)
            {
                //Select an index and item
                int rdIndex = rd.Next(0, copyList.Count);
                MusicInfo remove = copyList[rdIndex];

                //remove it from copyList and add it to output
                copyList.Remove(remove);
                musicDatas.Add(remove);
            }
           // return outputList;
        }

    }
}
