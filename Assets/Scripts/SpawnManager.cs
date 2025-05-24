using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using Unity.XR.CoreUtils;
using UnityEngine.EventSystems;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private Player playerPrefab;
    [SerializeField]
    private Enemy enemyPrefab;
    [SerializeField]
    private Camera camera;

    // private ARRaycastManager raycastManager;
    private Player player;
    private Enemy enemy;

    // private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    InputAction clickAction;

    void Awake()
    {
        // raycastManager = FindFirstObjectByType<ARRaycastManager>();

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
                player.jumpSpeed = 0.5f;
                player.GetComponent<CharacterController>().enabled = true;
            }
            else if (enemy == null)
            {
                enemy = Instantiate(enemyPrefab);
                enemy.transform.position = hit.point;
                Assert.IsNotNull(player, "Player object null");
                enemy.player = player;
                enemy.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }

        // if (raycastManager.Raycast(touchVector, hits, TrackableType.PlaneWithinPolygon))
        // {
        //     Pose hitPose = hits[0].pose;

        //     // Create the player
        //     if (player == null)
        //     {
        //         player = Instantiate(playerPrefab, hitPose.position, hitPose.rotation);
        //         player.GetComponent<Player>().cam = camera;
        //         player.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        //         player.speed *= 0.1f;
        //     }
        //     else if (enemy == null)
        //     {
        //         enemy = Instantiate(enemyPrefab, hitPose.position, hitPose.rotation);
        //         Assert.IsNotNull(player, "Player object null");
        //         enemy.player = player;
        //         enemy.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        //     }
        // }
    }
}
