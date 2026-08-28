namespace Epic3
{
    public class TileProperty
    {
        public int OwnerId { get; set; } = -1; // -1이면 주인 없음
        
        public bool HasBuilding { get; set; } = false;
        public bool HasLandMark { get; set; } = false;
        
        public bool IsOwned => OwnerId != -1;
    }
}