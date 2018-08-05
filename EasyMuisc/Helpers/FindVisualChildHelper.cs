using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EasyMusic.Helper
{

    public static class FindVisualChildHelper
    {
        public static T GetFirstChildOfType<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return null;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);

                var result = (child as T) ?? GetFirstChildOfType<T>(child);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }


        /// <summary>
        /// WPF中查找元素的父元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i_dp"></param>
        /// <returns></returns>
        public static T FindParent<T>(DependencyObject i_dp) where T : DependencyObject
        {
            DependencyObject dobj = (DependencyObject)VisualTreeHelper.GetParent(i_dp);
            if (dobj != null)
            {
                if (dobj is T)
                {
                    return (T)dobj;
                }
                else
                {
                    dobj = FindParent<T>(dobj);
                    if (dobj != null && dobj is T)
                    {
                        return (T)dobj;
                    }
                }
            }
            return null;
        }



        /// <summary>
        ///WPF查找元素的子元素
        /// </summary>
        /// <typeparam name="childItem"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static childItem FindVisualChild<childItem>(DependencyObject obj)
    where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

    }
}
