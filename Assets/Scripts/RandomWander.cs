using System;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class RandomWander : MonoBehaviour
{
    public float speed = 12f;                               // Drone wandering speed
    public float changeDirectionTime = 3f;                  // How often to change direction
    public Vector3 areaSize = new Vector3(200, 50, 200);    // Allowed romaing box

    private Vector3 targetDirection;
    private float timer;
    void Start()
    {
        PickNewDirection();
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        // Change direction every few seconds
        if (timer >= changeDirectionTime)
            PickNewDirection();
        
        // Move drone
        transform.position += targetDirection * speed * Time.deltaTime;
        
        // Keep drone inside allowed area
        Vector3 localPos = transform.localPosition;
        
        localPos.x = Mathf.Clamp(local)



    }

    void PickNewDirection()
    {
        timer = 0f;
        targetDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-0.2f, 0.2f),
            Random.Range(-1f, 1f)
            ).normalized;
    }
}
