using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shell;
using FzLib.Control.FlatStyle;

namespace EasyMusic.Windows
{
    public class WindowBase : Window
    {
        Grid mainGrid;
        public WindowBase()
        {
            //WindowTitleBrushBindingSystemThemeColor = false;

            //Win10StyleCommandsButton = true;
            //FontFamily = new System.Windows.Media.FontFamily("微软雅黑");
            //Resources.Add("DefaultFont", FontFamily);
            //WindowTitleBrush = Setting.BackgroundColor;
            Initialized += (p2, p3) =>
            {
                mainGrid = Content as Grid;
                Header = WindowHeader.CreatTitle(this);
                WindowChrome.SetWindowChrome(this, new WindowChrome() { ResizeBorderThickness = new Thickness(16,0,16,16),CaptionHeight=0 });
                WindowHelper.RepairWindowBehavior(this);
            };
            BorderThickness = new Thickness(16);
            var effect = new DropShadowEffect();
            effect.BlurRadius = 16;
            effect.ShadowDepth = 0;
            effect.Opacity = 0.5;
            effect.Color = Colors.Gray;
            Effect = effect;

        }

        public WindowHeader Header { get; private set; }
        private Thickness defaultMargin = default;
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Maximized)
            {
                defaultMargin = mainGrid.Margin;
                BorderThickness = new Thickness();
                //mainGrid.Margin = new Thickness(4, Header.ActualHeight+4, 4, 4);
                //Header.Margin =new Thickness (0, -WindowHeader.marginTop , 0, 0);
                WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(0);
            }
            else
            {
                BorderThickness = new Thickness(16);
                //Header.Margin = new Thickness(0, -WindowHeader.marginTop, 0, 0);
               // mainGrid.Margin = defaultMargin;
                WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(16, 0, 16, 16);
            };
      
        }
    }
}
