using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private Enemy enemyPrefab;

    [SerializeField]
    private Transform enemySpawnPoint;

    void Awake()
    {
        Enemy enemy = Instantiate(enemyPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
        enemy.GetComponent<LightshipNavMeshAgent>().enabled = false;
        enemy.player = player;
    }
}
