//using Alarm4Rest.Data;
using SmartAlarmData;
using Alarm4Rest_Viewer.RestAlarmLists;
using Alarm4Rest_Viewer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;

namespace Alarm4Rest_Viewer.CustControl
{
    public class filterRibbonTabViewModel : PropertyChangeEventBase
    {
        #region Filter Properties
        public static List<string> fltSelectedStations = new List<string>();
        public static List<string> fltSelectedPriority = new List<string>();
        public static List<string> fltSelectedMessage = new List<string>();
        public static List<string> fltSelectedGroupDescription = new List<string>();
        public Expression<Func<RestorationAlarmLists, bool>> filterParseDeleg;

        public static List<Item> filters = new List<Item>();

        private HashSet<Item> mfltCheckedItems;

        private ObservableCollection<Item> mfltPriorityItems;
        public IEnumerable<Item> fltPriorityItems { get { return mfltPriorityItems; } }


        private ObservableCollection<Item> mfltGroupDescItems;
        public IEnumerable<Item> fltGroupDescItems { get { return mfltGroupDescItems; } }

        private ObservableCollection<Item> mfltMessageItems;
        public IEnumerable<Item> fltMessageItems { get { return mfltMessageItems; } }

        private ObservableCollection<Item> mfltStationItems;
        public IEnumerable<Item> fltStationItems { get { return mfltStationItems; } }

        private ObservableCollection<string> _fltStations;
        public ObservableCollection<string> fltStations
        {
            get { return _fltStations; }
            set
            {
                _fltStations = value;
                OnPropertyChanged("fltStations");
            }
        }
        public string fltSelectedStationsView
        {
            get { return _fltSelectedStationsView; }
            set
            {
                Set(ref _fltSelectedStationsView, value);
                OnPropertyChanged("fltSelectedStationsView");
            }
        }
        public string fltSelectedPriorityView
        {
            get { return _fltSelectedPriorityView; }
            set
            {
                Set(ref _fltSelectedPriorityView, value);
                OnPropertyChanged("fltSelectedPriorityView");
            }
        }
        public string fltSelectedGroupDescriptionView
        {
            get { return _fltSelectedGroupDescriptionView; }
            set
            {
                Set(ref _fltSelectedGroupDescriptionView, value);
                OnPropertyChanged("fltSelectedGroupDescriptionView");
            }
        }

        public string fltSelectedMessageView
        {
            get { return _fltSelectedMessageView; }
            set
            {
                Set(ref _fltSelectedMessageView, value);
                OnPropertyChanged("fltSelectedMessageView");
            }
        }

        private string _fltSelectedStationsView;
        private string _fltSelectedPriorityView;
        private string _fltSelectedGroupDescriptionView;
        private string _fltSelectedMessageView;
        public string filterText
        {
            get { return _filterText; }
            set
            {
                Set(ref _filterText, value);
                OnPropertyChanged("Text");
            }

        }

        private int _pageSize;
        public int pageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = value;
                OnPropertyChanged("pageSize");
            }
        }

        private string _filterText;
        #endregion

        public filterRibbonTabViewModel()
        {
            #region Initialize filter menu

            //RestAlarmsListViewModel.RestAlarmChanged += OnRestAlarmChanged;
            RestAlarmsRepo.RestAlarmChanged += OnRestAlarmChanged;
            mfltStationItems = new ObservableCollection<Item>();
            mfltPriorityItems = new ObservableCollection<Item>();
            mfltGroupDescItems = new ObservableCollection<Item>();
            mfltMessageItems = new ObservableCollection<Item>();
            mfltCheckedItems = new HashSet<Item>();

            mfltStationItems.CollectionChanged += fltItems_CollectionChanged;
            mfltPriorityItems.CollectionChanged += fltItems_CollectionChanged;
            mfltGroupDescItems.CollectionChanged += fltItems_CollectionChanged;
            mfltMessageItems.CollectionChanged += fltItems_CollectionChanged;

            RunFilterCmd = new RelayCommand(o => onFilterAlarms(), o => canFilter());

            
            #endregion

        }

        #region Filter Helper function

        /* WPF call method with 2 parameter*/
        RelayCommand _RunFilterTimeCondCmd;
        public ICommand RunFilterTimeCondCmd
        {
            get
            {
                if (_RunFilterTimeCondCmd == null)
                {
                    _RunFilterTimeCondCmd = new RelayCommand(p => RunFilterTimeCond(p),
                        p => true);
                }
                return _RunFilterTimeCondCmd;
            }
        }

        // WPF Call with 2 parameter
        private async void RunFilterTimeCond(object value)
        {
            RestAlarmsRepo.fDateTimeCondItem = (TimeCondItem)value;
            RestAlarmsRepo.fDateTimeCondEnd = DateTime.Now;
            await RestAlarmsRepo.TGetCustAlarmAct();

            //Console.WriteLine(filterParseDeleg.Body);
        }

        private void OnRestAlarmChanged(object source, RestEventArgs arg)
        {
            if (arg.message == "Start Success")
            {

                // Adding Station ComboBox items
                foreach (var Station in RestAlarmsRepo.StationsName)
                {
                    if(Station != null)
                    mfltStationItems.Add(new Item(Station.ToString(), "StationName"));

                }

                // Adding Priority ComboBox items
                foreach (var Priority in RestAlarmsRepo.Priority)
                {
                    if (Priority != null)
                        mfltPriorityItems.Add(new Item(Priority.ToString(), "Priority"));

                }

                // Adding GoupDescription ComboBox items
                foreach (var DeviceType in RestAlarmsRepo.DeviceType)
                {
                    if (DeviceType != null)
                        mfltGroupDescItems.Add(new Item(DeviceType.ToString(), "GroupDescription"));
                }

                // Adding Message ComboBox items
                foreach (var Message in RestAlarmsRepo.Message)
                {
                    if (Message != null)
                        mfltMessageItems.Add(new Item(Message.ToString(), "Message"));
                }

            }
        }

        private void fltItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Item item in e.OldItems)
                {
                    item.PropertyChanged -= fltItem_PropertyChanged;
                    mfltCheckedItems.Remove(item);
                }
            }
            if (e.NewItems != null)
            {
                foreach (Item item in e.NewItems)
                {
                    item.PropertyChanged += fltItem_PropertyChanged;
                    if (item.IsChecked) mfltCheckedItems.Add(item);
                }
            }

        }

        private void fltItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                Item item = (Item)sender;
                if (item.IsChecked)
                {
                    mfltCheckedItems.Add(item);
                    switch (item.FieldName)
                    {
                        case "StationName":
                            fltSelectedStations.Add(item.Value.TrimEnd());
                            break;
                        case "Priority":
                            fltSelectedPriority.Add(item.Value.TrimEnd());
                            break;
                        case "Message":
                            fltSelectedMessage.Add(item.Value.TrimEnd());
                            break;
                        case "GroupDescription":
                            fltSelectedGroupDescription.Add(item.Value.TrimEnd());
                            break;
                    }

                    //selectedStations.Add(item.Value.TrimEnd());
                    filters.Add(item); //Add to filter parse
                }
                else
                {
                    mfltCheckedItems.Remove(item);
                    switch (item.FieldName)
                    {
                        case "StationName":
                            fltSelectedStations.Remove(item.Value.TrimEnd());
                            break;
                        case "Priority":
                            fltSelectedPriority.Remove(item.Value.TrimEnd());
                            break;
                        case "Message":
                            fltSelectedMessage.Remove(item.Value.TrimEnd());
                            break;
                        case "GroupDescription":
                            fltSelectedGroupDescription.Remove(item.Value.TrimEnd());
                            break;
                    }
                    //selectedStations.Remove(item.Value.TrimEnd());
                    filters.Remove(item); //Remove from filter parse
                }
                UpdateFilterParseTxt();
            }
        }
        private void UpdateFilterParseTxt()
        {
            fltSelectedStationsView = string.Join(" | ", fltSelectedStations.ToArray());
            fltSelectedPriorityView = string.Join(" | ", fltSelectedPriority.ToArray());
            fltSelectedGroupDescriptionView = string.Join(" | ", fltSelectedGroupDescription.ToArray());
            fltSelectedMessageView = string.Join(" | ", fltSelectedMessage.ToArray());

            //To Do
            List<string> filterTextList = new List<string>();
            if (fltSelectedStationsView.Count() > 0) filterTextList.Add("(" + fltSelectedStationsView + ")");
            if (fltSelectedPriorityView.Count() > 0) filterTextList.Add("(" + fltSelectedPriorityView + ")");
            if (fltSelectedGroupDescriptionView.Count() > 0) filterTextList.Add("(" + fltSelectedGroupDescriptionView + ")");
            if (fltSelectedMessageView.Count() > 0) filterTextList.Add("(" + fltSelectedMessageView + ")");

            if (filterTextList.Count() > 0)
            {
                filterText = string.Join(" & ", filterTextList.ToArray());
                Console.WriteLine(filterText);
            }
        }

        public RelayCommand RunFilterCmd { get; private set; }

        public bool canFilter()
        {
            return mfltCheckedItems.Count != 0;
        }
        public async void onFilterAlarms()
        {
            //Implement for each query Group by PropertyName : StationName , Priority or Desc.
            //ExpressGen();
            Console.WriteLine("Run Filter cmd");
            //CustAlarmViewModel = _custAlarmViewModel;

            IEnumerable<IGrouping<string, Item>> groupFields =
                    from item in filters
                    group item by item.FieldName;

            filterParseDeleg = FilterExpressionBuilder.GetExpression<RestorationAlarmLists>(groupFields);

            RestAlarmsRepo.filterParseDeleg = filterParseDeleg;
            RestAlarmsRepo.fDateTimeCondEnd = DateTime.Now;
            await RestAlarmsRepo.TGetCustAlarmAct();

            Console.WriteLine(filterParseDeleg.Body);

        }

        #endregion Filter Helper function
    }
}
