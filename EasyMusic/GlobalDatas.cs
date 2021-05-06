using EasyMusic.Helper;
using EasyMusic.Info;
using FzLib.UI.Dialog;
using FzLib.Program.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Animation;
using System.Linq;

namespace EasyMusic
{
    public static class GlobalDatas
    {
        static GlobalDatas()
        {
            if (File.Exists("ConfigPath.ini"))
            {
                try
                {
                    string path = File.ReadAllText("ConfigPath.ini");
                    if (Directory.Exists(path))
                    {
                        ConfigPath = path;
                    }
                }
                catch
                {
                }
            }
            if (ConfigPath == null)
            {
                ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EasyMusic";
            }

            string settingPath = ConfigPath + "\\Config.json";

            try
            {
                Setting = Settings.OpenOrCreate<Settings>(settingPath);
            }
            catch (Exception ex)
            {
                FzLib.UI.Dialog.MessageBox.ShowException("读取配置文件失败，将重置配置文件", ex, true);
                Setting = Settings.Create<Settings>(settingPath);
            }

            listenHistory = new ListenHistoryHelper();
        }

        /// <summary>
        /// 支持的格式
        /// </summary>
        public readonly static string[] supportedExtension = { "mp3", ".wav", "flac", "aac" };

        public static Settings Setting { get; set; }
        public static string ConfigPath { get; }

        public static bool IsExtensionSupported(string ext)
        {
            return supportedExtension.Contains(ext.Trim().Trim('.').ToLower());
        }

        public static string GetExtensionFilter()
        {
            return string.Join(",", supportedExtension.Select(p => "." + p));
        }

        /// <summary>
        /// 托盘图标
        /// </summary>
        public static TrayIcon trayIcon = null;

        /// <summary>
        /// 程序目录
        /// </summary>
        public static string programDirectory = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName;

        public static ListenHistoryHelper listenHistory;

        public static string argPath = null;

        /// <summary>
        /// 创建并启动一个新的浮点数据类型动画
        /// </summary>
        /// <param name="obj">动画主体</param>
        /// <param name="property">更改的属性</param>
        /// <param name="to">目标值</param>
        /// <param name="duration">时间</param>
        /// <param name="decelerationRatio">减缓时间</param>
        /// <param name="completed">完成以后的事件</param>
        /// <returns></returns>
        public static Storyboard NewDoubleAnimation(FrameworkElement obj, DependencyProperty property, double to, double duration, double decelerationRatio = 0, EventHandler completed = null, bool stopAfterComplete = false, EasingFunctionBase easingFunction = null)
        {
            DoubleAnimation ani = new DoubleAnimation
            {
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(duration)),//动画时间1秒
                DecelerationRatio = decelerationRatio,
                FillBehavior = stopAfterComplete ? FillBehavior.Stop : FillBehavior.HoldEnd,
                EasingFunction = easingFunction,
            };

            Storyboard.SetTarget(ani, obj);
            Storyboard.SetTargetProperty(ani, new PropertyPath(property));
            Storyboard story = new Storyboard();
            //Debug.WriteLine(Timeline.GetDesiredFrameRate(story));

            story.Children.Add(ani);
            if (completed != null)
            {
                story.Completed += completed;
            }
            story.Begin();
            return story;
        }

        /// <summary>
        /// 支持多个过滤器的枚举
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IList<string> EnumerateMusics(string path, SearchOption searchOption)
        {
            return Directory.EnumerateFiles(path,"*", searchOption).Where(p => IsExtensionSupported(Path.GetExtension(p))).ToList();
        }

        public static double ScreenHight => SystemParameters.PrimaryScreenHeight;
        public static double ScreenWidth => SystemParameters.PrimaryScreenWidth;

        public static int GetRandomNumber(int from, int smallerThan)
        {
            RNGCryptoServiceProvider r = new RNGCryptoServiceProvider();
            byte[] b = new byte[8];
            r.GetBytes(b);
            return (b[0] + b[1] + b[2] + b[3] + b[4] + b[5] + b[6] + b[7]) % (smallerThan - from) + from;
        }

        /// <summary>
        /// 歌词对象
        /// </summary>
        public static LyricInfo lrc;
    }
}