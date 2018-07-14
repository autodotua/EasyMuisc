using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static EasyMuisc.Tools.Tools;
using static EasyMuisc.GlobalDatas;
using EasyMuisc.Tools;

namespace EasyMuisc.UserControls
{
    /// <summary>
    /// LrcListView.xaml 的交互逻辑
    /// </summary>
    public partial class LrcListView : UserControl
    {
        //ScrollViewer scroll;
        public LrcListView()
        {
            InitializeComponent();
        }

        //public LrcListView(ObservableCollection<string> lrcs)
        //{
        //    lbx.ItemsSource = Lrcs;
        //}

        public void Add(TextBlock txt)
        {
            var item = new ListBoxItem() { Content = txt, Foreground = Foreground, FontWeight = FontWeight };
            //TriggerActionCollection tac = new TriggerActionCollection();
            //DoubleAnimation ta = new DoubleAnimation();
            //Storyboard sb = new Storyboard();
            //tac.Add(sb);

            //Trigger trigger = new Trigger()
            //{
            //    a
            //};
            lbx.Items.Add(item);
        }

        //public void ChangeFontSize(double size)
        //{
        //    lbx.FontSize = size;
        //}
        public void Clear()
        {
            lbx.Items.Clear();
        }
        public void RefreshPlaceholder(double height, double highLightFontSize)
        {
            Resources["topHalfHeight"] = height - highLightFontSize / 2;
            Resources["bottomHalfHeight"] = height - highLightFontSize;

        }

        private void MouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        public void ScrollTo(int index, List<int> indexArray, double fontSize)
        {
            double height = (indexArray[index] - 1) * fontSize * FontFamily.LineSpacing * (1 + 1.8 / fontSize);//瞎几把乱写的公式


            //NewDoubleAnimation(scroll,
            //     ScrollAnimationBehavior.VerticalOffsetProperty,
            //     height,
            //     0.8,
            //     0,
            //     null,
            //     false,
            //     new CubicEase()
            //     {
            //         EasingMode = EasingMode.EaseInOut
            //     });

            DoubleAnimation ani = new DoubleAnimation(-height, set.AnimationDuration) { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(ani, lbx);
            Storyboard.SetTargetProperty(ani, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            Storyboard storyToSmall = new Storyboard() { Children = { ani } };
            storyToSmall.Begin();
        }



        DoubleAnimation aniFontSize = new DoubleAnimation
        {
            Duration = set.AnimationDuration,// new Duration(TimeSpan.FromSeconds(0.8)),//动画时间1秒
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut },
            //DecelerationRatio = 0.5,
        };

        public void RefreshFontSize(int index)
        {

            //return;

            for (int i = 0; i < lbx.Items.Count; i++)
            {
                var txt = ((lbx.Items[i] as ListBoxItem).Content as TextBlock);
                if (i == index)
                {
                    //    //txt.FontSize = highlight;
                    //Tools.NewDoubleAnimation(
                    //    txt,
                    //    FontSizeProperty,
                    //    highlight,
                    //    0.8,0.5
                    //    );
                    aniFontSize.To = set.HighlightLrcFontSize;
                    txt.BeginAnimation(TextBlock.FontSizeProperty, aniFontSize);
                    //BeginStoryboard(story);

                }
                else if (txt.FontSize != set.NormalLrcFontSize)
                {
                    //txt.FontSize = normal;
                    aniFontSize.To = set.NormalLrcFontSize;
                    txt.BeginAnimation(TextBlock.FontSizeProperty, aniFontSize);
                }
            }



        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //scroll =
            //     FindVisualChildHelper.FindVisualChild<ScrollViewer>(lbx);
            //scroll.ScrollChanged += (p1, p2) => Debug.WriteLine(p2.VerticalOffset);
        }

        //private void svw_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{

        //    scroll?.ScrollToHorizontalOffset(scroll.ScrollableWidth / 2);
        //}
    }




    //public static class ScrollAnimationBehavior
    //{
    //    #region Private ScrollViewer for ListBox

    //    private static ScrollViewer _listBoxScroller = new ScrollViewer();

    //    #endregion

    //    #region VerticalOffset Property

    //    public static DependencyProperty VerticalOffsetProperty =
    //        DependencyProperty.RegisterAttached("VerticalOffset",
    //                                            typeof(double),
    //                                            typeof(ScrollAnimationBehavior),
    //                                            new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

    //    public static void SetVerticalOffset(FrameworkElement target, double value)
    //    {
    //        target.SetValue(VerticalOffsetProperty, value);
    //    }

    //    public static double GetVerticalOffset(FrameworkElement target)
    //    {
    //        return (double)target.GetValue(VerticalOffsetProperty);
    //    }

    //    #endregion

    //    #region TimeDuration Property

    //    public static DependencyProperty TimeDurationProperty =
    //        DependencyProperty.RegisterAttached("TimeDuration",
    //                                            typeof(TimeSpan),
    //                                            typeof(ScrollAnimationBehavior),
    //                                            new PropertyMetadata(new TimeSpan(0, 0, 0, 0, 0)));

    //    public static void SetTimeDuration(FrameworkElement target, TimeSpan value)
    //    {
    //        target.SetValue(TimeDurationProperty, value);
    //    }

    //    public static TimeSpan GetTimeDuration(FrameworkElement target)
    //    {
    //        return (TimeSpan)target.GetValue(TimeDurationProperty);
    //    }

    //    #endregion

    //    #region PointsToScroll Property

    //    public static DependencyProperty PointsToScrollProperty =
    //        DependencyProperty.RegisterAttached("PointsToScroll",
    //                                            typeof(double),
    //                                            typeof(ScrollAnimationBehavior),
    //                                            new PropertyMetadata(0.0));

    //    public static void SetPointsToScroll(FrameworkElement target, double value)
    //    {
    //        target.SetValue(PointsToScrollProperty, value);
    //    }

    //    public static double GetPointsToScroll(FrameworkElement target)
    //    {
    //        return (double)target.GetValue(PointsToScrollProperty);
    //    }

    //    #endregion

    //    #region OnVerticalOffset Changed

    //    private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    //    {
    //        ScrollViewer scrollViewer = target as ScrollViewer;

    //        if (scrollViewer != null)
    //        {
    //            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
    //        }
    //    }

    //    #endregion

    //    #region IsEnabled Property

    //    public static DependencyProperty IsEnabledProperty =
    //                                            DependencyProperty.RegisterAttached("IsEnabled",
    //                                            typeof(bool),
    //                                            typeof(ScrollAnimationBehavior),
    //                                            new UIPropertyMetadata(false, OnIsEnabledChanged));

    //    public static void SetIsEnabled(FrameworkElement target, bool value)
    //    {
    //        target.SetValue(IsEnabledProperty, value);
    //    }

    //    public static bool GetIsEnabled(FrameworkElement target)
    //    {
    //        return (bool)target.GetValue(IsEnabledProperty);
    //    }

    //    #endregion

    //    #region OnIsEnabledChanged Changed

    //    private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    //    {
    //        var target = sender;

    //        //if (target != null && target is ScrollViewer)
    //        //{
    //        //    ScrollViewer scroller = target as ScrollViewer;
    //        //    scroller.Loaded += new RoutedEventHandler(scrollerLoaded);
    //        //}

    //        if (target != null && target is ListBox)
    //        {
    //            ListBox listbox = target as ListBox;
    //            listbox.Loaded += new RoutedEventHandler(listboxLoaded);
    //        }
    //    }

    //    #endregion

    //    #region AnimateScroll Helper

    //    private static void AnimateScroll(ScrollViewer scrollViewer, double ToValue)
    //    {
    //        DoubleAnimation verticalAnimation = new DoubleAnimation();

    //        verticalAnimation.From = scrollViewer.VerticalOffset;
    //        verticalAnimation.To = ToValue;
    //        verticalAnimation.Duration = new Duration(GetTimeDuration(scrollViewer));

    //        Storyboard storyboard = new Storyboard();

    //        storyboard.Children.Add(verticalAnimation);
    //        Storyboard.SetTarget(verticalAnimation, scrollViewer);
    //        Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(ScrollAnimationBehavior.VerticalOffsetProperty));
    //        storyboard.Begin();
    //    }

    //    #endregion

    //    #region NormalizeScrollPos Helper

    //    private static double NormalizeScrollPos(ScrollViewer scroll, double scrollChange, Orientation o)
    //    {
    //        double returnValue = scrollChange;

    //        if (scrollChange < 0)
    //        {
    //            returnValue = 0;
    //        }

    //        if (o == Orientation.Vertical && scrollChange > scroll.ScrollableHeight)
    //        {
    //            returnValue = scroll.ScrollableHeight;
    //        }
    //        else if (o == Orientation.Horizontal && scrollChange > scroll.ScrollableWidth)
    //        {
    //            returnValue = scroll.ScrollableWidth;
    //        }

    //        return returnValue;
    //    }

    //    #endregion

    //    #region UpdateScrollPosition Helper

    //    private static void UpdateScrollPosition(object sender)
    //    {
    //        ListBox listbox = sender as ListBox;

    //        if (listbox != null)
    //        {
    //            double scrollTo = 0;

    //            for (int i = 0; i < (listbox.SelectedIndex); i++)
    //            {
    //                ListBoxItem tempItem = listbox.ItemContainerGenerator.ContainerFromItem(listbox.Items[i]) as ListBoxItem;

    //                if (tempItem != null)
    //                {
    //                    scrollTo += tempItem.ActualHeight;
    //                }
    //            }

    //            AnimateScroll(_listBoxScroller, scrollTo);
    //        }
    //    }

    //    #endregion

    //    //#region SetEventHandlersForScrollViewer Helper

    //    //private static void SetEventHandlersForScrollViewer(ScrollViewer scroller)
    //    //{
    //    //    scroller.PreviewMouseWheel += new MouseWheelEventHandler(ScrollViewerPreviewMouseWheel);
    //    //    scroller.PreviewKeyDown += new KeyEventHandler(ScrollViewerPreviewKeyDown);
    //    //}

    //    //#endregion

    //    //#region scrollerLoaded Event Handler

    //    //private static void scrollerLoaded(object sender, RoutedEventArgs e)
    //    //{
    //    //    ScrollViewer scroller = sender as ScrollViewer;

    //    //    SetEventHandlersForScrollViewer(scroller);
    //    //}

    //    //#endregion

    //    #region listboxLoaded Event Handler

    //    private static void listboxLoaded(object sender, RoutedEventArgs e)
    //    {
    //        ListBox listbox = sender as ListBox;

    //        _listBoxScroller = FindVisualChildHelper.GetFirstChildOfType<ScrollViewer>(listbox);
    //        //SetEventHandlersForScrollViewer(_listBoxScroller);

    //        SetTimeDuration(_listBoxScroller, new TimeSpan(0, 0, 0, 0, 200));
    //        SetPointsToScroll(_listBoxScroller, 16.0);

    //        listbox.SelectionChanged += new SelectionChangedEventHandler(ListBoxSelectionChanged);
    //        listbox.Loaded += new RoutedEventHandler(ListBoxLoaded);
    //        listbox.LayoutUpdated += new EventHandler(ListBoxLayoutUpdated);
    //    }

    //    #endregion

    //    //#region ScrollViewerPreviewMouseWheel Event Handler

    //    //private static void ScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    //    //{
    //    //    double mouseWheelChange = (double)e.Delta;
    //    //    ScrollViewer scroller = (ScrollViewer)sender;
    //    //    double newVOffset = GetVerticalOffset(scroller) - (mouseWheelChange / 3);

    //    //    if (newVOffset < 0)
    //    //    {
    //    //        AnimateScroll(scroller, 0);
    //    //    }
    //    //    else if (newVOffset > scroller.ScrollableHeight)
    //    //    {
    //    //        AnimateScroll(scroller, scroller.ScrollableHeight);
    //    //    }
    //    //    else
    //    //    {
    //    //        AnimateScroll(scroller, newVOffset);
    //    //    }

    //    //    e.Handled = true;
    //    //}

    //    //#endregion

    //    //#region ScrollViewerPreviewKeyDown Handler

    //    //private static void ScrollViewerPreviewKeyDown(object sender, KeyEventArgs e)
    //    //{
    //    //    ScrollViewer scroller = (ScrollViewer)sender;

    //    //    Key keyPressed = e.Key;
    //    //    double newVerticalPos = GetVerticalOffset(scroller);
    //    //    bool isKeyHandled = false;

    //    //    if (keyPressed == Key.Down)
    //    //    {
    //    //        newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos + GetPointsToScroll(scroller)), Orientation.Vertical);
    //    //        isKeyHandled = true;
    //    //    }
    //    //    else if (keyPressed == Key.PageDown)
    //    //    {
    //    //        newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos + scroller.ViewportHeight), Orientation.Vertical);
    //    //        isKeyHandled = true;
    //    //    }
    //    //    else if (keyPressed == Key.Up)
    //    //    {
    //    //        newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos - GetPointsToScroll(scroller)), Orientation.Vertical);
    //    //        isKeyHandled = true;
    //    //    }
    //    //    else if (keyPressed == Key.PageUp)
    //    //    {
    //    //        newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos - scroller.ViewportHeight), Orientation.Vertical);
    //    //        isKeyHandled = true;
    //    //    }

    //    //    if (newVerticalPos != GetVerticalOffset(scroller))
    //    //    {
    //    //        AnimateScroll(scroller, newVerticalPos);
    //    //    }

    //    //    e.Handled = isKeyHandled;
    //    //}

    //    //#endregion

    //    #region ListBox Event Handlers

    //    private static void ListBoxLayoutUpdated(object sender, EventArgs e)
    //    {
    //        UpdateScrollPosition(sender);
    //    }

    //    private static void ListBoxLoaded(object sender, RoutedEventArgs e)
    //    {
    //        UpdateScrollPosition(sender);
    //    }

    //    private static void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    //    {
    //        UpdateScrollPosition(sender);
    //    }

    //    #endregion
    //}






}
