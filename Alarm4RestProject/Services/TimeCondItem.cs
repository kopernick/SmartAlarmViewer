/* ********************************************
 * TimeCondItem |TimeType|Count|IsCheched|
 * 
 * Sample Item : |"Day"|2|True|, |Month|3|False|, |Day|5|False|, etc
 * 
 * ********************************************/
using System;
using System.Globalization;
using System.Windows.Data;

namespace Alarm4Rest_Viewer.Services
{
    public class TimeCondItem : PropertyChangeEventBase
    {
        public string TimeType { get; private set; }
        public int Value { get; private set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { Set(ref _isChecked, value); }
        }
        public TimeCondItem()
        {
        }
        public  TimeCondItem Clone()
        {
            TimeCondItem obj = this;
            return obj;
        }
        public TimeCondItem(int value)
        {
            Value = value;
            TimeType = "Day";
        }
        public TimeCondItem(string timeType, int value)
        {
            Value = value;
            TimeType = timeType;
        }
        public TimeCondItem(string timeType, int value, bool isChecked)
        {
            Value = value;
            TimeType = timeType;
            Set(ref _isChecked, isChecked);
        }
        public override string ToString()
        {
            return Value+" "+TimeType;
        }
    }
    public class MultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // return String.Format("{0} {1}", values[0], values[1]);

                TimeCondItem data = new TimeCondItem(values[0].ToString(), System.Convert.ToInt32(values[1]));
                return data;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
