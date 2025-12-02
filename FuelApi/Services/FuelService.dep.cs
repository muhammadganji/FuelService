//using FuelApi.Models;
//using OfficeOpenXml;
//namespace FuelApi.Services;

//public class FuelService
//{
//    private readonly List<FuelDataPoint> _dataPoints;
//    private readonly Func<double, double> _voltageToLiters;

//    public FuelService()
//    {
//        //ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
//        ExcelPackage.License.SetNonCommercialPersonal("muhammad ganji nezhad");

//        var filePath = "fuel.xlsx";
//        if (!File.Exists(filePath))
//            throw new FileNotFoundException("فایل fuel.xlsx پیدا نشد. آن را در کنار exe قرار دهید.");

//        Console.WriteLine("در حال بارگذاری و پردازش fuel.xlsx ...");
//        (_dataPoints, _voltageToLiters) = LoadAndProcessExcel(filePath);
//        Console.WriteLine($"بارگذاری کامل شد: {_dataPoints.Count:N0} رکورد در حافظه");
//    }

//    private (List<FuelDataPoint>, Func<double, double>) LoadAndProcessExcel(string filePath)
//    {
//        using var package = new ExcelPackage(new FileInfo(filePath));

//        // 1. خواندن کالیبراسیون
//        var calibSheet = package.Workbook.Worksheets["calibration"];
//        var calibPoints = new List<(double Voltage, double Liters)>();

//        for (int row = 2; row <= calibSheet.Dimension.End.Row; row++)
//        {
//            var totalLiters = calibSheet.Cells[row, 2].GetValue<double>();
//            var voltage = calibSheet.Cells[row, 3].GetValue<double>();
//            calibPoints.Add((voltage, totalLiters));
//        }
//        calibPoints = calibPoints.OrderBy(p => p.Voltage).ToList();

//        // تابع اینترپولیشن کالیبراسیون
//        Func<double, double> calibrator = voltage =>
//        {
//            if (voltage <= calibPoints[0].Voltage) return calibPoints[0].Liters;
//            if (voltage >= calibPoints[^1].Voltage) return calibPoints[^1].Liters;

//            for (int i = 0; i < calibPoints.Count - 1; i++)
//            {
//                var (v1, l1) = calibPoints[i];
//                var (v2, l2) = calibPoints[i + 1];
//                if (voltage >= v1 && voltage <= v2)
//                {
//                    var ratio = (voltage - v1) / (v2 - v1);
//                    return l1 + ratio * (l2 - l1);
//                }
//            }
//            return 0;
//        };

//        // 2. خواندن دیتای اصلی + فیلتر نویز + کالیبراسیون
//        var resultSheet = package.Workbook.Worksheets["Result 1"];
//        var rawPoints = new List<(DateTime Time, double Voltage)>();

//        for (int row = 2; row <= resultSheet.Dimension.End.Row; row++)
//        {
//            var timeStr = resultSheet.Cells[row, 1].Text.Trim();
//            if (DateTime.TryParse(timeStr, out var time))
//            {
//                var voltage = resultSheet.Cells[row, 2].GetValue<double>();
//                rawPoints.Add((time, voltage));
//            }
//        }

//        // مرتب‌سازی بر اساس زمان
//        rawPoints = rawPoints.OrderBy(p => p.Time).ToList();

//        // فیلتر نویز: میانگین متحرک ۵ تایی
//        var filtered = new List<FuelDataPoint>();
//        const int window = 5;

//        for (int i = 0; i < rawPoints.Count; i++)
//        {
//            int start = Math.Max(0, i - window / 2);
//            int end = Math.Min(rawPoints.Count - 1, i + window / 2);
//            double avgVoltage = rawPoints.Skip(start).Take(end - start + 1).Average(p => p.Voltage);

//            double liters = calibrator(avgVoltage);

//            filtered.Add(new FuelDataPoint
//            {
//                Timestamp = rawPoints[i].Time,
//                RawVoltage = rawPoints[i].Voltage,
//                FilteredVoltage = avgVoltage,
//                FuelLiters = liters
//            });
//        }

//        return (filtered, calibrator);
//    }

//    /// <summary>
//    /// دریافت میزان سوخت در لحظه دلخواه (با اینترپولیشن زمانی)
//    /// </summary>
//    public double GetFuelLevelAt(DateTime target)
//    {
//        if (_dataPoints.Count == 0)
//            throw new InvalidOperationException("داده‌ای بارگذاری نشده");

//        if (target < _dataPoints[0].Timestamp || target > _dataPoints[^1].Timestamp)
//            throw new ArgumentException($"زمان باید بین {_dataPoints[0].Timestamp:yyyy-MM-dd HH:mm:ss} و {_dataPoints[^1].Timestamp:yyyy-MM-dd HH:mm:ss} باشد.");

//        // پیدا کردن نزدیک‌ترین نقاط قبل و بعد
//        var before = _dataPoints.LastOrDefault(p => p.Timestamp <= target);
//        var after = _dataPoints.FirstOrDefault(p => p.Timestamp >= target);

//        if (before == null) return after!.FuelLiters;
//        if (after == null) return before.FuelLiters;
//        if (before.Timestamp == after.Timestamp) return before.FuelLiters;

//        // اینترپولیشن خطی بر اساس زمان
//        double timeRatio = (target - before.Timestamp).TotalSeconds /
//                           (after.Timestamp - before.Timestamp).TotalSeconds;

//        return before.FuelLiters + timeRatio * (after.FuelLiters - before.FuelLiters);
//    }
//}