using EasyMusic.Enum;
using EasyMusic.Info;
using System;
using System.Linq;
using Un4seen.Bass;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicListHelper;
using static FzLib.Control.Dialog.DialogBox;
using static Un4seen.Bass.Bass;

namespace EasyMusic.Helper
{
    public static class MusicControlHelper
    {
        private static BassModuleHelper bassInstance;

        public static BassModuleHelper Music
        {
            get
            {
                return bassInstance;
            }
            set
            {
                bassInstance?.Dispose();
                bassInstance = value;
            }
        }

        public static int Device
        {
            get => BASS_GetDevice();
            set
            {
                var devices = BASS_GetDeviceInfos();
                if (value < devices.Length)
                {
                    var device = devices[value];
                    if (device.status.HasFlag(BASSDeviceInfo.BASS_DEVICE_ENABLED) && !device.status.HasFlag(BASSDeviceInfo.BASS_DEVICE_INIT))
                    {
                        BASS_Init(value, Setting.SampleRate, BASSInit.BASS_DEVICE_DEFAULT, MainWindow.Current.Handle);
                    }

                    if (BASS_SetDevice(value))
                    {
                        if (Music != null)
                        {
                            Music.Device = value;
                        }
                    }
                    else
                    {
                        ShowError("设置总输出设备错误：" + BASS_ErrorGetCode().ToString());
                    }
                }
            }
        }

        static MusicControlHelper()
        {
            CycleMode = (CycleMode)Setting.CycleMode;
        }

        public static CycleMode CycleMode { get; set; }

        public static double Volumn
        {
            get => Setting.Volumn;
            set
            {
                Setting.Volumn = value;
                if (Music != null)
                {
                    Music.Volumn = value;
                }
                MainWindow.Current.controlBar.Notify("SliderVolumnBinding");
            }
        }

        public static void PlayOrPlayNew()
        {
            if (Music == null)
            {
                if (MusicCount != 0)
                {
                    PlayNew(MusicDatas.First());
                    return;
                }
            }

            Music?.Play();
        }

        /// <summary>
        /// 播放新的歌曲
        /// </summary>
        /// <param name="index">指定列表中的歌曲索引</param>
        /// <returns></returns>
        public static void PlayNew(MusicInfo music, bool playAtOnce = true)
        {
            if (Music != null && Setting.MusicFxMode == Enum.MusicFxRemainMode.Each)
            {
                MusicFxConfigHelper.Instance.Set(Music.FilePath, new MusicFxInfo(Music.Pitch, Music.Tempo));
            }
            Music = new BassModuleHelper(music);
            Music.Initialiaze();
            if (playAtOnce)
            {
                Music.Play();
            }
        }

        public static void PlayLast()
        {
            if (HistoryCount == 0)
            {
                PlayListLast();
            }
            else
            {
                CurrentHistoryIndex--;
                if (CurrentHistoryIndex != -1)
                {
                    PlayNew(GetHistory(CurrentHistoryIndex));
                }
            }
        }

        public static void PlayNext(bool manual)
        {
            if (CurrentHistoryIndex == HistoryCount - 1)
            {
                PlayListNext(manual);
            }
            else
            {
                PlayNew(GetHistory(++CurrentHistoryIndex));
            }
        }

        /// <summary>
        /// 根据不同的播放循环模式播放下一首
        /// </summary>
        public static void PlayListNext(bool manual, CycleMode? mode = null)
        {
            if (!mode.HasValue)
            {
                mode = CycleMode;
            }
            switch (mode.Value)
            {
                case CycleMode.ListCycle:
                    if (MusicCount > 1)
                    {
                        if (Music != null)
                        {
                            if (MusicDatas.Last() == Music.Info)
                            {
                                PlayNew(MusicDatas.First());
                            }
                            else
                            {
                                PlayNew(MusicDatas[MusicDatas.IndexOf(Music.Info) + 1]);
                            }
                        }
                        else
                        {
                            PlayNew(MusicDatas[0]);
                        }
                    }
                    else
                    {
                        Music.PlayAgain();
                    }
                    break;

                case CycleMode.Shuffle:
                    if (MusicCount == 1)
                    {
                        Music.PlayAgain();
                        break;
                    }

                    Random r = new Random();
                    MusicInfo[] playedMusics;
                    //如果有n首歌曲，那么就随机从不存在于历史记录最后的n首歌中随机选取一首
                    if (historyList.Count < MusicCount)
                    {
                        playedMusics = historyList.Distinct().ToArray();
                    }
                    else
                    {
                        playedMusics = historyList.Skip(historyList.Count - MusicCount).Distinct().ToArray();
                    }
                    var notPlayedMusics = MusicDatas.Except(playedMusics).ToArray();
                    MusicInfo music;
                    if (notPlayedMusics.Length == 0)
                    {
                        music = MusicDatas[r.Next(MusicCount)];
                    }
                    else
                    {
                        do
                        {
                            music = notPlayedMusics[r.Next(notPlayedMusics.Length)];
                        } while (music == Music?.Info);
                    }
                    PlayNew(music);
                    break;

                case CycleMode.SingleCycle:
                    if (manual)
                    {
                        //手动点击下一曲，还是得下一曲的
                        PlayListNext(true, CycleMode.ListCycle);
                    }
                    else
                    {
                        Music.PlayAgain();
                    }
                    break;
            }
        }

        public static void PlayListLast()
        {
            if (MusicCount > 1)
            {
                if (MusicDatas.First() == Music.Info)
                {
                    PlayNew(MusicDatas.Last());
                }
                else
                {
                    PlayNew(MusicDatas[MusicDatas.IndexOf(Music.Info) - 1]);
                }
            }
            else
            {
                Music.PlayAgain();
            }
        }
    }
}