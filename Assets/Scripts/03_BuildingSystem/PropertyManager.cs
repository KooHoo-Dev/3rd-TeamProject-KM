using BlueMarble;
using Jay;
using UnityEngine;

namespace Epic3
{
    public class PropertyManager : SingletonBase<PropertyManager>
    {
        public TerritoryManager territoryManager;
        private BlueMarbleWorld worldBoard => BlueMarbleWorld.Instance;
        
        // [SerializeField] TerritoryUI territoryUI;

        public void RequestPurchaseTile(string ownerId, Tile tile)
        {
            int tileIndex = GetTileIndex(tile);

            if (tileIndex == -1)
            {
                Debug.LogError("타일 인덱스를 찾을 수 없습니다.");
                return;
            }

            TileState currentTileState = territoryManager.GetTileState(tileIndex);

            if (currentTileState.IsOccupied == false)
            {
                Debug.Log("빈 땅입니다. 영토 구매 UI를 띄우세요.");
                territoryManager.PurchaseTile(ownerId, tileIndex);
            }
            else if (currentTileState.OwnerId != ownerId)
            {
                Debug.Log($"남의 땅입니다 (소유자: {currentTileState.OwnerId}). 통행료 정산 및 야차 신청으로 넘어갑니다.");
                territoryManager.PayToll(ownerId, tileIndex);
            }
            else 
            {
                Debug.Log("내 땅입니다. 건물 또는 랜드마크 건설 UI를 띄우세요.");
                territoryManager.PurchaseLandMark(ownerId, tileIndex);
                territoryManager.PurchaseBuilding(ownerId, tileIndex); 
            }
        }

        private int GetTileIndex(Tile targetTile)
        {
            if (worldBoard.IsBoardReady == false) return -1;

            int maxIndex = worldBoard.Board.MaxTileIndex;

            worldBoard.Board.TryGetTileIndex(targetTile.transform.position, out int index);
            
            for (int i = 0; i <= maxIndex; i++)
            {
                if (worldBoard.Board.GetTile(i) == targetTile)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}