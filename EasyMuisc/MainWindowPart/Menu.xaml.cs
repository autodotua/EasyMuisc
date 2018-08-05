using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static EasyMusic.Helper.MusicControlHelper;

namespace EasyMusic
{
    public partial class MainWindow : Window
    {

        #region 任务栏按钮
        /// <summary>
        /// 单击任务栏上的播放按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbiPlayClickEventHandler(object sender, EventArgs e)
        {
            Music.Play();
            tbiPlay.Visibility = Visibility.Collapsed;
            tbiPause.Visibility = Visibility.Visible;

        }
        /// <summary>
        /// 单击任务栏上的暂停按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbiPauseClickEventHandler(object sender, EventArgs e)
        {
            Music.Pause();
            tbiPause.Visibility = Visibility.Collapsed;
            tbiPlay.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 单击任务栏上的上一曲按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbiLastClickEventHandler(object sender, EventArgs e)
        {
            PlayLast();
        }
        /// <summary>
        /// 单击任务栏上的下一曲按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbiNextClickEventHandler(object sender, EventArgs e)
        {
            PlayNext();
        }

        #endregion

    }
}
