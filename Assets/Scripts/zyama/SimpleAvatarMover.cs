using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAvatarMover : MonoBehaviour
{
    public float moveRangeX = 2f;   // ç∂âE
    public float moveRangeY = 1f;   // è„â∫
    public float moveRangeZ = 2f;   // ëOå„
    public float moveSpeed = 1.2f;  // ë¨ìx

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * moveSpeed) * moveRangeX;
        float y = Mathf.Cos(Time.time * moveSpeed) * moveRangeY;
        float z = Mathf.Sin(Time.time * moveSpeed * 0.5f) * moveRangeZ;

        transform.position = startPosition + new Vector3(x, y, z);
    }
}