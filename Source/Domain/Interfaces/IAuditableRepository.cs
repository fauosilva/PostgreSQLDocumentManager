using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IAuditableRepository<T, TKeyType> : IBaseRepository<T, TKeyType> where T : AuditableEntity<TKeyType>
    {
    }
}
