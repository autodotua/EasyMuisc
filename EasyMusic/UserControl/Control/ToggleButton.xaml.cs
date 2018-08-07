using System;
using System.Windows;
using System.Windows.Controls;

namespace EasyMusic.UserControls
{
    /// <summary>
    /// ToggleButton.xaml 的交互逻辑
    /// </summary>
    public partial class ToggleButton : System.Windows.Controls.UserControl
    {
        //Border border;
        public ToggleButton()
        {
            InitializeComponent();
            //Loaded += (p1, p2) =>
            //  {
            //      border = FindVisualChildHelper.FindVisualChild<Border>(btn);
            //      if (isPressed)
            //      {
            //          border.Padding = new Thickness(12, 6, 12, 6);

            //      }
            //  };
        }
        private bool isPressed = false;
        public  bool IsPressed
        {
            get => isPressed;
            set
            {
                isPressed = value;
                //if (border != null)
                //{
                //    border.Padding = value ? new Thickness(12, 8, 12, 8) : new Thickness(8,2,8,2);
                //}
                FontWeight = value ? FontWeights.Bold : FontWeights.Normal;
                
            }
        }

        public string Text
        {
            get => btn.Content as string;
            set => btn.Content = value;
        }
        

        public event EventHandler Select;

        public void RaiseClickEvent()
        {
            btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void BtnClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (!IsPressed)
            {
                IsPressed = true;
                Select(this, new EventArgs());
            }
        }
    }
}
