using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
//using Alarm4Rest.Data;
using SmartAlarmData;
using System.Windows.Threading;
using System.Linq.Expressions;
using System.Threading;
using System.Collections;
using System.ComponentModel;

namespace Alarm4Rest_Viewer.Services
{
     public static class RestAlarmsRepo //Ultilities Class
    {
        #region Properties
        //public static Alarm4RestorationContext DBContext;
        public static RestorationAlarmDbContext DBContext;
        public static List<RestorationAlarmLists> RestAlarmListDump { get; private set; }
        public static List<RestorationAlarmLists> CustAlarmListDump { get; private set; }
        public static List<RestorationAlarmLists> QueryAlarmListDump { get; private set; }
        public static List<string> StationsName { get; private set; }

        //private static DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        private static DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        //private static Thread thrChkNewEvents;
        public static int MainPage { get; private set; }
        public static int pageSize { get; set; }
        public static int LastAlarmRecIndex { get; set; }
        public static int LastMaxAlarmRecIndex { get; set; }

        public static int RestPageCount { get; set; }
        public static int RestPageIndex { get; set; }
        public static int PreviousAlarmRecIndex { get; private set; }
        public static int startNewRestItemArray { get; private set; }
        public static RestorationAlarmLists maxPkRecIndex { get; private set; }
        public static List<string> AlarmListFields { get; private  set; }
        public static int restAlarmCount { get; private set; }

        public static int startNewCustItemArray { get; private set; }
        public static int LastCustAlarmRecIndex { get; set; }
        public static int custPageIndex { get; set; }
        public static int custPageCount { get; set; }
        public static int custAlarmCount { get; private set; }

        public static int startNewQueryItemArray { get; private set; }
        public static int LastQueryAlarmRecIndex { get; set; }
        public static int queryPageIndex { get; set; }
        public static int queryPageCount { get; set; }
        public static int queryAlarmCount { get; private set; }

        public static TimeCondItem qDateTimeCondItem { get; set; }
        public static DateTime qDateTimeCondEnd { get; set; }
        public static DateTime qDateTimeCondStart { get; set; }

        public static TimeCondItem fDateTimeCondItem { get; set; }
        public static DateTime fDateTimeCondEnd { get; set; }

        public static List<string> Priority { get; private set; }
        public static List<string> DeviceType { get; private set; }
        public static List<string> Message { get; private set; }


        //Global Filter Keyword
        public static Expression<Func<RestorationAlarmLists, bool>> filterParseDeleg;
        public static List<OrderFieldModel> orderParseDeleg;

        //public static List<string> filter_Parse { get; set; }

        #endregion Properties

        public static void InitializeRepository()
        {
            InitializeComponent();
            DBContext = new RestorationAlarmDbContext();
            //DBContext = new Alarm4RestorationContext();
            RestAlarmListDump = new List<RestorationAlarmLists>();
            CustAlarmListDump = new List<RestorationAlarmLists>();
            QueryAlarmListDump = new List<RestorationAlarmLists>();

            // MainRibbonTapVM เป็นผู้ Initailize

            //qDateTimeCondItem = new TimeCondItem();
            //qDateTimeCondEnd = DateTime.Now;
            //qDateTimeCondStart = qDateTimeCondEnd.AddDays((-1) * 5);

            fDateTimeCondItem = new TimeCondItem("Week", 1);
            fDateTimeCondEnd = DateTime.Now;

            filterParseDeleg = null;
            orderParseDeleg = null;

            LastAlarmRecIndex = 0;
            LastMaxAlarmRecIndex = 0;
            LastQueryAlarmRecIndex = 0;

            //StationsName = new List<string>();

            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 20);
            dispatcherTimer.Start();
        }

        //Get All Data from SQL
        public static async Task GetInitDataRepositoryAsync()
        {
            RestEventArgs arg = new RestEventArgs();

           try
            {
                RestAlarmListDump = await GetRestAlarmsAsync();
                if (RestAlarmListDump.Count != 0)
                    LastAlarmRecIndex = RestAlarmListDump[0].PkAlarmListID; //Set Last PkAlarmList initializing

                LastMaxAlarmRecIndex = LastAlarmRecIndex;

                QueryAlarmListDump = await TGetQueryAlarmsAsync();
                if(QueryAlarmListDump.Count != 0)
                    LastQueryAlarmRecIndex = QueryAlarmListDump[0].PkAlarmListID;

                StationsName = await GetStationNameAsync();
                Priority = await GetPriorityAsync();

                DeviceType = await GetDeviceTypeAsync();

                Message = await GetMessageAsync();

                AlarmListFields = GetAlarmListFields();

                //Send Message to Subscriber
                arg.message = "Start Success";
                onRestAlarmChanged(arg);//Raise Event
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
            }
            catch
            {
                //Send Message to Subscriber
                arg.message = "Start Fail";
                onRestAlarmChanged(arg);//Raise Event
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
            }
        }

        private static void InitializeComponent()
        {
            MainPage = 1;
            LastAlarmRecIndex = -1;
            LastMaxAlarmRecIndex = -1;
            LastCustAlarmRecIndex = -1;
            LastQueryAlarmRecIndex = -1;
            PreviousAlarmRecIndex = 0;
            maxPkRecIndex = null;
            RestPageIndex = 1;
            custPageIndex = 1;
            queryPageIndex = 1;
            pageSize = 35;
        }

        public static Task<List<RestorationAlarmLists>> GetAllRestAlarmsAsync()
        {
            return DBContext.RestorationAlarmList.ToListAsync<RestorationAlarmLists>();
        }

        public static async Task<List<string>> GetStationNameAsync()
        {
            return await DBContext.Station
                .Select(x=>x.StationName)
                .ToListAsync<string>();
        }

        public static async Task<List<string>> GetPriorityAsync()
        {
            return await DBContext.RestorationAlarmList
                .Select(x => x.Priority)
                .Distinct()
                .ToListAsync<string>();
        }

        private static List<string> GetAlarmListFields()
        {
            return typeof(RestorationAlarmLists).GetProperties()
                                    .Select(property => property.Name)
                                    .ToList();
        }

        public static async Task<List<string>> GetDeviceTypeAsync()
        {
            return await DBContext.Device
                .Select(x => x.DeviceType)
                .Distinct()
                .ToListAsync<string>();
        }

        public static async Task<List<string>> GetMessageAsync()
        {
            return await DBContext.RestorationAlarmList
                .Select(x => x.Message)
                .Distinct()
                .ToListAsync<string>();
        }
        

        public static async Task<List<RestorationAlarmLists>> GetRestAlarmsAsync()
        {
            //To Do ปรับปรุงให้มีการ Query ครั้งเดียว
            //Get RestAlarm count
            restAlarmCount = (from alarm in DBContext.RestorationAlarmList
                              orderby alarm.PkAlarmListID descending
                              select alarm).Count();

            if (restAlarmCount % pageSize == 0)
            {
                RestPageCount = (restAlarmCount / pageSize);
            }else
            { 
                RestPageCount = (restAlarmCount / pageSize) + 1;
            }
            
            //Get one page
            return await DBContext.RestorationAlarmList
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Skip((RestPageIndex - 1) * pageSize)
                            .Take(pageSize)
                            //.Take(pageSize)
                            .ToListAsync<RestorationAlarmLists>();
        }

        public static async Task<List<RestorationAlarmLists>> GetRestAlarmsAsync(int pageIndex, int pageSize)
        {
            //To Do ปรับปรุงให้มีการ Query ครั้งเดียว
            //Get RestAlarm count
            restAlarmCount = (from alarm in DBContext.RestorationAlarmList
                              orderby alarm.PkAlarmListID descending
                              select alarm).Count();

            if (restAlarmCount % pageSize == 0)
            {
                RestPageCount = (restAlarmCount / pageSize);
            }
            else
            {
                RestPageCount = (restAlarmCount / pageSize) + 1;
            }

            //Get one page
            return await DBContext.RestorationAlarmList
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            //.Take(pageSize)
                            .ToListAsync<RestorationAlarmLists>();
        }

        public static async Task<List<RestorationAlarmLists>> GetCustomRestAlarmsAsync()
        {

            //To Do ปรับปรุงให้มีการ Query ครั้งเดียว
            //Get RestAlarm count
            custAlarmCount = DBContext.RestorationAlarmList
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Where(filterParseDeleg)
                            .Count();

            if (custAlarmCount % pageSize == 0)
            {
                custPageCount = (custAlarmCount / pageSize);
            }
            else
            {
                custPageCount = (custAlarmCount / pageSize) + 1;
            }

            //Get one page
            return await DBContext.RestorationAlarmList
                            //.OrderBy(c => c.Priority).ThenByDescending(c => c.PkAlarmListID)
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Where(filterParseDeleg)
                            .Skip((custPageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync<RestorationAlarmLists>();
        }

        public static async Task<List<RestorationAlarmLists>> GetQueryAlarmsAsync()
        {

            List<SortDescription> sortList = new List<SortDescription>();
            foreach (var Item in orderParseDeleg)
            {
                if (Item.IsChecked)
                    sortList.Add(new SortDescription(Item.FieldName, Item.IsOrderByAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));
            }

            IEnumerable<RestorationAlarmLists> Query;

            //Get one page
            if (filterParseDeleg == null)
            {
                Query = await DBContext.RestorationAlarmList
                            .OrderByDescending(c => c.PkAlarmListID)
                            .ToListAsync();
            }
            else
            {
                Query = await DBContext.RestorationAlarmList
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Where(filterParseDeleg)
                            .ToListAsync();
            }

            var resultList = Query.BuildOrderBy(
                                new SortDescription(sortList[0].PropertyName, sortList[0].Direction),
                                new SortDescription(sortList[1].PropertyName, sortList[1].Direction));

            //var resultList = Query.BuildOrderBy(
            //                    new SortDescription(sortOreder[0], ListSortDirection.Ascending),
            //                    new SortDescription(sortOreder[1], ListSortDirection.Ascending));

            queryAlarmCount = resultList.Count();

            if (queryAlarmCount % pageSize == 0)
            {
                queryPageCount = (queryAlarmCount / pageSize);
            }
            else
            {
                queryPageCount = (queryAlarmCount / pageSize) + 1;
            }
            return resultList
                            .Skip((queryPageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
        }

        public static async Task<List<RestorationAlarmLists>> TGetQueryAlarmsAsync()
        {

            DateTime inclusiveStart = GetQueryTimeCond();

            List<SortDescription> sortList = new List<SortDescription>();
            foreach (var Item in orderParseDeleg)
            {
                if (Item.IsChecked)
                    sortList.Add(new SortDescription(Item.FieldName, Item.IsOrderByAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));
            }

            IEnumerable<RestorationAlarmLists> Query;

            //Get one page
            if (filterParseDeleg == null)
            {
                Query = await DBContext.RestorationAlarmList
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Where(c => c.DateTime >= inclusiveStart && c.DateTime < qDateTimeCondEnd)
                            .ToListAsync();
            }
            else
            {
                Query = await DBContext.RestorationAlarmList
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Where(filterParseDeleg)
                            .Where(c => c.DateTime >= inclusiveStart && c.DateTime < qDateTimeCondEnd)
                            .ToListAsync();
            }

              Query = Query.BuildOrderBy(
                                new SortDescription(sortList[0].PropertyName, sortList[0].Direction),
                                new SortDescription(sortList[1].PropertyName, sortList[1].Direction));

            // var resultList = Query; 

            queryAlarmCount = Query.Count();

            if (queryAlarmCount % pageSize == 0)
            {
                queryPageCount = (queryAlarmCount / pageSize);
            }
            else
            {
                queryPageCount = (queryAlarmCount / pageSize) + 1;
            }
            return Query
                            .Skip((queryPageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
        }

        public static async Task<List<RestorationAlarmLists>> TGetCustomRestAlarmsAsync()
        {

            DateTime inclusiveStart = GetFilterTimeCond();

            //To Do ปรับปรุงให้มีการ Query ครั้งเดียว
            //Get CustRestAlarm count
            custAlarmCount = DBContext.RestorationAlarmList
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Where(filterParseDeleg)
                            .Where(c => c.DateTime >= inclusiveStart
                                        && c.DateTime < fDateTimeCondEnd)
                            .Count();

            if (custAlarmCount % pageSize == 0)
            {
                custPageCount = (custAlarmCount / pageSize);
            }
            else
            {
                custPageCount = (custAlarmCount / pageSize) + 1;
            }

            //Get one page
            return await DBContext.RestorationAlarmList
                            //.OrderBy(c => c.Priority).ThenByDescending(c => c.PkAlarmListID)
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Where(filterParseDeleg)
                            .Where(c => c.DateTime >= inclusiveStart
                                        && c.DateTime < fDateTimeCondEnd)
                            .Skip((custPageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync<RestorationAlarmLists>();
        }


        public static async Task<List<RestorationAlarmLists>> GetCustomRestAlarmsAsync(Expression<Func<RestorationAlarmLists, bool>> filter_Parse, int custPageIndex = 1, int pageSize = 30)
        {

            //To Do ปรับปรุงให้มีการ Query ครั้งเดียว
            //Get RestAlarm count
            custAlarmCount = DBContext.RestorationAlarmList
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Where(filter_Parse)
                            .Count();

            if (custAlarmCount % pageSize == 0)
            {
                custPageCount = (custAlarmCount / pageSize);
            }
            else
            {
                custPageCount = (custAlarmCount / pageSize) + 1;
            }

            //Get one page
            return await DBContext.RestorationAlarmList
                            .OrderByDescending(c => c.PkAlarmListID)
                            .Where(filter_Parse)
                            .Skip((custPageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync<RestorationAlarmLists>();
        }

        public static async Task GetCustAlarmAct()
        {
            RestEventArgs arg = new RestEventArgs();
            //Test Raise read "LoadStationName"
            if (filterParseDeleg == null) return;
            CustAlarmListDump = await GetCustomRestAlarmsAsync();
            if (CustAlarmListDump.Count != 0)
            {
                LastCustAlarmRecIndex = CustAlarmListDump[0].PkAlarmListID;
                startNewCustItemArray = CustAlarmListDump.Count - 1;
                arg.message = "GetFilterAlarmCust";
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
            else //filter result no item
            {

                arg.message = "filterAlarmCustNoResult";
                LastCustAlarmRecIndex = -1;
                startNewCustItemArray = -1;
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
        }

        public static async Task TGetCustAlarmAct()
        {
            RestEventArgs arg = new RestEventArgs();

            //Test Raise read "LoadStationName"
            if (filterParseDeleg == null) return;
            CustAlarmListDump = await TGetCustomRestAlarmsAsync();
            if (CustAlarmListDump.Count != 0)
            {
                LastCustAlarmRecIndex = CustAlarmListDump[0].PkAlarmListID;
                startNewCustItemArray = CustAlarmListDump.Count - 1;
                arg.message = "filterAlarmCust";
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
            else //filter result no item
            {

                arg.message = "filterAlarmCustNoResult";
                LastCustAlarmRecIndex = -1;
                startNewCustItemArray = -1;
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
        }

        public static async Task GetQueryAlarmAct(SortItem orderParseDeleg)
        {
            RestEventArgs arg = new RestEventArgs();

            //Test Raise read "LoadStationName"
            if (orderParseDeleg == null) return;
            QueryAlarmListDump = await GetQueryAlarmsAsync();
            if (QueryAlarmListDump.Count != 0)
            {
                LastQueryAlarmRecIndex = QueryAlarmListDump[0].PkAlarmListID;
                startNewQueryItemArray = QueryAlarmListDump.Count - 1;
                arg.message = "GetQueryAlarm";
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
            else //filter result no item
            {

                arg.message = "GetQueryAlarmNoResult";
                LastQueryAlarmRecIndex = -1;
                startNewQueryItemArray = -1;
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
        }
        public static async Task TGetQueryAlarmAct()
        {
            RestEventArgs arg = new RestEventArgs();

            //Test Raise read "LoadStationName"
            if (orderParseDeleg == null) return;
            QueryAlarmListDump = await TGetQueryAlarmsAsync();
            if (QueryAlarmListDump.Count != 0)
            {
                LastQueryAlarmRecIndex = QueryAlarmListDump[0].PkAlarmListID;
                startNewQueryItemArray = QueryAlarmListDump.Count - 1;
                arg.message = "GetQueryAlarm";
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
            else //filter result no item
            {

                arg.message = "GetQueryAlarmNoResult";
                LastQueryAlarmRecIndex = -1;
                startNewQueryItemArray = -1;
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
        }

        public static async Task GetQueryAlarmAct()
        {
            RestEventArgs arg = new RestEventArgs();

            //Test Raise read "LoadStationName"
            if (orderParseDeleg == null) return;
            QueryAlarmListDump = await GetQueryAlarmsAsync();
            if (QueryAlarmListDump.Count != 0)
            {
                LastQueryAlarmRecIndex = QueryAlarmListDump[0].PkAlarmListID;
                startNewQueryItemArray = QueryAlarmListDump.Count - 1;
                arg.message = "GetQueryAlarm";
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
            else //filter result no item
            {

                arg.message = "GetQueryAlarmNoResult";
                LastQueryAlarmRecIndex = -1;
                startNewQueryItemArray = -1;
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
        }

        public static async Task GetRestAlarmAct()
        {
            RestEventArgs arg = new RestEventArgs();
            //Test Raise read "LoadStationName"
            RestAlarmListDump = await GetRestAlarmsAsync();
            if (RestAlarmListDump.Count != 0)
            {
                LastAlarmRecIndex = RestAlarmListDump[0].PkAlarmListID;
                startNewRestItemArray = RestAlarmListDump.Count - 1;
                arg.message = "GetRestAlarm";
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event

            }
            else //filter result no item
            {
                arg.message = "GetRestAlarmNoResult";
                LastAlarmRecIndex = -1;
                LastMaxAlarmRecIndex = -1;
                startNewRestItemArray = -1;
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
        }

        private static DateTime GetQueryTimeCond()
                {
            //exclusiveEnd = DateTime.ParseExact(exclusiveEnd.ToString(), "dd/MM/yyyy HH:mm:ss.000", System.Globalization.CultureInfo.InvariantCulture);
            
            DateTime inclusiveStart = new DateTime();
                TimeSpan ts = new TimeSpan(0, 0, 0);
                    switch (qDateTimeCondItem.TimeType)
                    {
                        case "Day":
                            qDateTimeCondEnd = DateTime.Now;
                            inclusiveStart = qDateTimeCondEnd.AddDays((-1) * (qDateTimeCondItem.Value-1));
                            inclusiveStart = inclusiveStart.Date + ts; // Reset time to HH: mm: ss.000" -> 0000:00.000
                            break;

                        case "Week":
                            qDateTimeCondEnd = DateTime.Now;
                            int diff = qDateTimeCondEnd.DayOfWeek - DayOfWeek.Monday;
                            if (diff< 0)
                            {
                                diff += 7;
                            }
                            inclusiveStart= qDateTimeCondEnd.AddDays((-1 * diff)+((-7) * (qDateTimeCondItem.Value - 1)));
                            inclusiveStart = inclusiveStart.Date + ts; // Reset time to HH: mm: ss.000" -> 0000:00.000
                            break;

                        case "Month":
                            qDateTimeCondEnd = DateTime.Now;
                            inclusiveStart = qDateTimeCondEnd.AddMonths((-1) * (qDateTimeCondItem.Value - 1));
                            inclusiveStart = new DateTime(inclusiveStart.Year, inclusiveStart.Month, 1);
                            break;

                        case "All":
                            qDateTimeCondEnd = DateTime.Now;
                            inclusiveStart = DateTime.Now.AddYears((-1)*2);
                            inclusiveStart = new DateTime(inclusiveStart.Year, 1, 1);
                            break;

                        case "User":
                            inclusiveStart = qDateTimeCondStart;
                            //qDateTimeCondEnd = qDateTimeCondEnd.AddDays(1);
                            break;
                default: //All two year
                            inclusiveStart = qDateTimeCondEnd.AddYears(-2);
                            inclusiveStart = new DateTime(inclusiveStart.Year, 1, 1);
                            break;
        
                    }

            return inclusiveStart;
        }

        private static DateTime GetFilterTimeCond()
        {
            //exclusiveEnd = DateTime.ParseExact(exclusiveEnd.ToString(), "dd/MM/yyyy HH:mm:ss.000", System.Globalization.CultureInfo.InvariantCulture);
            DateTime inclusiveStart = new DateTime();
            TimeSpan ts = new TimeSpan(0, 0, 0);
            switch (fDateTimeCondItem.TimeType)
            {
                case "Day":
                    inclusiveStart = fDateTimeCondEnd.AddDays((-1) * (fDateTimeCondItem.Value - 1));
                    inclusiveStart = inclusiveStart.Date + ts; // Reset time to HH: mm: ss.000" -> 0000:00.000
                    break;

                case "Week":
                    int diff = fDateTimeCondEnd.DayOfWeek - DayOfWeek.Monday;
                    if (diff < 0)
                    {
                        diff += 7;
                    }
                    inclusiveStart = fDateTimeCondEnd.AddDays(-1 * diff).Date;
                    inclusiveStart = inclusiveStart.Date + ts; // Reset time to HH: mm: ss.000" -> 0000:00.000
                    break;

                case "Month":
                    inclusiveStart = fDateTimeCondEnd.AddMonths((-1) * (fDateTimeCondItem.Value - 1));
                    inclusiveStart = new DateTime(inclusiveStart.Year, inclusiveStart.Month, 1);
                    break;

                default: //All one year
                    inclusiveStart = fDateTimeCondEnd.AddYears(-1);
                    inclusiveStart = new DateTime(inclusiveStart.Year, 1, 1);
                    break;

            }

            return inclusiveStart;
        }
        private static async void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Predicate<int> isEqualLast = (newIndex) => LastMaxAlarmRecIndex == newIndex;
            try
            {

                maxPkRecIndex = (from alarm in DBContext.RestorationAlarmList
                                 orderby alarm.PkAlarmListID descending
                                 select alarm).FirstOrDefault();

                if (maxPkRecIndex == null || isEqualLast(maxPkRecIndex.PkAlarmListID)) return; //Exit if Has no data or No new Alarm

                //Get Update Data
                RestAlarmListDump = await GetRestAlarmsAsync();
                CheckNewRestAlarm();

                if (filterParseDeleg != null)
                {
                    CustAlarmListDump = await GetCustomRestAlarmsAsync();
                    if (CustAlarmListDump.Count != 0)
                        CheckNewCustomRestAlarm();
                }

                Console.WriteLine("Timer Tick Success");
            }
            catch
            {
                Console.WriteLine("Timer Tick Load Fail");
            }
           
        }

        private static void CheckNewRestAlarm()
        {
            RestEventArgs arg = new RestEventArgs();

            //Func<int, int, bool> hasNewAlarm = hasNewAalrmChk;

            //Test using Predicate
            Predicate<int> hasNewAlarmChk = (newLastIndex) => newLastIndex > LastMaxAlarmRecIndex;

            if (hasNewAlarmChk(maxPkRecIndex.PkAlarmListID))
            {
                //To Do LastAlarmRecIndex_of_Page

                LastMaxAlarmRecIndex = maxPkRecIndex.PkAlarmListID;
                PreviousAlarmRecIndex = LastAlarmRecIndex;
                //LastAlarmRecIndex = maxPkRecIndex.PkAlarmListID;
                LastAlarmRecIndex = RestAlarmListDump[0].PkAlarmListID;
                startNewRestItemArray = LastAlarmRecIndex-PreviousAlarmRecIndex-1;
                arg.message = "hasNewAlarm";
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
            else //Database has been reset
            {
                //Restart Process
                PreviousAlarmRecIndex = 0;
                LastMaxAlarmRecIndex = 0;
                LastAlarmRecIndex = maxPkRecIndex.PkAlarmListID;
                startNewRestItemArray = RestAlarmListDump.Count-1;
                
                arg.message = "Reset";
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
        }

        private static void CheckNewCustomRestAlarm()
        {
            RestEventArgs arg = new RestEventArgs();

            //Func<int, int, bool> hasNewAlarm = hasNewAalrmChk;

            //Test using Predicate
            Predicate<List<RestorationAlarmLists>> hasAllNew = (alarm) => LastAlarmRecIndex < alarm[alarm.Count-1].PkAlarmListID || LastAlarmRecIndex > alarm[0].PkAlarmListID;
            Predicate<int> isEqualLastCust = (Index) => Index == LastCustAlarmRecIndex;

            if (isEqualLastCust(CustAlarmListDump[0].PkAlarmListID)) return; //No new custom filter alarm

            if (hasAllNew(CustAlarmListDump)) //All New alarm or DB has been reset
            {
                LastCustAlarmRecIndex = CustAlarmListDump[0].PkAlarmListID;
                startNewCustItemArray = CustAlarmListDump.Count - 1;
                arg.message = "hasAllNewAlarmCust";
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
            else
            {
                //Finding Start last RecIndex Position
                int i = 0;
                foreach (RestorationAlarmLists alarm in CustAlarmListDump)
                {
                    //Get Starting Position
                    if (isEqualLastCust(alarm.PkAlarmListID))
                    {
                        startNewCustItemArray = i-1;
                        break;
                    }
                    i++;
                }

                LastCustAlarmRecIndex = CustAlarmListDump[0].PkAlarmListID;
                arg.message = "hasNewAlarmCust";
                Console.WriteLine(DateTime.Now.ToString() + " : Raise Event " + arg.message);
                onRestAlarmChanged(arg);//Raise Event
            }
        }

        //private static Expression<Func<RestorationAlarmList, bool>> FilterAppeding(string[] filterStr, DateTime startDate)
        //{
        //    var rule = PredicateExtensions.PredicateExtensions.Begin<RestorationAlarmList>();
        //    foreach (var filterWord in filterStr)
        //    {
        //        rule = rule.Or(FilterAppeding(filterWord, startDate));
        //    }
        //    return rule;
        //}


        public static event EventHandler<RestEventArgs> RestAlarmChanged;
        private static void onRestAlarmChanged(RestEventArgs arg)
        {
            if (RestAlarmChanged != null)
                RestAlarmChanged(null, arg);
        }
    }


}
