using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMuisc
{
    public class MusicInfo
    {
        public string MusicName { get; internal set; }
        public string Singer { get; internal set; }
        public string Length { get; internal set; }
        public string Album { get; internal set; }
        public string Path { get; internal set; }
        public bool Enable { get; internal set; }
    }
}
