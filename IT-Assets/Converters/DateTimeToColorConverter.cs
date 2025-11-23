using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace IT_Assets.Converters
{
    public class DateTimeToColorConverter : IValueConverter
    {
        public DateTime MaintenanceDate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime maintenanceDate)
            {
                // Check if the maintenance date is overdue (more than 1 month ago)
                return maintenanceDate.AddMonths(1) < DateTime.Now ? Colors.Red : Colors.Black;
            }
            return Colors.Black; // Default color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}