
using FluentValidation;
using StudentTracker.Domain.Primitives;
using StudentTracker.Domain.Repositories;

namespace StudentTracker.Application.Extensions.FluentValidator;
public static class EntityExistValidator
{
    public static IRuleBuilderOptions<T, TKey> EntityExist<T, TEntity, TKey>(
    this IRuleBuilder<T, TKey> ruleBuilder,
    IGenericRepository<TEntity> entityRepo, string messageError = "not found")
    where TEntity : Entity<TKey>
    where TKey : IEquatable<TKey>
    {
        return ruleBuilder
            .MustAsync(async (id, cancellationToken)
                => await entityRepo.IsExistAsync(entity => entity.Id.Equals(id), cancellationToken)).WithMessage(messageError);
    }
}
