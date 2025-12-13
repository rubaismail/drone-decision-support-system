using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWander : MonoBehaviour
{
    public Transform centerPoint;   // Roam Center
    public float roamRadius = 80f;  // Size of allowed area around the roam center object
    public float speed = 8f;
    public float changeDirectionTime = 3f;

    private Vector3 _targetDirection;
    private float _timer;

    void Start()
    {
        PickNewDirection();
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= changeDirectionTime)
            PickNewDirection();

        Vector3 newPos = transform.position + _targetDirection * (speed * Time.deltaTime);

        // Keep drone inside radius around center point
        if (Vector3.Distance(newPos, centerPoint.position) < roamRadius)
        {
            transform.position = newPos;
        }
        else
        {
            // Steer drone gently back toward the center if it tries to leave
            Vector3 dirToCenter = (centerPoint.position - transform.position).normalized;
            transform.position += dirToCenter * (speed * Time.deltaTime * 0.8f);
        }
    }

    void PickNewDirection()
    {
        _timer = 0f;

        _targetDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-0.1f, 0.1f),
            Random.Range(-1f, 1f)
        ).normalized;
    }
}

