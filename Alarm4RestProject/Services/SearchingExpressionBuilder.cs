using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Alarm4Rest_Viewer.Services
{
    public class SearchingExpressionBuilder
    {
        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] {typeof(string) });
        private static MethodInfo toUpperMethod = typeof(string).GetMethod("ToUpper", new[] { typeof(string) });
        

        /*Methode that solve Many List field 
         Please gouping like this -->IEnumerable<IGrouping<string, Item>> gruopFields =
                                            from item in filters
                                            group item by item.FieldName;
         
         before call this methode:
         */
        public static Expression<Func<T, bool>> GetExpression<T>(IEnumerable<IGrouping<string, Item>> groupFilters, string search_Parse_Pri)
        {
            Expression outerExp; //And between field

            outerExp = null;
            ParameterExpression pe = Expression.Parameter(typeof(T), "RestorationAlarmList");

            try
            {

                foreach (IGrouping<string, Item> groupfield in groupFilters)
                {
                    
                    List<Item> gruopfields = groupfield.ToList();   
                    Expression innerExp = null; //Or in the same field

                    if (gruopfields.Count == 0) return null;
                    //If it is SearchingField Group
                    if (groupfield.Key == "FieldName")
                        
                        // has 1 item parameter
                        if (gruopfields.Count == 1)
                            innerExp = GetExpression<T>(pe, gruopfields[0], search_Parse_Pri, groupfield.Key); //Create expression from a single instance
                    
                        // has 2 item parameter
                        else if (gruopfields.Count == 2)

                            innerExp = GetExpression<T>(pe, gruopfields[0], gruopfields[1], search_Parse_Pri, groupfield.Key); //Create expression that utilizes OrElse mentality
                    
                        // More than 2 items parameter
                        else
                        {  
                            //Loop through filters until we have create an expression for each 
                            while (gruopfields.Count > 0)
                            {
                                {
                                    var f1 = gruopfields[0];
                                    var f2 = gruopfields[1];

                                    if (innerExp == null) //First time
                                        innerExp = GetExpression<T>(pe, gruopfields[0], gruopfields[1], search_Parse_Pri, groupfield.Key);
                                    else //Not First time
                                        innerExp = Expression.OrElse(innerExp, GetExpression<T>(pe, gruopfields[0], gruopfields[1], search_Parse_Pri, groupfield.Key));

                                    gruopfields.Remove(f1);
                                    gruopfields.Remove(f2);

                                    if (gruopfields.Count == 1)
                                    {
                                        innerExp = Expression.OrElse(innerExp, GetExpression<T>(pe, gruopfields[0], search_Parse_Pri, groupfield.Key));
                                        gruopfields.RemoveAt(0);
                                    }
                                }
                            }
                    }else if (groupfield.Key == "StationName")
                    {
                        // has 1 item parameter
                        if (gruopfields.Count == 1)
                            innerExp = GetExpression<T>(pe, gruopfields[0]); //Create expression from a single instance

                        // has 2 item parameter
                        else if (gruopfields.Count == 2)

                            innerExp = GetExpression<T>(pe, gruopfields[0], gruopfields[1]); //Create expression that utilizes OrElse mentality

                        // More than 2 items parameter
                        else
                        {
                            //Loop through filters until we have create an expression for each 
                            while (gruopfields.Count > 0)
                            {
                                var f1 = gruopfields[0];
                                var f2 = gruopfields[1];

                                if (innerExp == null) //First time
                                    innerExp = GetExpression<T>(pe, gruopfields[0], gruopfields[1]);
                                else //Not First time
                                    innerExp = Expression.OrElse(innerExp, GetExpression<T>(pe, gruopfields[0], gruopfields[1]));

                                gruopfields.Remove(f1);
                                gruopfields.Remove(f2);

                                if (gruopfields.Count == 1)
                                {
                                    innerExp = Expression.OrElse(innerExp, GetExpression<T>(pe, gruopfields[0]));
                                    gruopfields.RemoveAt(0);
                                }
                            }
                        }

                    }    

                    if (outerExp == null)
                    {
                        outerExp = innerExp;
                    }
                    else
                    {
                        outerExp = PredicateBuilder.AndT(innerExp, outerExp);

                    }

                    Console.WriteLine("{0}", groupfield.Key);

                } // Next field Group

                //Finishing
                return Expression.Lambda<Func<T, bool>>(outerExp, pe);
            }
            catch{
                return null;
            }
        }
        public static Expression<Func<T, bool>> GetExpression<T>(IEnumerable<IGrouping<string, Item>> groupFilters, string[] search_Parse_Pri_List, string search_Parse_Sec)
        {
            Expression outerExp; //And between field

            outerExp = null;
            ParameterExpression pe = Expression.Parameter(typeof(T), "RestorationAlarmList");

            //Primary keyword Search Exp Building

            if (search_Parse_Pri_List.Length > 0)
            {
                outerExp = GetPriSearchExpression<T>(pe, search_Parse_Pri_List);
            }
            
            try
            {
                
                foreach (IGrouping<string, Item> groupfield in groupFilters)
                {

                    List<Item> gruopfields = groupfield.ToList();
                    Expression innerExp = null; //Or in the same field

                    if (gruopfields.Count == 0) return null;
                    //If it is SearchingField Group
                    if (groupfield.Key == "FieldName" && search_Parse_Sec != "")
                    { 

                        // has 1 item parameter
                        if (gruopfields.Count == 1)
                            innerExp = GetExpression<T>(pe, gruopfields[0], search_Parse_Sec, groupfield.Key); //Create expression from a single instance

                        // has 2 item parameter
                        else if (gruopfields.Count == 2)

                            innerExp = GetExpression<T>(pe, gruopfields[0], gruopfields[1], search_Parse_Sec, groupfield.Key); //Create expression that utilizes OrElse mentality

                        // More than 2 items parameter
                        else
                        {
                            //Loop through filters until we have create an expression for each 
                            while (gruopfields.Count > 0)
                            {
                                {
                                    var f1 = gruopfields[0];
                                    var f2 = gruopfields[1];

                                    if (innerExp == null) //First time
                                        innerExp = GetExpression<T>(pe, gruopfields[0], gruopfields[1], search_Parse_Sec, groupfield.Key);
                                    else //Not First time
                                        innerExp = Expression.OrElse(innerExp, GetExpression<T>(pe, gruopfields[0], gruopfields[1], search_Parse_Sec, groupfield.Key));

                                    gruopfields.Remove(f1);
                                    gruopfields.Remove(f2);

                                    if (gruopfields.Count == 1)
                                    {
                                        innerExp = Expression.OrElse(innerExp, GetExpression<T>(pe, gruopfields[0], search_Parse_Sec, groupfield.Key));
                                        gruopfields.RemoveAt(0);
                                    }
                                }
                            }
                        }
                        }
                    else if(groupfield.Key == "StationName" || groupfield.Key == "Priority")
                    {
                        // has 1 item parameter
                        if (gruopfields.Count == 1)
                            innerExp = GetExpression<T>(pe, gruopfields[0]); //Create expression from a single instance

                        // has 2 item parameter
                        else if (gruopfields.Count == 2)

                            innerExp = GetExpression<T>(pe, gruopfields[0], gruopfields[1]); //Create expression that utilizes OrElse mentality

                        // More than 2 items parameter
                        else
                        {
                            //Loop through filters until we have create an expression for each 
                            while (gruopfields.Count > 0)
                            {
                                var f1 = gruopfields[0];
                                var f2 = gruopfields[1];

                                if (innerExp == null) //First time
                                    innerExp = GetExpression<T>(pe, gruopfields[0], gruopfields[1]);
                                else //Not First time
                                    innerExp = Expression.OrElse(innerExp, GetExpression<T>(pe, gruopfields[0], gruopfields[1]));

                                gruopfields.Remove(f1);
                                gruopfields.Remove(f2);

                                if (gruopfields.Count == 1)
                                {
                                    innerExp = Expression.OrElse(innerExp, GetExpression<T>(pe, gruopfields[0]));
                                    gruopfields.RemoveAt(0);
                                }
                            }
                        }

                    }

                    if (innerExp != null && outerExp != null)
                    {
                        outerExp = PredicateBuilder.AndT(innerExp, outerExp);
                    }else if(innerExp != null && outerExp == null)
                    {
                        outerExp = innerExp;
                    }
                    else if(innerExp == null && outerExp != null)
                    {
                        // Nothing to do
                    }else
                    {
                        return null;
                    }
                    

                    Console.WriteLine("{0}", groupfield.Key);

                } // Next field Group

                //Finishing
                return Expression.Lambda<Func<T, bool>>(outerExp, pe);
            }
            catch
            {
                return null;
            }
        }


        //Helper


        //For 1 parameter


        private static Expression GetPriSearchExpression<T>(ParameterExpression pe, string[] search_Parse_Pri_List)
        {
            //List<Item> gruopfields = search_Parse_Pri_List.ToList();
            Expression innerExp = null; //Or in the same field
            //Expression outerExp = null; //Or in the same field

            if (search_Parse_Pri_List.Length == 0) return null;
            //If it is SearchingField Group
            foreach (string st in search_Parse_Pri_List)
            {
                if (innerExp == null) //Start Building Expression
                {
                    innerExp = GetPriSearchExpression<T>(pe,st);
                }
                else
                {
                    innerExp = Expression.OrElse(innerExp, GetPriSearchExpression<T>(pe, st));
                }
            }
            return innerExp;
        }
        private static Expression GetPriSearchExpression<T>(ParameterExpression pe, string filter)
        {
            MemberExpression me = Expression.Property(pe, "PointName"); //search in PointName
            ConstantExpression constant = Expression.Constant(filter);
            return Expression.Call(me, containsMethod, constant);
        }

        private static Expression GetExpression<T>(ParameterExpression pe, Item filter, string keyWord, string memberExp)
        {
            MemberExpression me1 = null;
            ConstantExpression constant1 = null;

            switch (memberExp)
            {
                case "FieldName":
                     me1 = Expression.Property(pe, filter.Value.TrimEnd()); //change to variable
                     constant1 = Expression.Constant(keyWord);
                    break;
                default:
                    me1 = Expression.Property(pe, filter.FieldName); //change to variable
                    constant1 = Expression.Constant(filter.Value.TrimEnd());
                    break;
            }

            //Expression member = Expression.Call(me, typeof(string).GetMethod("ToUpper", System.Type.EmptyTypes));
            //return Expression.Call(member, containsMethod, constant);

            return Expression.Call(me1, containsMethod, constant1);
        }

        private static Expression GetExpression<T>(ParameterExpression pe, Item filter)
        {
            MemberExpression me = Expression.Property(pe, filter.FieldName); //change to variable
            ConstantExpression constant = Expression.Constant(filter.Value.TrimEnd());
            return Expression.Call(me, containsMethod, constant);
        }

        //For 2 parameter
        private static Expression GetExpression<T>(ParameterExpression pe, Item filter1, Item filter2)
        {

            Expression result1 = GetExpression<T>(pe, filter1);
            Expression result2 = GetExpression<T>(pe, filter2);

            return Expression.OrElse(result1, result2);
        }

        private static Expression GetExpression<T>(ParameterExpression pe, Item filter1, Item filter2, string keyWord, string memberExp)
        {

            Expression result1 = GetExpression<T>(pe, filter1, keyWord, memberExp);
            Expression result2 = GetExpression<T>(pe, filter2, keyWord, memberExp);

            return Expression.OrElse(result1, result2);
        }
    }

}