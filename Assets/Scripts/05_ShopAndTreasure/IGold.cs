namespace BlueMarble
{


    /// <summary>
    /// 
    /// </summary>
    public interface IGold
    {
        int CurrentGold { get; }



        bool TrySpend(int amount);
    }
}

    