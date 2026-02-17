namespace DungeonShooter
{
    /// <summary>
    /// Room 관련 상수 정의
    /// </summary>
    public static class RoomConstants
    {
        public const string TilemapGameObjectName = "Tilemaps";
        public const string ObjectsGameObjectName = "Objects";
        public const string TimemapComponentNameBase = "Tilemap_";
        public const string TilemapDecoName = TimemapComponentNameBase + "Deco";
        public const string TilemapGroundName = TimemapComponentNameBase + "Ground";
        public const int RoomSpacing = 28;
        public const int RoomSizeMinX = 7;
        public const int RoomSizeMinY = 7;
        public const int RoomSizeMaxX = 24;
        public const int RoomSizeMaxY = 24;
        public const int RoomCorridorSize = 5;
        public const int DefaultRoomCount = 15;
    }
}

