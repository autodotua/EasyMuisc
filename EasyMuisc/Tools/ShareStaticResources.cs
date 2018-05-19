using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace EasyMuisc
{
    public static class ShareStaticResources
    {
        /// <summary>
        /// 音乐播放句柄
        /// </summary>
        public static int stream = 0;
        /// <summary>
        /// 支持的格式
        /// </summary>
        public static string[] supportExtension = { ".mp3", ".MP3", ".wav", ".WAV" };
        /// <summary>
        /// 支持的格式，过滤器格式
        /// </summary>
        public static string supportExtensionWithSplit;
        public static int Pitch
        {
            get
            {
                float value = 0;
                Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, ref value);
                return (int)value;
            }
            set
            {
                if (value < -50)
                {
                    value = -50;
                }
                if (value > 50)
                {
                    value = 50;
                }
                Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, value);
                set.Pitch = value;
            }
        }
        public static int Tempo
        {
            get
            {
                float value = 0;
                Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, ref value);
                return (int)(value * 100);
            }
            set
            {
                if (value < -95)
                {
                    value = -95;
                }
                if (value > 200)
                {
                    value = 200;
                }
                Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, ((float)value));
                set.Tempo = value;
            }
        }
        public static Properties.Settings set = new Properties.Settings();

        public static IntPtr windowHandle;
        /// <summary>
        /// 托盘图标
        /// </summary>
        public static System.Windows.Forms.NotifyIcon trayIcon = null;
        /// <summary>
        /// 程序目录
        /// </summary>
       public static string programDirectory = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName;
        public static Tools.ListenHistoryHelper listenHistory = new Tools.ListenHistoryHelper();
        public static MainWindow mainWindow;
        
    }
}
