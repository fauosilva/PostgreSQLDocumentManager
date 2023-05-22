using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IAuditableRepository<T, TKeyType> : IBaseRepository<T, TKeyType> where T : AuditableEntity<TKeyType> where TKeyType : notnull
    {
    }
}
