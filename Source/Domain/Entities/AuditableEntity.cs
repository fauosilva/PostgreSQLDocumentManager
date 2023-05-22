﻿namespace Domain.Entities
{
    /// <summary>
    /// Entity that contains basic audit properties.
    /// </summary>
    public record AuditableEntity<T> : BaseEntity<T>
    {
        public DateTimeOffset Inserted_At { get; set; }
        public string Inserted_By { get; set; }
        public DateTimeOffset? Updated_At { get; set; }
        public string? Updated_By { get; set; }
    }
}
