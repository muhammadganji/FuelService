namespace FuelApi.Models;

public class FuelResult
{
    public DateTime Timestamp { get; set; }
    public double FuelLiters { get; set; }
    public string? Message { get; set; } = "";
    public static FuelResult Success(DateTime ts, double liters)
        =>new FuelResult
        {
            Timestamp = ts,
            FuelLiters = liters
        };

    public static FuelResult Failed(string message)
        => new FuelResult
        {
            Timestamp = default,
            FuelLiters = 0,
            Message = message
        };

    public bool IsSuccess => Message == null;
}
