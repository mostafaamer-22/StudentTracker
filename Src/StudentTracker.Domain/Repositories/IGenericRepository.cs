using StudentTracker.Domain.Specification;
using System.Linq.Expressions;

namespace StudentTracker.Domain.Repositories;
public interface IGenericRepository<TEntity> where TEntity : class
{
    bool HasData();
    IQueryable<TEntity> GetQueryable();
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
    IReadOnlyList<TEntity> GetAll();

    Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default);

    Task<(IQueryable<TEntity> data, int count)> GetWithSpecAsync(Specification<TEntity> specifications, CancellationToken cancellationToken = default);
    Task<TEntity?> GetEntityWithSpecAsync(Specification<TEntity> specifications, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByPropertyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    void Delete(TEntity entity);
    void DeleteRange(IEnumerable<TEntity> entity);
    Task<bool> IsExistAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}