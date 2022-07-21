using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace WPF_Timeline.Converters
{
    internal class TimeSpanInterpolationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(parameter.GetType() != typeof(TimeSpan[]))
            {
                throw new ArgumentException();
            }

            TimeSpan[] timeSpans = (TimeSpan[])parameter;

            if(timeSpans.Length != 2)
            {
                throw new ArgumentException();
            }

            return timeSpans[0] + (timeSpans[1] - timeSpans[0]) * (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter.GetType() != typeof(TimeSpan[]))
            {
                throw new ArgumentException();
            }

            TimeSpan[] timeSpans = (TimeSpan[])parameter;

            if (timeSpans.Length != 2)
            {
                throw new ArgumentException();
            }

            return ((TimeSpan)value - timeSpans[0]) / (timeSpans[1] - timeSpans[0]);
        }
    }
}
