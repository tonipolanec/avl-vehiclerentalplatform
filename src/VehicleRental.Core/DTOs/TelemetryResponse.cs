using System.ComponentModel.DataAnnotations;
using VehicleRental.Core.Entities;

namespace VehicleRental.Core.DTOs
{
    public class TelemetryResponse
    {
        public decimal Value { get; set; }
        public long Timestamp { get; set; }
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;

        public static TelemetryResponse FromEntity(Telemetry telemetry)
        {
            return new TelemetryResponse
            {
                Value = telemetry.Value,
                Timestamp = ((DateTimeOffset)telemetry.Timestamp).ToUnixTimeSeconds(),
                IsValid = telemetry.IsValid,
                ValidationMessage = telemetry.ValidationMessage
            };
        }
    }

    public class TelemetryHandshakeResponse
    {
        public string Message { get; set; } = string.Empty;

        public static TelemetryHandshakeResponse FromEntity(Telemetry telemetry)
        {
            return new TelemetryHandshakeResponse
            {
                Message = telemetry.ValidationMessage
            };
        }
    }

}