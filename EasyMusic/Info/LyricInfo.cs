﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicControlHelper;
using static FzLib.UI.Dialog.MessageBox;
using static System.Math;

namespace EasyMusic.Info
{
    public class LyricInfo
    {
        /// <summary>
        /// 歌曲
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 艺术家
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// 专辑
        /// </summary>
        public string Album { get; set; }

        /// <summary>
        /// 歌词作者
        /// </summary>
        public string LrcBy { get; set; }

        /// <summary>
        /// 偏移量
        /// </summary>
        public double Offset { get; set; }

        public int CurrentIndex { get; set; } = -1;

        /// <summary>
        /// 歌词
        /// </summary>
        public Dictionary<double, string> LrcContent { get; set; }

        public Dictionary<double, int> LineCount { get; set; }

        /// <summary>
        /// 获得歌词信息
        /// </summary>
        /// <param name="LrcPath">歌词路径</param>
        /// <returns>返回歌词信息(Lrc实例)</returns>
        public LyricInfo(string LrcPath)
        {
            Regex regex = new Regex(@"(?<time>\[[0-9.:\]\[\s]*\])(?<value>.*)", RegexOptions.Compiled);
            Regex timeRegex = new Regex(@"\[(?<time>[0-9.:]*)\]\s*", RegexOptions.Compiled);

            LrcContent = new Dictionary<double, string>();
            var tempLrcContent = new Dictionary<double, string>();
            LineCount = new Dictionary<double, int>();
            using (FileStream fs = new FileStream(LrcPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                string line;
                using (StreamReader sr = new StreamReader(fs, FzLib.Basic.String.GetEncoding(LrcPath)))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("[ti:"))
                        {
                            Title = SplitInfo(line);
                        }
                        else if (line.StartsWith("[ar:"))
                        {
                            Artist = SplitInfo(line);
                        }
                        else if (line.StartsWith("[al:"))
                        {
                            Album = SplitInfo(line);
                        }
                        else if (line.StartsWith("[by:"))
                        {
                            LrcBy = SplitInfo(line);
                        }
                        else if (line.StartsWith("[offset:"))
                        {
                            if (double.TryParse(SplitInfo(line), out double offset))
                            {
                                Offset = offset / 1000;
                            }
                        }
                        else
                        {
                            try
                            {
                                Match match = regex.Match(line);//分割时间和内容
                                string word = match.Groups["value"].Value;
                                MatchCollection timeMatch = timeRegex.Matches(match.Groups["time"].Value);//分割多个时间
                                foreach (var i in timeMatch)
                                {
                                    double time = TimeSpan.Parse("00:" + (i as Match).Groups["time"].Value).TotalSeconds;
                                    if (tempLrcContent.ContainsKey(time))//如果是双文歌词，两个歌词时间相同
                                    {
                                        if (tempLrcContent[time] != "" && word != "")
                                        {
                                            tempLrcContent[time] += Environment.NewLine + word;//将原来的歌词下面加一行新的歌词
                                            LineCount[time]++;//当前时间的歌词行数加1
                                        }
                                        else
                                        {
                                        }
                                    }
                                    else//第一次出现这个时间的歌词
                                    {
                                        tempLrcContent.Add(time, word);
                                        LineCount.Add(time, 1);
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            LrcContent = tempLrcContent.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);//将歌词排序
            LineCount = LineCount.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);//将每一个时间的歌词的行数排序
            for (int i = 1; i < LineCount.Count; i++)
            {
                //本来是每一时间自己的行数，这里要累加起来
                LineCount[LineCount.Keys.ElementAt(i)] += LineCount[LineCount.Keys.ElementAt(i - 1)];
            }
        }

        /// <summary>
        /// 处理信息
        /// </summary>
        /// <param name="line"></param>
        /// <returns>返回基础信息</returns>
        private static string SplitInfo(string line)
        {
            return line.Substring(line.IndexOf(":") + 1).TrimEnd(']');
        }

        public void CopyLyrics()
        {
            if (LrcContent.Count != 0)
            {
                StringBuilder str = new StringBuilder();
                //for (int i = 0; i < LrcContent.Count - 1; i++)
                //{
                //    str.Append(LrcContent[i] + Environment.NewLine);
                //}
                //str.Append(LrcContent[LrcContent.Count - 1]);
                foreach (var line in LrcContent)
                {
                    str.AppendLine(line.Value);
                }
                Clipboard.SetText(str.ToString());
            }

            /// <summary>
            /// 保存歌词
            /// </summary>
            /// <param name="saveAs"></param>
        }

        public void SaveLrc(bool saveAs)
        {
            StringBuilder str = new StringBuilder();
            if (Setting.PreferMusicInfo)
            {
                if (Music.Info.Name != "")
                {
                    str.Append("[ti:" + Music.Info.Name + "]" + Environment.NewLine);
                }
                if (Music.Info.Singer != "")
                {
                    str.Append("[ar:" + Music.Info.Singer + "]" + Environment.NewLine);
                }
                if (Music.Info.Album != "")
                {
                    str.Append("[al:" + Music.Info.Album + "]" + Environment.NewLine);
                }
            }
            else
            {
                if (Title != "")
                {
                    str.Append("[ti:" + Title + "]" + Environment.NewLine);
                }
                if (Artist != "")
                {
                    str.Append("[ar:" + Artist + "]" + Environment.NewLine);
                }
                if (Album != "")
                {
                    str.Append("[al:" + Album + "]" + Environment.NewLine);
                }
                if (LrcBy != "")
                {
                    str.Append("[by:" + LrcBy + "]" + Environment.NewLine);
                }
            }
            if (Setting.SaveLrcOffsetByTag && Offset != 0)
            {
                str.Append("[offset:" + (int)Round(Offset * 1000) + "]" + Environment.NewLine);
            }
            List<double> lrcTime = new List<double>();
            foreach (var i in LrcContent.Keys)
            {
                lrcTime.Add((Setting.SaveLrcOffsetByTag) ? i : i + Offset);
            }

            FileInfo file = new FileInfo(Music.FilePath);
            foreach (var time in LrcContent.Keys)
            {
                string word = LrcContent[time];
                int intMinute = (int)time / 60;
                string minute = string.Format("{0:00}", intMinute);
                string second = string.Format("{0:00.00}", time - 60 * intMinute);
                foreach (var j in word.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    str.Append("[" + minute + ":" + second + "]" + j + Environment.NewLine);
                }
            }
            if (saveAs)
            {
                CommonSaveFileDialog dialog = new CommonSaveFileDialog()
                {
                    AlwaysAppendDefaultExtension = true,
                    InitialDirectory = file.DirectoryName,
                    Title = "请选择目标文件夹",
                    DefaultFileName = file.Name.Replace(file.Extension, ".lrc"),
                };

                dialog.Filters.Add(new CommonFileDialogFilter("Lrc歌词", "lrc"));
                dialog.Filters.Add(new CommonFileDialogFilter("所有文件", "*"));

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        File.WriteAllText(dialog.FileName, str.ToString());
                        ShowPrompt("歌词保存成功");
                    }
                    catch (Exception ex)
                    {
                        ShowException("无法保存文件", ex);
                    }
                }
            }
            else
            {
                try
                {
                    File.WriteAllText(Music.FilePath.Replace(file.Extension, "") + ".lrc", str.ToString());
                    ShowPrompt("歌词保存成功");
                }
                catch (Exception ex)
                {
                    ShowException("无法保存文件", ex);
                }
            }
        }
    }
}