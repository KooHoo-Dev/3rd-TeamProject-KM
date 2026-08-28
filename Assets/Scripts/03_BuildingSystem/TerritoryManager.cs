using System.Collections.Generic;
using BlueMarble;
using UnityEngine;

namespace Epic3
{
    public class TerritorygManager : MonoBehaviour
    {
        // 경제 부분은 하드코딩 해놨는데 경제 시스템 만들어지면 교체 하겠습니다.(강민)
        
        public Dictionary<int, TileProperty> tileStates = new Dictionary<int, TileProperty>();

        public int Coin = 500;

        public int TilePrice = 300;
        public int BuildingPrice = 200;
        public int LandMarkPrice = 500;
        
        public BlueMarbleWorld worldBoard => BlueMarbleWorld.Instance; 

        public void GetTileState(IPlayer player)
        {
            // 이 함수가 하는 역할은 player가 위치한 곳의 타일 불러오기.
            Tile currentTile = worldBoard.Board.GetTile(player.Position);
            // TileProperty를 Tile 안에 넣기
        }
        
        /*   public void PurchaseTile(int ownerId, int tileIndex)
        {
            TileProperty currentTileState = GetTileState(tileIndex);
            
            if(currentTileState.IsOwned)
            {
                Debug.Log("이미 주인이 있습니다. 대전 신청으로 넘어갑니다");

                return;
            }

            if (Coin < 300)
            {
                Debug.Log($"[Tile] :300원 [Coin] :{Coin} 돈 부족");
                return;
            }
            
            Coin -= 300;
            Debug.Log($"[Tile] :300원 [Coin] :{Coin}");
            tileStates[tileIndex].OwnerId = ownerId;
        }

        // 순차적으로 이루어져야 하는데
        // 땅을 사고 나서 건물을 사야됨....
        
        public void PurchaseBuilding(int ownerId, int tileIndex)
        {
            TileProperty currentTileState = GetTileState(tileIndex);
            if ((currentTileState.OwnerId == ownerId) && currentTileState.HasBuilding == false)
            {
                if (Coin < 100)
                {
                    Debug.Log($"[Building] :100원 [Coin] :{Coin} 돈 부족");
                    return;
                }
                Coin -= 100;
                Debug.Log($"[Building] :100원 [Coin] :{Coin}");
                tileStates[tileIndex].HasBuilding = true;
            }
        }

        public void PurchaseLandMark(int ownerId, int tileIndex)
        {
            // 처음 들를때 되면 안되고, 한번 떠나가고 다시 돌아왔을 때 건물까지 지어져 있어야 랜드마크 건설 가능
            // 그럴려면 일단 생각나는건 이동할때 전 타일에게 정보를 넘겨주는 건데...
            
            TileProperty currentTileState = GetTileState(tileIndex);
            if ((currentTileState.OwnerId == ownerId) && currentTileState.HasBuilding)
            {
                if (Coin < 200)
                {
                    Debug.Log($"[LandMark] :200원 [Coin] :{Coin} 돈 부족");
                    return;
                }
                Coin -= 200;
                Debug.Log($"[LandMark] :200원 [Coin] :{Coin}");
                tileStates[tileIndex].HasLandMark = true;
            }
        }

        public void PayAToll(int myPlayerId, int tileIndex)
        {
            TileProperty currentTileState = GetTileState(tileIndex);
            
            if (currentTileState.IsOwned == false) return;
            if (currentTileState.OwnerId == myPlayerId) return;
            
            // 통행료 누적 계산
            int tollAmount = TilePrice;

            if (currentTileState.HasBuilding)
            {
                tollAmount += BuildingPrice;
            }
            if (currentTileState.HasLandMark)
            {
                // 기획서 밸런스에 따라 랜드마크 가격을 합산할지 별도 배율을 적용할지 결정
                tollAmount += LandMarkPrice;
            }

            if (Coin < tollAmount)
            {
                // 나중에 구현할 파산 및 급매 절차로 연결될 부분
                Debug.Log($"[Toll] {tollAmount}원 필요. [Coin] {Coin}원. 자금 부족으로 급매/파산 처리 필요");
                Coin = 0; 
            }
            else
            {
                Coin -= tollAmount;
                Debug.Log($"[Toll] 통행료 {tollAmount}원 지불. 남은 돈: {Coin}");
            }
        }

        public void StolenTerritory(int loserId,int winnerId, int tileIndex)
        {
            TileProperty currentTileState = GetTileState(tileIndex);

            if (currentTileState.HasLandMark) return;
            
                
            if(currentTileState.OwnerId == loserId)
                currentTileState.OwnerId = winnerId;
        }*/
        
    }
}