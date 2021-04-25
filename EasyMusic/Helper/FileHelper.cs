using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicListHelper;
using static FzLib.UI.Dialog.MessageBox;

namespace EasyMusic.Helper
{
    public class FileHelper
    {
        public static void ImportMusicsFolder(bool includeChildren)
        {
            CommonOpenFileDialog fbd = new CommonOpenFileDialog()
            {
                Title = "请选择包含音乐文件的文件夹",
                IsFolderPicker = true,
                Multiselect = true,
            };
            if (fbd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                List<string> musics = new List<string>();

                foreach (var folderName in fbd.FileNames)
                {
                    foreach (var i in EnumerateMusics(folderName, includeChildren ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    {
                        musics.Add(i);
                    }
                }

                if (musics.Count >= 1)
                {
                    AddMusic(musics.ToArray());
                }
            }
        }

        public static void ImportMusicsFiles()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = "请选择音乐文件",
                Multiselect = true
            };
            //dialog.Filters.Add(new CommonFileDialogFilter("MP3音乐", ".mp3"));
            //dialog.Filters.Add(new CommonFileDialogFilter("波形音乐", ".wav"));
            //dialog.Filters.Add(new CommonFileDialogFilter("FLAC无损音乐", ".flac"));
            //dialog.Filters.Add(new CommonFileDialogFilter("AAC音乐", ".aac"));
            dialog.Filters.Add(new CommonFileDialogFilter("支持的格式", GetExtensionFilter()));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok && dialog.FileNames != null)
            {
                List<string> musics = new List<string>();
                foreach (var i in dialog.FileNames)
                {
                    musics.Add(i);
                }
                if (musics.Count >= 1)
                {
                    AddMusic(musics.ToArray());
                }
            }
        }

        public static void ExportMusicList()
        {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog()
            {
                DefaultExtension = "bin",
            };
            dialog.Filters.Add(new CommonFileDialogFilter("Csv表格", "csv"));
            dialog.Filters.Add(new CommonFileDialogFilter("所有文件", "*"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    SaveListToFile(dialog.FileName);
                }
                catch (Exception ex)
                {
                    ShowException("无法保存文件", ex);
                }
            }
        }

        public static void ImportMusicList()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                DefaultExtension = "bin",
            };
            dialog.Filters.Add(new CommonFileDialogFilter("Csv表格", "csv"));
            dialog.Filters.Add(new CommonFileDialogFilter("所有文件", "*"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    ReadFileToList(dialog.FileName, false);
                }
                catch (Exception ex)
                {
                    ShowException(ex);
                }
            }
        }
    }
}