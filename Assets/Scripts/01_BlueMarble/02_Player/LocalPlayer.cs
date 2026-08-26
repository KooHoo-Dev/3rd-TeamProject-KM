using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Jay;
using Sirenix.OdinInspector;

namespace BlueMarble
{
    public class LocalPlayer : SingletonBase<BlueMarbleWorld>
    {
        [Title("Settings")]
        [SerializeField] private float jumpPower = 1f;
        [SerializeField] private float jumpDuration = 0.5f;
        
        [field: Title("States")]
        [field: ShowInInspector, ReadOnly] public int CurrentTileIndex { get; private set; }

        private BlueMarbleWorld World => BlueMarbleWorld.Instance;

        public async UniTask MoveBy(int count)
        {
            Board board = World.Board;
            
            for (int i = 0; i < count; i++)
            {
                int targetIdx = CurrentTileIndex + i + 1;
                Vector3 goal = board.GetTile(targetIdx).transform.position;

                await transform
                    .DOJump(goal, jumpPower, 1, jumpDuration)
                    .SetEase(Ease.OutCubic)
                    .ToUniTask();
            }
        }
    }
}