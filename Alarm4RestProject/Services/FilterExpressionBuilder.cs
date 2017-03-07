using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Alarm4Rest_Viewer.Services
{
    public class FilterExpressionBuilder
    {
        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] {typeof(string) });
        private static MethodInfo toUpperMethod = typeof(string).GetMethod("ToUpper", new[] { typeof(string) });
        

        //Methode that solve only one List field
        public static Expression<Func<T, bool>> GetExpression<T>(List<Item> filterParses)
        {
            try
            {

                if (filterParses.Count == 0) return null;

                ParameterExpression pe = Expression.Parameter(typeof(T), "RestorationAlarm"); 

                Expression exp = null;

                if (filterParses.Count == 1)
                    exp = GetExpression<T>(pe, filterParses[0]); //Create expression from a single instance
                else if (filterParses.Count == 2)

                    exp = GetExpression<T>(pe, filterParses[0], filterParses[1]); //Create expression that utilizes OrElse mentality
                else
                {
                    //Loop through filters until we have create an expression for each 
                    while (filterParses.Count > 0)
                    {
                        var f1 = filterParses[0];
                        var f2 = filterParses[1];

                        if (exp == null)
                            exp = GetExpression<T>(pe, filterParses[0], filterParses[1]);
                        else
                            exp = Expression.OrElse(exp, GetExpression<T>(pe, filterParses[0], filterParses[1]));
                        filterParses.Remove(f1);
                        filterParses.Remove(f2);

                        if (filterParses.Count == 1)
                        {
                            exp = Expression.OrElse(exp, GetExpression<T>(pe, filterParses[0]));
                            filterParses.RemoveAt(0);
                        }
                    }
                }


                return Expression.Lambda<Func<T, bool>>(exp, pe);
            }
            catch
            {
                return null;
            }
        }


        /*Methode that solve Many List field 
         Please gouping likt this -->IEnumerable<IGrouping<string, Item>> gruopFields =
                                            from item in filters
                                            group item by item.FieldName;
         
         before call this methode:
         */
        public static Expression<Func<T, bool>> GetExpression<T>(IEnumerable<IGrouping<string, Item>> groupFilters)
        {
            Expression outerExp; //And between field

            outerExp = null;
            ParameterExpression pe = Expression.Parameter(typeof(T), "RestorationAlarm");

            try
            {

                foreach (IGrouping<string, Item> groupfield in groupFilters)
                {
                    List<Item> gruopfields = groupfield.ToList();

                    if (gruopfields.Count == 0) return null;

                    Expression innerExp = null; //Or in the same field

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

        //Helper

        //For 1 parameter
        private static Expression GetExpression<T>(ParameterExpression pe, Item filter)
        {
            MemberExpression me = Expression.Property(pe, filter.FieldName); //change to variable
            ConstantExpression constant = Expression.Constant(filter.Value.TrimEnd());

            //Expression member = Expression.Call(me, typeof(string).GetMethod("ToUpper", System.Type.EmptyTypes));
            //return Expression.Call(member, containsMethod, constant);

            return Expression.Call(me, containsMethod, constant);
        }

        //For 2 parameter
        private static Expression GetExpression<T>(ParameterExpression pe, Item filter1, Item filter2)
        {

            Expression result1 = GetExpression<T>(pe, filter1);
            Expression result2 = GetExpression<T>(pe, filter2);

            return Expression.OrElse(result1, result2);
        }
    }

    internal static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
        public static Expression AndT(this Expression expr1, Expression expr2)
        {
            //IEnumerable<Expression> exp = expr1.Parameters.Cast<Expression>();

            //ParameterExpression pe = Expression.Parameter(typeof(T), "RestorationAlarm");

            //var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            //return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
            return Expression.AndAlso(expr1, expr2);
        }


    }
}
