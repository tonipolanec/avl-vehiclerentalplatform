using System.ComponentModel.DataAnnotations;
using VehicleRental.Core.Entities;

namespace VehicleRental.Core.DTOs
{
    public class TelemetryResponse
    {
        public decimal Value { get; set; }
        public long Timestamp { get; set; }

        public static TelemetryResponse FromEntity(Telemetry telemetry)
        {
            return new TelemetryResponse
            {
                Value = telemetry.Value,
                Timestamp = ((DateTimeOffset)telemetry.Timestamp).ToUnixTimeSeconds(),
            };
        }
    }

    public class TelemetryConfirmationResponse
    {
        public string Message { get; set; } = string.Empty;

        public static TelemetryConfirmationResponse FromEntity(Telemetry telemetry)
        {
            return new TelemetryConfirmationResponse
            {
                Message = telemetry.ValidationMessage
            };
        }
    }

}