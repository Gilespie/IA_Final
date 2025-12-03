using UnityEngine;

public class Persuit : ISteering
{
    private Transform _character;
    private Transform _target;
    private float _maxSpeed;
    private Rigidbody _targetRB;
    private float _timePrediction;

    public  Persuit(Transform character, Transform target, float maxSpeed, float timePrediction)
    {
        _character = character;
        _target = target;
        _maxSpeed = maxSpeed;
        _timePrediction = timePrediction;
        _targetRB = target.GetComponent<Rigidbody>();
    }

    public Vector3 ChangeVelocity(Vector3 velocity)
    {
        float currentTimePrediction = _timePrediction * (_target.position - _character.position).magnitude;
        Vector3 futurePos = _target.position + _targetRB.velocity * currentTimePrediction;

        Vector3 directionToFuture = (futurePos - _character.position).NoY().normalized;
        Vector3 directionToTarget = (_target.position - _character.position).NoY().normalized;

        if (Vector3.Dot(directionToTarget, directionToFuture) < 0)
        {
            directionToFuture = directionToTarget;
        }

        Vector3 desiredVelocity = directionToFuture * _maxSpeed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += steering * Time.deltaTime;

        return velocity;
    }
}
