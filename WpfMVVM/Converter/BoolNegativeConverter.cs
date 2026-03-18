using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace WpfMvvm.Converter
{
    /// <summary>
    /// bool値を正負反転するコンバータ
    /// 参考：http://kitunechan.hatenablog.jp/entry/2016/11/24/154531
    /// </summary>
    public class BoolNegativeConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool boolValue))
            {
                throw new ArgumentException(Properties.Resources.ArgumentExceptionMessage, nameof(value));
            }

            return !boolValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool boolValue))
            {
                throw new ArgumentException(Properties.Resources.ArgumentExceptionMessage, nameof(value));
            }

            return !boolValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
