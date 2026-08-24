using System;
using BlueMarble.Board;
using BlueMarble.Board.Builder;
using Cysharp.Threading.Tasks;
using Jay;

namespace BlueMarble
{
    public class DataManager : SingletonBase<DataManager>
    {
        public TileTable Tiles { get; } = new();

        public event Action OnDataLoaded;
        
        public bool IsDataLoaded { get; private set; } = false;

        private async UniTaskVoid Start()
        {
            await LoadAsync();
            
            IsDataLoaded = true;
            OnDataLoaded?.Invoke();
        }

        private async UniTask LoadAsync()
        {
            // 만약 추가적인 데이터가 들어오면 t2로 확장하여 whenall에 대입
            // 예시) UniTask t2 = Fights.LoadAsync();
            //      await UniTask.WhenAll(t1, t2);
            UniTask t1 = Tiles.LoadAsync();
            
            await UniTask.WhenAll(t1);
        }

        public UniTask EnsureDataLoaded() 
            => UniTask.WaitUntil(this, static self => self.IsDataLoaded);

        #region GET_DEF

        public TileDef GetTileDef(int id) => Tiles.Get(id);

        #endregion
        
        #region  TRY_GET_DEF

        public bool TryGetTileDef(TileType type, TileDirection dir, out TileDef result)
        {
            result = null;
            
            // Corner 혹은 Special 타일의 경우, 방향 보정이 필요
            dir = type switch
            {
                TileType.Corner => TileDirection.All,
                TileType.Special when dir is TileDirection.East or TileDirection.West 
                    => TileDirection.Vertical,
                _ => dir
            };

            foreach (var def in Tiles.GetDefs())
            {
                if (def.Type != type) continue;
                if (def.Dir != dir) continue;

                result = def;
                return true;
            }

            return false;
        }

        #endregion
    }
}