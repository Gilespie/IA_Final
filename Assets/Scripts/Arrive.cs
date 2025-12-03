using UnityEngine;

public class Arrive : ISteering
{
    private Transform _character;
    private Transform _target;
    private float _maxSpeed;
    private float _slowingRange;

    public Arrive(Transform character, Transform target, float maxSpeed, float slowingRange)
    {
        _character = character;
        _target = target;
        _maxSpeed = maxSpeed;
        _slowingRange = slowingRange;
    }

    public Vector3 ChangeVelocity(Vector3 velocity)
    {
        Vector3 offsetToTarget = (_target.position - _character.position).NoY();
        float distance = offsetToTarget.magnitude;
        float rampedSpeed = _maxSpeed * (distance/_slowingRange);
        rampedSpeed = Mathf.Min(rampedSpeed, _maxSpeed);

        Vector3 directionToTarget = offsetToTarget.normalized;

        Vector3 desiredVelocity = directionToTarget * rampedSpeed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += steering * Time.deltaTime;

        return velocity;
    }
}