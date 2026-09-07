using System;
using System.Collections.Generic;
using BlueMarble.Server;
using Combat;
using Cysharp.Threading.Tasks;
using Jay;
using UnityEngine;

public readonly struct TerritoryBill
{
    public bool IsBuyTile { get; }
    public bool IsBuyBuilding { get; }
    public bool IsBuyLandMark { get; }
    
    public TerritoryBill (bool isBuyTile, bool isBuyBuilding, bool isBuyLandMark)
    {
        IsBuyTile = isBuyTile;
        IsBuyBuilding = isBuyBuilding;
        IsBuyLandMark = isBuyLandMark;
    }
}

namespace BlueMarble
{
    public class TerritoryManager : SingletonBase<TerritoryManager>
    {
        private KeyValuePair<int, UniTaskCompletionSource> updateWaiter;
        private UniTaskCompletionSource<TerritoryBill> purchaseWaiter;

        public event Action<Tile, int> OnPurchaseRequested;
        public event Action OnUpdated;
        
        private RoomClient Client => RoomClient.Instance;
        private BlueMarbleWorld World => BlueMarbleWorld.Instance;
        private EconomyManager Economy => EconomyManager.Instance;
        
        protected override void OnInitialize()
        {
            Client.OnTerritoryUpdated += HandleTerritoryUpdated;
        }

        protected override void OnDispose()
        {
            Client.OnTerritoryUpdated -= HandleTerritoryUpdated;
        }
        
        #region RESOLVE
        
        public async UniTask ResolveAsync(string playerId, Tile tile)
        {
            TileState state = tile.State;

            // 점령되지 않은 땅이라면
            if (state.IsOccupied == false)
            {
                await HandleUnoccupiedAsync(playerId, tile);
                return;
            }

            // 여기서부턴 점령된 땅
            // 해당 땅의 점령자가 자신이라면
            if (state.OwnerId == playerId)
            {
                await HandleOwnedAsync(playerId, tile);
                return;
            }
            
            // 해당 땅의 점령자가 타인이라면
            await HandleOpponentAsync(playerId, tile);
        }
        
        private async UniTask HandleUnoccupiedAsync(string playerId, Tile tile)
        {
            int minPrice = tile.GetMinimumPurchasePrice();

            if (Economy.TryGetGold(playerId, out int currentGold) == false) return;
            if (currentGold < minPrice) return;

            await RequestPurchaseAsync(playerId, tile, currentGold);
        }
        
        private async UniTask HandleOwnedAsync(string playerId, Tile tile)
        {
            TileState state = tile.State;

            // 랜드마크가 지어진 땅은 아무런 효과 없음
            if (state.HasLandMark) return;
            
            int requiredGold = tile.GetMinimumPurchasePrice();
            
            if (Economy.TryGetGold(playerId, out int currentGold) == false) return;
            if (currentGold < requiredGold) return;
            
            await RequestPurchaseAsync(playerId, tile, currentGold);
        }

        private async UniTask HandleOpponentAsync(string playerId, Tile tile)
        {
            TileState state = tile.State;

            int tollAmount = tile.GetTollAmount();
            
            if (Economy.TryGetGold(playerId, out int currentGold) == false) return;
            
            // 이 부분은 추후 수정 좀 해야함
            int paidAmount = Mathf.Min(currentGold, tollAmount);
            if (paidAmount > 0)
            {
                // TODO: 추후 파산 시스템 연결
                await Economy.LoseGoldTo(playerId, paidAmount);
                await Economy.GainGoldTo(state.OwnerId, paidAmount);
            }

            if (state.HasLandMark) return;
            
            // TODO: 전투 시스템 연결
            // 1. 전투 UI를 활성화, UI의 결정이 끝날 때 까지 대기
            //    UI의 결정은 반환값을 bool로 받아오면 될듯?
            // 2. 만약 true라면 상대방에게 해당 의사를 전달(서버를 통해)
            //    해당 의사를 전달 받은 상대방에게 승낙/거절 UI를 띄워주고, 해당 결정을 기다림
            // 3. 만약 승낙했다면, 그대로 전투 페이즈 돌입
            // 3-1. 만약 거절했다면, 상대방 클라이언트에서 코인 토스(일단 구현은 연출 없이 결과만 반반으로 따져서 진행)
            //      코인토스에서 앞면이 나왔다면, 거절 성공 / 뒷면이 나왔다면 강제로 전투 요청을 승낙
            // 4. 전투의 승자가 누구인지 반환받고(이거도 전투 시작 함수에서 반환값으로 받아오면?)
            // 5. 해당 승자가 자신이라면, 해당 영토의 소유권을 넘겨받는다
            // 5-1. 해당 승자가 상대방이라면, 자신은 영토의 통행료 만큼을 대상에게 지급한다
            // 5-1-1. 만약 지불할 금액이 부족하다면 가능한 만큼 지불하고, 파산한다 (게임 오버)
            bool isAttackerWin = await CombatSystem.Instance.StartCombat(playerId, tile);

            if (isAttackerWin)
            {
                Debug.Log("공격자 승리");
                // TODO: 인수 금액 지불 및 ChangeOwnerAsync 호출
            }
            else
            {
                Debug.Log("공격자 패배");
                // TODO: 패널티 로직 처리
            }
        }

        private async UniTask RequestPurchaseAsync(string playerId, Tile tile, int currentGold)
        {
            TileState state = tile.State;
            purchaseWaiter = new UniTaskCompletionSource<TerritoryBill>();
            
            OnPurchaseRequested?.Invoke(tile, currentGold);

            TerritoryBill bill = await purchaseWaiter.Task;
            purchaseWaiter = null;

            // 점령되지 않은 땅의 경우, IsBuyTile이 false이면 아무것도 구매하지 않겠다는 뜻
            if (state.IsOccupied == false && bill.IsBuyTile == false) return;

            int totalPrice = tile.GetPurchasePrice(bill);
            
            // 서버에게 금액 차감 요청 전달 후, 처리까지 대기
            await Economy.LoseGoldTo(playerId, totalPrice);
            
            // 구매 정보대로 영토 상태 업데이트
            await UpdateTerritoryAsync(playerId, tile, bill);
        }
        
        #endregion
        
        #region PUBLIC
        
        public UniTask ChangeOwnerAsync(Tile tile, string ownerId)
        {
            return UpdateTerritoryAsync(
                ownerId,
                tile,
                new TerritoryBill(true, tile.State.HasBuilding, tile.State.HasLandMark));
        }

        public void SubmitPurchase(bool isBuyTile, bool isBuyBuilding, bool isBuyLandMark)
        {
            if (purchaseWaiter == null) return;
            
            TerritoryBill bill = new(isBuyTile, isBuyBuilding, isBuyLandMark);
            purchaseWaiter.TrySetResult(bill);
        }
        
        #endregion
        
        private async UniTask UpdateTerritoryAsync(string ownerId, Tile tile, TerritoryBill bill)
        {
            int tileId = World.Board.GetTileIndex(tile);
            
            bool hasBuilding = tile.State.HasBuilding || bill.IsBuyBuilding;
            bool hasLandMark = tile.State.HasLandMark || bill.IsBuyLandMark;
            
            UniTaskCompletionSource waiter = new();
            updateWaiter = new KeyValuePair<int, UniTaskCompletionSource>(tileId, waiter);

            try
            {
                await Client.RequestUpdateTerritory(ownerId, tileId, hasBuilding, hasLandMark);
                await waiter.Task;
            }
            finally
            {
                updateWaiter = new KeyValuePair<int, UniTaskCompletionSource>(-1, null);
            }
        }

        private void HandleTerritoryUpdated(UpdateTerritoryMessage msg)
        {
            if (World.Board.TryGetTile(msg.TileId, out Tile tile) == false) return;

            tile.State.OwnerId = msg.OwnerId;
            tile.State.HasBuilding = msg.HasBuilding;
            tile.State.HasLandMark = msg.HasLandMark;
            
            // TODO: 건물 생성 혹은 소유주 변경 연출 등등 처리
            
            if (msg.TileId == updateWaiter.Key)
                updateWaiter.Value.TrySetResult();
            
            OnUpdated?.Invoke();
        }
    }
}
