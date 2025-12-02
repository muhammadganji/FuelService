using FuelApi.DataStores;
using FuelApi.Models;
using FuelApi.Models.Dtos;

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

        var validation = Validation(target);
        if (!string.IsNullOrWhiteSpace(validation))
            return FuelResult.Failed(validation);

        int indexLeft = 0;
        int indexRight = _dataPoints.Count - 1;

        while (indexLeft <= indexRight)
        {
            int mid = (indexLeft + indexRight) / 2;
            var midTime = _dataPoints[mid].Timestamp;

            if (midTime == target)
            {
                return FuelResult.Success(target, _dataPoints[mid].FuelLiters);
            }

            if (midTime < target)
                indexLeft = mid + 1;
            else
                indexRight = mid - 1;
        }

        FuelDataPointDto? before = null;
        FuelDataPointDto? after = null;

        if (indexRight >= 0)
            before = _dataPoints[indexRight];

        if (indexLeft < _dataPoints.Count)
            after = _dataPoints[indexLeft];

        double s2 = 1;
        var s1 = (target - before!.Timestamp).TotalSeconds;
        s2 = (after!.Timestamp - before.Timestamp).TotalSeconds;
        var subLiter = (after.FuelLiters - before.FuelLiters);
        var ratio = s1 / s2;
        var liters = before.FuelLiters + ratio * subLiter;

        return FuelResult.Success(target, Math.Round(liters, 2));
    }


    /// <summary>
    /// اعتبارسنجی تاریخ ورودی کاربر
    /// </summary>
    private string Validation(DateTime target)
    {

        if (_dataPoints.Count == 0)
            return "هیچ داده ای بارگذاری نشده است.";

        var minTime = _dataPoints.First().Timestamp;
        var maxTime = _dataPoints.Last().Timestamp;

        if (target <= minTime || target >= maxTime)
            return $"زمان بین محدوده {minTime:yyyy-MM-dd HH:mm} تا {maxTime:yyyy-MM-dd HH:mm} قابل قبول است";

        return string.Empty;
    }





}