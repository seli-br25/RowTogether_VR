using System;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    public float speed;
    public bool randomSpeed;
    public float minSpeed, maxSpeed;
    public Transform targetPos;
    private Vector3 initialPos;
    private Vector3 travelDirection;
    private float maxDistance; // Maximum distance between initial and target positions
    private bool movingForward = true; // Track if moving toward target or returning to initial
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
        if (pattern == MovingPattern.Random)
        {
            Array patterns = Enum.GetValues(typeof(MovingPattern));
            pattern = (MovingPattern)patterns.GetValue(UnityEngine.Random.Range(0, patterns.Length -1));
        }
    }

    private void Update()
    {

        Vector3 target = movingForward ? targetPos.position : initialPos;

        switch (pattern)
        {
            case MovingPattern.Linear:
                LinearMovement(target); 
                break;
            case MovingPattern.Cubic:
                CubicMovement(target);
                break;
            case MovingPattern.Square:
                SquareMovement(target);
                break;
            case MovingPattern.Quadratic:
                QuadraticMovement(target);
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
        // Move towards the current target (either targetPos or initialPos) at a constant speed
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void SquareMovement(Vector3 target)
    {
        // Smooth movement with Lerp for smoother transition
        float distanceToTarget = Vector3.Distance(transform.position, target);

        // Adjust speed based on distance to create a deceleration effect
        float adjustedSpeed = speed * Mathf.Sqrt(distanceToTarget / maxDistance);
        // Move towards the target using the adjusted speed
        transform.position = Vector3.MoveTowards(transform.position, target, adjustedSpeed * Time.deltaTime);
    }

    private void CubicMovement(Vector3 target)
    {
        float distanceToTarget = Vector3.Distance(transform.position, target);

        // Adjust speed based on distance to create a deceleration effect
        float adjustedSpeed = speed * Mathf.Pow(distanceToTarget / maxDistance, 1 / 3f);
        // Move towards the target using the adjusted speed
        transform.position = Vector3.MoveTowards(transform.position, target, adjustedSpeed * Time.deltaTime);
    }

    private void QuadraticMovement(Vector3 target)
    {
        float distanceToTarget = Vector3.Distance(transform.position, target);

        // Adjust speed based on distance to create a deceleration effect
        float adjustedSpeed = speed * Mathf.Pow(distanceToTarget / maxDistance, 1 / 4f);
        // Move towards the target using the adjusted speed
        transform.position = Vector3.MoveTowards(transform.position, target, adjustedSpeed * Time.deltaTime);
    }

    private bool HasOvershotDestination()
    {
        // Determine the current target position
        Vector3 target = movingForward ? targetPos.position : initialPos;

        // Calculate the vector from the starting point (initial or target) to the current position
        Vector3 toCurrent = transform.position - (movingForward ? initialPos : targetPos.position);

        // Calculate the vector from the starting point to the target
        Vector3 toTarget = target - (movingForward ? initialPos : targetPos.position);

        // Check if we have moved past the target using dot product (indicates overshoot if negative)
        return Vector3.Dot(toCurrent, toTarget) >= toTarget.sqrMagnitude;
    }
}
