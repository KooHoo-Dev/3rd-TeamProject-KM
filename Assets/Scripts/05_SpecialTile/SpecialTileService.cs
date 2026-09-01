using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BlueMarble
{
    /// <summary>
    /// E5 특수 타일 효과를 처리하는 서비스.
    /// TurnManager의 ResolveTileLanding / ResolveTilePassing에서 호출되는 것을 가정한다.
    ///
    /// 상점(Shop) 관련 흐름은 "상점이 실제로 닫힐 때까지 기다려야" 하므로,
    /// 상점을 거칠 가능성이 있는 메서드는 전부 UniTask를 반환하도록 바꿨다.
    /// (HandleTileLanding, HandleTilePassing, HandleArriveShop, HandleShopPass,
    ///  HandleWorldTourMove, HandleArriveGoldenKey, ApplyForceMove)
    /// </summary>
    public class SpecialTileService
    {
        private readonly IGold gold;
        private readonly ITurnSkipRegistry turnSkipRegistry;
        private readonly IBoardTeleporter boardTeleporter;
        private readonly IShopOpener shopOpener;
        

        public SpecialTileService(IGold gold, ITurnSkipRegistry turnSkipRegistry,
            IBoardTeleporter boardTeleporter, IShopOpener shopOpener)
        {
            this.gold = gold;
            this.turnSkipRegistry = turnSkipRegistry;
            this.boardTeleporter = boardTeleporter;
            this.shopOpener = shopOpener;
        }

        public async UniTask HandleTileLanding(string playerId, int spaceId)
        {
            SpaceCategory category = SpaceCategoryMap.GetCategory(spaceId);

            switch (category)
            {
                case SpaceCategory.Start:
                    HandleArriveStart(playerId);
                    break;

                case SpaceCategory.Island:
                    HandleArriveIsland(playerId);
                    break;

                case SpaceCategory.GoldenKey:
                    await HandleArriveGoldenKey(playerId);
                    break;

                case SpaceCategory.Shop:
                    await HandleArriveShop(playerId);
                    break;

                default:
                    break;
            }
        }

        public async UniTask HandleTilePassing(string playerId, int spaceId)
        {
            SpaceCategory category = SpaceCategoryMap.GetCategory(spaceId);

            if (category == SpaceCategory.Start)
            {
                HandleArriveStart(playerId);
            }
            else if (category == SpaceCategory.Shop)
            {
                // 지나치기만 해도 상점 칸에서 멈춰서 열고, 닫힐 때까지 기다린 뒤 이동을 이어간다.
                await HandleArriveShop(playerId);
            }
        }

        // ... HandleArriveStart, HandleArriveIsland, DrawGoldenKeyCard,
        //     ApplyGoldChange, ApplyGrantItem, GetSpaceId 는 기존과 동일

        private async UniTask HandleArriveShop(string playerId)
        {
            Debug.Log($"[SpecialTileService] {playerId}: 상점 도착/통과 -> 상점 오픈 요청");
            await shopOpener.OpenShopAsync(playerId);
            Debug.Log($"[SpecialTileService] {playerId}: 상점 닫힘 -> 이동 재개");
        }

        /// <summary>
        /// 세계 여행: 플레이어가 선택한 목적지로 즉시 이동.
        /// 이동 중 출발지 통과/도착 시 급여를 지급하고, 도착한 칸의 효과를 이어서 실행한다.
        /// </summary>
        public async UniTask HandleWorldTourMove(string playerId, int targetTileIndex)
        {
            bool passedStart = boardTeleporter.TeleportTo(playerId, targetTileIndex);
            bool passedShop = boardTeleporter.TeleportTo(playerId, targetTileIndex);

            if (passedStart)
            {
                HandleStartPass(playerId);
            }

            if (passedShop)
            {
                await HandleArriveShop(playerId);
            }

            Debug.Log($"[SpecialTileService] {playerId}: 세계 여행으로 {targetTileIndex}번 칸 이동 완료");

            int targetSpaceId = GetSpaceId(targetTileIndex);

            if (SpaceCategoryMap.GetCategory(targetSpaceId) != SpaceCategory.Start)
            {
                await HandleTileLanding(playerId, targetSpaceId);
            }
        }


        /// <summary>텔레포트(세계 여행, 황금 열쇠 강제 이동) 중 출발지를 지나갔거나 도착했을 때 전용 호출.</summary>
        public void HandleStartPass(string playerId)
        {
            HandleArriveStart(playerId);
        }


        public async UniTask HandleShopPass(string playerId)
        {
            await HandleArriveShop(playerId);
        }



        /// <summary>
        /// 출발지 도착 or 통과 할때 주는 급여
        /// </summary>
        /// <param name="playerId"></param>
        private void HandleArriveStart(string playerId)
        {
            gold.AddGold(SalaryConfig.PassAmount);
            Debug.Log($"[SpecialTileService] {playerId}: 출발지 급여 {SalaryConfig.PassAmount} 지급");
        }


        /// <summary>
        /// 무인도 도착
        /// </summary>
        /// <param name="playerId"></param>
        private void HandleArriveIsland(string playerId)
        {
            turnSkipRegistry.MarkSkipNextTurn(playerId);
            Debug.Log($"[SpecialTileService] {playerId}: 무인도 도착 -> 다음 턴 스킵 예약");
        }


        /// <summary>
        /// 황금카드 도착
        /// </summary>
        /// <param name="playerId"></param>
        private async UniTask HandleArriveGoldenKey(string playerId)
        {
            GoldenKeyCard card = DrawGoldenKeyCard();
            Debug.Log($"[SpecialTileService] {playerId}: 황금 열쇠 카드 뽑음 -> {card.EffectType}");

            switch (card.EffectType)
            {
                case GoldenKeyEffectType.GoldChange:
                    ApplyGoldChange(playerId, card.GoldAmount);
                    break;

                case GoldenKeyEffectType.GrantItem:
                    ApplyGrantItem(playerId, card.ItemName);
                    break;

                case GoldenKeyEffectType.ForceMove:
                    // 강제 이동 목적지가 상점일 수도 있으므로 끝까지 기다린다.
                    await ApplyForceMove(playerId, card.TargetTileIndex);
                    break;
            }
        }

        /// <summary>
        /// 황금카드 뽑기
        /// </summary>
        /// <returns></returns>
        private GoldenKeyCard DrawGoldenKeyCard()
        {
            int index = Random.Range(0, GoldenKeyDeck.Cards.Count);
            return GoldenKeyDeck.Cards[index];
        }

        /// <summary>
        /// 돈 +-
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="amount"></param>
        private void ApplyGoldChange(string playerId, int amount)
        {
            if (amount >= 0)
            {
                gold.AddGold(amount);
                Debug.Log($"[SpecialTileService] {playerId}: 황금 열쇠 자금 획득 {amount}");
            }
            else
            {
                gold.TrySpend(-amount);
                Debug.Log($"[SpecialTileService] {playerId}: 황금 열쇠 자금 손실 {-amount}");
            }
        }


        /// <summary>
        /// 아이템
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="itemName"></param>
        private void ApplyGrantItem(string playerId, string itemName)
        {
            // TODO: E8 InventoryManager 연결 예정. 지금은 로그만 출력.
            Debug.Log($"[SpecialTileService] {playerId}: 황금 열쇠 장비 획득(임시) - {itemName}");
        }


        /// <summary>
        /// 황금열쇠 강제 이동
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="targetTileIndex"></param>
        private async UniTask ApplyForceMove(string playerId, int targetTileIndex)
        {
            bool passedStart = boardTeleporter.TeleportTo(playerId, targetTileIndex);

            if (passedStart)
            {
                HandleStartPass(playerId);
            }

            Debug.Log($"[SpecialTileService] {playerId}: 황금 열쇠 강제 이동 -> {targetTileIndex}번 칸");

            int targetSpaceId = GetSpaceId(targetTileIndex);

            if (SpaceCategoryMap.GetCategory(targetSpaceId) != SpaceCategory.Start)
            {
                await HandleTileLanding(playerId, targetSpaceId); // 도착 효과 연쇄 (황금열쇠끼리도 재귀 가능)
            }
        }

        /// <summary>
        /// 타일 가져오기
        /// </summary>
        /// <param name="tileIndex"></param>
        /// <returns></returns>
        private int GetSpaceId(int tileIndex)
        {
            Tile tile = BlueMarbleWorld.Instance.Board.GetTile(tileIndex);
            return tile.SpaceDef.Id;
        }
    }
}