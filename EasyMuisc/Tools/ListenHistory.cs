using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EasyMuisc.ShareStaticResources;
using static EasyMuisc.MusicHelper;
using System.Xml;
using System.IO;

namespace EasyMuisc.Tools
{
    public class ListenHistory
    {
        public ListenHistory()
        {
            ListenTimes = new List<DateTime>() { DateTime.Now };
            Name = CurrentMusic.Name;
            Length = CurrentMusic.Length;
        }

        public List<DateTime> ListenTimes { get; set; }
        public string Name { get; set; }
        public double Length { get; set; }
        private string Singer { get; set; }
        public int ListenNumber => ListenTimes.Count;
    }

    public class ListenHistoryHelper
    {
        private string XmlPath => set.ListenHistoryPath.Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).Replace("\\\\", "");

        XmlDocument xml = new XmlDocument();
        XmlElement root;

        IEnumerable<XmlElement> Histories => root.ChildNodes.Cast<XmlElement>();

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

        public bool AddHistory()
        {
            string name = CurrentMusic.Name;
            string length = CurrentMusic.Length.ToString();
            string singer = CurrentMusic.Singer;
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
            timeElement.SetAttribute("Time", DateTime.Now.ToString());
            element.AppendChild(timeElement);
            try
            {
                xml.Save(XmlPath);
            }
            catch(Exception ex)
            {

            }
            return false;
        }
    }
}
