using System.Linq.Expressions;

namespace Application.Core
{
    public static class DateFilterExtensions
    {
        public static IQueryable<T> ApplyDateFilter<T>(
            this IQueryable<T> query,
            DateFilter filter,
            Expression<Func<T,
            DateTime?>> selector)
        {
            if (filter == null) return query;

            if (filter.GreaterThan.HasValue)
                query = query.Where(Expression.Lambda<Func<T, bool>>(
                    Expression.GreaterThan(
                        Expression.Invoke(selector, selector.Parameters),
                        Expression.Constant(filter.GreaterThan.Value, typeof(DateTime?))),
                    selector.Parameters));

            if (filter.GreaterThanOrEqual.HasValue)
                query = query.Where(Expression.Lambda<Func<T, bool>>(
                    Expression.GreaterThanOrEqual(
                        Expression.Invoke(selector, selector.Parameters),
                        Expression.Constant(filter.GreaterThanOrEqual.Value, typeof(DateTime?))),
                    selector.Parameters));

            if (filter.LessThan.HasValue)
                query = query.Where(Expression.Lambda<Func<T, bool>>(
                    Expression.LessThan(
                        Expression.Invoke(selector, selector.Parameters),
                        Expression.Constant(filter.LessThan.Value, typeof(DateTime?))),
                    selector.Parameters));

            if (filter.LessThanOrEqual.HasValue)
                query = query.Where(Expression.Lambda<Func<T, bool>>(
                    Expression.LessThanOrEqual(
                        Expression.Invoke(selector, selector.Parameters),
                        Expression.Constant(filter.LessThanOrEqual.Value, typeof(DateTime?))),
                    selector.Parameters));

            if (filter.Equal.HasValue)
                query = query.Where(Expression.Lambda<Func<T, bool>>(
                    Expression.Equal(
                        Expression.Invoke(selector, selector.Parameters),
                        Expression.Constant(filter.Equal.Value, typeof(DateTime?))),
                    selector.Parameters));

            return query;
        }
    }

}