using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeatTrader.Evolution
{
    public static class UI
    {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject dep) where T : DependencyObject
        {
            if (dep != null)
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dep); i++)
                {
                    var child = VisualTreeHelper.GetChild(dep, i);

                    if (child is T) yield return (T)child;

                    foreach (var descendant in FindVisualChildren<T>(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            return FindVisualChildren<T>(obj).FirstOrDefault();
        }

        public static T FindVisualTreeParent<T>(this DependencyObject dep) where T : DependencyObject
        {
            while (dep != null && !(dep is T))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            return dep as T;
        }

        public static void WaitForUpdate()
        {
            if (Application.Current == null) return;

            var frame = new DispatcherFrame();

            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;

                return null;

            }), null);

            System.Windows.Threading.Dispatcher.PushFrame(frame);
        }
    }
}