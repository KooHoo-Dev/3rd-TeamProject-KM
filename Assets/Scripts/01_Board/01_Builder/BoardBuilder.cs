using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlueMarble.Board.Builder
{
    public class BoardBuilder : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private Transform boardRoot;
        [SerializeField] private Transform startPoint;

        [Space]
        [SerializeField] private Vector2 offset;

        [Title("Readonly")]
        [ShowInInspector] private TileDirection currentDir = TileDirection.South;
        [ShowInInspector] private readonly List<Tile> createdList = new();
        
        private DataManager Data => DataManager.Instance;

        private async UniTaskVoid Start()
        {
            await Data.EnsureDataLoaded();
            
            Tile created = CreateTile(TileType.Corner, TileDirection.All, 0);
            created.transform.position = startPoint.position;
        }
        
        private Tile CreateTile(TileType type, TileDirection dir, int spaceId)
        {
            if (Data.TryGetTileDef(type, dir, out var def) == false)
            {
                Debug.LogWarning($"TileDef not found: {type}");
                return null;
            }
            
            BoardSpaceDef spaceDef = Data.GetBoardSpaceDef(spaceId);
            
            Tile created = Instantiate(def.Prefab, boardRoot);
            created.Initialize(def, spaceDef);
            created.UpdateVisuals();
            
            createdList.Add(created);

            return created;
        }

        public void CreateTile(TileType type, int spaceId)
        {
            // CreateTile에서 CreatedList에 생성한 타일을 즉시 Add하므로
            // 가장 최근 타일을 미리 캐싱해두고 위치 계산에 사용해야 함
            Tile lastCreated = createdList[^1];
            
            Tile created = CreateTile(type, currentDir, spaceId);
            created.transform.position = created.CalculateDistFrom(lastCreated, currentDir, offset);

            // 코너 타일이 설치되었다면 다음 방향으로 전환
            if (type == TileType.Corner) currentDir++;
        }

        public void RemoveLastTile()
        {
            // 출발점 타일은 지우지 못하도록 Early Return
            if (createdList.Count == 1)
            {
                Debug.LogWarning("Cannot remove start tile.");
                return;
            }
            
            Tile lastCreated = createdList[^1];
            
            // 지울 타일이 코너 타일이면, 타일 생성 방향을 이전으로 되돌림
            if (lastCreated.Def.Type == TileType.Corner)
                currentDir--;
            
            createdList.Remove(lastCreated);
            Destroy(lastCreated.gameObject);
        }

        public void SaveBoard()
        {
            // CreatedList에 저장해둔 순서대로 데이터 변환 후, Json파일로 저장
        }
    }
}