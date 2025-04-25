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
        };

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<TelemetryRecord>().ToList();

        // Sort records by timestamp
        records.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));

        // Get the last uploaded telemetry timestamp for each vehicle and type
        var lastTimestamps = new Dictionary<(string vin, string name), long>();
        foreach (var vehicleId in _vehicleIds.Values)
        {
            foreach (var telemetryType in _telemetryTypeIds.Keys)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{_apiBaseUrl}/vehicles/{vehicleId}/{telemetryType}");
                    if (response.IsSuccessStatusCode)
                    {
                        var telemetry = await response.Content.ReadFromJsonAsync<TelemetryResponse>();
                        if (telemetry != null)
                        {
                            var vin = _vehicleIds.FirstOrDefault(x => x.Value == vehicleId).Key;
                            lastTimestamps[(vin, telemetryType)] = telemetry.Timestamp;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not get last telemetry timestamp for vehicle {vehicleId}, type {telemetryType}: {ex.Message}");
                }
            }
        }

        // Find the starting index for each vehicle and type
        var startIndices = new Dictionary<(string vin, string name), int>();
        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            var key = (record.vin, record.name.ToLower());
            if (!startIndices.ContainsKey(key))
            {
                if (lastTimestamps.TryGetValue(key, out var lastTimestamp))
                {
                    if (record.timestamp > lastTimestamp)
                    {
                        startIndices[key] = i;
                    }
                }
                else
                {
                    startIndices[key] = i;
                }
            }
        }

        // Process records starting from the appropriate index for each vehicle and type
        var currentIndices = new Dictionary<(string vin, string name), int>(startIndices);
        while (currentIndices.Any(kvp => kvp.Value < records.Count))
        {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            foreach (var kvp in currentIndices.ToList())
            {
                if (kvp.Value >= records.Count) continue;

                var record = records[kvp.Value];
                if (record.vin == kvp.Key.vin && record.name.ToLower() == kvp.Key.name)
                {
                    if (currentTime >= record.timestamp)
                    {
                        await SendTelemetryData(record);
                        currentIndices[kvp.Key]++;
                    }
                }
            }

            // Wait for 1 second before checking again
            await Task.Delay(1000);
        }

        Console.WriteLine("All telemetry data processed!");
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

            // Check if telemetry data already exists
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/vehicles/{vehicleId}/{record.name.ToLower()}");
            if (response.IsSuccessStatusCode)
            {
                var existingTelemetry = await response.Content.ReadFromJsonAsync<TelemetryResponse>();
                if (existingTelemetry != null && existingTelemetry.Timestamp == record.timestamp)
                {
                    Console.WriteLine($"Skipping duplicate telemetry data: vehicle {vehicleId} - {record.name} = {record.value} at {DateTimeOffset.FromUnixTimeSeconds(record.timestamp).ToOffset(TimeSpan.FromHours(2))}");
                    return;
                }
            }

            var request = new TelemetryRequest
            {
                VehicleId = vehicleId,
                TelemetryTypeId = telemetryTypeId,
                Value = record.value,
                Timestamp = record.timestamp
            };

            response = await _httpClient.PostAsJsonAsync(_apiBaseUrl, request);
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

