using JoyGodot.Assets.Scripts.Entities.AI;

namespace JoyGodot.Assets.Scripts.Helpers
{
    public static class SectorStringConvertor
    {
        public static string ConvertSector(Sector sector)
        {
            switch(sector)
            {
                case Sector.Centre:
                    return "centre";

                case Sector.East:
                    return "east";

                case Sector.North:
                    return "north";

                case Sector.NorthEast:
                    return "north east";

                case Sector.NorthWest:
                    return "north west";

                case Sector.South:
                    return "south";

                case Sector.SouthEast:
                    return "south east";

                case Sector.SouthWest:
                    return "south west";

                case Sector.West:
                    return "west";

                default:
                    return "UNKNOWN SECTOR";
            }
        }
    }
}
