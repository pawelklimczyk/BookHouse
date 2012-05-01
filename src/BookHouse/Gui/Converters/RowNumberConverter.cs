using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace BooksHouse.Gui.Converters
{
    class RowNumberConverter:IMultiValueConverter
    {
            #region IMultiValueConverter Members

            public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                Object item = values[0];
                DataGrid grid = values[1] as DataGrid;

                int index = grid.Items.IndexOf(item);

                return (index+1).ToString();
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            #endregion
        

    }
}
