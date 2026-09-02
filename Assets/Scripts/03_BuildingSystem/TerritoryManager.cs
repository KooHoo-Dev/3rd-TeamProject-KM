using System.Collections.Generic;
using BlueMarble;
using UnityEngine;

namespace Epic3
{
    public class TerritoryManager : MonoBehaviour
    {
        public Dictionary<int, TileState> tileStates = new Dictionary<int, TileState>();

        public int Coin = 100000;
        
        // 싱글톤으로 선언된 BlueMarbleWorld를 가져옵니다.
        private BlueMarbleWorld worldBoard => BlueMarbleWorld.Instance; 

        public TileState GetTileState(int tileIndex)
        {
            if (tileStates.ContainsKey(tileIndex) == false)
            {
                tileStates.Add(tileIndex, new TileState());
            }
            return tileStates[tileIndex];
        }

        // 지어진 건물, 랜드마크의 가격까지 포함시켜야함
        public int CalculateTotalAssets(string ownerId)
        {
            int totalAssets = 0;
            foreach (Tile tile in worldBoard.Board.GetOccupiedTiles(ownerId))
            {
                if (tile.State.HasLandMark) totalAssets += tile.SpaceDef.Price * 6; 
                else if (tile.State.HasBuilding) totalAssets += tile.SpaceDef.Price * 3;
                else totalAssets = tile.SpaceDef.Price;
            }
            return  totalAssets;
        }
        

        public void PurchaseTile(Tile tile, int tilePrice)
        {
            if (Coin < tilePrice)
            {
                Debug.Log($"[Tile] :{tilePrice}원 [Coin] :{Coin} 돈 부족");
                return;
            }

            Coin -= tilePrice;
            Debug.Log($"[Tile] 구매 완료. 남은 돈: {Coin}");
        }

        public void PurchaseBuilding(Tile tile, int tilePrice)
        {
            if (Coin < tilePrice * 2)
            {
                Debug.Log($"[Building]: {tilePrice*2}원 [Coin]: {Coin} 부족");
                return;
            }
            
            Coin -= tilePrice*2;
            Debug.Log($"[Building] 구매 완료. 남은 돈: {Coin}");
        }

        public void PurchaseLandMark(Tile tile, int tilePrice)
        {
            if (Coin < tilePrice * 3)
            {
                Debug.Log($"[LandMark]: {tilePrice*2}원 [Coin]: {Coin} 부족");
                return;
            }
            Coin -= tilePrice*3;
            Debug.Log($"[LandMark] 구매 완료. 남은 돈: {Coin}");
        }

        public void PayToll(string myPlayerId, Tile tile)
        {
            Tile currentTile = worldBoard.Board.GetTile(tile.transform.position);            
            
            int tilePrice = currentTile.SpaceDef.Price;
            
            if (currentTile.State.IsOccupied == false) return;
            if (currentTile.State.OwnerId == myPlayerId) return;
            
            int tollAmount = tilePrice;

            if (currentTile.State.HasBuilding) tollAmount += tilePrice * 2;
            if (currentTile.State.HasLandMark) tollAmount += tilePrice * 3;

            if (Coin < tollAmount)
            {
                Debug.Log($"[Toll] {tollAmount}원 필요. 자금 부족! 급매/파산 처리 필요.");
                Coin = 0; 
            }
            else
            {
                Coin -= tollAmount;
                Debug.Log($"[Toll] 통행료 {tollAmount}원 지불. 남은 돈: {Coin}");
            }
        }

        // 약탈 시 소유권 변경 (랜드마크 방어 포함)
        public void StolenTerritory(string loserId, string winnerId, int tileIndex)
        {
            TileState currentTileState = GetTileState(tileIndex);

            // 랜드마크가 지어져 있으면 약탈 불가
            if (currentTileState.HasLandMark) return;
            
            if (currentTileState.OwnerId == loserId)
            {
                currentTileState.OwnerId = winnerId;
                Debug.Log($"{tileIndex}번 타일의 소유권이 {winnerId}에게 넘어갔습니다.");
            }
        }
    }
}