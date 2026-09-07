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

            await Client.AnnouncePlayerActivity(PlayerActivityType.Combat);

            try
            {
                FightUI fightUI = FightUI.Instance;
                bool wantsFight = fightUI != null && await fightUI.ShowAsync(
                    FightUIMode.Challenge,
                    playerId,
                    state.OwnerId);

                if (!wantsFight)
                {
                    await PayTollAsync(playerId, state.OwnerId, tile.GetTollAmount());
                    return;
                }

                CombatOutcome outcome = await CombatSystem.Instance.StartCombat(playerId, tile);

                if (outcome == CombatOutcome.AttackerWin)
                {
                    await ChangeOwnerAsync(tile, playerId);
                    return;
                }

                await PayTollAsync(playerId, state.OwnerId, tile.GetTollAmount());
            }
            finally
            {
                FightUI.Instance?.Hide();
                await Client.AnnouncePlayerActivity(PlayerActivityType.None);
            }
        }

        private async UniTask PayTollAsync(string payerId, string ownerId, int tollAmount)
        {
            if (Economy.TryGetGold(payerId, out int currentGold) == false) return;

            int paidAmount = Mathf.Min(currentGold, tollAmount);
            if (paidAmount <= 0) return;

            // TODO: 추후 파산 시스템 연결
            await Economy.LoseGoldTo(payerId, paidAmount);
            await Economy.GainGoldTo(ownerId, paidAmount);
        }

        private async UniTask RequestPurchaseAsync(string playerId, Tile tile, int currentGold)
        {
            TileState state = tile.State;
            purchaseWaiter = new UniTaskCompletionSource<TerritoryBill>();

            await Client.AnnouncePlayerActivity(PlayerActivityType.TerritoryPurchase);

            TerritoryBill bill;

            try
            {
                OnPurchaseRequested?.Invoke(tile, currentGold);
                bill = await purchaseWaiter.Task;
            }
            finally
            {
                purchaseWaiter = null;
                await Client.AnnouncePlayerActivity(PlayerActivityType.None);
            }

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
