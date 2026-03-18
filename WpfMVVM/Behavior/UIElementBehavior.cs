using System;
using System.Windows;

namespace WpfMvvm.Behavior
{
    public static class UIElementBehavior
    {
        /// <summary>
        /// ゲッター
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static bool GetIsFocused(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }

            if (!(dependencyObject is UIElement element))
            {
                throw new InvalidCastException();
            }

            return element.IsFocused;
        }

        /// <summary>
        /// セッター
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static void SetIsFocused(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(IsFocusedProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsFocused.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused",
                typeof(bool),
                typeof(UIElementBehavior),
                new PropertyMetadata(false, OnIsFocusedChanged));

        /// <summary>
        ///
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        private static void OnIsFocusedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (!(dependencyObject is UIElement element))
            {
                return;
            }

            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (element.Focusable && oldValue)
            {
                element.LostFocus -= Element_LostFocus;
            }
            if (element.Focusable && newValue)
            {
                element.LostFocus += Element_LostFocus;
                element.Focus();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Element_LostFocus(object sender, RoutedEventArgs e)
        {
            SetIsFocused(sender as DependencyObject, false);
        }
    }
}