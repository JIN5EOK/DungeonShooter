using UnityEditor;
using System.IO;
using System.Reflection;
using System.Text;

namespace DungeonShooter
{
    /// <summary>
    /// CSV 테이블 파일을 자동으로 생성하는 에디터 유틸리티
    /// MenuItem을 통해 에디터 메뉴에서 접근 가능
    /// </summary>
    public static class CSVTableGenerator
    {
        private const string DataTablePath = "Assets/_Data/Tables";

        /// <summary>
        /// SkillTable.csv 파일을 생성합니다.
        /// </summary>
        [MenuItem("Tools/Generate Tables/Generate SkillTable.csv")]
        public static void GenerateSkillTableCSV()
        {
            GenerateCSVTemplate(typeof(SkillTableEntry), "SkillTable.csv");
        }

        /// <summary>
        /// ItemTable.csv 파일을 생성합니다.
        /// </summary>
        [MenuItem("Tools/Generate Tables/Generate ItemTable.csv")]
        public static void GenerateItemTableCSV()
        {
            GenerateCSVTemplate(typeof(ItemTableEntry), "ItemTable.csv");
        }

        /// <summary>
        /// CSV 템플릿 파일을 생성합니다.
        /// 헤더만 포함하고 데이터는 비워둡니다.
        /// </summary>
        private static void GenerateCSVTemplate(System.Type tableEntryType, string fileName)
        {
            // 디렉토리 생성
            if (!Directory.Exists(DataTablePath))
            {
                Directory.CreateDirectory(DataTablePath);
            }

            var filePath = Path.Combine(DataTablePath, fileName);

            // 헤더 생성
            var properties = tableEntryType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var headerBuilder = new StringBuilder();

            foreach (var property in properties)
            {
                if (headerBuilder.Length > 0)
                    headerBuilder.Append(",");
                headerBuilder.Append(property.Name);
            }

            // CSV 파일 작성
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.WriteLine(headerBuilder.ToString());
                // 데이터 행은 비워둠 (사용자가 직접 입력하도록)
            }

            AssetDatabase.Refresh();
            LogHandler.Log(nameof(CSVTableGenerator),$"CSV 템플릿 생성 완료: {filePath}");
            EditorUtility.DisplayDialog("성공", $"CSV 파일이 생성되었습니다: {filePath}", "확인");
        }
    }
}
