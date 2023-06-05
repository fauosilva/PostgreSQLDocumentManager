using ApplicationCore.Interfaces.Dtos;
using ApplicationCore.Interfaces.Entities;
using FluentAssertions.Equivalency;

namespace UnitTests.Extensions.FluentAssertions
{
    public static class FluentAssertionExtensions
    {
        public static EquivalencyAssertionOptions<TSubject> WithAuditableMapping<TSubject>(this EquivalencyAssertionOptions<TSubject> options) where TSubject : IAuditableEntity
        {
            return options.WithMapping<IAuditableEntityDto>(e => e.Inserted_At, s => s!.InsertedAt)
                          .WithMapping<IAuditableEntityDto>(e => e.Inserted_By, s => s!.InsertedBy)
                          .WithMapping<IAuditableEntityDto>(e => e.Updated_At, s => s!.UpdatedAt)
                          .WithMapping<IAuditableEntityDto>(e => e.Updated_By, s => s!.UpdatedBy);
        }
    }
}
