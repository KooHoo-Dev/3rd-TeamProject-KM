   
using System.Collections.Generic;



namespace BlueMarble
{
    public enum SpaceCategory
    {
        Property,   // 일반 영토
        Start,      // 출발지
        Island,     // 무인도
        WorldTour,  // 세계 여행
        GoldenKey,  // 황금 열쇠
        Shop,       // 상점 (E5-T2에서 이미 구현)
        Treasure    // 보물창고 (E5-T2에서 이미 구현)
    }
    
    
    public static class SpaceCategoryMap
    {
        // spaces.tsv의 Id 기준. BoardSpaceDef.cs를 건드리지 않기 위한 자체 매핑 테이블.
        private static readonly Dictionary<int, SpaceCategory> categoryById = new()
        {
            { 0, SpaceCategory.Start },
            { 7, SpaceCategory.Island },
            { 14, SpaceCategory.Treasure },
            { 21, SpaceCategory.WorldTour },
            { 28, SpaceCategory.GoldenKey },
            { 29, SpaceCategory.Shop },
        };

        public static SpaceCategory GetCategory(int spaceId)
        {
            if (categoryById.TryGetValue(spaceId, out SpaceCategory category))
                return category;

            return SpaceCategory.Property; // 매핑에 없으면 일반 영토로 취급
        }
    }
    
}