using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    [SerializeField] private float worldY = 0;
    [SerializeField] public Vector3 upperLeftBounds;
    [SerializeField] public Vector3 lowerRightBounds;

    private Vector3 upperRightBounds;
    private Vector3 lowerLeftBounds;
    [SerializeField] Vector3[] unityBounds;
    [SerializeField] Texture2D dbgFlowField;
    [SerializeField] float worldWidth;
    [SerializeField] float worldHeight;

    public int numCellsX;
    public int numCellsY;

    Vector3[,] flowFieldMatrix;
    public EventHandler onFieldUpdated;

    private void Awake()
    {

    }

    public Vector3 getValueAtWorld(Vector3 worldPos)
    {
        float worldGsdX = worldWidth / (float)numCellsX;
        float worldGsdZ = worldHeight / (float)numCellsY;


        float deltaX = worldPos.x - upperLeftBounds.x;
        float deltaZ = upperLeftBounds.z - worldPos.z;

        int x = (int)Mathf.Floor(deltaX / worldGsdX);
        int y = (int)Mathf.Floor(deltaZ / worldGsdZ);

        if (x < 0) x = 0;
        if (x >= numCellsX) x = numCellsX - 1;

        if (y < 0) y = 0;
        if (y >= numCellsY) y = numCellsY - 1;

        return flowFieldMatrix[y, x];
    }

    public Vector3 getWorldPosition(int x, int y)
    {
        float worldGsdX = worldWidth / (float)numCellsX;
        float worldGsdY = worldHeight / (float)numCellsY;

        float worldX = upperLeftBounds.x + (x * worldGsdX);
        float worldZ = upperLeftBounds.z - (y * worldGsdY);
        return new Vector3(worldX, worldY, worldZ);
    }

    public int getNumRows()
    {
        return numCellsY;
    }

    public int getNumCols()
    {
        return numCellsX;
    }
    public Vector3[,] getFlowField()
    {
        return flowFieldMatrix;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        upperRightBounds = new Vector3(lowerRightBounds.x, 0, upperLeftBounds.z);
        lowerLeftBounds = new Vector3(upperLeftBounds.x, 0, lowerRightBounds.z);
        unityBounds = new Vector3[] { upperLeftBounds, upperRightBounds, lowerRightBounds, lowerLeftBounds };

        worldHeight = upperLeftBounds.z - lowerLeftBounds.z;
        worldWidth = upperRightBounds.x - upperLeftBounds.x;

        flowFieldMatrix = new Vector3[numCellsY, numCellsX];
        dbgFlowField = new Texture2D(numCellsX, numCellsY);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.Log("Loading Perlin Flow Fileld");
            loadDebugFlowField();
        }
    }

    [Header("Perlin Debug Noise")]
    [SerializeField] private float noiseXOrig;
    [SerializeField] private float noiseYOrig;
    [Range(0.00001f, 2000)]
    [SerializeField] private float scale;

    private float getPerlin(int x, int y)
    {
        float scaledX = x / (float)numCellsX;
        float scaledY = y / (float)numCellsY;

        return Mathf.PerlinNoise(noiseXOrig + scaledX * scale, noiseYOrig + scaledY * scale);
    }
    private void loadDebugFlowField()
    {
        float minX = int.MaxValue;
        float maxX = -int.MaxValue;
        float minZ = int.MaxValue;
        float maxZ = -int.MaxValue;
        for (int y = 0; y < numCellsY; y++)
        {
            for (int x = 0; x < numCellsX; x++)
            {


                float currPerlin = getPerlin(x, y);
                float xVec = Mathf.Cos(Mathf.PI * 2 * currPerlin);
                float zVec = Mathf.Sin(Mathf.PI * 2 * currPerlin);

                minX = Mathf.Min(xVec, minX);
                minZ = Mathf.Min(zVec, minZ);

                maxX = Mathf.Max(xVec, maxX);
                maxZ = Mathf.Max(zVec, maxZ);


                flowFieldMatrix[y, x] = new Vector3(xVec, 0, zVec).normalized;
                dbgFlowField.SetPixel(x, y, new Color(currPerlin, currPerlin, currPerlin));
            }
        }
        Debug.Log($"X:{minX} - {maxX}");
        Debug.Log($"Z:{minZ} - {maxZ}");
        dbgFlowField.Apply();
        onFieldUpdated?.Invoke(this, EventArgs.Empty);
    }

    bool writeOnce = true;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < unityBounds.Length; i++)
        {
            Vector3 curr = unityBounds[i];
            Vector3 next = (i < unityBounds.Length - 1) ? unityBounds[i + 1] : unityBounds[0];
            if (writeOnce) Debug.Log($"{i} {curr} --> {next}");
            Gizmos.DrawLine(curr, next);
            Gizmos.DrawSphere(curr, 10);

        }
        writeOnce = false;

    }

    public static float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
    {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }
}
