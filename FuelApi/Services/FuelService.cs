using FuelApi.DataStores;
using FuelApi.Models;
using FuelApi.Models.Dtos;
using FuelApi.Models.Inputs;
using OfficeOpenXml;

namespace FuelApi.Services;

public class FuelService
{
    /// <summary>
    /// نقاط داده زمانی 
    /// </summary>
    private readonly List<FuelDataPointDto> _dataPoints;

    public FuelService()
    {
        _dataPoints = DataStore.GetAllDataPoints();
    }

    

    /// <summary>
    /// متد عمومی که در ای پی آی صدا زده میشه
    /// </summary>
    public FuelResult GetFuelFromDate(DateTime target)
    {
        if (_dataPoints.Count == 0)
            return FuelResult.Failed("هیچ داده ای بارگذاری نشده است.");

        var minTime = _dataPoints.First().Timestamp;
        var maxTime = _dataPoints.Last().Timestamp;

        if (target <= minTime || target >= maxTime)
            return FuelResult.Failed($"زمان بین محدوده {minTime:yyyy-MM-dd HH:mm} تا {maxTime:yyyy-MM-dd HH:mm} قابل قبول است");

        return FindFuelFromDate(target);
    }

    /// <summary>
    /// مقدار سوخت از روی تاریخ
    /// </summary>
    private FuelResult FindFuelFromDate(DateTime target)
    {
        var before = _dataPoints.Last(p => p.Timestamp <= target);
        var after = _dataPoints.First(p => p.Timestamp >= target);

        if (before.Timestamp == after.Timestamp)
            return FuelResult.Success(target, before.FuelLiters);
        var s1 = (target - before.Timestamp).TotalSeconds;
        var s2 = (after.Timestamp - before.Timestamp).TotalSeconds;
        // محاسبه نسبت زمان به مبدا قبلی 
        double ratio = s1 / s2;
        var subLiter = (after.FuelLiters - before.FuelLiters);

        double liters = before.FuelLiters + ratio * subLiter;

        return FuelResult.Success(target, Math.Round(liters, 2));
    }



}