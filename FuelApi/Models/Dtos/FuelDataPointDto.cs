namespace FuelApi.Models.Dtos;

public class FuelDataPointDto
{
    public DateTime Timestamp { get; set; }
    public double RawVoltage { get; set; }
    public double FilteredVoltage { get; set; }
    public double FuelLiters { get; set; }
}
