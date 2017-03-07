using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Alarm4Rest.Data;
using SmartAlarmData;
using System.Linq.Expressions;
using Alarm4Rest_Viewer.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Alarm4Rest_Viewer.RestAlarmLists;

namespace Alarm4Rest_Viewer.CustControl
{
    class filterToolBarViewModel : PropertyChangeEventBase
    {
        public static List<string> selectedStations = new List<string>();
        public static List<string> selectedPriority = new List<string>();
        public static List<string> selectedGroupDescription = new List<string>();
        public Expression<Func<RestorationAlarmLists, bool>> filterParseDeleg;

        public static List<Item> filters = new List<Item>();

        private HashSet<Item> mCheckedItems;

        private ObservableCollection<Item> mpriorityItems;
        public IEnumerable<Item> priorityItems { get { return mpriorityItems; } }


        private ObservableCollection<Item> mgroupDescItems;
        public IEnumerable<Item> groupDescItems { get { return mgroupDescItems; } }


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
        public string selectedPriorityView
        {
            get { return _selectedPriorityView; }
            set
            {
                Set(ref _selectedPriorityView, value);
                OnPropertyChanged("selectedPriorityView");
            }
        }
        public string selectedGroupDescriptionView
        {
            get { return _selectedGroupDescriptionView; }
            set
            {
                Set(ref _selectedGroupDescriptionView, value);
                OnPropertyChanged("selectedGroupDescriptionView");
            }
        }

        public string _selectedStationsView;
        public string _selectedPriorityView;
        public string _selectedGroupDescriptionView;

        public string filterText
        {
            get { return _filterText; }
            set
            {
                Set(ref _filterText, value);
                OnPropertyChanged("Text");
            }

        }

        private string _filterText;

        public filterToolBarViewModel()
        {
            mstationItems = new ObservableCollection<Item>();
            mpriorityItems = new ObservableCollection<Item>();
            mgroupDescItems = new ObservableCollection<Item>();
            mCheckedItems = new HashSet<Item>();

            //Subscribe to RestAlarmChanged of the RestAlarmsListViewModel
            RestAlarmsListViewModel.RestAlarmChanged += OnRestAlarmChanged; 

            mstationItems.CollectionChanged += Items_CollectionChanged;
            mpriorityItems.CollectionChanged += Items_CollectionChanged;
            mgroupDescItems.CollectionChanged += Items_CollectionChanged;

            RunFilterCmd = new RelayCommand(o => onFilterAlarms(), o => canFilter());
        }

        //Auto Get Station Name when DB has been loaded.
        private void OnRestAlarmChanged(object source, RestEventArgs arg)
        {
            if (arg.message == "hasLoaded")
            {
                
                // Adding Station ComboBox items
                foreach (var Station in RestAlarmsRepo.StationsName)
                {
                    mstationItems.Add(new Item(Station.ToString(), "StationName"));
                }

                // Adding Priority ComboBox items
                foreach (var Priority in RestAlarmsRepo.Priority)
                {
                    mpriorityItems.Add(new Item(Priority.ToString(), "Priority"));
                }

                // Adding GoupDescription ComboBox items
                foreach (var GroupDescription in RestAlarmsRepo.DeviceType)
                {
                    mgroupDescItems.Add(new Item(GroupDescription.ToString(), "GroupDescription"));
                }
            }
        }

        //Manual Get Station Name
        public void LoadFilterParameter()
        {

                //Stations = new ObservableCollection<string>(
                //                await RestAlarmsRepo.GetStationNameAsync());

                // Adding Station ComboBox items
                mstationItems.Add(new Item("All", "StationName"));
                foreach (var Station in RestAlarmsRepo.StationsName)
                {
                    mstationItems.Add(new Item(Station.ToString(), "StationName"));
                }

                // Adding Priority ComboBox items
                mpriorityItems.Add(new Item("All", "Priority"));
                foreach (var Priority in RestAlarmsRepo.Priority)
                {
                    mpriorityItems.Add(new Item(Priority.ToString(), "Priority"));
                }

                // Adding GoupDescription ComboBox items
                mgroupDescItems.Add(new Item("All", "GroupDescription"));
                foreach (var GroupDescription in RestAlarmsRepo.DeviceType)
                {
                    mgroupDescItems.Add(new Item(GroupDescription.ToString(), "GroupDescription"));
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

            UpdateFilterParseTxt();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                Item item = (Item)sender;
                if (item.IsChecked)
                {
                    mCheckedItems.Add(item);
                    switch(item.FieldName)
                    {
                        case "StationName":
                            selectedStations.Add(item.Value.TrimEnd());
                            break;
                        case "Priority":
                            selectedPriority.Add(item.Value.TrimEnd());
                            break;
                        case "GroupDescription":
                            selectedGroupDescription.Add(item.Value.TrimEnd());
                            break;
                    }

                    //selectedStations.Add(item.Value.TrimEnd());
                    filters.Add(item); //Add to filter parse
                }
                else
                {
                    mCheckedItems.Remove(item);
                    switch (item.FieldName)
                    {
                        case "StationName":
                            selectedStations.Remove(item.Value.TrimEnd());
                            break;
                        case "Priority":
                            selectedPriority.Remove(item.Value.TrimEnd());
                            break;
                        case "GroupDescription":
                            selectedGroupDescription.Remove(item.Value.TrimEnd());
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
            selectedStationsView = string.Join(" | ", selectedStations.ToArray());
            selectedPriorityView = string.Join(" | ", selectedPriority.ToArray());
            selectedGroupDescriptionView = string.Join(" | ", selectedGroupDescription.ToArray());

            //To Do
            List<string> filterTextList = new List<string>();
            if(selectedStationsView.Count() > 0) filterTextList.Add("("+ selectedStationsView +")");
            if (selectedPriorityView.Count() > 0) filterTextList.Add("(" + selectedPriorityView + ")");
            if (selectedGroupDescriptionView.Count() > 0) filterTextList.Add("(" + selectedGroupDescriptionView + ")");

            if (filterTextList.Count() > 0)
            {
                filterText = string.Join(" & ", filterTextList.ToArray());
                Console.WriteLine(filterText);
            }
        }

        private string _filter_Parse;
        public string filter_Parse
        {
            get { return _filter_Parse; }
            set
            {
                _filter_Parse = value;
                OnPropertyChanged("filter_Parse");
                // RestAlarmsRepo.filter_Parse = value;
            }
        }
        public RelayCommand RunFilterCmd { get; private set; }

        public bool canFilter()
        {
            return mCheckedItems.Count != 0;
        }
        public async void onFilterAlarms()
        {
            //Implement for each query Group by PropertyName : StationName , Priority or Desc.

            //ExpressGen();

            IEnumerable<IGrouping<string, Item>> groupFields =
                    from item in filters
                    group item by item.FieldName;

            filterParseDeleg = FilterExpressionBuilder.GetExpression<RestorationAlarmLists>(groupFields);

            RestAlarmsRepo.filterParseDeleg = filterParseDeleg;
            await RestAlarmsRepo.GetCustAlarmAct();

            Console.WriteLine(filterParseDeleg.Body);

        }
    }
}

