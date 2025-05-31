using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private Enemy enemyPrefab;

    void Awake()
    {
        // TODO: Fix obsolete code
        // Enemy enemy = Instantiate(enemyPrefab, patrolPoints.transform);

        // enemy.player = player;
        // List<Transform> points = new();
        // foreach (Transform child in patrolPoints.transform)
        // {
        //     Assert.IsNotNull(child);
        //     points.Add(child);
        // }
        // enemy.patrolPoints = points;
        // enemy.ChangeState(Enemy.STATE.PATROL);
    }
}
