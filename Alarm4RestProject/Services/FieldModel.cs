using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alarm4Rest_Viewer.Services
{
    public class AlarmFieldModel
    {
        public string FieldName  { get; set; }
    }

    public class SortFieldModel
    {
        public SortFieldModel(string fieldName, bool isCheck,bool isOrderByAsc)
        {
            FieldName = fieldName;
            IsChecked = isCheck;
            IsOrderByAsc = isOrderByAsc;
        }
        public string FieldName { get;  set; }
        public bool IsChecked { get;  set; }
        public bool IsOrderByAsc { get;  set; }
    }
}
