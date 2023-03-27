using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Extensions;

public static class QueryableExtensions
{
    public static ProjectionExpression<TSource> Project<TSource>(this IQueryable<TSource> source)
    {
        return new(source);
    }

    public static async Task<HashSet<TEntity>> ToHashSetAsync<TEntity>(this IQueryable<TEntity> source,
        CancellationToken cancellationToken = default)
    {
        var asyncEnumerator = source.AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);
        var result = new HashSet<TEntity>();

        try
        {
            while (true)
            {
                var hasNext = await asyncEnumerator.MoveNextAsync();

                if (!hasNext)
                {
                    break;
                }

                result.Add(asyncEnumerator.Current);
            }
        }
        finally
        {
            await asyncEnumerator.DisposeAsync();
        }

        asyncEnumerator = null;
        
        return result;
    }


    public static IQueryable<TEntity> Pagination<TEntity>(this IQueryable<TEntity> source,
        int currentPage,
        int limit) where TEntity : class
    {
        return source
            .Skip((currentPage - 1) * limit)
            .Take(limit)
            .AsQueryable();
    }


    public static async Task<HashSet<TEntity>> ToHashSetPaginationAsync<TEntity>(this IQueryable<TEntity> source,
        int currentPage,
        int limit) where TEntity : class
    {
        return await source
            .Skip((currentPage - 1) * limit)
            .Take(limit)
            .ToHashSetAsync();
    }
    public static async Task<IList<TEntity>> ToListPaginationAsync<TEntity>(this IQueryable<TEntity> source,
        int currentPage,
        int limit) where TEntity : class
    {
        return await source
            .Skip((currentPage - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }


    public static Task<IQueryable<TSource>> WhereAsync<TSource>(this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate) where TSource : class
    {
        return Task.Run(() => source.Where(predicate));
    }

    public static IQueryable<T> Select<T>(this IQueryable<T> source, string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return source;

        var parameter = Expression.Parameter(source.ElementType, "p");
        var property = Expression.Property(parameter, columnName);
        var lambda = Expression.Lambda(property, parameter);

        //string methodName = isAscending ? "OrderBy" : "OrderByDescending";  
        const string METHOD_NAME = "Select"; // : "OrderByDescending";  

        Expression methodCallExpression = Expression.Call(typeof(Queryable),
            METHOD_NAME,
            new[] {source.ElementType, property.Type},
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(methodCallExpression)
            .AsQueryable();
    }

    private static class PropertyAccessorCache<T> where T : class
    {
        private static IDictionary<string, LambdaExpression> Cache { get; }

        static PropertyAccessorCache()
        {
            var storage = new Dictionary<string, LambdaExpression>();

            var T = typeof(T);
            var parameter = Expression.Parameter(T, "p");

            foreach (var property in T.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var lambdaExpression = Expression.Lambda(propertyAccess, parameter);
                storage[property.Name] = lambdaExpression;
            }

            Cache = storage;
        }

        public static LambdaExpression Get(string propertyName)
        {
            return Cache.TryGetValue(propertyName, out var result)
                ? result
                : null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="columnNames"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IQueryable<T> SortBy<T>(this IQueryable<T> source, IEnumerable<string> columnNames)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var queryExpr = source.Expression;
        var parameter = Expression.Parameter(source.ElementType, "");
        var methodName = "OrderBy";

        foreach (var columnName in columnNames)
        {
            var property = Expression.Property(parameter, columnName);
            var lambda = Expression.Lambda(property, parameter);

            queryExpr = Expression.Call(typeof(Queryable),
                methodName,
                new[] {source.ElementType, property.Type},
                queryExpr,
                Expression.Quote(lambda));

            methodName = "ThenBy";
        }

        return source.Provider.CreateQuery<T>(queryExpr)
            .AsQueryable();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="orders"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<T> SortBy<T>(this IEnumerable<T> source, IList<IDictionary<string, string>> orders)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var entityType = typeof(T);

        var entityParameter = Expression.Parameter(entityType, "p");

        var orderType = orders[0]["orderType"];

        var methodName = orderType == "asc"
            ? "OrderBy"
            : "OrderByDescending";

        var orderBy = orders[0]["orderBy"];
        var orderProperty = entityType.GetProperty(orderBy,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        var propertyAccess = Expression.MakeMemberAccess(entityParameter, orderProperty!);
        var orderByExpression = Expression.Lambda(propertyAccess, entityParameter);
        var resultExpression = Expression.Call(typeof(Enumerable),
            methodName,
            new[] {entityType, orderProperty.PropertyType},
            Expression.Quote(orderByExpression));

        var items = orders.TakeLast(orders.Count - 1);

        foreach (var order in items)
        {
            orderBy = order["orderBy"];
            orderType = order["orderType"];

            methodName = orderType == "asc"
                ? "ThenBy"
                : "ThenByDescending";

            orderProperty = entityType.GetProperty(orderBy,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            propertyAccess = Expression.MakeMemberAccess(entityParameter, orderProperty!);

            orderByExpression = Expression.Lambda(propertyAccess, entityParameter);

            resultExpression = Expression.Call(typeof(Queryable),
                methodName,
                new[] {entityType, orderProperty.PropertyType},
                resultExpression,
                Expression.Quote(orderByExpression));
        }

        return source;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> Query<T>(this IEnumerable<T> source, string propertyName, object propertyValue)
        where T : class
    {
        //dummy.Where(p => p.AdSoyad.ToLower().Contains(filterValue.ToString()))

        var entityParam = Expression.Parameter(typeof(T), typeof(T).Name.ToLower());

        var entityProperty = Expression.Property(entityParam, propertyName);

        var value = Convert.ChangeType(propertyValue, entityProperty.Type);

        var constant = Expression.Constant(value);

        Expression left =
            Expression.Call(entityProperty, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));

        Expression right = Expression.Call(constant, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));

        var searchTypeMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});

        var expression = Expression.Call(left, searchTypeMethod, right);

        var finalExpression = Expression.Lambda<Func<T, bool>>(expression, entityParam);

        return source.Where(finalExpression.Compile());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> Filter<T>(this IQueryable<T> source, string propertyName, object propertyValue)
        where T : class
    {
        var param = Expression.Parameter(typeof(T), typeof(T).Name.ToLower());
        var property = Expression.Property(param, propertyName);
        var searchValue = Convert.ChangeType(propertyValue, property.Type);

        Expression matchExpression = property;

        if (matchExpression.Type != typeof(string))
        {
            matchExpression = Expression.Convert(matchExpression, typeof(object));
            matchExpression = Expression.Convert(matchExpression, typeof(string));
        }

        var pattern = Expression.Constant($"%{searchValue}%");

        var expr = Expression.Call(typeof(DbFunctionsExtensions),
            "Like",
            Type.EmptyTypes,
            Expression.Constant(EF.Functions),
            matchExpression,
            pattern);

        return source.Where(Expression.Lambda<Func<T, bool>>(expr, param))
            .AsQueryable();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <param name="success"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> Where<T>(this IQueryable<T> source,
        string propertyName,
        object propertyValue,
        out bool success) where T : class
    {
        success = false;
        var mba = PropertyAccessorCache<T>.Get(propertyName);

        if (mba == null) return source;

        object value;

        try
        {
            value = Convert.ChangeType(propertyValue, mba.ReturnType);
        }
        catch (SystemException ex) when (ex is InvalidCastException ||
                                         ex is FormatException ||
                                         ex is OverflowException ||
                                         ex is ArgumentNullException)
        {
            return source;
        }

        var eqe = Expression.Equal(mba.Body, Expression.Constant(value, mba.ReturnType));

        var queryExpr = Expression.Lambda(eqe, mba.Parameters[0]);

        success = true;

        var resultExpression = Expression.Call(null,
            GetMethodInfo<IQueryable<T>,
                Expression<Func<T, bool>>,
                IQueryable<T>>(Queryable.Where),
            new[] {source.Expression, Expression.Quote(queryExpr)});

        return source.Provider.CreateQuery<T>(resultExpression);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="columnName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return source;

        var parameter = Expression.Parameter(source.ElementType, "");
        var property = Expression.Property(parameter, columnName);
        var lambda = Expression.Lambda(property, parameter);

        //string methodName = isAscending ? "OrderBy" : "OrderByDescending";  
        var methodName = "OrderBy"; // : "OrderByDescending";  

        Expression methodCallExpression = Expression.Call(typeof(Queryable),
            methodName,
            new[] {source.ElementType, property.Type},
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(methodCallExpression)
            .AsQueryable();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="columnName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return source;

        var parameter = Expression.Parameter(source.ElementType, "");
        var property = Expression.Property(parameter, columnName);
        var lambda = Expression.Lambda(property, parameter);
        var methodName = "OrderByDescending";

        Expression methodCallExpression = Expression.Call(typeof(Queryable),
            methodName,
            new[] {source.ElementType, property.Type},
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(methodCallExpression)
            .AsQueryable();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="orders"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IQueryable<T> Order<T>(this IQueryable<T> source, IList<IDictionary<string, string>> orders)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var entityName = nameof(T).ToLower();
        var entityType = source.ElementType;

        var entityParameter = Expression.Parameter(entityType, entityName);

        var orderType = orders[0]["orderType"];

        var methodName = orderType == "asc"
            ? "OrderBy"
            : "OrderByDescending";

        var orderBy = orders[0]["orderBy"];
        var orderProperty = entityType.GetProperty(orderBy,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        var propertyAccess = Expression.MakeMemberAccess(entityParameter, orderProperty!);
        var orderByExpression = Expression.Lambda(propertyAccess, entityParameter);
        var resultExpression = Expression.Call(typeof(Queryable),
            methodName,
            new[] {entityType, orderProperty.PropertyType},
            source.Expression,
            Expression.Quote(orderByExpression));

        var items = orders.TakeLast(orders.Count - 1);

        foreach (var order in items)
        {
            orderBy = order["by"];
            orderType = order["type"];

            methodName = orderType == "asc"
                ? "ThenBy"
                : "ThenByDescending";

            orderProperty = entityType.GetProperty(orderBy,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            propertyAccess = Expression.MakeMemberAccess(entityParameter, orderProperty!);

            orderByExpression = Expression.Lambda(propertyAccess, entityParameter);

            resultExpression = Expression.Call(typeof(Queryable),
                methodName,
                new[] {entityType, orderProperty.PropertyType},
                resultExpression,
                Expression.Quote(orderByExpression));
        }

        return source.Provider.CreateQuery<T>(resultExpression)
            .AsQueryable();
    }

    private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f)
    {
        return f.Method;
    }

    // public void TranslateInto(string[] companies) 
    // { 
    //     IQueryable<String>  queryableData = companies.AsQueryable(); 
    //     ParameterExpression pe            = Expression.Parameter(typeof (string), "company"); 
    //     Expression          right         = Expression.Constant("coho winery"); 
    //     Expression          equal         = Expression.Equal(pe, right); 
    //     MethodCallExpression whereCallExpression = Expression.Call( 
    //                                                                typeof (Queryable), 
    //                                                                "Where", 
    //                                                                new[] {queryableData.ElementType}, 
    //                                                                queryableData.Expression, 
    //                                                                Expression.Lambda<Func<string, bool>>(equal, new[] {pe})); 
    //     // ***** End Where ***** 
    //     IQueryable<string> resulList = queryableData.Provider.CreateQuery<string>(whereCallExpression); 
    //     resulList.Dump();
    //     Console.WriteLine ("........................ ");
    //     foreach (string company in resulList)
    //     {
    //         Console.WriteLine (company);
    //     }
    //     Console.WriteLine (",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");
    //     return; 
    // }
}