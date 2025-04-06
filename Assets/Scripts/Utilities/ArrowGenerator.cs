using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ArrowGenerator : MonoBehaviour
{
    public float stemLength;
    public float stemWidth;
    public float tipLength;
    public float tipWidth;

    [System.NonSerialized]
    public List<UnityEngine.Vector3> verticesList;
    [System.NonSerialized]
    public List<int> trianglesList;
    public List<Vector3> pathPoints;

    Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //GenerateArrow();
    }

    //void Update()
    //{
    //    GenerateArrow();
    //}

    //arrow is generated starting at Vector3.zero
    //arrow is generated facing right, towards radian 0.
    public void GenerateArrow(int distance)
    {
        List<Vector3> verticesList = new List<Vector3>();
        if (verticesList.Count > 0) verticesList.RemoveAt(verticesList.Count - 1);
        List<int> trianglesList = new List<int>();

        if (pathPoints.Count < 2) return;

        transform.position = pathPoints[0];
        // Determine the maximum index based on the specified distance
        int maxIndex = Mathf.Min(pathPoints.Count - 4, Mathf.CeilToInt(distance));

        for (int i = 0; i < maxIndex; i++)
        {
            Vector3 direction = (pathPoints[i + 1] - pathPoints[i]).normalized;
            float segmentDistance = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);

            bool isLastSegment = (distance < segmentDistance && i == maxIndex - 1);

            if (isLastSegment)
            {
                segmentDistance = distance;
                direction = (pathPoints[i] + direction * segmentDistance) - pathPoints[i];
            }

            Vector3 stemOrigin = pathPoints[i] - transform.position;
            Vector3 stemEnd = pathPoints[i + 1] - transform.position;
            float stemHalfWidth = stemWidth / 2f;

            // Calculate a vector perpendicular to the direction
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;

            // Stem points
            verticesList.Add(stemOrigin + (perpendicular * stemHalfWidth));
            verticesList.Add(stemOrigin - (perpendicular * stemHalfWidth));
            verticesList.Add(stemEnd + (perpendicular * stemHalfWidth));
            verticesList.Add(stemEnd - (perpendicular * stemHalfWidth));

            int baseIndex = verticesList.Count - 4;
            trianglesList.Add(baseIndex);
            trianglesList.Add(baseIndex + 2);
            trianglesList.Add(baseIndex + 1);

            trianglesList.Add(baseIndex + 1);
            trianglesList.Add(baseIndex + 2);
            trianglesList.Add(baseIndex + 3);
            if (isLastSegment || i == maxIndex - 1)
            {
                // Generate the tip of the arrow
                GenerateArrowTip(verticesList, trianglesList, direction, verticesList[baseIndex + 2]);

                if (isLastSegment)
                    break;
            }
        }

        mesh.Clear();
        mesh.vertices = verticesList.ToArray();
        mesh.triangles = trianglesList.ToArray();
        //mesh.RecalculateNormals();
    }

    private void GenerateArrowTip(List<Vector3> verticesList, List<int> trianglesList, Vector3 direction, Vector3 tipOrigin)
    {
        float tipHalfWidth = tipWidth / 2;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;

        verticesList.Add(tipOrigin + (perpendicular * tipHalfWidth));
        verticesList.Add(tipOrigin - (perpendicular * tipHalfWidth));
        verticesList.Add(tipOrigin + (tipLength * direction.normalized));

        int baseIndex = verticesList.Count - 3;
        trianglesList.Add(baseIndex);
        trianglesList.Add(baseIndex + 1);
        trianglesList.Add(baseIndex + 2);
    }

    public void ClearArrow()
    {
        mesh.Clear();
    }
}