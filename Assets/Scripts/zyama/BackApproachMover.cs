using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackApproachMover : MonoBehaviour
{
    public Transform playerHead;

    public float startDistance = 8f;
    public float endDistance = 1.5f;
    public float moveSpeed = 1.2f;
    public bool repeat = true;

    private float currentDistance;

    void Start()
    {
        currentDistance = startDistance;
        SetPositionBehindPlayer();
    }

    void Update()
    {
        currentDistance -= moveSpeed * Time.deltaTime;

        if (currentDistance <= endDistance)
        {
            if (repeat)
            {
                currentDistance = startDistance;
            }
            else
            {
                currentDistance = endDistance;
            }
        }

        SetPositionBehindPlayer();
    }

    void SetPositionBehindPlayer()
    {
        if (playerHead == null) return;

        Vector3 behindDirection = -playerHead.forward;
        Vector3 targetPosition =
            playerHead.position + behindDirection * currentDistance;

        targetPosition.y = 1f;

        transform.position = targetPosition;
    }
}