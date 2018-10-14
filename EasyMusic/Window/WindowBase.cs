﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using WpfControls.FlatStyle;

namespace EasyMusic.Windows
{
    public class WindowBase : Window
    {
        public WindowBase()
        {
            //WindowTitleBrushBindingSystemThemeColor = false;

            //Win10StyleCommandsButton = true;
            //FontFamily = new System.Windows.Media.FontFamily("微软雅黑");
            //Resources.Add("DefaultFont", FontFamily);
            //WindowTitleBrush = Setting.BackgroundColor;
            Initialized += (p2, p3) =>  Header = WindowHeader.CreatTitle(this);
            BorderThickness = new Thickness(16);
            var effect = new DropShadowEffect();
            effect.BlurRadius = 16;
            effect.ShadowDepth = 0;
            effect.Opacity = 0.5;
            effect.Color = Colors.Gray;
            Effect = effect;

        }

        public WindowHeader Header { get; private set; }
    }
}
