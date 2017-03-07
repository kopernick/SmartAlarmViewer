using System;
using Alarm4Rest_Viewer.Services;
//using Alarm4Rest.Data;
using SmartAlarmData;
using System.Collections.ObjectModel;
using System.Timers;
using System.ComponentModel;
using System.Windows.Input;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Alarm4Rest_Viewer.CustomAlarmLists
{
    class CustomAlarmListViewModel: PropertyChangeEventBase
    {

        //private IRestAlarmsRepository _repository = new RestAlarmsRepository();
        public static List<RestorationAlarmLists> CustRestAlarmListDump { get; private set; }

        //public static List<RestorationAlarmList> CustAlarmListDump { get; private set; }

        private int _custPageIndex;
        public int custPageIndex
        {
            get { return _custPageIndex; }
            set
            {
                _custPageIndex = value;
                OnPropertyChanged("custPageIndex");
            }
        }

        private int _custPageCount;
        public int custPageCount
        {
            get { return _custPageCount; }
            set
            {
                _custPageCount = value;
                OnPropertyChanged("custPageCount");
            }
        }

        private int _custAlarmCount;
        public int custAlarmCount
        {
            get { return _custAlarmCount; }
            set
            {
                _custAlarmCount = value;
                OnPropertyChanged("custAlarmCount");
            }

        }
        public int pageSize { get; private set; }

        private Timer _timer = new Timer(5000);

        public CustomAlarmListViewModel()
        {
            CustRestAlarmListDump = new List<RestorationAlarmLists>();
            RestorationDetailCommand = new RelayCommand(o=>OnRestorationDetailCommand(),o=>canShowDetail());

            FirstPageCommand = new RelayCommand(O => onFirstPageCommand(), O => canPrePageCommand());
            PrePageCommand = new RelayCommand(O => onPrePageCommand(), O => canPrePageCommand());
            NextPageCommand = new RelayCommand(O => onNextPageCommand(), O => canNextPageCommand());
            LastPageCommand = new RelayCommand(O => onLastPageCommand(), O => canNextPageCommand());
            CustomAlarms = new ObservableCollection<RestorationAlarmLists>();

            RestAlarmsRepo.RestAlarmChanged += OnCustAlarmChanged;
            custPageIndex = 1;
            pageSize = 35;
            custAlarmCount = 0;
            //RestAlarmsRepo.pageSize = pageSize;
            //RestAlarmsRepo.custPageIndex = custPageIndex;

            //_timer.Elapsed += (s, e) => NotificationMessage = "This is Alarm bar 2 : " + DateTime.Now.ToLocalTime();
            //_timer.Start();
        }

        RelayCommand _EnterPageCommand;
        public ICommand EnterPageCommand
        {
            get
            {
                if (_EnterPageCommand == null)
                {
                    _EnterPageCommand = new RelayCommand(p => EnterPage(p),
                        p => true);
                }
                return _EnterPageCommand;
            }
        }

        private async void EnterPage(object txtPage)
        {

            custPageIndex = Convert.ToInt32(txtPage);
            if (_custPageIndex <= 0) custPageIndex = 1;
            if (_custPageIndex > RestAlarmsRepo.custPageCount) custPageIndex = RestAlarmsRepo.custPageCount;

            Console.WriteLine(DateTime.Now.ToString() + " : goto page : " + _custPageIndex);
            RestAlarmsRepo.custPageIndex = custPageIndex;
            //RestAlarmsRepo.fDateTimeCondEnd = DateTime.Now;
            await RestAlarmsRepo.TGetCustAlarmAct();

            //To Do function Update RestAlarmsRepo.RestAlarmListDump 
            //CustRestAlarmListDump = await RestAlarmsRepo.GetCustomRestAlarmsAsync(RestAlarmsRepo.filterParseDeleg, custPageIndex, pageSize);
            //CustomAlarms = new ObservableCollection<RestorationAlarmList>(CustRestAlarmListDump);
        }

        public RelayCommand FirstPageCommand { get; private set; }
        private bool canPrePageCommand()
        {
            return (_custPageIndex > 1);
        }

        private async void onFirstPageCommand()
        {
            custPageIndex = 1;
            Console.WriteLine(DateTime.Now.ToString() + " : goto page : " + _custPageIndex);
            RestAlarmsRepo.custPageIndex = custPageIndex;
            //RestAlarmsRepo.fDateTimeCondEnd = DateTime.Now;
            await RestAlarmsRepo.TGetCustAlarmAct();

        }

        public RelayCommand PrePageCommand { get; private set; }

        private async void onPrePageCommand()
        {
            custPageIndex -= 1;
            Console.WriteLine(DateTime.Now.ToString() + " : goto page : " + _custPageIndex);
            RestAlarmsRepo.custPageIndex = custPageIndex;
            //RestAlarmsRepo.fDateTimeCondEnd = DateTime.Now;
            await RestAlarmsRepo.TGetCustAlarmAct();

        }
        public RelayCommand NextPageCommand { get; private set; }

        private async void onNextPageCommand()
        {
            custPageIndex += 1;
            Console.WriteLine(DateTime.Now.ToString() + " : goto page : " + _custPageIndex);
            RestAlarmsRepo.custPageIndex = custPageIndex;
            //RestAlarmsRepo.fDateTimeCondEnd = DateTime.Now;
            await RestAlarmsRepo.TGetCustAlarmAct();

        }

        public RelayCommand LastPageCommand { get; private set; }
        private bool canNextPageCommand()
        {
            return (_custPageIndex < RestAlarmsRepo.custPageCount);
        }

        private async void onLastPageCommand()
        {
            custPageIndex = RestAlarmsRepo.custPageCount;
            Console.WriteLine(DateTime.Now.ToString() + " : goto page : " + _custPageIndex);
            RestAlarmsRepo.custPageIndex = custPageIndex;

            //RestAlarmsRepo.fDateTimeCondEnd = DateTime.Now;
            await RestAlarmsRepo.TGetCustAlarmAct();

        }

        //New alarm Process
        private void OnCustAlarmChanged(object source, RestEventArgs arg)
        {

            switch (arg.message)
            {
                case "Start Success":

                    Console.WriteLine(DateTime.Now.ToString() + " : Custom Alarm List" + arg.message);
                    CustomAlarms = new ObservableCollection<RestorationAlarmLists>(RestAlarmsRepo.CustAlarmListDump);
                    custPageCount = RestAlarmsRepo.custPageCount;
                    custAlarmCount = RestAlarmsRepo.custAlarmCount;

                    NotificationMessage = "Database has been Loaded : " + DateTime.Now.ToLocalTime();

                    break;

                case "Start Fail":

                    Console.WriteLine(DateTime.Now.ToString() + " : Custom Alarm List" + arg.message);
                    NotificationMessage = "Can't Loaded Database : " + DateTime.Now.ToLocalTime();

                    break;
                case "hasNewAlarmCust":
                    Console.WriteLine(DateTime.Now.ToString() + " : Custom Alarm List Recieved New Alarm");
                    if (RestAlarmsRepo.PreviousAlarmRecIndex < 0) break;
                    for (int i = RestAlarmsRepo.startNewCustItemArray; i >= 0; i--)
                    {
                        CustomAlarms.Insert(0, RestAlarmsRepo.CustAlarmListDump[i]);
                        if (CustomAlarms.Count > pageSize) CustomAlarms.RemoveAt(pageSize);
                    }

                    custPageCount = RestAlarmsRepo.custPageCount;
                    custAlarmCount = RestAlarmsRepo.custAlarmCount;
                    NotificationMessage = "Has New Alarm : " + DateTime.Now.ToLocalTime();
                    break;

                case "hasAllNewAlarmCust":
                    Console.WriteLine(DateTime.Now.ToString() + " : Custom Alarm List Recieved All new or Reset");
                    CustomAlarms.Clear();
                    for (int i = RestAlarmsRepo.startNewCustItemArray; i >= 0; i--)
                        CustomAlarms.Insert(0,RestAlarmsRepo.CustAlarmListDump[i]);

                    custPageCount = RestAlarmsRepo.custPageCount;
                    custAlarmCount = RestAlarmsRepo.custAlarmCount;
                    NotificationMessage = "Database has been reset : " + DateTime.Now.ToLocalTime();
                    break;

                case "filterAlarmCust":
                    Console.WriteLine(DateTime.Now.ToString() + " : Custom Alarm List has been Filtered/Searched");
                    CustomAlarms.Clear();
                    for (int i = RestAlarmsRepo.startNewCustItemArray; i >= 0; i--)
                        CustomAlarms.Insert(0, RestAlarmsRepo.CustAlarmListDump[i]);

                    custPageCount = RestAlarmsRepo.custPageCount;
                    custAlarmCount = RestAlarmsRepo.custAlarmCount;
                    //NotificationMessage = "Filtering : " + DateTime.Now.ToLocalTime();
                    break;

                case "GetFilterAlarmCust":
                    Console.WriteLine(DateTime.Now.ToString() + " : Custom Alarm List Execute Navigation");
                    CustomAlarms.Clear();
                    for (int i = RestAlarmsRepo.startNewCustItemArray; i >= 0; i--)
                        CustomAlarms.Insert(0, RestAlarmsRepo.CustAlarmListDump[i]);

                    custPageCount = RestAlarmsRepo.custPageCount;
                    custAlarmCount = RestAlarmsRepo.custAlarmCount;
                    //NotificationMessage = "Filtering : " + DateTime.Now.ToLocalTime();
                    break;


                case "filterAlarmCustNoResult":
                    Console.WriteLine(DateTime.Now.ToString() + " : Custom Alarm List has been Filtered/Searched but no data");
                    CustomAlarms.Clear();

                    custPageCount = RestAlarmsRepo.custPageCount;
                    custAlarmCount = RestAlarmsRepo.custAlarmCount;
                    NotificationMessage = "No Filtering/Searching result  : " + DateTime.Now.ToLocalTime();
                    break;

                default:
                    Console.WriteLine(DateTime.Now.ToString() + " : Custom Alarm List no Msg. match");
                    break;
            }
        }
        //public bool canFilter()
        //{
        //    return RestAlarmsRepo.filter_Parse.Count != 0; ;
        //}
        //public async void onFilterAlarms()
        //{

        //    CustomAlarms = await RestAlarmsRepo.GetCustomRestAlarmsAsync(RestAlarmsRepo.filter_Parse, pageIndex, pageSize);
        //    RestAlarmsRepo.LastCustomAlarmRecIndex = CustomAlarms[0].PkAlarmListID; //Get Last PkAlarmList initializing

        //    //CustomAlarms = new ObservableCollection<RestorationAlarmList>(await RestAlarmsRepo.GetRestAlarmsAsync());
        //}

        private ObservableCollection<RestorationAlarmLists> _restorationAlarms;
        public ObservableCollection<RestorationAlarmLists> CustomAlarms
        {
            get { return _restorationAlarms; }
            set
            {
                _restorationAlarms = value;
                OnPropertyChanged("CustomAlarms");
            }
        }

        public RelayCommand RestorationDetailCommand { get; private set; }
        public ICommand RunFilterCmd { get; private set; }

        public event Action<RestorationAlarmLists> RestorationDetailRequested = delegate { };

        private bool canLoadData()
        {
            return (RestAlarmsRepo.RestAlarmListDump.Count != 0);
        }
        private bool canShowDetail()
        {
            return (_selectedEvent != null);
        }
        private void OnRestorationDetailCommand()
        {
            RestorationDetailRequested(_selectedEvent);
        }


        private RestorationAlarmLists _selectedEvent;

        public RestorationAlarmLists SelectedEvent
        {
            get { return _selectedEvent; }
            set
            {
                if (_selectedEvent != value)
                {
                    _selectedEvent = value;
                    OnPropertyChanged("SelectedEvent");

                }
            }
        }

        private string _NotificationMessage;
        public string NotificationMessage
        {
            get { return _NotificationMessage; }
            set
            {
                if (value != _NotificationMessage)
                {
                    _NotificationMessage = value;
                    OnPropertyChanged("NotificationMessage");
                }
            }
        }



    }
}
