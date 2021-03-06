﻿using EasyMusic.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicControlHelper;

namespace EasyMusic.Helper
{
    public class ListenHistoryHelper
    {
        public static string XmlPath => ConfigPath + "\\ListenHistory.xml";

        private XmlDocument xml = new XmlDocument();
        private XmlElement root;

        private XmlElement lastTimeElement;

        private IEnumerable<XmlElement> Histories => root.ChildNodes.Cast<XmlElement>();

        public ListenHistoryHelper()
        {
            if (!File.Exists(XmlPath))
            {
                new FileInfo(XmlPath).Directory.Create();
                XmlDeclaration xdec = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
                xml.AppendChild(xdec);
                root = xml.CreateElement("EasyMusicListenHistory");
                xml.AppendChild(root);
                xml.Save(XmlPath);
            }
            else
            {
                xml.Load(XmlPath);
                root = xml.LastChild as XmlElement;
            }
        }

        public void RecordEnd()
        {
            if (lastTimeElement == null)
            {
                return;
            }
            DateTime now = DateTime.Now;
            TimeSpan span = now - DateTime.Parse(lastTimeElement.GetAttribute("BeginTime"));
            lastTimeElement.SetAttribute("EndTime", now.ToString());
            try
            {
                xml.Save(XmlPath);
            }
            catch (Exception ex)
            {
            }
        }

        public IEnumerable<ListenHistoryInfo> GetListenHistories()
        {
            List<ListenHistoryInfo> histories = new List<ListenHistoryInfo>();
            foreach (XmlElement element in root.ChildNodes)
            {
                string name = element.GetAttribute("Name");
                int length = int.Parse(element.GetAttribute("Length"));
                string singer = element.GetAttribute("Singer");
                Dictionary<DateTime, DateTime?> times = new Dictionary<DateTime, DateTime?>();
                foreach (XmlElement child in element.ChildNodes)
                {
                    DateTime start = DateTime.Parse(child.GetAttribute("BeginTime"));
                    DateTime? end = null;
                    if (child.HasAttribute("EndTime"))
                    {
                        end = DateTime.Parse(child.GetAttribute("EndTime"));
                        if ((end.Value - start).TotalSeconds < Setting.ThresholdValueOfListenTime)
                        {
                            continue;
                        }
                    }
                    if (!times.ContainsKey(start))
                    {
                        times.Add(start, end);
                    }
                }
                if (times.Count == 0)
                {
                    continue;
                }
                ListenHistoryInfo history = new ListenHistoryInfo()
                {
                    Name = name,
                    Length = length,
                    ListenTimes = times,
                    Singer = singer,
                };
                histories.Add(history);
            }
            return histories;
        }

        public bool Record()
        {
            string name = Music.Info.Name;
            string length = Music.Info.Length.ToString();
            string singer = Music.Info.Singer;
            XmlElement element = Histories.FirstOrDefault(p => p.GetAttribute("Name") == name && p.GetAttribute("Length") == length && p.GetAttribute("Singer") == singer);
            if (element == null)
            {
                element = xml.CreateElement("History");
                element.SetAttribute("Name", name);
                element.SetAttribute("Length", length);
                element.SetAttribute("Singer", singer);
                root.AppendChild(element);
            }

            XmlElement timeElement = xml.CreateElement("Listen");
            timeElement.SetAttribute("BeginTime", DateTime.Now.ToString());
            element.AppendChild(timeElement);
            lastTimeElement = timeElement;

            try
            {
                xml.Save(XmlPath);
            }
            catch (Exception ex)
            {
            }
            return false;
        }
    }
}