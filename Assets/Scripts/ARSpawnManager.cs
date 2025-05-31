using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using Unity.XR.CoreUtils;
using UnityEngine.EventSystems;
using Niantic.Lightship.AR.NavigationMesh;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private Player playerPrefab;
    [SerializeField]
    private Enemy enemyPrefab;
    [SerializeField]
    private Camera camera;

    private Player player;
    private Enemy enemy;

    InputAction clickAction;

    void Awake()
    {
        clickAction = InputSystem.actions.FindAction("Point");
        Assert.IsNotNull(clickAction);
    }

    void Update()
    {
        if (Touchscreen.current == null)
            return;

        bool touched = clickAction.WasPerformedThisFrame();
        Vector2 touch = clickAction.ReadValue<Vector2>();
        bool isOverUi = EventSystem.current.IsPointerOverGameObject();

        if (!touched || isOverUi)
            return;

        if (player != null && enemy != null)
            return;

        Ray ray = camera.ScreenPointToRay(touch);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Create the player
            if (player == null)
            {
                player = Instantiate(playerPrefab);
                player.transform.position = hit.point + new Vector3(0, 3f, 0);
                // player.GetComponent<Player>().cam = camera;
                player.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                player.speed *= 0.1f;
                player.jumpSpeed = 5f;
                player.GetComponent<CharacterController>().enabled = true;
            }
            else if (enemy == null)
            {
                enemy = Instantiate(enemyPrefab);
                enemy.transform.position = hit.point + new Vector3(0, 5f, 0);
                enemy.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                Assert.IsNotNull(player, "Player object null");
                enemy.player = player;
                enemy.navmesh = FindFirstObjectByType<LightshipNavMeshManager>().LightshipNavMesh;
            }
        }
    }
}
