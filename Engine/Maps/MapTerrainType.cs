namespace CorrinoEngine.Maps
{
    public enum MapTerrainType
    {
        Unknown = -1,
        Sand = 0,
        Rock = 1,
        Cliff = 2,
        NonBuildRock = 3,
        InfantryRock = 4,
        Dustbowl = 5,
        MapEdge = 6,
        Ramp = 7
    }

    public static class MapTerrainRules
    {
        public static MapTerrainType FromId(int id)
        {
            return id switch
            {
                0 => MapTerrainType.Sand,
                1 => MapTerrainType.Rock,
                2 => MapTerrainType.Cliff,
                3 => MapTerrainType.NonBuildRock,
                4 => MapTerrainType.InfantryRock,
                5 => MapTerrainType.Dustbowl,
                6 => MapTerrainType.MapEdge,
                7 => MapTerrainType.Ramp,
                _ => MapTerrainType.Unknown
            };
        }

        public static bool IsBuildable(int id)
        {
            return FromId(id) == MapTerrainType.Rock;
        }

        public static bool IsGroundPassable(int id)
        {
            return FromId(id) switch
            {
                MapTerrainType.Sand => true,
                MapTerrainType.Rock => true,
                MapTerrainType.NonBuildRock => true,
                MapTerrainType.Dustbowl => true,
                MapTerrainType.Ramp => true,
                _ => false
            };
        }
    }
}
