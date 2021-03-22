using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoundsDrawer : MonoBehaviour
{
    public static BoundsDrawer Instance;

    [SerializeField]
    private GameObject target;
    [SerializeField]
    private Color boundsColor;

    private List<Bounds> allBounds = new List<Bounds>();

    private List<Vector3> allEdges = new List<Vector3>();

    private Vector3 leftBottomNear;
    private Vector3 leftBottomFar;
    private Vector3 leftTopNear;
    private Vector3 leftTopFar;
    private Vector3 rightBottomNear;
    private Vector3 rightBottomFar;
    private Vector3 rightTopNear;
    private Vector3 rightTopFar;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Attempting to dupplicate singleton");
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        Quaternion originalRotation = transform.rotation;

        transform.rotation = Quaternion.identity;

        GetAllBounds();
        FillEdges();

        transform.rotation = originalRotation;

        TransformEdges();
        CreateEdgesForTarget();
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(leftBottomNear, rightBottomNear, boundsColor);
        Debug.DrawLine(leftTopNear, rightTopNear, boundsColor);
        Debug.DrawLine(leftBottomNear, leftTopNear, boundsColor);
        Debug.DrawLine(rightBottomNear, rightTopNear, boundsColor);
        Debug.DrawLine(leftBottomFar, rightBottomFar, boundsColor);
        Debug.DrawLine(leftTopFar, rightTopFar, boundsColor);
        Debug.DrawLine(leftBottomFar, leftTopFar, boundsColor);
        Debug.DrawLine(rightBottomFar, rightTopFar, boundsColor);
        Debug.DrawLine(leftBottomNear, leftBottomFar, boundsColor);
        Debug.DrawLine(rightBottomNear, rightBottomFar, boundsColor);
        Debug.DrawLine(leftTopNear, leftTopFar, boundsColor);
        Debug.DrawLine(rightTopNear, rightTopFar, boundsColor);
    }

    private void GetAllBounds()
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        Collider[] colliders = target.GetComponentsInChildren<Collider>();

        var rendererBounds = renderers.Select(renderer => renderer.bounds);
        var colliderBounds = colliders.Select(collider => collider.bounds);

        allBounds.Clear();

        allBounds.AddRange(rendererBounds);
        allBounds.AddRange(colliderBounds);
    }

    private void FillEdges()
    {
        allEdges.Clear();

        foreach (Bounds bounds in allBounds)
        {
            GetEdgesFromBounds(bounds);
        }
    }

    private void GetEdgesFromBounds(Bounds bounds)
    {
        allEdges.Add(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z) - transform.position);
        allEdges.Add(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z) - transform.position);
        allEdges.Add(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z) - transform.position);
        allEdges.Add(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z) - transform.position);
        allEdges.Add(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z) - transform.position);
        allEdges.Add(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z) - transform.position);
        allEdges.Add(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z) - transform.position);
        allEdges.Add(new Vector3(bounds.max.x, bounds.max.y, bounds.max.z) - transform.position);
    }

    private void TransformEdges()
    {
        for (int i = 0; i < allEdges.Count; i++)
        {
            allEdges[i] = transform.TransformPoint(allEdges[i]);
        }
    }

    private void CreateEdgesForTarget()
    {
        float left = allEdges.Min(edge => edge.x);
        float right = allEdges.Max(edge => edge.x);
        float bottom = allEdges.Min(edge => edge.y);
        float top = allEdges.Max(edge => edge.y);
        float near = allEdges.Min(edge => edge.z);
        float far = allEdges.Max(edge => edge.z);

        leftBottomNear = new Vector3(left, bottom, near);
        leftBottomFar = new Vector3(left, bottom, far);
        leftTopNear = new Vector3(left, top, near);
        leftTopFar = new Vector3(left, top, far);
        rightBottomNear = new Vector3(right, bottom, near);
        rightBottomFar = new Vector3(right, bottom, far);
        rightTopNear = new Vector3(right, top, near);
        rightTopFar = new Vector3(right, top, far);
    }
 }