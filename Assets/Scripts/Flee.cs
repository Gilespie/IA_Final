using UnityEngine;

public class Flee : ISteering
{
    private Transform _character;
    private Transform _target;
    private float _maxSpeed;


    public Flee(Transform character, Transform target, float maxSpeed)
    {
        _character = character;
        _target = target;
        _maxSpeed = maxSpeed;
    }

    public Vector3 ChangeVelocity(Vector3 velocity)
    {
        Vector3 directionFromTarget = (_character.position - _target.position).NoY().normalized;
        Vector3 desiredVelocity = directionFromTarget * _maxSpeed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += steering * Time.deltaTime;

        return velocity;
    }
}