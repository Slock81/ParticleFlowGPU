using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    [SerializeField] private Vector3 velocity = Vector3.zero;
    [SerializeField] private Vector3 acceleration = Vector3.zero;

    [SerializeField] public float maxVelocity = 10.0f;
    [SerializeField, Range(0, 1)] public float dragPerc;

    public void addAcceleration(Vector3 p_acceleration)
    {
        acceleration += p_acceleration;
    }

    public void updateParticle()
    {
        Vector3 currPos = transform.position;
        currPos += velocity;
        velocity += acceleration;

        if (velocity.magnitude > maxVelocity)
        {
            velocity = velocity.normalized * maxVelocity;
        }

        acceleration *= (1 - dragPerc);
        transform.position = currPos;
    }
}
