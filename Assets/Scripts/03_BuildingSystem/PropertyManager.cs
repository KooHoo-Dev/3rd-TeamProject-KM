using System;
using BlueMarble;
using Jay;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Epic3
{
    public class PropertyManager : SingletonBase<PropertyManager>
    {
        public TerritoryManager territoryManager;
        private BlueMarbleWorld worldBoard => BlueMarbleWorld.Instance;

        // UI에서 구독할 이벤트 함수
        public event Action<TileState, int> OnShowPurchaseUI;

        // UI에서 체크한 값을 받아온다
        private UniTaskCompletionSource<(bool buyTile, bool buyBuilding, bool buyLandmark)> tcs;

        public async UniTask RequestPurchase(string ownerId, Tile tile)
        {
            Tile targetTile = worldBoard.Board.GetTile(tile.transform.position);
            int tilePrice = targetTile.SpaceDef.Price;
            TileState state = targetTile.State;

            if (state.IsOccupied && state.OwnerId != ownerId) return;
            if (state.IsOccupied && state.HasLandMark) return;
            
            tcs = new UniTaskCompletionSource<(bool, bool, bool)>();

            // UI 팝업을 띄우라고 이벤트 알림
            OnShowPurchaseUI?.Invoke(state, tilePrice);

            // UI 쪽에서 스위치를 누를 때까지 여기서 대기
            var decision = await tcs.Task;

            // UI에서 넘어온 결과에 따라 건설 진행
            if (decision.buyTile) territoryManager.PurchaseTile(targetTile, tilePrice);
            if (decision.buyBuilding) territoryManager.PurchaseBuilding(targetTile, tilePrice);
            if (decision.buyLandmark) territoryManager.PurchaseLandMark(targetTile, tilePrice);
        }

        // 구매/취소 버튼을 눌렀을 때 호출할 함수
        public void SubmitPurchaseDecision(bool buyTile, bool buyBuilding, bool buyLandmark)
        {
            if (tcs != null)
            {
                tcs.TrySetResult((buyTile, buyBuilding, buyLandmark));
                tcs = null;
            }
        }
    }
}