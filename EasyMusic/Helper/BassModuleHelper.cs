using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using static Un4seen.Bass.Bass;
using static EasyMusic.GlobalDatas;
using static FzLib.Basic.String;
using System.IO;
using static FzLib.Control.Dialog.DialogBox;
using System.Windows.Media.Imaging;
using FzLib.Control.Dialog;
using System.Windows;
using static EasyMusic.Helper.MusicListHelper;
using EasyMusic.Enum;
using System.Diagnostics;

namespace EasyMusic.Helper
{
    public class BassModuleHelper : IDisposable
    {
        private int stream;

        /// <summary>
        /// 内部音量
        /// </summary>
        public double Volumn
        {
            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                if (value < 0)
                {
                    value = 0;
                }
                BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, (float)value);

            }
            get
            {
                float value = 0;
                BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, ref value);
                return value;
            }
        }

        public int Device
        {
            get => BASS_ChannelGetDevice(stream);
            set
            {
                if (BASS_ChannelSetDevice(stream, value))
                {
                    BASS_ChannelPause(stream);
                    BASS_ChannelPlay(stream, false);
                }
                else
                {
                    ShowError("设置音乐输出设备错误：" + BASS_ErrorGetCode().ToString());
                }
            }
        }

        public void Dispose()
        {
            BASS_StreamFree(stream);
        }
        public Info.MusicInfo Info { get; private set; }
        public double Position
        {
            get => BASS_ChannelBytes2Seconds(stream, BASS_ChannelGetPosition(stream));

            set
            {
                BASS_ChannelSetPosition(stream, value);
                MainWindow.Current.UpdateLyric(value);
            }
        }

        public bool IsEnd => BASS_ChannelGetLength(stream) - BASS_ChannelGetPosition(stream) <= 144;
        public int Pitch
        {
            get
            {
                float value = 0;
                BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, ref value);
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
                BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, value);
                Setting.Pitch = value;
            }
        }
        public int Tempo
        {
            get
            {
                float value = 0;
                BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, ref value);
                return (int)(value);
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
                BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, ((float)(value)));
                Setting.Tempo = value;
            }
        }

        public string Name { get; private set; }

        public string FilePath { get; private set; }
        public FileInfo MusicFile { get; private set; }
        public double Length { get; private set; }
        public BitmapImage AlbumImage { get; private set; }


        #region 播放控制


        /// <summary>
        /// 若在PlayNew时没立即播放则暂时不记录
        /// </summary>
        bool notRecordYet = false;

        public BassModuleHelper(Info.MusicInfo music)
        {
            Info = music;

            FilePath = music.Path;
            MusicFile = new FileInfo(FilePath);

        }
        public bool Initialiaze()
        {
            if (!File.Exists(FilePath))
            {
                if (ShowMessage($"歌曲{Info.Name}（{Info.Path}）不存在！是否从列表中删除？", DialogType.Warn, MessageBoxButton.YesNo) == 0)
                {
                    RemoveMusic(Info);
                }
                return false;
            }
            if (CurrentHistoryIndex == HistoryCount - 1)
            {
                if (CurrentHistoryIndex == -1 || CurrentHistory != Info)
                {
                    AddHistory(Info);//加入历史记录
                }
            }
            if (Setting.RecordListenHistory)
            {
                listenHistory.RecordEnd();
                notRecordYet = false;
            }
            InitialiazeMusic();//初始化歌曲
            Volumn = Setting.Volumn;

            if (Setting.RecordListenHistory)
            {

                notRecordYet = true;
            }
            return true;

        }

        /// <summary>
        /// 初始化新的歌曲
        /// </summary>
        private void InitialiazeMusic()
        {
            try
            {
                var tempStream = BASS_StreamCreateFile(FilePath, 0, 0, BASSFlag.BASS_STREAM_DECODE);//获取歌曲句柄
                                                                                                    // var decoder = Bass.BASS_StreamCreateFile(path, 0, 0,BASSFlag. BASS_STREAM_DECODE); // create a "decoding channel" from a file
                stream = Un4seen.Bass.AddOn.Fx.BassFx.BASS_FX_TempoCreate(tempStream, BASSFlag.BASS_FX_FREESOURCE);


                //Device = MusicControlHelper.Device;
                if (Setting.MusicFxMode == MusicFxRemainMode.All)
                {
                    Pitch = Setting.Pitch;
                    Tempo = Setting.Tempo;
                }
                else if(Setting.MusicFxMode == MusicFxRemainMode.Each)
                {
                    var fx = MusicFxConfigHelper.Instance.Get(FilePath);
                    Pitch = fx.Pitch;
                    Tempo = fx.Tempo;
                }

                Name = MusicFile.Name.RemoveEnd(MusicFile.Extension);
                Length = BASS_ChannelBytes2Seconds(stream, BASS_ChannelGetLength(stream));
            }
            catch (Exception ex)
            {
                ShowException("初始化音乐失败", ex);
                return;
            }
            AlbumImage = GetMusicAlbumImage();
            MainWindow.Current.InitializeNewMusic();
            MainWindow.Current.OnStatusChanged(ControlStatus.Initialized);
        }

        public BASSActive Status => BASS_ChannelIsActive(stream);

        /// <summary>
        /// （暂停后）播放
        /// </summary>
        /// <returns></returns>
        public void Play()
        {
            if (BASS_ChannelPlay(stream, false))
            {
                MainWindow.Current.OnStatusChanged(ControlStatus.Play);
                BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, Convert.ToSingle(MusicControlHelper.Volumn), Convert.ToInt32( Setting.VolumnChangeTime.TotalMilliseconds));

                if (Setting.RecordListenHistory && notRecordYet)
                {
                    listenHistory.Record();
                    notRecordYet = false;
                }
            }
        }
        /// <summary>
        /// 播放当前歌曲
        /// </summary>
        /// <returns></returns>
        public void PlayAgain()
        {
            Position = 0;
            Play();
        }
        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        public async void Pause()
        {
            MainWindow.Current.OnStatusChanged(ControlStatus.Pause);

            BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 0f, Convert.ToInt32(Setting.VolumnChangeTime.TotalMilliseconds));
            await Task.Delay(Convert.ToInt32(Setting.VolumnChangeTime.TotalMilliseconds));
            BASS_ChannelPause(stream);
            //Position = lastPosition;
        }

        public void PlayOrPause()
        {
            if (Status == BASSActive.BASS_ACTIVE_PLAYING)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        #endregion
        /// <summary>
        /// 读取Mp3信息
        /// </summary>
        /// <param name="path"></param>
        private BitmapImage GetMusicAlbumImage()//copy
        {
            string[] tags = new string[6];
            FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[10];
            string mp3ID = "";
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(buffer, 0, 10);
            int size = (buffer[6] & 0x7F) * 0x200000 + (buffer[7] & 0x7F) * 0x400 + (buffer[8] & 0x7F) * 0x80 + (buffer[9] & 0x7F);
            mp3ID = Encoding.Default.GetString(buffer, 0, 3);
            if (mp3ID.Equals("ID3", StringComparison.OrdinalIgnoreCase))
            {
                //如果有扩展标签头就跨过 10个字节
                if ((buffer[5] & 0x40) == 0x40)
                {
                    fs.Seek(10, SeekOrigin.Current);
                    size -= 10;
                }
                return ReadFrame();
            }
            return null;
            BitmapImage ReadFrame()//copy
            {
                while (size > 0)
                {
                    //读取标签帧头的10个字节
                    fs.Read(buffer, 0, 10);
                    size -= 10;
                    //得到标签帧ID
                    string FramID = Encoding.Default.GetString(buffer, 0, 4);
                    //计算标签帧大小，第一个字节代表帧的编码方式
                    int frmSize = 0;

                    frmSize = buffer[4] * 0x1000000 + buffer[5] * 0x10000 + buffer[6] * 0x100 + buffer[7];
                    if (frmSize == 0)
                    {
                        //就说明真的没有信息了
                        break;
                    }
                    //bFrame 用来保存帧的信息
                    byte[] bFrame = new byte[frmSize];
                    fs.Read(bFrame, 0, frmSize);
                    size -= frmSize;
                    string str = GetFrameInfoByEcoding(bFrame, bFrame[0], frmSize - 1);
                    if (FramID.CompareTo("APIC") == 0)
                    {
                        try
                        {
                            int i = 0;
                            while (true)
                            {
                                if (255 == bFrame[i] && 216 == bFrame[i + 1])
                                {
                                    break;
                                }
                                i++;
                            }
                            byte[] imge = new byte[frmSize - i];
                            fs.Seek(-frmSize + i, SeekOrigin.Current);
                            fs.Read(imge, 0, imge.Length);
                            MemoryStream ms = new MemoryStream(imge);
                            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                            string imgPath = Path.GetTempFileName();
                            FileStream save = new FileStream(imgPath, FileMode.Create);
                            img.Save(save, System.Drawing.Imaging.ImageFormat.Jpeg);
                            save.Close();
                            return new BitmapImage(new Uri(imgPath));
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
                return null;
            }
            string GetFrameInfoByEcoding(byte[] b, byte conde, int length)//copy
            {
                string str = "";
                switch (conde)
                {
                    case 0:
                        str = Encoding.GetEncoding("ISO-8859-1").GetString(b, 1, length);
                        break;
                    case 1:
                        str = Encoding.GetEncoding("UTF-16LE").GetString(b, 1, length);
                        break;
                    case 2:
                        str = Encoding.GetEncoding("UTF-16BE").GetString(b, 1, length);
                        break;
                    case 3:
                        str = Encoding.UTF8.GetString(b, 1, length);
                        break;
                }
                return str;
            }
        }

    }
}
