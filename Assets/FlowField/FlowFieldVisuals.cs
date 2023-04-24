using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldVisuals : MonoBehaviour
{
    public FlowField flowField;
    [SerializeField, Range(0, 100)] private float lineLength = 10;
    public Material lineMaterial;
    public float lineWidth = 1.0f;
    public Color lineColor = Color.white;

    //Particles
    public Particle particlePrefab;
    [SerializeField, Range(1, 1000)] int numParticles = 10;
    [SerializeField] private List<Particle> particles = new List<Particle>();

    void Awake()
    {
        flowField = GetComponent<FlowField>();
        flowField.onFieldUpdated += flowFieldUpdated;



    }

    private LineRenderer[,] lineValues;
    private LineRenderer createLine()
    {
        //I know this should be a pool, but whatever for this. 
        GameObject newGo = new GameObject();
        newGo.transform.parent = transform; //Keep it tidy
        LineRenderer newLine = newGo.AddComponent<LineRenderer>();
        newLine.positionCount = 2;
        newLine.startWidth = lineWidth;
        newLine.endWidth = lineWidth;

        newLine.startColor = Color.white;
        newLine.endColor = Color.white;

        newLine.material = lineMaterial;
        return newLine;
    }
    public void flowFieldUpdated(object p_sender, EventArgs p_args)
    {
        int numRows = flowField.getNumRows();
        int numCols = flowField.getNumCols();
        if (lineValues == null)
        {

            lineValues = new LineRenderer[numRows, numCols];
            for (int y = 0; y < numRows; y++)
            {
                for (int x = 0; x < numCols; x++)
                {
                    LineRenderer newLine = createLine();
                    lineValues[y, x] = newLine;
                }
            }
        }



        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numCols; x++)
            {
                LineRenderer currLine = lineValues[y, x];
                Vector3 worldPos = flowField.getWorldPosition(x, y);
                Vector3 worldAngle = flowField.getFlowField()[y, x].normalized;
                currLine.SetPosition(0, worldPos);
                currLine.SetPosition(1, worldPos + (worldAngle * lineLength)); //TEST
            }
        }


        if (particles.Count < 1)
        {
            for (int i = 0; i < numParticles; i++)
            {
                Vector3 randPos = flowField.getWorldPosition(UnityEngine.Random.Range(0, numCols), UnityEngine.Random.Range(0, numRows));
                particles.Add(GameObject.Instantiate(particlePrefab, randPos, Quaternion.identity));
            }
        }

    }



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButton(0))
        {
            Camera cam = Camera.main;
            Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(camRay.origin, camRay.direction, out hit, Mathf.Infinity))
            {
                Vector3 hitWorld = hit.point;
                particles.Add(GameObject.Instantiate(particlePrefab, hitWorld, Quaternion.identity));
            }
        }

        //Add in acceleration
        foreach (Particle p in particles)
        {
            Vector3 worldPos = p.transform.position;

            //Check out of bounds
            float x = worldPos.x;
            float z = worldPos.z;

            float minX = flowField.upperLeftBounds.x;
            float maxX = flowField.lowerRightBounds.x;

            if (x < minX)
            {
                x = maxX;
            }
            else if (x > maxX)
            {
                x = minX;
            }

            float minZ = flowField.lowerRightBounds.z;
            float maxZ = flowField.upperLeftBounds.z;

            if (z < minZ)
            {
                z = maxZ;
            }
            else if (z > maxZ)
            {
                z = minZ;
            }

            worldPos.x = x;
            worldPos.z = z;
            p.transform.position = worldPos;

            Vector3 accel = flowField.getValueAtWorld(worldPos) * Time.deltaTime;
            p.addAcceleration(accel);
            p.updateParticle();


        }


    }
}
