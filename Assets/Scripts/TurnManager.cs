using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<int> players =  new List<int>();
    public int registerPlayer = 4;
    private int currentPlayer = 0;
    public int roundCount = 1;
    
    private void Start()
    {
        for(int i =  0; i < registerPlayer; i++)
        {
            players.Add(i);
        }
        
        Debug.Log($"바뀌기 전 순서");
        ShowSequence();
        
        Shuffle(players);
        
        Debug.Log("바뀐 후 순서");
        ShowSequence();
    }

    private void Update()
    {
        MyTurn(currentPlayer);
    }
    
    // 순서를 배정한다
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

    // 본인의 턴인지 확인한다
    private bool IsPlayerTurn(int playerID)
    {
        return (players[currentPlayer] == playerID);
    }

    // 순서를 넘긴다
    private void AdvancedPlayer(int playerID)
    {
        if (IsPlayerTurn(playerID) == false) return;
        if (currentPlayer == registerPlayer - 1) roundCount++; 
        currentPlayer = (currentPlayer + 1) % players.Count;
    }

    // 주사위 값을 구한다
    private int GetDiceResult()
    {
        int randNum1 = Random.Range(1, 7);
        int randNum2 = Random.Range(1, 7);
        
        return randNum1 + randNum2;
    }

    // 자기 턴인지 확인이 되면 주사위 값만큼 움직인다
    private void MyTurn(int playerID)
    {
        if (IsPlayerTurn(playerID) == false) return;
        int moveDistance = GetDiceResult();
        MovePlayer(moveDistance);
    }

    // 플레이어를 움직인다.
    private async void MovePlayer(int distance)
    {
        // 일단 비워둠
    }
    
    // 로그용/ 순서 확인
    private void ShowSequence()
    {
        foreach (int i in players)
        {
            Debug.Log(i);
        }
    }
    
}
