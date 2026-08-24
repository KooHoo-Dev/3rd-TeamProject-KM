using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public Queue<int> players =  new Queue<int>();
    public int registerPlayer = 4;

    private void Start()
    {
        for(int i =  0; i < registerPlayer; i++)
        {
            players.Enqueue(i);
        }
        Debug.Log($"바뀌기 전 순서");
        ShowSequence();
        ShuffleQueue(players);
    }
    
    private void ShuffleQueue(Queue<int> player)
    {
        List<int> temp = new List<int>(player);
        player.Clear();
        
        Shuffle(temp);

        foreach (int i in temp)
        {
            players.Enqueue(i);
        }
        Debug.Log("바뀐 후 순서");
        ShowSequence();
    }
    
    private static void Shuffle(List<int> list)
    {
        int n =  list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            int value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public bool IsPlayerTurn(int player)
    {
        if (players.Peek() == player) return true;
        return false;
    }
    
    private void ShowSequence()
    {
        foreach (int i in players)
        {
            Debug.Log(i);
        }
    }
    
}
