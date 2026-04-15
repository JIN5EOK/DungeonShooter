using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace DungeonShooter
{
    public static class CSVSerializer
    {
        public static List<TDto> ReadCsv<TDto>(string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                LogHandler.LogError(nameof(CSVSerializer), $"CSV 파일이 존재하지 않습니다: {csvPath}");
                return new List<TDto>();
            }

            try
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    NewLine = "\n",
                };

                using var reader = new StreamReader(csvPath);
                using var csv = new CsvReader(reader, csvConfig);
                return csv.GetRecords<TDto>().ToList();
            }
            catch (Exception ex)
            {
                LogHandler.LogError(nameof(CSVSerializer), $"CSV 읽기 실패: {ex.Message}");
                LogHandler.LogException(nameof(CSVSerializer), ex);
                return new List<TDto>();
            }
        }

        public static bool WriteCsv<TDto>(IEnumerable<TDto> records, string csvPath)
        {
            if (records == null)
            {
                LogHandler.LogError(nameof(CSVSerializer), "records가 null입니다.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(csvPath))
            {
                LogHandler.LogError(nameof(CSVSerializer), "csvPath가 비었습니다.");
                return false;
            }

            try
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    NewLine = "\n",
                };

                using var writer = new StreamWriter(csvPath);
                using var csv = new CsvWriter(writer, csvConfig);
                csv.WriteRecords(records);
                return true;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(nameof(CSVSerializer), $"CSV 쓰기 실패: {ex.Message}");
                LogHandler.LogException(nameof(CSVSerializer), ex);
                return false;
            }
        }
    }
}

