using EasyMusic.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfControls.Dialog;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicListHelper;
using static WpfControls.Dialog.DialogHelper;

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


        static MusicControlHelper()
        {
            CycleMode = (CycleMode)Setting.CycleMode;
        }

        public static CycleMode CycleMode { get; set; }
        private static double volumn = 1;
        public static double Volumn
        {
            get => volumn;
            set
            {
                volumn = value;
                Music.Volumn = value;
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
            Music = new BassModuleHelper(music);
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
                PlayNew(CurrentHistoryIndex == -1 ? GetHistory(CurrentHistoryIndex = HistoryCount - 1) : GetHistory(CurrentHistoryIndex));
            }
        }

        public static void PlayNext()
        {
            if (CurrentHistoryIndex == HistoryCount - 1)
            {
                PlayListNext();
            }
            else
            {
                PlayNew(GetHistory(++CurrentHistoryIndex));
            }
        }

        /// <summary>
        /// 根据不同的播放循环模式播放下一首
        /// </summary>
        public static void PlayListNext()
        {
            switch (CycleMode)
            {
                case CycleMode.ListCycle:
                    if (MusicCount > 1)
                    {
                        if (MusicDatas.Last() == Music.MusicInfo)
                        {
                            PlayNew(MusicDatas.First());
                        }
                        else
                        {
                            PlayNew(MusicDatas[MusicDatas.IndexOf(Music.MusicInfo) + 1]);
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
                    int index;
                    Random r = new Random();
                    MusicInfo music = null;
                    do
                    {
                        index = r.Next(0, MusicCount);
                        music = MusicDatas[index];
                    }
                    while (music != Music.MusicInfo);
                    PlayNew(music);
                    break;
                case CycleMode.SingleCycle:
                    Music.PlayAgain();
                    break;
            }
        }
        public static void PlayListLast()
        {
            if (MusicCount > 1)
            {
                if (MusicDatas.First() == Music.MusicInfo)
                {
                    PlayNew(MusicDatas.Last());
                }
                else
                {
                    PlayNew(MusicDatas[MusicDatas.IndexOf(Music.MusicInfo) - 1]);
                }
            }
            else
            {
                Music.PlayAgain();
            }

        }

    }
}
