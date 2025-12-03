using UnityEngine;

public class Seek : ISteering
{
    private Transform _character;
    private Transform _target;
    private float _maxSpeed;

    public Seek(Transform character, Transform target, float maxSpeed)
    {
        _character = character;
        _target = target;
        _maxSpeed = maxSpeed;
    }

    public Vector3 ChangeVelocity(Vector3 velocity)
    {
        Vector3 directionToTarget = (_target.position - _character.position).NoY().normalized;
        Vector3 desiredVelocity = directionToTarget * _maxSpeed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += steering * Time.deltaTime;

        return velocity;
    }
}