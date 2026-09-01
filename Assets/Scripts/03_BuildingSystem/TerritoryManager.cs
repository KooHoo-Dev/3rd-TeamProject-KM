using System.Collections.Generic;
using BlueMarble;
using UnityEngine;

namespace Epic3
{
    public class TerritoryManager : MonoBehaviour
    {
        public Dictionary<int, TileState> tileStates = new Dictionary<int, TileState>();

        public int Coin = 500;
        public int TilePrice = 300;
        public int BuildingPrice = 200;
        public int LandMarkPrice = 500;
        
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

        public void OnPlayerArrived(IPlayer player)
        {
            if (!worldBoard.IsBoardReady) return;

            if (worldBoard.Board.TryGetTileIndex(player.Position, out int tileIndex) == false)
            {
                Debug.LogError("플레이어가 보드 밖에 있습니다!");
                return;
            }

            TileState currentTileState = GetTileState(tileIndex);
            Tile currentTile = worldBoard.Board.GetTile(tileIndex);

            if (currentTile.Def.Type != TileType.Property)
            {
                Debug.Log($"이곳은 일반 영토가 아닌 특수 타일({currentTile.Def.Type})입니다.");
                return;
            }

            string playerId = player.Id;

            if (currentTileState.IsOccupied == false)
            {
                Debug.Log("빈 땅입니다. 영토 구매 UI를 띄우세요.");
                PurchaseTile(playerId, tileIndex);
            }
            else if (currentTileState.OwnerId != playerId)
            {
                Debug.Log($"남의 땅입니다 (소유자: {currentTileState.OwnerId}). 통행료 정산 및 야차 신청으로 넘어갑니다.");
                PayToll(playerId, tileIndex);
            }
            else 
            {
                Debug.Log("내 땅입니다. 건물 또는 랜드마크 건설 UI를 띄우세요.");
                PurchaseLandMark(playerId, tileIndex);
                PurchaseBuilding(playerId, tileIndex); 
            }
        }

        // 지어진 건물, 랜드마크의 가격까지 포함시켜야함
        public int CalculateTotalAssets(string ownerId)
        {
            int totalAssets = 0;
            TileState currentState = new TileState();
            for (int i = 0; i < worldBoard.Board.MaxTileIndex; i++)
            {
                if (currentState.OwnerId == ownerId)
                {
                    Tile ownerTile = worldBoard.Board.GetTile(i);
                    totalAssets = ownerTile.SpaceDef.Price;
                }
            }
            
            return  totalAssets;
        }
        
        public void PurchaseTile(string ownerId, int tileIndex)
        {
            TileState currentTileState = GetTileState(tileIndex);
            
            if(currentTileState.IsOccupied)
            {
                Debug.Log("이미 주인이 있습니다. 대전 신청으로 넘어갑니다");
                return;
            }

            if (Coin < TilePrice)
            {
                Debug.Log($"[Tile] :{TilePrice}원 [Coin] :{Coin} 돈 부족");
                return;
            }
            
            Coin -= TilePrice;
            Debug.Log($"[Tile] 구매 완료. 남은 돈: {Coin}");
            currentTileState.OwnerId = ownerId;
        }

        public void PurchaseBuilding(string ownerId, int tileIndex)
        {
            TileState currentTileState = GetTileState(tileIndex);
            
            if (currentTileState.OwnerId == ownerId && currentTileState.HasBuilding == false)
            {
                if (Coin < BuildingPrice)
                {
                    Debug.Log($"[Building] :{BuildingPrice}원 [Coin] :{Coin} 돈 부족");
                    return;
                }
                Coin -= BuildingPrice;
                currentTileState.HasBuilding = true;
                Debug.Log($"건물 건설 완료. 남은 돈: {Coin}");
            }
        }

        public void PurchaseLandMark(string ownerId, int tileIndex)
        {
            TileState currentTileState = GetTileState(tileIndex);
            
            if (currentTileState.OwnerId == ownerId && currentTileState.HasBuilding && currentTileState.HasLandMark == false)
            {
                if (Coin < LandMarkPrice)
                {
                    Debug.Log($"[LandMark] :{LandMarkPrice}원 [Coin] :{Coin} 돈 부족");
                    return;
                }
                Coin -= LandMarkPrice;
                currentTileState.HasLandMark = true;
                Debug.Log($"랜드마크 건설 완료. 남은 돈: {Coin}");
            }
        }

        public void PayToll(string myPlayerId, int tileIndex)
        {
            TileState currentTileState = GetTileState(tileIndex);
            
            if (currentTileState.IsOccupied == false) return;
            if (currentTileState.OwnerId == myPlayerId) return;
            
            int tollAmount = TilePrice;

            if (currentTileState.HasBuilding) tollAmount += BuildingPrice;
            if (currentTileState.HasLandMark) tollAmount += LandMarkPrice;

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