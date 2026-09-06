using System;
using System.Collections.Generic;
using BlueMarble.Server;
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
            // 결투 시스템 어쩌구저쩌구... 암튼 연결해야 함
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
        }
    }
}
