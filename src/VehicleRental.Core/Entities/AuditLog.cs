using System;
using System.Text.Json;

namespace VehicleRental.Core.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public required string EntityName { get; set; }
        public int EntityId { get; set; }
        public required string Action { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }

        // Helper methods for JSON conversion
        public T? GetOldValues<T>() where T : class
        {
            return string.IsNullOrEmpty(OldValues) ? null : JsonSerializer.Deserialize<T>(OldValues);
        }

        public T? GetNewValues<T>() where T : class
        {
            return string.IsNullOrEmpty(NewValues) ? null : JsonSerializer.Deserialize<T>(NewValues);
        }

        public void SetOldValues<T>(T values) where T : class
        {
            OldValues = JsonSerializer.Serialize(values);
        }

        public void SetNewValues<T>(T values) where T : class
        {
            NewValues = JsonSerializer.Serialize(values);
        }
    }
}