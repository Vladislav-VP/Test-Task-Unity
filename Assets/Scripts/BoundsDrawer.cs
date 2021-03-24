using System;
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

    private Bounds genericBounds;

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

        Quaternion originalRotation = target.transform.rotation;

        target.transform.rotation = Quaternion.identity;

        GetAllBounds();
        FillEdges();

        target.transform.rotation = originalRotation;
        
        TransformEdges();
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

        genericBounds = new Bounds(Vector3.zero, Vector3.zero);

        allBounds.Clear();

        allBounds.AddRange(rendererBounds);
        allBounds.AddRange(colliderBounds);

        foreach (Bounds bounds in allBounds)
        {
            genericBounds.Encapsulate(bounds);
        }
    }

    private void FillEdges()
    {
        allEdges.Clear();
        GetEdgesFromBounds(genericBounds);
    }

    private void GetEdgesFromBounds(Bounds bounds)
    {
        leftBottomNear = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z) - target.transform.position;
        leftBottomFar = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z) - target.transform.position;
        leftTopNear = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z) - target.transform.position;
        leftTopFar = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z) - target.transform.position;
        rightBottomNear = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z) - target.transform.position;
        rightBottomFar = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z) - target.transform.position;
        rightTopNear = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z) - target.transform.position;
        rightTopFar = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z) - target.transform.position;
        
        allEdges.Add(leftBottomNear);
        allEdges.Add(leftBottomFar);
        allEdges.Add(leftTopNear);
        allEdges.Add(leftTopFar);
        allEdges.Add(rightBottomNear);
        allEdges.Add(rightBottomFar);
        allEdges.Add(rightTopNear);
        allEdges.Add(rightTopFar);
    }
    
    private void TransformEdges()
    {
        leftBottomNear = target.transform.TransformPoint(leftBottomNear);
        leftBottomFar = target.transform.TransformPoint(leftBottomFar);
        leftTopNear = target.transform.TransformPoint(leftTopNear);
        leftTopFar = target.transform.TransformPoint(leftTopFar);
        rightBottomNear = target.transform.TransformPoint(rightBottomNear);
        rightBottomFar = target.transform.TransformPoint(rightBottomFar);
        rightTopNear = target.transform.TransformPoint(rightTopNear);
        rightTopFar = target.transform.TransformPoint(rightTopFar);
    }
 }