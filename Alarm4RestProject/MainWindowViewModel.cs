using System;
using System.Collections.Generic;
using System.Linq;
using Alarm4Rest_Viewer.Services;
using Alarm4Rest_Viewer.CustomAlarmLists;
using Alarm4Rest_Viewer.RestAlarmLists;
using System.Windows.Input;
using Alarm4Rest_Viewer.QueryAlarmLists;
using System.Windows.Controls.Ribbon;
using System.Collections.ObjectModel;

namespace Alarm4Rest_Viewer
{
    public class MainWindowViewModel : PropertyChangeEventBase
    {

        //private filterToolBarViewModel _filterToolViewModel = new filterToolBarViewModel();
        //private searchToolBarViewModel _searchToolViewModel = new searchToolBarViewModel();
        private CustomAlarmListViewModel _custAlarmViewModel = new CustomAlarmListViewModel();
        private QueryAlarmsListViewModel _queryAlarmViewModel = new QueryAlarmsListViewModel();

        TimeCondItem DateTimeCond = new TimeCondItem();

        DateTime exclusiveEnd = new DateTime();

        public RelayCommand EnableSearchCmd { get; private set; }
        public RelayCommand EnableFilterCmd { get; private set; }
        public RelayCommand EnableCustView { get; private set; }

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

        #region Sorting Templat

        //-----------------------------------SortFiel Items----------------------------------//

        private ObservableCollection<SortFieldModel> _FieldOrders;
        public ObservableCollection<SortFieldModel> FieldOrders
        {
            get { return _FieldOrders; }
            set
            {
                _FieldOrders = value;
                OnPropertyChanged("FieldOrders");
            }
        }
        //public static List<SortItem> sortOrderList = new List<SortItem>();
        //public SortItem orderParseDeleg;
        #endregion

        #region Filter Properties
        
        #endregion

        #region Search Properties
        
        #endregion
        public MainWindowViewModel()
        {
            //_filterToolViewModel = new filterToolBarViewModel();
            //_searchToolViewModel = new searchToolBarViewModel();


            RestAlarmsRepo.InitializeRepository(); // Start define --> DBContext = new Alarm4RestorationContext();

            //EnableSearchCmd = new RelayCommand(o => onSearchAlarms(), o => canSearch());
            //EnableFilterCmd = new RelayCommand(o => onFilterAlarms(), o => canFilter());
            EnableCustView = new RelayCommand(o => onCustView(), o => canViewMain());

            //RestAlarmsListViewModel.RestAlarmChanged += OnRestAlarmChanged;
            RestAlarmsRepo.RestAlarmChanged += OnRestAlarmChanged;

            pageSize = RestAlarmsRepo.pageSize;
            DateTimeCond = new TimeCondItem("Week", 1);
            exclusiveEnd = DateTime.Now;

            SetPageSize = new RelayCommand(o => onSetPageSize(), o => canSetPageSize());

            #region Initialize Sort menu

            InitSortOrderField();
            RestAlarmsRepo.sortParseDeleg = FieldOrders.ToList();

            #endregion

            #region Initialize filter menu

            #endregion

            #region Initialize Search menu

            #endregion
        }

        #region Helper Function
        private void OnRestAlarmChanged(object source, RestEventArgs arg)
        {

            if (arg.message == "Start Success")
            {
                CustAlarmViewModel = _queryAlarmViewModel;
            }
        }

        public RelayCommand SetPageSize { get; private set; }

        public bool canSetPageSize()
        {
            return (RestAlarmsRepo.pageSize != pageSize && pageSize != 0);
        }
        public async void onSetPageSize()
        {
            RestAlarmsRepo.pageSize = pageSize;
            await RestAlarmsRepo.GetRestAlarmAct();
            await RestAlarmsRepo.GetCustAlarmAct();
            await RestAlarmsRepo.TGetQueryAlarmAct();
        }

        #endregion

        #region Sort Template Helper function
        private void InitSortOrderField()
        {
            FieldOrders = new ObservableCollection<SortFieldModel>();

            //Defual Order fields
            FieldOrders.Add(new SortFieldModel("DateTime", true, false));          //Defual First Order field
            FieldOrders.Add(new SortFieldModel("StationName", true, true));      //Defual Second Order field
            FieldOrders.Add(new SortFieldModel("DeviceType", false, true));
            FieldOrders.Add(new SortFieldModel("PointName", false, true));


        }

        #endregion

        #region Filter Helper function

        #endregion

        #region Search Helper function

        #endregion


        RelayCommand _RibbonTabSelectionChangedCommand;
        public ICommand RibbonTabSelectionChangedCommand
        {
            get
            {
                if (_RibbonTabSelectionChangedCommand == null)
                {
                    _RibbonTabSelectionChangedCommand = new RelayCommand(p => OnRibbonTabSelectionChanged(p), p => true);
                }
                return _RibbonTabSelectionChangedCommand;
            }
        }

        private void OnRibbonTabSelectionChanged(object e)
        {
            //ExpressGen();
            Ribbon ribbon = (Ribbon)e;
            //MyRibbon.SelectedItem.
            if (ribbon == null) return;
            var ribbonTab = ribbon.SelectedItem;   //Get the default container and using the container get the singleton region manager
            //var container = ServiceLocator.Current.GetInstance<IUnityContainer>();
            //var regionManager = container.Resolve<IRegionManager>();
            string header = (ribbonTab as RibbonTab).Header.ToString();

            switch (header)
            {
                case "Main":
                    Console.WriteLine(header);
                    CustAlarmViewModel = _queryAlarmViewModel;
                    break;
                case "Advance Search":
                    Console.WriteLine(header);
                    CustAlarmViewModel = _custAlarmViewModel;
                    break;

                case "Advance Filter":
                    Console.WriteLine(header);
                    CustAlarmViewModel = _custAlarmViewModel;
                    break;
            }
        }
        private PropertyChangeEventBase _CustAlarmViewModel;
        public PropertyChangeEventBase CustAlarmViewModel
        {
            get { return _CustAlarmViewModel; }
            set
            {
                SetProperty(ref _CustAlarmViewModel, value);
            }
        }


        public void onCustView()
        {
            //ExpressGen();
            Console.WriteLine("Run MainView cmd");
            CustAlarmViewModel = null;
            //RestorationAlarmLists.RestAlarmsListViewModel.LoadRestorationAlarmsAsync();

        }


        public bool canViewMain()
        {
            return true;
        }

    }
}
