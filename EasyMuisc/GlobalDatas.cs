using EasyMusic.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace EasyMusic
{
    public static class GlobalDatas
    {
        /// <summary>
        /// 支持的格式
        /// </summary>
        public static string[] supportExtension = { ".mp3", ".MP3", ".wav", ".WAV" };
        /// <summary>
        /// 支持的格式，过滤器格式
        /// </summary>
        public static string supportExtensionWithSplit;
        public static Properties.Settings Setting { get; } = new Properties.Settings();

        public static IntPtr windowHandle;
        /// <summary>
        /// 托盘图标
        /// </summary>
        public static WpfCodes.Program.TrayIcon trayIcon = null;
        /// <summary>
        /// 程序目录
        /// </summary>
        public static string programDirectory = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName;
        public static ListenHistoryHelper listenHistory = new ListenHistoryHelper();

        public static string argPath = null;
    }
}
