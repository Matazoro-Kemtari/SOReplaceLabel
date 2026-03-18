using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace WpfMvvm.Converter
{
    /// <summary>
    /// 全てのBool値がFalseかどうかを判定し、Bool値を返すコンバータ
    /// データバインド検証エラーが無いかを判定する
    /// </summary>
    public class IsNullConverter : MarkupExtension, IValueConverter
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
            return value == null;
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
