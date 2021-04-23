using EasyMusic.Enum;
using EasyMusic.Helper;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class WidthConverter : IValueConverter
    {
        //当值从绑定源传播给绑定目标时，调用方法Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value - 64;
        }

        //当值从绑定目标传播给绑定源时，调用此方法ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class NullableConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? isCheck = value as bool?;
            if (null == isCheck)
            {
                return false;
            }
            else
            {
                return isCheck.Value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}