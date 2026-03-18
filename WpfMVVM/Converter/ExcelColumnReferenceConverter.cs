using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace WpfMvvm.Converter
{
    /// <summary>
    /// ExcelのA1形式列文字を
    /// R1C1形式数値に変換する
    /// </summary>
    public class ExcelColumnReferenceConverter : MarkupExtension, IValueConverter
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
            if (!(value is string a1ColumnStr))
            {
                throw new ArgumentException(Properties.Resources.ArgumentExceptionMessage, nameof(value));
            }
            return A1ToR1C1Column(a1ColumnStr);
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
            if (!(value is int ric1ColumnIndex))
            {
                throw new ArgumentException(Properties.Resources.ArgumentExceptionMessage, nameof(value));
            }
            
            return R1C1ToA1Column(ric1ColumnIndex);
        }

        /// <summary>
        /// A1形式の列番号をR1C1形式の数値に変換する
        /// </summary>
        /// <param name="a1ColumnStr"></param>
        /// <returns></returns>
        public static int A1ToR1C1Column(string a1ColumnStr)
        {
            if(a1ColumnStr == null)
            {
                throw new ArgumentNullException(nameof(a1ColumnStr));
            }
            if(string.IsNullOrWhiteSpace(a1ColumnStr))
            {
                throw new ArgumentException(Properties.Resources.ArgumentExceptionMessage, nameof(a1ColumnStr));
            }

            int r1c1ColumnIndex = 0;
            for(int index = 0; index < a1ColumnStr.Length; index++)
            {
                var c = a1ColumnStr[index];
                if(c < 'A' || c > 'Z')
                {
                    throw new ArgumentException(Properties.Resources.ArgumentExceptionMessage, nameof(a1ColumnStr));
                }

                var exponent = a1ColumnStr.Length - index - 1;
                var digitValue = c - 'A' + 1;
                r1c1ColumnIndex += (int)Math.Pow(26, exponent) * digitValue;
            }

            return r1c1ColumnIndex;
        }

        /// <summary>
        /// A1形式の列番号をR1C1形式の数値に変換する
        /// </summary>
        /// <param name="a1ColumnStr"></param>
        /// <returns></returns>
        public static string R1C1ToA1Column(int ric1ColumnIndex)
        {
            if (ric1ColumnIndex <= 0)
            {
                throw new ArgumentException(Properties.Resources.ArgumentExceptionMessage, nameof(ric1ColumnIndex));
            }

            var a1ColumnStrSB = new StringBuilder();

            var checkValue = ric1ColumnIndex;
            do
            {
                //基数との除算の余りを計算
                var modValue = (checkValue-1) % 26;
                var digitChar = (char)('A' + modValue);
                a1ColumnStrSB.Insert(0, digitChar);

                //基数で割る
                checkValue = (checkValue - 1) / 26;
            }
            while (checkValue > 0);

            return a1ColumnStrSB.ToString();
        }

        /// <summary>
        /// XAML上での記述の簡略化
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
