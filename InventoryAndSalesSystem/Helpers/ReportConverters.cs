using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InventoryAndSalesSystem.Helpers
{
    public class ReportTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            var currentType = value.ToString();
            var expectedType = parameter.ToString();

            return currentType == expectedType ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProfitColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal profit)
            {
                if (profit > 0)
                    return "Positive";
                else if (profit < 0)
                    return "Negative";
                else
                    return "Neutral";
            }
            return "Neutral";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}