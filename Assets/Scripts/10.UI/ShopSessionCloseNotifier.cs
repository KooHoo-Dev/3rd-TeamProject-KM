using System;
using UnityEngine;

namespace BlueMarble
{
    /// <summary>
    /// 부착된 오브젝트가 SetActive(false)로 꺼지는 순간 OnClosed를 호출한다.
    /// ShopUIController가 ShopEntryPanel에 이 컴포넌트를 붙여서,
    /// "상점 세션이 진짜로 끝난 시점"(왼쪽 진입 패널이 닫힐 때)을 감지하는 데 쓴다.
    /// </summary>
    public class ShopSessionCloseNotifier : MonoBehaviour
    {
        public Action OnClosed;

        private void OnDisable()
        {
            OnClosed?.Invoke();
        }
    }
}