using LinqKit;
using System.Linq.Expressions;
namespace StudentTracker.Domain.Specification;
public abstract class Specification<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, bool>> Criteria { get; private set; } = entity => true;
    public IReadOnlyList<string> Includes => _includes.ToList().AsReadOnly();
    public IReadOnlyList<Expression<Func<TEntity, object>>> OrderByExpression => _orderByExpression.ToList().AsReadOnly();
    public IReadOnlyList<Expression<Func<TEntity, object>>> OrderByDescendingExpression => _orderByDescendingExpression.ToList().AsReadOnly();
    public IReadOnlyList<(Expression<Func<TEntity, object>> Selector, string)> GroupByExpression => _groupByExpression.ToList().AsReadOnly();

    private readonly HashSet<string> _includes = new();
    private readonly HashSet<Expression<Func<TEntity, object>>> _orderByExpression = new();
    private readonly HashSet<Expression<Func<TEntity, object>>> _orderByDescendingExpression = new();
    private readonly HashSet<(Expression<Func<TEntity, object>> Selector, string)> _groupByExpression = new();

    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool IsTotalCountEnable { get; private set; }
    public bool IsDistinct { get; private set; }
    public bool IsGlobalFiltersIgnored { get; private set; }
    public bool IsSplitQuery { get; private set; }

    protected Specification(bool splitQuery = false)
    {
        IsSplitQuery = splitQuery;
    }

    protected Specification<TEntity> AddInclude(string includeExpression)
    {
        if (string.IsNullOrWhiteSpace(includeExpression))
            throw new ArgumentException("Include expression cannot be null or empty.", nameof(includeExpression));
        _includes.Add(includeExpression);
        return this;
    }

    protected Specification<TEntity> AddCriteria(Expression<Func<TEntity, bool>> criteriaExpression)
    {
        Criteria = Criteria.And(criteriaExpression);
        return this;
    }

    protected Specification<TEntity> AddOrderBy(Expression<Func<TEntity, object>> orderByExpression , 
        bool SortDesc)
    {
        if (orderByExpression is null)
            throw new ArgumentException("orderby expression cannot be null or empty.", nameof(orderByExpression));
        if(SortDesc)
            _orderByDescendingExpression.Add(orderByExpression);
        else
            _orderByExpression.Add(orderByExpression);
        return this;
    }

    protected Specification<TEntity> AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
    {
        if (orderByDescendingExpression is null)
            throw new ArgumentException("orderby descending expression cannot be null or empty.", nameof(orderByDescendingExpression));
        _orderByDescendingExpression.Add(orderByDescendingExpression);
        return this;
    }

    protected Specification<TEntity> AddGroupBy(Expression<Func<TEntity, object>> selector, string groupingKey)
    {
        if (selector is null)
            throw new ArgumentException("Group by selector cannot be null.", nameof(selector));
        _groupByExpression.Add((selector, groupingKey));
        return this;
    }

    protected Specification<TEntity> ApplyPaging(int pageSize, int pageIndex)
    {
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
        if (pageIndex <= 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));

        Skip = pageSize * (pageIndex - 1);
        Take = pageSize;
        IsPagingEnabled = true;
        EnableTotalCount();
        return this;
    }

    protected void EnableTotalCount() => IsTotalCountEnable = true;
    protected void EnableDistinct() => IsDistinct = true;
    protected void IgnoreGlobalFilters() => IsGlobalFiltersIgnored = true;

    public Specification<TEntity> CombineWith(params Specification<TEntity>[] specifications)
    {
        foreach (var specification in specifications)
        {
            Criteria = Criteria.And(specification.Criteria);

            foreach (var include in specification.Includes)
                _includes.Add(include);

            foreach (var orderBy in specification.OrderByExpression)
                _orderByExpression.Add(orderBy);


            foreach (var orderByDescending in specification.OrderByDescendingExpression)
                _orderByDescendingExpression.Add(orderByDescending);

            foreach (var groupBy in specification.GroupByExpression)
                _groupByExpression.Add(groupBy);

            if (specification.IsPagingEnabled)
            {
                ApplyPaging(specification.Take, (specification.Skip / specification.Take) + 1);
            }

            if (specification.IsTotalCountEnable)
            {
                EnableTotalCount();
            }

            if (specification.IsDistinct)
            {
                EnableDistinct();
            }

            if (specification.IsGlobalFiltersIgnored)
            {
                IgnoreGlobalFilters();
            }

            IsSplitQuery = IsSplitQuery || specification.IsSplitQuery;
        }

        return this;
    }

}
