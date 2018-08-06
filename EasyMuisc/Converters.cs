using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using EasyMusic.Helper;

namespace EasyMusic.Converter
{
    public class CycleModeButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CycleMode mode = EasyMusic.Helper.MusicControlHelper.CycleMode;
            if (mode == CycleMode.ListCycle && parameter as string == "0")
            {
                return Visibility.Visible;
            }
            if (mode == CycleMode.SingleCycle && parameter as string == "1")
            {
                return Visibility.Visible;
            }
            if (mode == CycleMode.Shuffle && parameter as string == "2")
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
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
