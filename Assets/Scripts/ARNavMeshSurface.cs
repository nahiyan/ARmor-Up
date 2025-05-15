using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.ARFoundation;

public class ARNavMeshSurface : MonoBehaviour
{
    private NavMeshSurface surface;
    private ARPlaneManager planeManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        planeManager = FindFirstObjectByType<ARPlaneManager>();

        Assert.IsNotNull(surface);
        Assert.IsNotNull(planeManager);

        surface.BuildNavMesh();
        planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        surface.BuildNavMesh();
    }
}
