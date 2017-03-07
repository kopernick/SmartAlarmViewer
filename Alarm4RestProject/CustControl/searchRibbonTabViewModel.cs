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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Alarm4Rest_Viewer.CustControl
{
    class searchRibbonTabViewModel : PropertyChangeEventBase
    {
        #region Search Properties
        public static List<string> selectedStations = new List<string>();
        public static List<string> selectedField = new List<string>();
        public static List<string> selectedGroupDescription = new List<string>();
        public Expression<Func<RestorationAlarmLists, bool>> searchParseDeleg;

        public static List<Item> searchList = new List<Item>();

        private HashSet<Item> mCheckedItems;

        private ObservableCollection<Item> mfieldItems;
        public IEnumerable<Item> fieldItems { get { return mfieldItems; } }


        private ObservableCollection<Item> mpriorityItems;
        public IEnumerable<Item> priorityItems { get { return mpriorityItems; } }


        private ObservableCollection<Item> mstationItems;
        public IEnumerable<Item> stationItems { get { return mstationItems; } }

        private ObservableCollection<string> _Stations;
        public ObservableCollection<string> Stations
        {
            get { return _Stations; }
            set
            {
                _Stations = value;
                OnPropertyChanged("Stations");
            }
        }
        public string selectedStationsView
        {
            get { return _selectedStationsView; }
            set
            {
                Set(ref _selectedStationsView, value);
                OnPropertyChanged("selectedStationsView");
            }
        }
        public string selectedFieldView
        {
            get { return _selectedFieldView; }
            set
            {
                Set(ref _selectedFieldView, value);
                OnPropertyChanged("selectedFieldView");
            }
        }
        public string selectedPriorityView
        {
            get { return _selectedPriorityView; }
            set
            {
                Set(ref _selectedPriorityView, value);
                OnPropertyChanged("selectedPriorityView");
            }
        }

        public string _selectedStationsView;
        public string _selectedFieldView;
        public string _selectedPriorityView;

        public string searchText
        {
            get { return _searchText; }
            set
            {
                Set(ref _searchText, value);
                OnPropertyChanged("Text");
            }

        }

        private string _searchText;
        //Default Search Keyword
        private string _search_Parse_Pri;
        public string search_Parse_Pri
        {
            get { return _search_Parse_Pri; }
            set
            {
                _search_Parse_Pri = value;
                OnPropertyChanged("search_Parse_Pri");
                // RestAlarmsRepo.filter_Parse = value;
            }
        }

        //Option Search Keyword
        private string _search_Parse_Sec;
        public string search_Parse_Sec
        {
            get { return _search_Parse_Sec; }
            set
            {
                _search_Parse_Sec = value;
                OnPropertyChanged("search_Parse_Sec");
                // RestAlarmsRepo.filter_Parse = value;
            }
        }

        #endregion


        public searchRibbonTabViewModel()
        {
            #region Initialize Search menu

            //RestAlarmsListViewModel.RestAlarmChanged += OnRestAlarmChanged;
            RestAlarmsRepo.RestAlarmChanged += OnRestAlarmChanged;

            mstationItems = new ObservableCollection<Item>();
            mfieldItems = new ObservableCollection<Item>();
            mpriorityItems = new ObservableCollection<Item>();
            mCheckedItems = new HashSet<Item>();

            mstationItems.CollectionChanged += Items_CollectionChanged;
            mfieldItems.CollectionChanged += Items_CollectionChanged;
            mpriorityItems.CollectionChanged += Items_CollectionChanged;
            search_Parse_Pri = "";
            search_Parse_Sec = "";

            RunSearchCmd = new RelayCommand(o => onSearchAlarms(), o => canSearch());

            #endregion
        }

        #region Search Helper function

        /* WPF call method with 2 parameter*/
        RelayCommand _RunSearchTimeCondCmd;
        public ICommand RunSearchTimeCondCmd
        {
            get
            {
                if (_RunSearchTimeCondCmd == null)
                {
                    _RunSearchTimeCondCmd = new RelayCommand(p => RunSearchTimeCond(p),
                        p => true);
                }
                return _RunSearchTimeCondCmd;
            }
        }

        private async void RunSearchTimeCond(object value)
        {
            RestAlarmsRepo.fDateTimeCondItem = (TimeCondItem)value;
            RestAlarmsRepo.fDateTimeCondEnd = DateTime.Now;
            await RestAlarmsRepo.TGetCustAlarmAct();

        }

        //Auto Get Station Name when DB has been loaded.
        private void OnRestAlarmChanged(object source, RestEventArgs arg)
        {
            if (arg.message == "Start Success")
            {

                // Adding Station ComboBox items
                foreach (var Station in RestAlarmsRepo.StationsName)
                {
                if(Station != null)
                    mstationItems.Add(new Item(Station.ToString(), "StationName"));
                }

                // Adding Priority ComboBox items
                foreach (var Priority in RestAlarmsRepo.Priority)
                {
                if (Priority != null)
                    mpriorityItems.Add(new Item(Priority.ToString(), "Priority"));
                }

                // Adding Search field items
                mfieldItems.Add(new Item("PointName", "FieldName"));
                mfieldItems.Add(new Item("Message", "FieldName"));
                //mpfieldItems.Add(new Item("Priority", "FieldName"));
                mfieldItems.Add(new Item("GroupPointName", "FieldName"));
                mfieldItems.Add(new Item("GroupDescription", "FieldName"));

            }
        }
        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Item item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                    mCheckedItems.Remove(item);
                }
            }
            if (e.NewItems != null)
            {
                foreach (Item item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                    if (item.IsChecked) mCheckedItems.Add(item);
                }
            }

            //UpdateFilterParseTxt();
        }
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                Item item = (Item)sender;
                if (item.IsChecked)
                {
                    mCheckedItems.Add(item);
                    switch (item.FieldName)
                    {
                        case "StationName":
                            selectedStations.Add(item.Value.TrimEnd());
                            break;
                        case "FieldName":
                            selectedField.Add(item.Value.TrimEnd());
                            break;
                        case "Priority":
                            selectedGroupDescription.Add(item.Value.TrimEnd());
                            break;
                    }

                    //selectedStations.Add(item.Value.TrimEnd());
                    searchList.Add(item); //Add to filter parse
                }
                else
                {
                    mCheckedItems.Remove(item);
                    switch (item.FieldName)
                    {
                        case "StationName":
                            selectedStations.Remove(item.Value.TrimEnd());
                            break;
                        case "FieldName":
                            selectedField.Remove(item.Value.TrimEnd());
                            break;
                        case "Priority":
                            selectedGroupDescription.Remove(item.Value.TrimEnd());
                            break;
                    }
                    //selectedStations.Remove(item.Value.TrimEnd());
                    searchList.Remove(item); //Remove from filter parse
                }
                UpdateSeachParseTxt();
            }
        }
        private void UpdateSeachParseTxt()
        {
            selectedStationsView = string.Join(" | ", selectedStations.ToArray());
            selectedFieldView = string.Join(" | ", selectedField.ToArray());
            selectedPriorityView = string.Join(" | ", selectedGroupDescription.ToArray());

            //To Do
            List<string> filterTextList = new List<string>();
            if (selectedFieldView.Count() > 0) filterTextList.Add("Search " + searchText + "in field(s)" + "(" + selectedFieldView + ")");
            if (selectedStationsView.Count() > 0) filterTextList.Add(" @ station :" + "(" + selectedStationsView + ")");
            if (filterTextList.Count() > 0)
            {
                searchText = string.Join(" & ", filterTextList.ToArray());
                Console.WriteLine(searchText);
            }
        }

        public RelayCommand RunSearchCmd { get; private set; }

        public bool canSearch()
        {
            return (_search_Parse_Pri != "" || _search_Parse_Sec != "");
        }
        public async void onSearchAlarms()
        {
            //Implement for each query Group by PropertyName : StationName , Priority or Desc.
            //ExpressGen();

            IEnumerable<IGrouping<string, Item>> groupFields =
                    from item in searchList
                    group item by item.FieldName;

            string[] search_Parse_Pri_List = search_Parse_Pri.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine(search_Parse_Pri_List.Length);

            //searchParseDeleg = SearchingExpressionBuilder.GetExpression<RestorationAlarmList>(groupFields, search_Parse_Pri);
            searchParseDeleg = SearchingExpressionBuilder.GetExpression<RestorationAlarmLists>(groupFields, search_Parse_Pri_List, _search_Parse_Sec);

            if (searchParseDeleg == null)
            {
                Console.WriteLine("Expression Building Error");
            }
            else
            {
                RestAlarmsRepo.custPageIndex = 1;
                RestAlarmsRepo.filterParseDeleg = searchParseDeleg;
                RestAlarmsRepo.fDateTimeCondEnd = DateTime.Now;
                await RestAlarmsRepo.TGetCustAlarmAct();
                Console.WriteLine(searchParseDeleg.Body);
            }

        }
        #endregion
    }
}
