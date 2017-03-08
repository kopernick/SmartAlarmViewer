using System;
using Alarm4Rest_Viewer.Services;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq.Expressions;
//using Alarm4Rest.Data;
using SmartAlarmData;
using System.Linq;
using System.Collections.ObjectModel;
using Solutions.Wpf.DragDrop;

namespace Alarm4Rest_Viewer.CustControl
{
    class mainRibbonTapViewModel : PropertyChangeEventBase, IDropTarget
    {

        #region Properties
        public static List<SortItem> sortOrderList = new List<SortItem>();
        public static List<string> qSelectedGroupDescription = new List<string>();
        public static List<string> fltSelectedPriority = new List<string>();
        

        public Expression<Func<RestorationAlarmLists, bool>> queryParseDeleg;
        public static List<Item> qFilters = new List<Item>();

        private HashSet<Item> mQCheckedItems;        
        
        //-----------------------------------SortFiel Items----------------------------------//
        private ObservableCollection<Item> mfieldItems1;
        public IEnumerable<Item> fieldItems1 { get { return mfieldItems1; } }

        private ObservableCollection<Item> mfieldItems2;
        public IEnumerable<Item> fieldItems2 { get { return mfieldItems2; } }

        private ObservableCollection<Item> mfieldItems3;
        public IEnumerable<Item> fieldItems3 { get { return mfieldItems3; } }

        private ObservableCollection<AlarmFieldModel> _AlarmListFields;
        public ObservableCollection<AlarmFieldModel> AlarmListFields
        {
            get { return _AlarmListFields; }
            set
            {
                _AlarmListFields = value;
                OnPropertyChanged("AlarmListFields");
            }
        }

        //-----------------------------------Category----------------------------------//
        private Item _CatDesc1;
        private Item _CatDesc2;
        private Item _CatDesc3;
        private Item _CatDesc4;
        private Item _CatDesc5;
        private Item _CatDesc6;
        private Item _CatDesc7;
        private Item _CatDesc8;

        private TimeCondItem _Today;
        private TimeCondItem _Last2Days;
        private TimeCondItem _ThisWeek;
        private TimeCondItem _Last2Weeks;
        private TimeCondItem _ThisMonth;
        private TimeCondItem _EveryDays;
        private TimeCondItem _UserSel;

        public Item CatDesc1 { get { return _CatDesc1; } }
        public Item CatDesc2 { get { return _CatDesc2; } }
        public Item CatDesc3 { get { return _CatDesc3; } }
        public Item CatDesc4 { get { return _CatDesc4; } }
        public Item CatDesc5 { get { return _CatDesc5; } }
        public Item CatDesc6 { get { return _CatDesc6; } }
        public Item CatDesc7 { get { return _CatDesc7; } }
        public Item CatDesc8 { get { return _CatDesc8; } }


        //---------------------------------Time Filtering-----------------------------------//
        public TimeCondItem Today { get { return _Today; } }
        public TimeCondItem Last2Days { get { return _Last2Days; } }
        public TimeCondItem ThisWeek { get { return _ThisWeek; } }
        public TimeCondItem Last2Weeks { get { return _Last2Weeks; } }
        public TimeCondItem ThisMonth { get { return _ThisMonth; } }
        public TimeCondItem EveryDays { get { return _EveryDays; } }
        public TimeCondItem UserSel { get { return _UserSel; } }

        private DateTime _exclusiveEnd = new DateTime();
        private DateTime _exclusiveStart = new DateTime();

        public DateTime ExclusiveEnd
        {
            get
            {
                return _exclusiveEnd;
            }
            set
            {
                if (value != null)
                {
                    RestAlarmsRepo.qDateTimeCondEnd = value.AddDays(1);
                    _exclusiveEnd = value;
                }
            }
        }

        public DateTime ExclusiveStart
        {
            get
            {

                return _exclusiveStart;
            }
            set
            {
                RestAlarmsRepo.qDateTimeCondStart = value;
                _exclusiveStart = value;
            }
        }
        #endregion

        //------------------------------Class Construction--------------------------------------//
        public mainRibbonTapViewModel()
        {
            //To Do ต้องประการที่เดียว
            InitSortOrderTemplate();
            InitCategoryFiltering();
            InitTimeFiltering();
            InitSortOrderField();

            RestAlarmsRepo.RestAlarmChanged += OnRestAlarmChanged;
            RestAlarmsRepo.qDateTimeCondItem = _Last2Weeks;

            RestAlarmsRepo.orderParseDeleg = sortOrderList.First(i => i.ID == 1);

            RunUserQueryCmd = new RelayCommand(o => onUserQuery(), o => canUserQuery());
        }

        //------------------------------Helper Function--------------------------------------//
        private void InitSortOrderField()
        {
            _AlarmListFields = new ObservableCollection<AlarmFieldModel>();

            mfieldItems1 = new ObservableCollection<Item>();
            mfieldItems1.Add(new Item("first", "DateTime"));
            mfieldItems1.Add(new Item("first", "StationName"));
            mfieldItems1.Add(new Item("first", "Priority"));

            mfieldItems2 = new ObservableCollection<Item>();
            mfieldItems2.Add(new Item("second", "DateTime"));
            mfieldItems2.Add(new Item("second", "StationName"));
            mfieldItems2.Add(new Item("second", "Priority"));

            mfieldItems3 = new ObservableCollection<Item>();
            mfieldItems3.Add(new Item("third", "DateTime"));
            mfieldItems3.Add(new Item("third", "StationName"));
            mfieldItems3.Add(new Item("third", "Priority"));

        }
        private void InitSortOrderTemplate()
        {
            sortOrderList.Add(new SortItem(1, "StationName", "DateTime", "Priority"));
            sortOrderList.Add(new SortItem(2, "StationName", "Priority", "DateTime"));
            sortOrderList.Add(new SortItem(3, "DateTime", "StationName", "Priority"));
        }

        private void InitCategoryFiltering()
        {
            //isChecked87X = true;

            //Item(name (show at UI), value (contain), Field Name);
            _CatDesc1 = new Item("Tx Protection", "Tx Relay", "GroupDescription");
            _CatDesc1.IsChecked = true;
            if (_CatDesc1.IsChecked) qFilters.Add(_CatDesc1);

            _CatDesc2 = new Item("Bus Protection", "Bus Relay", "GroupDescription");
            _CatDesc2.IsChecked = true;
            if (_CatDesc2.IsChecked) qFilters.Add(_CatDesc2);

            _CatDesc3 = new Item("Bkr Protection", "Bkr Relay", "GroupDescription");
            _CatDesc3.IsChecked = true;
            if (_CatDesc3.IsChecked) qFilters.Add(_CatDesc3);

            _CatDesc4 = new Item("Line Protection", "Line Relay", "GroupDescription");
            _CatDesc4.IsChecked = true;
            if (_CatDesc4.IsChecked) qFilters.Add(_CatDesc4);

            _CatDesc5 = new Item("Shunt Reactor Protection", "Reactor Relay", "GroupDescription");
            _CatDesc5.IsChecked = true;
            if (_CatDesc5.IsChecked) qFilters.Add(_CatDesc5);

            _CatDesc6 = new Item("C-Bank Protection", "C-Bank Relay", "GroupDescription");
            _CatDesc6.IsChecked = true;
            if (_CatDesc6.IsChecked) qFilters.Add(_CatDesc6);

            _CatDesc7 = new Item("Bkr Status", "Bkr Status", "GroupDescription");
            _CatDesc7.IsChecked = true;
            if (_CatDesc3.IsChecked) qFilters.Add(_CatDesc7);

            _CatDesc8 = new Item("Etc. Protection", "Etc. Relay", "GroupDescription");
            _CatDesc8.IsChecked = false;
            if (_CatDesc3.IsChecked) qFilters.Add(_CatDesc8);
        }

        private void InitTimeFiltering()
        {
            _Today = new TimeCondItem("Day", 1, false);
            _Last2Days = new TimeCondItem("Day", 2, false);
            _ThisWeek = new TimeCondItem("Week", 1, false);
            _Last2Weeks = new TimeCondItem("Week", 2, true); //Default
            _ThisMonth = new TimeCondItem("Month", 1, false);
            _EveryDays = new TimeCondItem("All", 1, false);
            _UserSel = new TimeCondItem("User", 1, false);

            _exclusiveEnd = DateTime.Now;
            _exclusiveStart = _exclusiveEnd.AddDays((-1) * 5);
        }

        private void OnRestAlarmChanged(object source, RestEventArgs arg)
        {
            if (arg.message == "Start Success")
            {

                // Adding AlarmListFields ComboBox items
                foreach (var AlarmListField in RestAlarmsRepo.AlarmListFields)
                {
                    if (AlarmListField != null)
                        _AlarmListFields.Add(new AlarmFieldModel { FieldName = AlarmListField.ToString() });
                }

            }
        }


        #region Sort Function
        /* WPF call method with 1 parameter*/

        RelayCommand _RunStdSortQuery;
        public ICommand RunStdSortQuery
        {
            get
            {
                if (_RunStdSortQuery == null)
                {
                    _RunStdSortQuery = new RelayCommand(p => onRunStdSort(p), p => true);
                }
                return _RunStdSortQuery;
            }
        }

        private async void onRunStdSort(object txtSortTemplate)
        {
            //CustAlarmViewModel = null;

            int sortTemplate = Convert.ToInt32(txtSortTemplate);
            RestAlarmsRepo.orderParseDeleg = sortOrderList.First(i => i.ID == sortTemplate);

            RestAlarmsRepo.qDateTimeCondEnd = DateTime.Now;

            await RestAlarmsRepo.TGetQueryAlarmAct();

            Console.WriteLine(RestAlarmsRepo.orderParseDeleg.ID);
        }

        /* WPF call method with 2 parameter*/
        RelayCommand _RunQueryTimeCondCmd;
        public ICommand RunQueryTimeCondCmd
        {
            get
            {
                if (_RunQueryTimeCondCmd == null)
                {
                    _RunQueryTimeCondCmd = new RelayCommand(p => RunQueryTimeCond(), p => true);
                }
                return _RunQueryTimeCondCmd;
            }
        }

        // WPF Call with 2 parameter
        private void RunQueryTimeCond()
        {
            TimeCondItem Dummy = new TimeCondItem();

            if (_Today.IsChecked)
            {
                Dummy = _Today.Clone();
            }
            else if (_Last2Days.IsChecked)
            {
                Dummy = _Last2Days.Clone();
            }
            else if(_ThisWeek.IsChecked)
            {
                Dummy = _ThisWeek.Clone();
            }
            else if (_Last2Weeks.IsChecked)
            {
                Dummy = _Last2Weeks.Clone();
            }
            else if (_ThisMonth.IsChecked)
            {
                Dummy = _ThisMonth.Clone();
            }
            else if (_EveryDays.IsChecked)
            {
                Dummy = _EveryDays.Clone();
            }
            else if(_UserSel.IsChecked)
            {
                Dummy = _UserSel.Clone();
            }

            RestAlarmsRepo.qDateTimeCondItem = Dummy;
            //RestAlarmsRepo.qDateTimeCondEnd = DateTime.Now;
            //await RestAlarmsRepo.TGetQueryAlarmAct();

        }

        #endregion Sort Function


        #region User Query Function
        public RelayCommand RunUserQueryCmd { get; private set; }

        public bool canUserQuery()
        {
            return qFilters.Count != 0;
        }
        public async void onUserQuery()
        {

            Console.WriteLine("Run Standard Query cmd");

            IEnumerable<IGrouping<string, Item>> groupFields =
                    from item in qFilters
                    group item by item.FieldName;

            // Preparing for New Database
            //queryParseDeleg = FilterExpressionBuilder.GetExpression<RestorationAlarmList>(groupFields);

            RestAlarmsRepo.filterParseDeleg = queryParseDeleg;
            //RestAlarmsRepo.qDateTimeCondEnd = DateTime.Now;
            await RestAlarmsRepo.TGetQueryAlarmAct();
            //Console.WriteLine(queryParseDeleg.Body);

        }

        RelayCommand _CheckCommand;
        public ICommand CheckCommand
        {
            get
            {
                if (_CheckCommand == null)
                {
                    _CheckCommand = new RelayCommand(p => onCheckCommand(p), p => true);
                }
                return _CheckCommand;
            }
        }

        private void onCheckCommand(object Category)
        {

            if ((string)Category == _CatDesc1.Name)
            {
                AddRemoveProcess(ref _CatDesc1);
            }
            if ((string)Category == _CatDesc2.Name)
            {
                AddRemoveProcess(ref _CatDesc2);
            }
            else
            {

            }

        }
        private void AddRemoveProcess(ref Item _CatDescX)
        {
            Item CatTemp = _CatDescX;

            var qf = qFilters
                    .Where(i => i.Value == CatTemp.Value && i.FieldName == CatTemp.FieldName).ToList();

            if (CatTemp.IsChecked)
            {
                if (qf.Count == 0) qFilters.Add(CatTemp);
            }
            else
            {
                if (qf.Count != 0)
                {
                    foreach (var item in qf)
                        qFilters.Remove(item);
                }
            }
        }



        void IDropTarget.DragOver(DropInfo dropInfo)
        {
            //if (dropInfo.Data is PupilViewModel && dropInfo.TargetItem is SchoolViewModel)
            //{
            //    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            //    dropInfo.Effects = DragDropEffects.Move;
            //}
        }

        void IDropTarget.Drop(DropInfo dropInfo)
        {
            //SchoolViewModel school = (SchoolViewModel)dropInfo.TargetItem;
            //PupilViewModel pupil = (PupilViewModel)dropInfo.Data;
            //school.Pupils.Add(pupil);
            //((IList)dropInfo.DragInfo.SourceCollection).Remove(pupil);
        }


        //public void DragOver(DropInfo dropInfo)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Drop(DropInfo dropInfo)
        //{
        //    throw new NotImplementedException();
        //}



        ///* WPF call method with 1 parameter*/
        //private bool isChecked87X;
        //public bool IsChecked87X
        //{
        //    get { return isChecked87X; }
        //    set
        //    {
        //        isChecked87X = value;
        //       // OnPropertyChanged("IsChecked87X");
        //    }
        //}


        #endregion User Query Function

    }
}