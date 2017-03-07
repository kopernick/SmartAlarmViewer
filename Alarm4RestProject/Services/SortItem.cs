using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alarm4Rest_Viewer.Services
{
    public class SortItem
    {
        public struct strSortOrder
        {
            public string sortField;
            public ListSortDirection sortDirection;
        }


        private SortItem sortOrder;

        public int ID { get; private set; }
        public string sortFirstOrder { get; private set; }
        public string sortSecondOrder { get; private set; }
        public string sortThirdOrder { get; private set; }

        public SortItem(string sort1)
        {
            this.sortFirstOrder = sort1;
        }
        public SortItem(string sort1, string sort2)
        {
            this.sortFirstOrder = sort1;
            this.sortSecondOrder = sort2;
        }
        public SortItem(string sort1, string sort2, string sort3)
        {
            this.sortFirstOrder = sort1;
            this.sortSecondOrder = sort2;
            this.sortThirdOrder = sort3;
        }
        public SortItem(int id, string sort1, string sort2, string sort3)
        {
            this.ID = id;
            this.sortFirstOrder = sort1;
            this.sortSecondOrder = sort2;
            this.sortThirdOrder = sort3;
        }
        public SortItem(SortItem sortOrder)
        {
            this.sortOrder = sortOrder;
        }
        public string[] ToArray()
        {

            string[] strArray = { this.sortFirstOrder,this.sortSecondOrder,this.sortThirdOrder};
            return strArray;
        }
    }
}
