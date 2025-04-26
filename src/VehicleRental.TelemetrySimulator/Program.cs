using System.Net.Http.Json;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using VehicleRental.Core.DTOs;

namespace VehicleRental.TelemetrySimulator;

public class TelemetryRecord
{
    public string vin { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public decimal value { get; set; }
    public long timestamp { get; set; }
    public bool Sent { get; set; }
}

public class TelemetryRequest
{
    public int VehicleId { get; set; }
    public int TelemetryTypeId { get; set; }
    public decimal Value { get; set; }
    public long Timestamp { get; set; }
}

public class TelemetryRecordMap : ClassMap<TelemetryRecord>
{
    public TelemetryRecordMap()
    {
        Map(m => m.vin).Name("vin");
        Map(m => m.name).Name("name");
        Map(m => m.value).Name("value");
        Map(m => m.timestamp).Name("timestamp");
        Map(m => m.Sent).Ignore();
    }
}

class Program
{
    private static readonly HttpClient _httpClient;
    private static readonly string _apiBaseUrl = "http://localhost:5270/api/telemetry";
    private static readonly Dictionary<string, int> _vehicleIds = new()
    {
        { "WAUZZZ4V4KA000002", 1 },
        { "WVWZZZAUZLW000003", 2 },
        { "WBA3A5C58DF123456", 3 }
    };

    private static readonly Dictionary<string, int> _telemetryTypeIds = new()
    {
        { "odometer", 1 },
        { "battery_soc", 2 }
    };

    static Program()
    {
        _httpClient = new HttpClient();
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("Telemetry Simulator started...");
        Console.WriteLine("Press Ctrl+C to exit");

        // Read and process telemetry data
        var csvPath = "telemetry.csv";
        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"Error: telemetry.csv not found at {csvPath}");
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<TelemetryRecordMap>();
        var records = csv.GetRecords<TelemetryRecord>().ToList();

        var lastTimestamp = 0L;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/vehicles/1/odometer");
        if (response.IsSuccessStatusCode)
        {
            var telemetry = await response.Content.ReadFromJsonAsync<TelemetryResponse>();
            if (telemetry != null)
            {
                lastTimestamp = telemetry.Timestamp;
            }
        }

        while (true)
        {
            currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (var record in records.Where(r => !r.Sent))
            {
                if (record.timestamp <= lastTimestamp)
                {
                    record.Sent = true;
                    continue;
                }

                if (currentTime >= record.timestamp)
                {
                    await SendTelemetryData(record);
                    record.Sent = true;
                }
            }

            // If all records are sent, exit the loop
            if (records.All(r => r.Sent))
            {
                Console.WriteLine("All telemetry data processed!");
                break;
            }

            // Wait for 1 second before checking again
            await Task.Delay(1000);
        }
    }

    private static async Task SendTelemetryData(TelemetryRecord record)
    {
        try
        {
            if (!_vehicleIds.TryGetValue(record.vin, out var vehicleId))
            {
                Console.WriteLine($"Warning: Vehicle with VIN {record.vin} not found");
                return;
            }

            if (!_telemetryTypeIds.TryGetValue(record.name.ToLower(), out var telemetryTypeId))
            {
                Console.WriteLine($"Warning: Telemetry type {record.name} not found");
                return;
            }

            var request = new TelemetryRequest
            {
                VehicleId = vehicleId,
                TelemetryTypeId = telemetryTypeId,
                Value = record.value,
                Timestamp = record.timestamp
            };

            var response = await _httpClient.PostAsJsonAsync(_apiBaseUrl, request);
            response.EnsureSuccessStatusCode();
            if (record.name == "odometer")
            {
                Console.WriteLine($"Sent telemetry data: vehicle {_vehicleIds[record.vin]} - {record.name}    = {record.value}\t\t at {DateTimeOffset.FromUnixTimeSeconds(record.timestamp).ToOffset(TimeSpan.FromHours(2))}");
            }
            else
            {
                Console.WriteLine($"Sent telemetry data: vehicle {_vehicleIds[record.vin]} - {record.name} = {record.value}\t\t at {DateTimeOffset.FromUnixTimeSeconds(record.timestamp).ToOffset(TimeSpan.FromHours(2))}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending telemetry data: {ex.Message}");
        }
    }
}

