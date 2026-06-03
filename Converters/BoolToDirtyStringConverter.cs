using System;
using System.Globalization;
using System.Windows.Data;

namespace Mp3TagFree.Converters
{
    public class BoolToDirtyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDirty && isDirty)
            {
                return "●"; // Bullet symbol representing unsaved edits
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
