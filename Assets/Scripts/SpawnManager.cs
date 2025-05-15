using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private Enemy enemy;
    [SerializeField]
    private Camera camera;

    private ARRaycastManager raycastManager;
    private Player playerObject;
    private Enemy enemyObject;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    InputAction clickAction;

    void Awake()
    {
        raycastManager = FindFirstObjectByType<ARRaycastManager>();

        clickAction = InputSystem.actions.FindAction("Point");
        Assert.IsNotNull(clickAction);
    }

    void Update()
    {
        if (Touchscreen.current == null)
            return;

        bool touched = clickAction.WasPerformedThisFrame();
        Vector2 touchVector = clickAction.ReadValue<Vector2>();
        if (!touched)
            return;

        if (playerObject != null && enemyObject != null)
            return;

        if (raycastManager.Raycast(touchVector, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // Create the player
            if (playerObject == null)
            {
                playerObject = Instantiate(player, hitPose.position, hitPose.rotation);
                playerObject.GetComponent<Player>().cam = camera;
                playerObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                playerObject.speed *= 0.1f;
            }
            else if (enemyObject == null)
            {
                enemyObject = Instantiate(enemy, hitPose.position, hitPose.rotation);
                Assert.IsNotNull(playerObject, "Player object null");
                enemyObject.player = playerObject;
                enemyObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }
    }
}
