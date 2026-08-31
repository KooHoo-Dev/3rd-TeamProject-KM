using UnityEngine;
using BlueMarble;

namespace Player
{
    public class TestInventoryFiller : MonoBehaviour
    {
        private EquipmentTable equipmentTable; 

        private async void Start()
        {
            equipmentTable = new EquipmentTable();

            // TSV 파일에서 진짜 데이터를 비동기로 읽어옵니다.
            await equipmentTable.LoadAsync();

            InventoryManager inv = InventoryManager.Instance;

            // ID 1번부터 5번까지 차례대로 인벤토리에 넣습니다.
            for (int i = 1; i <= 5; i++)
            {
                // DataTable에 있는 Get 함수를 사용해서 진짜 명세서를 가져옵니다.
                EquipmentDef itemDef = equipmentTable.Get(i); 
                
                if (itemDef != null)
                {
                    inv.TryAddEquipment(itemDef);
                }
            }

            Debug.Log("[테스트] TSV 데이터 기반으로 5개의 장비가 인벤토리에 세팅되었습니다!");
        }
    }
}