using System;
using System.Collections.Generic;
using System.Linq;
using static EasyMusic.Helper.MusicListHelper;

namespace EasyMusic.Info
{
    public class ListenHistoryInfo
    {
        public Dictionary<DateTime, DateTime?> ListenTimes { get; set; }
        public string LastListenTime => ListenTimes.Last().Key.ToString();
        public string Name { get; set; }
        public int Length { get; set; }
        public string DisplayLength => GetStringLength(Length);
        public string Singer { get; set; }
        public int ListenNumber => ListenTimes.Count;
        //public string TotalTime
        //{
        //    get
        //    {
        //        TimeSpan totalTime = new TimeSpan();
        //        foreach (var time in ListenTimes)
        //        {
        //            if(!time.Value.HasValue)
        //            {
        //                continue;
        //            }
        //            TimeSpan span = time.Value.Value - time.Key;
        //            totalTime += span;
        //        }
        //        return totalTime.ToString();
        //    }
        //}
    }
}