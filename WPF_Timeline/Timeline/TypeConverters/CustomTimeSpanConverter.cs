using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace WPF_Timeline.Timeline.TypeConverters
{
    internal class CustomTimeSpanConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if(sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if(destinationType == typeof(TimeSpan) || destinationType == typeof(DateTime))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if(value.GetType() == typeof(string))
            {
                if(DateTime.TryParse((string)value, out DateTime date))
                {
                    return TimeSpan.FromTicks(date.Ticks);
                }

                if(TimeSpan.TryParse((string)value, out TimeSpan timeSpan))
                {
                    return timeSpan;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
