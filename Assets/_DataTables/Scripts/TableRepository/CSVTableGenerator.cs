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
        private const string DataTablePath = "Assets/_DataTables/Tables";

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
        /// StageConfigTable.csv 파일을 생성합니다.
        /// </summary>
        [MenuItem("Tools/Generate Tables/Generate StageConfigTable.csv")]
        public static void GenerateStageConfigTableCSV()
        {
            GenerateCSVTemplate(typeof(StageConfigTableEntry), "StageConfigTable.csv");
        }

        /// <summary>
        /// PlayerConfigTable.csv 파일을 생성합니다.
        /// </summary>
        [MenuItem("Tools/Generate Tables/Generate PlayerConfigTable.csv")]
        public static void GeneratePlayerConfigTableCSV()
        {
            GenerateCSVTemplate(typeof(PlayerConfigTableEntry), "PlayerConfigTable.csv");
        }

        /// <summary>
        /// CSV 템플릿 파일을 생성합니다.
        /// 헤더와 예시 데이터를 포함합니다.
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
                
                // 예시 데이터 추가
                if (tableEntryType == typeof(SkillTableEntry))
                {
                    writer.WriteLine("1,파이어볼,적을 태우는 화염구,skill_fireball_icon,skill_example,range:5.0/speed:2.5,damage:30/count:3,30,0,0.5,5.0,1,10.0");
                }
                else if (tableEntryType == typeof(ItemTableEntry))
                {
                    writer.WriteLine("1,item_example,Example Item,Consume,10,1,0,0,0,item_icon");
                }
                else if (tableEntryType == typeof(StageConfigTableEntry))
                {
                    writer.WriteLine("1,ground_tile_default,enemies_stage1,start_rooms_stage1,normal_rooms_stage1,boss_rooms_stage1");
                }
                else if (tableEntryType == typeof(PlayerConfigTableEntry))
                {
                    writer.WriteLine("1,플레이어1,기본 플레이어 캐릭터,Player,1,1,2");
                }
            }

            AssetDatabase.Refresh();
            LogHandler.Log(nameof(CSVTableGenerator),$"CSV 템플릿 생성 완료: {filePath}");
            EditorUtility.DisplayDialog("성공", $"CSV 파일이 생성되었습니다: {filePath}", "확인");
        }
    }
}
