using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Alarm4Rest_Viewer.Services
{
    public static class SortExpression
    {
        public static IEnumerable<T> BuildOrderBy<T>(this IEnumerable<T> source, params SortDescription[] properties)
        {
            if (properties == null || properties.Length == 0) return source;

            var typeOfT = typeof(T);

            Type t = typeOfT;

            IOrderedEnumerable<T> result = null;
            var thenBy = false;

            foreach (var item in properties
                .Select(prop => new { PropertyInfo = t.GetProperty(prop.PropertyName), prop.Direction }))
            {
                var oExpr = Expression.Parameter(typeOfT, "o");
                var propertyInfo = item.PropertyInfo;
                var propertyType = propertyInfo.PropertyType;
                var isAscending = item.Direction == ListSortDirection.Ascending;

                if (thenBy)
                {
                    var prevExpr = Expression.Parameter(typeof(IOrderedEnumerable<T>), "prevExpr");
                    var expr1 = Expression.Lambda<Func<IOrderedEnumerable<T>, IOrderedEnumerable<T>>>(
                        Expression.Call(
                            (isAscending ? thenByMethod : thenByDescendingMethod).MakeGenericMethod(typeOfT, propertyType),
                            prevExpr,
                            Expression.Lambda(
                                typeof(Func<,>).MakeGenericType(typeOfT, propertyType),
                                Expression.MakeMemberAccess(oExpr, propertyInfo),
                                oExpr)
                            ),
                        prevExpr)
                        .Compile();

                    result = expr1(result);
                }
                else
                {
                    var prevExpr = Expression.Parameter(typeof(IEnumerable<T>), "prevExpr");
                    var expr1 = Expression.Lambda<Func<IEnumerable<T>, IOrderedEnumerable<T>>>(
                        Expression.Call(
                            (isAscending ? orderByMethod : orderByDescendingMethod).MakeGenericMethod(typeOfT, propertyType),
                            prevExpr,
                            Expression.Lambda(
                                typeof(Func<,>).MakeGenericType(typeOfT, propertyType),
                                Expression.MakeMemberAccess(oExpr, propertyInfo),
                                oExpr)
                            ),
                        prevExpr)
                        .Compile();

                    result = expr1(source);
                    thenBy = true;
                }
            }
            return result;
        }

        private static MethodInfo orderByMethod =
            MethodOf(() => Enumerable.OrderBy(default(IEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        private static MethodInfo orderByDescendingMethod =
            MethodOf(() => Enumerable.OrderByDescending(default(IEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        private static MethodInfo thenByMethod =
            MethodOf(() => Enumerable.ThenBy(default(IOrderedEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        private static MethodInfo thenByDescendingMethod =
            MethodOf(() => Enumerable.ThenByDescending(default(IOrderedEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        public static MethodInfo MethodOf<T>(Expression<Func<T>> method)
        {
            MethodCallExpression mce = (MethodCallExpression)method.Body;
            MethodInfo mi = mce.Method;
            return mi;
        }
    }

}
 