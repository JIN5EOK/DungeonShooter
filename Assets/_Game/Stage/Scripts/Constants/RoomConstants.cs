namespace DungeonShooter
{
    /// <summary>
    /// Room 관련 상수 정의
    /// </summary>
    public static class RoomConstants
    {
        public const string RANDOM_ENEMY_SPAWN_ADDRESS = "RandomEnemySpawn";
        public const string PLAYER_SPAWN_ADDRESS = "PlayerSpawn";
        public const string TILEMAPS_GAMEOBJECT_NAME = "Tilemaps";
        public const string OBJECTS_GAMEOBJECT_NAME = "Objects";
        public const string TILEMAP_COMPONENT_NAME_BASE = "Tilemap_";
        public const string TILEMAP_DECO_NAME = TILEMAP_COMPONENT_NAME_BASE + "Deco";
        public const string TILEMAP_GROUND_NAME = TILEMAP_COMPONENT_NAME_BASE + "Ground";
        public const int ROOM_SPACING = 28;
        public const int ROOM_SIZE_MIN_X = 7;
        public const int ROOM_SIZE_MIN_Y = 7;
        public const int ROOM_SIZE_MAX_X = 24;
        public const int ROOM_SIZE_MAX_Y = 24;
        public const int ROOM_CORRIDOR_SIZE = 5;
        public const int DEFAULT_ROOM_COUNT = 15;
    }
}

