using System.Collections.Generic;
using UnityEngine;

namespace Epic3
{
    public class BuildingManager : MonoBehaviour
    {
        public Dictionary<int, TileProperty> tileStates = new Dictionary<int, TileProperty>();

        public int Coin = 500;
        
        // 타일을 구매하고 싶다.
        // 내가 어떤 땅에 있는지 확인부터 해야함.(근데 이건 나중에)
        // 구매 버튼을 누르면 해당 땅의 상태부터 확인
        // 주인이 있는지 확인하고 주인이 있으면 대전 신청으로 넘어감
        // 주인이 없으면 영토 구매.
        // 영토 구매후 건물을 지을지 확인.
        // 주인이 나고 건물이 지어져있으면 랜드마크를 세울지

        // 특정 인덱스의 상태를 받아오는 함수
        public TileProperty GetTileState(int tileIndex)
        {
            if(tileStates.ContainsKey(tileIndex) == false) tileStates.Add(tileIndex, new TileProperty());

            return tileStates[tileIndex];
        }

        public void PurchaseTile(int ownerId, int tileIndex)
        {
            TileProperty currentTileState = GetTileState(tileIndex);
            
            if(currentTileState.IsOwned)
            {
                Debug.Log("이미 주인이 있습니다. 대전 신청으로 넘어갑니다");

                return;
            }

            // 경제 시스템 들어오면 교체
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
            else Debug.Log("이미 주인이 있습니다. 대전 신청으로 넘어갑니다");
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
            else Debug.Log("이미 주인이 있습니다. 대전 신청으로 넘어갑니다");
        }
    }
}