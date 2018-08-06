using EasyMusic.Helper;
using System;
using System.Globalization;
using System.Windows.Data;

namespace EasyMusic.Info
{
    [Serializable]
    public class MusicInfo
    {
        public string Name { get; set; }
        public string Singer { get; set; }
        public int Length { get; set; }

        public string Album { get; set; }
        public string Path { get; set; }
    }

   
}
