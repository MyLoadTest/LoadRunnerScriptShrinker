using System;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn.Converters
{
    [ContentProperty(nameof(ConversionMap))]
    internal sealed class LogRecordTypeToBitmapImageConverter : DictionaryOneWayConverter<LogRecordType, BitmapImage>
    {
    }
}