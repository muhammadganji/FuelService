using FuelApi.Models.Dtos;
using FuelApi.Models.Inputs;
using OfficeOpenXml;

namespace FuelApi.DataStores
{
    public static class DataStore
    {
        /// <summary>
        /// نقاط داده زمانی 
        /// </summary>
        private static List<FuelDataPointDto> DataPoints { get; set; } = new();
        /// <summary>
        /// کالیبراسیون
        /// </summary>
        private static List<CalibrationInput> Calibrations { get; set; } = new();

        public static readonly string filePath = "fuel.xlsx";

        /// <summary>
        /// این متد در لحظه ی شروع سیستم اجرا میشود
        /// </summary>
        public static void LoadDataFromExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("muhammad ganji nezhad");

            SetCalibrations();
            SetDataPoints();
        }

        public static List<FuelDataPointDto> GetAllDataPoints()
        {
            
            return DataPoints;
        }

        public static List<CalibrationInput> GetAllCalibrations()
        {
            return Calibrations;
        }

        private static void SetDataPoints()
        {

            if (DataPoints.Count == 0)
            {
                var rawData = LoadRawFuelData(filePath);
                var filteredData = ApplyMovingAverageFilter(rawData, windowSize: 2);
                var dataPoint = ConvertToFuelDataPoints(filteredData, rawData);
                DataPoints = dataPoint;
            }
        }

        private static void SetCalibrations()
        {
            if (Calibrations.Count == 0)
            {
                var filePath = "fuel.xlsx";
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("فایل fuel.xlsx پیدا نشد");
                using var package = new ExcelPackage(new FileInfo(filePath));
                var sheetCalibration = package.Workbook.Worksheets["calibration"]
                            ?? throw new InvalidOperationException("شیت 'calibration' پیدا نشد");

                var result = new List<CalibrationInput>();
                var rowCount = sheetCalibration.Dimension.End.Row;
                for (int row = 2; row <= rowCount; row++)
                {
                    double totalLiter = sheetCalibration.Cells[row, 2].GetValue<double>(); // TotalLiter
                    double voltNo = sheetCalibration.Cells[row, 3].GetValue<double>(); // VoltNo
                    result.Add(new CalibrationInput { VoltNo = voltNo, TotalLiter = totalLiter });
                }
                Calibrations = result.OrderBy(c => c.VoltNo).ToList();
            }
        }

        /// <summary>
        /// بارگذاری وضعیت سوخت در بازه های زمانی
        /// </summary>
        private static List<RawFuelDateInput> LoadRawFuelData(string filePath)
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheetResult = package.Workbook.Worksheets["Result 1"]
                        ?? throw new InvalidOperationException("شیت 'Result 1' پیدا نشد.");

            var result = new List<RawFuelDateInput>();
            var rowCount = sheetResult.Dimension.End.Row;
            for (int row = 2; row <= rowCount; row++)
            {
                var timeText = sheetResult.Cells[row, 1].Text.Trim(); // ServerDateTime
                if (DateTime.TryParse(timeText, out var time))
                {
                    double voltage = sheetResult.Cells[row, 2].GetValue<double>(); // AnalogIN1
                    result.Add(new RawFuelDateInput { ServerDateTime = time, AnalogN1 = voltage });
                }
            }
            return result.OrderBy(w => w.ServerDateTime).ToList();
        }


        /// <summary>
        /// حذف نویز به کمک میانگین گیری
        /// </summary>
        /// <param name="windowSize">هر سطر داده میانگین چند عدد اطراف اون باشه</param>
        private static List<FuelDateFilterDto> ApplyMovingAverageFilter(
            List<RawFuelDateInput> rawData,
            int windowSize = 5
            )
        {

            /*
             من هر لحظه ولتاژ سنسور رو نمی ‌گیرم مستقیم استفاده کنم — خیلی نوسان داره
            به جاش می‌گم = بیا ۵ تا مقدار آخر (قبل و بعد این لحظه) رو میانگین بگیر،
            اینجوری یه عدد نرم و آروم بهم می ‌ده که واقعی ‌تره
             */

            var result = new List<FuelDateFilterDto>();
            var rowCount = rawData.Count;
            for (int i = 0; i < rowCount; i++)
            {
                int start = Math.Max(0, i - windowSize / 2); // دو تای پایین
                int end = Math.Min(rowCount - 1, i + windowSize / 2); // دوتای بالا

                double avgVoltage = 0;
                for (int j = start; j <= end; j++)
                    avgVoltage += rawData[j].AnalogN1;
                avgVoltage /= (end - start + 1);

                result.Add(new FuelDateFilterDto
                {
                    FilteredVoltage = avgVoltage,
                    TimeStamp = rawData[i].ServerDateTime
                });
            }

            return result;
        }



        private static List<FuelDataPointDto> ConvertToFuelDataPoints(
            List<FuelDateFilterDto> filtered,
            List<RawFuelDateInput> raw)
        {
            var list = new List<FuelDataPointDto>();
            var rowCount = raw.Count;
            for (int i = 0; i < rowCount; i++)
            {
                list.Add(new FuelDataPointDto
                {
                    Timestamp = raw[i].ServerDateTime,
                    RawVoltage = raw[i].AnalogN1,
                    FilteredVoltage = filtered[i].FilteredVoltage,
                    FuelLiters = VoltageToLiters(filtered[i].FilteredVoltage),
                });
            }

            return list;
        }


        /// <summary>
        ///  تبدیل ولتاژ فیلتر شده به لیتر 
        /// </summary>
        private static double VoltageToLiters(double filterVoltage)
        {
            var countColibration = Calibrations.Count;
            var firstPoint = Calibrations[0]; // کمترین مقدار باک
            var lastPoint = Calibrations[countColibration - 1];  // بیشترین مقدار باک

            if (filterVoltage <= firstPoint.VoltNo)
                return firstPoint.TotalLiter;  // باک خالی
            if (filterVoltage >= lastPoint.VoltNo)
                return lastPoint.TotalLiter; // باک پر

            for (int i = 0; i < countColibration - 1; i++)
            {
                // نقطه پایین‌تر
                var lowerPoint = Calibrations[i];
                var upperPoint = Calibrations[i + 1]; // نقطه بالاتر

                if (filterVoltage >= lowerPoint.VoltNo && filterVoltage <= upperPoint.VoltNo)
                {
                    // ولتاژ بین این دو نقطه است
                    double voltageDifference = upperPoint.VoltNo - lowerPoint.VoltNo;
                    double howFarWeAre = filterVoltage - lowerPoint.VoltNo;
                    double percentage = howFarWeAre / voltageDifference;  // عددی بین 0 تا 1

                    // حالا همون درصد رو از لیترها هم جلو می‌ریم
                    double liters = lowerPoint.TotalLiter +
                                       (percentage * (upperPoint.TotalLiter - lowerPoint.TotalLiter));
                    liters = Math.Max(liters, 2);
                    return liters;
                }
            }

            return 0; // در عمل این هیچ وقت صدا زده نمیشه
        }



    }
}
