using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingObstaclePhysics : MonoBehaviour
{
    public float speed;
    public bool randomSpeed;
    public float minSpeed, maxSpeed;
    public float clampedSpeed = 1;
    public Transform targetPos;
    private Vector3 initialPos;
    private Vector3 travelDirection;
    private float maxDistance; // Maximum distance between initial and target positions
    private bool movingForward = true; // Track if moving toward target or returning to initial

    private float distanceToTarget;
    private float adjustedSpeed;
    private Rigidbody rb;
    private Vector3 direction;
    private Vector3 target;
    public enum MovingPattern
    {
        Linear,
        Square,
        Cubic,
        Quadratic,
        Random
    }
    public MovingPattern pattern;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // Use physics to move the object

        if (randomSpeed)
        {
            speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        }

        // Store initial position
        initialPos = transform.position;

        // Calculate normalized direction vector from initial to target
        travelDirection = (targetPos.position - initialPos).normalized;

        // Calculate the maximum distance (from initialPos to targetPos)
        maxDistance = Vector3.Distance(initialPos, targetPos.position);

        // Randomize the pattern if required
        if (pattern == MovingPattern.Random)
        {
            Array patterns = Enum.GetValues(typeof(MovingPattern));
            pattern = (MovingPattern)patterns.GetValue(UnityEngine.Random.Range(0, patterns.Length - 1));
        }
    }

    private void FixedUpdate()
    {
        target = movingForward ? targetPos.position : initialPos;

        switch (pattern)
        {
            case MovingPattern.Linear:
                LinearMovement(target);
                break;
            case MovingPattern.Square:
                SquareMovement(target, clampedSpeed);
                break;
            case MovingPattern.Cubic:
                CubicMovement(target, clampedSpeed);
                break;
            case MovingPattern.Quadratic:
                QuadraticMovement(target, clampedSpeed);
                break;
            default:
                break;
        }

        // Check if the object has overshot the target position
        if (HasOvershotDestination())
        {
            movingForward = !movingForward; // Toggle direction when target is reached or overshot
        }
    }

    private void LinearMovement(Vector3 target)
    {
        direction = (target - transform.position).normalized;
        // Ensure speed is at least minSpeed
        rb.velocity = direction * speed;
    }

    private void SquareMovement(Vector3 target, float clampedSpeed)
    {
        distanceToTarget = Vector3.Distance(transform.position, target);
        adjustedSpeed = speed * Mathf.Sqrt(distanceToTarget / maxDistance);
        direction = (target - transform.position).normalized;
        rb.velocity = direction * Mathf.Max(adjustedSpeed, clampedSpeed);
    }

    private void CubicMovement(Vector3 target, float clampedSpeed)
    {
        distanceToTarget = Vector3.Distance(transform.position, target);
        adjustedSpeed = speed * Mathf.Pow(distanceToTarget / maxDistance, 1 / 3f);
        direction = (target - transform.position).normalized;
        rb.velocity = direction * Mathf.Max(adjustedSpeed, clampedSpeed);
    }

    private void QuadraticMovement(Vector3 target, float clampedSpeed)
    {
        distanceToTarget = Vector3.Distance(transform.position, target);
        adjustedSpeed = speed * Mathf.Pow(distanceToTarget / maxDistance, 1 / 4f);
        direction = (target - transform.position).normalized;
        rb.velocity = direction * Mathf.Max(adjustedSpeed, clampedSpeed);
    }

    private bool HasOvershotDestination()
    {
        // Determine the current target position
        //Vector3 target = movingForward ? targetPos.position : initialPos;

        // Calculate the vector from the starting point (initial or target) to the current position
        Vector3 toCurrent = transform.position - (movingForward ? initialPos : targetPos.position);

        // Calculate the vector from the starting point to the target
        Vector3 toTarget = target - (movingForward ? initialPos : targetPos.position);

        // Check if we have moved past the target using dot product (indicates overshoot if negative)
        return Vector3.Dot(toCurrent, toTarget) >= toTarget.sqrMagnitude;
    }
}
