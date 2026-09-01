using BlueMarble;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Jay;
using Sirenix.OdinInspector;

public interface IPlayer
{
    string Id { get; }
    Vector3 Position { get; }

    int ActionDisabledTurnCount { get; set; }

    UniTask<Tile> MoveNext();
}

namespace BlueMarble
{
    public class LocalPlayer : SingletonBase<LocalPlayer>, IPlayer
    {
        [field: ShowInInspector, ReadOnly] public string Id { get; private set; }

        [Title("Settings")]
        [SerializeField] private float jumpPower = 1f;
        [SerializeField] private float jumpDuration = 0.5f;
        
        [field: Title("States")]
        [field: ShowInInspector, ReadOnly] public int CurrentTileIndex { get; private set; }
        [field: ShowInInspector, ReadOnly] public int ActionDisabledTurnCount { get; set; } = 0;

        public Vector3 Position => transform.position;
        
        private BlueMarbleWorld World => BlueMarbleWorld.Instance;
        
        public void Initialize(string id)
        {
            Id = id;
            PlayerRegistry.Register(this);
        }

        public void OnDisable()
        {
            PlayerRegistry.Unregister(this);
        }
        
        public async UniTask<Tile> MoveNext()
        {
            await UniTask.WaitUntil(this, self => self.World.IsBoardReady);

            Board board = World.Board;
            
            // 다음 목적지 설정
            CurrentTileIndex += 1;
                
            // 만약 다음 목적지가 보드의 끝이었다면, 시작점으로 목표 변경
            if (CurrentTileIndex > board.MaxTileIndex)
                CurrentTileIndex = 0;
            
            Tile goal = board.GetTile(CurrentTileIndex);
            
            await transform
                .DOJump(goal.transform.position, jumpPower, 1, jumpDuration)
                .SetEase(Ease.OutCubic)
                .ToUniTask(TweenCancelBehaviour.Complete);

            return goal;
        }

        public async UniTask<Tile> MoveTo(Tile goal)
        {
            while (true)
            {
                Tile current = await MoveNext();
                if (current == goal) return current;
            }
        }
    }
}