using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RendererHelper
{
    private static Plane[] planes;

    private static IEnumerator Start()
    {
        while (Camera.main == null) yield return null;
        planes = new Plane[(GeometryUtility.CalculateFrustumPlanes(Camera.main)).Length];
    }

    public static bool IsVisibleFrom(Renderer renderer, Camera camera)
    {
        planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}