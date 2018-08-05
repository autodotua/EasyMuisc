using EasyMusic.Helper;
using System;
using System.Globalization;
using System.Windows.Data;

namespace EasyMusic
{
    [Serializable]
    public class MusicInfo
    {
        public string Name { get; set; }
        public string Singer { get; set; }
        //public string DisplayLength => MusicHelper.GetStringLength(Length);
        public int Length { get; set; }

        public string Album { get; set; }
        public string Path { get; set; }
    }

    public class LengthToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MusicListHelper.GetStringLength((int)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
