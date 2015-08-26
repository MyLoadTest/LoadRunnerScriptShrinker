using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn.Converters
{
    [ContentProperty(nameof(ConversionMap))]
    internal abstract class DictionaryOneWayConverter<TSource, TTarget> : IValueConverter
    {
        protected DictionaryOneWayConverter()
        {
            ConversionMap = new Dictionary<TSource, TTarget>();
        }

        public Dictionary<TSource, TTarget> ConversionMap
        {
            get;
            internal set;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!targetType.IsAssignableFrom(typeof(TTarget)))
            {
                throw new ArgumentException(@"Invalid target type.", nameof(targetType));
            }

            var castValue = (TSource)value;
            return ConversionMap[castValue];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}