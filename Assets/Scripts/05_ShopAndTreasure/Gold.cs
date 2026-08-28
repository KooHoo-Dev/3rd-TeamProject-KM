using BlueMarble;
using UnityEngine;

namespace BlueMarble
{
    public class Gold : IGold
    {
        public int CurrentGold { get; private set; }


        public Gold(int startGold)
        {
            CurrentGold = startGold;
            Debug.Log($"초기 금화 {CurrentGold}");
        }
        
        public bool TrySpend(int amount)
        {
            if (amount > CurrentGold)
            {
                Debug.Log($"금화 부족 : 보유 {CurrentGold}, 필요 : {amount}");
                return false;
            }

            CurrentGold -= amount;
            Debug.Log($"{amount} 지출 , 남은 금화 {CurrentGold}");
            return true;

        }
    }
}