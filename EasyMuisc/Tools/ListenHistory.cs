using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EasyMuisc.ShareStaticResources;
using static EasyMuisc.MusicHelper;

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

    public static class ListenHistoryHelper
    {
        public static bool AddHistory()
        {
            return false;
        }
    }
}
