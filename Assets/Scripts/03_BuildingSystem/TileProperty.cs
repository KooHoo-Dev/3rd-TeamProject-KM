namespace Epic3
{
    public class TileProperty
    {
        public string OwnerId { get; set; } = string.Empty; // -1이면 주인 없음
        
        public bool HasBuilding { get; set; } = false;
        public bool HasLandMark { get; set; } = false;
        
        public bool IsOwned => !string.IsNullOrEmpty(OwnerId);
    }
}