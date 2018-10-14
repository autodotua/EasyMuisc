using System.Windows;
using System.Windows.Media;
using WpfControls.FlatStyle;
using static EasyMusic.GlobalDatas;

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
            Initialized+=(p2,p3) =>
            WindowHeader.CreatTitle(this);
        }
    }
}
