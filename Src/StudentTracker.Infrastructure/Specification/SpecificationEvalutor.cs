using Microsoft.EntityFrameworkCore;
using StudentTracker.Domain.Specification;
using System.Collections.Concurrent;

namespace StudentTracker.Infrastructure.Specification;
public static class SpecificationEvaluator<TEntity> where TEntity : class
{
    private static readonly ConcurrentDictionary<string, object> QueryCache = new();

    public static async Task<(IQueryable<TEntity> data, int count)> GetQueryAsync(
        IQueryable<TEntity> inputQuery,
        Specification<TEntity> specifications,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specifications);

        return await EvaluateSpecificationAsync(inputQuery, specifications, cancellationToken);
    }

    private static async Task<(IQueryable<TEntity> data, int count)> EvaluateSpecificationAsync(
        IQueryable<TEntity> inputQuery,
        Specification<TEntity> specifications,
        CancellationToken cancellationToken)
    {
        IQueryable<TEntity> queryable = inputQuery;

        if (specifications.IsGlobalFiltersIgnored)
            queryable = queryable.AsNoTracking().IgnoreQueryFilters();

        if (specifications.Criteria is not null)
            queryable = queryable.Where(specifications.Criteria);

        if (specifications.OrderByDescendingExpression.Any())
        {
            IOrderedQueryable<TEntity>? orderedQuery = null;

            foreach (var orderByDesc in specifications.OrderByDescendingExpression)
                orderedQuery = orderedQuery is null
                    ? queryable.OrderByDescending(orderByDesc)
                    : orderedQuery.ThenByDescending(orderByDesc);

            queryable = orderedQuery!;
        }
        else if (specifications.OrderByExpression.Any())
        {
            IOrderedQueryable<TEntity>? orderedQuery = null;

            foreach (var orderBy in specifications.OrderByExpression)
                orderedQuery = orderedQuery is null
                    ? queryable.OrderBy(orderBy)
                    : orderedQuery.ThenBy(orderBy);

            queryable = orderedQuery!;
        }

        foreach (var (selector, _) in specifications.GroupByExpression)
        {
            queryable = queryable.GroupBy(selector).SelectMany(g => g);
        }

        if (specifications.IsDistinct)
            queryable = queryable.Distinct();

        int count = specifications.IsTotalCountEnable
            ? await queryable.CountAsync(cancellationToken)
            : 0;

        if (specifications.IsPagingEnabled)
            queryable = queryable.Skip(specifications.Skip).Take(specifications.Take);

        foreach (var include in specifications.Includes)
            queryable = queryable.Include(include);

        queryable = specifications.IsSplitQuery
            ? queryable.AsSplitQuery().TagWith($"Split Query - {typeof(TEntity).Name}")
            : queryable.AsSingleQuery().TagWith($"Single Query - {typeof(TEntity).Name}");

        return (queryable, count);
    }
}
