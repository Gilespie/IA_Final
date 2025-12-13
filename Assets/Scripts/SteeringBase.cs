using System;
using UnityEngine;

public class SteeringBase : MonoBehaviour
{
    [Header("Steering")]
    [SerializeField] protected float _maxSpeed;
    [SerializeField] protected float _maxForce;
    [SerializeField] protected float _slowingRange = 3f;
    [SerializeField] protected float _timePrediction = 3f;
    [SerializeField] protected float _radius;
    [SerializeField] protected float _personalArea;
    [SerializeField] protected float _avoidanceWeight;
    [SerializeField] protected LayerMask _obstacleMask;
    [SerializeField] float _wanderRadius = 2f;
    [SerializeField] float _wanderDistance = 3f;
    [SerializeField] float _wanderJitter = 0.2f;

    Vector3 _wanderTarget = Vector3.forward;
    protected Vector3 _velocity;
    public Vector3 Velocity => _velocity;
    protected ObstacleAvoidance _avoid;

    protected virtual void Awake()
    {
        _avoid = new ObstacleAvoidance(transform, _radius, _personalArea, _obstacleMask);
    }

    public Vector3 Seek(Vector3 target)
    {
        Vector3 desired = (target - transform.position).NoY();
        desired = desired.normalized * _maxSpeed;
        return  CalculateSteering(desired);
    }

    /*public Vector3 Arrive(Vector3 target, Vector3 velocity)//old
    {
        Vector3 offsetToTarget = (target - transform.position).NoY();
        float distance = offsetToTarget.magnitude;
        float rampedSpeed = _maxSpeed * (distance / _slowingRange);
        rampedSpeed = Mathf.Min(rampedSpeed, _maxSpeed);

        Vector3 directionToTarget = offsetToTarget.normalized;

        Vector3 desiredVelocity = directionToTarget * rampedSpeed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += steering * Time.deltaTime * _rotationForce;

        return velocity;
    }*/

    public Vector3 Arrive(Vector3 target)//new
    {
        Vector3 toTarget = (target - transform.position).NoY();
        float distance = toTarget.magnitude;

        if (distance < 0.01f)
            return Vector3.zero;

        float speed = _maxSpeed;

        if (distance < _slowingRange)
            speed = _maxSpeed * (distance / _slowingRange);

        Vector3 desired = toTarget.normalized * speed;
        return CalculateSteering(desired);
    }

    /*public Vector3 Evade(Transform target, Vector3 velocity)//old
    {
        float currentTimePrediction = _timePrediction * (target.position - transform.position).magnitude;
        Vector3 futurePos = target.position + target.GetComponent<Hunter>().Velocity * currentTimePrediction;

        Vector3 directionToFuture = (transform.position - futurePos).NoY().normalized;

        Vector3 desiredVelocity = directionToFuture * _maxSpeed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += steering * Time.deltaTime * _rotationForce;

        return velocity;
    }*/

    public Vector3 Evade(Transform target, Vector3 targetVelocity)//new
    {
        Vector3 toTarget = target.position - transform.position;
        float prediction = toTarget.magnitude / _maxSpeed;
        prediction = Mathf.Min(prediction, _timePrediction);

        Vector3 futurePos = target.position + targetVelocity * prediction;

        Vector3 desired = (transform.position - futurePos).NoY().normalized * _maxSpeed;
        return CalculateSteering(desired);
    }

    /*public Vector3 Persuit(Transform target, Vector3 velocity)//old
    {
        float currentTimePrediction = _timePrediction * (target.position - transform.position).magnitude;
        Vector3 futurePos = target.position + target.GetComponent<Lider>().Velocity * currentTimePrediction;

        Vector3 directionToFuture = (futurePos - transform.position).NoY().normalized;
        Vector3 directionToTarget = (target.position - transform.position).NoY().normalized;

        if (Vector3.Dot(directionToTarget, directionToFuture) < 0)
        {
            directionToFuture = directionToTarget;
        }

        Vector3 desiredVelocity = directionToFuture * _maxSpeed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += steering * Time.deltaTime;

        return velocity;
    }*/

    public Vector3 Pursuit(Transform target, Vector3 targetVelocity)//new
    {
        Vector3 toTarget = target.position - transform.position;
        float prediction = toTarget.magnitude / _maxSpeed;
        prediction = Mathf.Min(prediction, _timePrediction);

        Vector3 futurePos = target.position + targetVelocity * prediction;

        Vector3 desired = (futurePos - transform.position).NoY().normalized * _maxSpeed;
        return CalculateSteering(desired);
    }

    public Vector3 Wander()
    {
        _wanderTarget += new Vector3(
            UnityEngine.Random.Range(-1f, 1f) * _wanderJitter,
            0,
            UnityEngine.Random.Range(-1f, 1f) * _wanderJitter
        );

        _wanderTarget = _wanderTarget.normalized * _wanderRadius;

        Vector3 targetWorld =
            transform.position +
            transform.forward * _wanderDistance +
            _wanderTarget;

        return Seek(targetWorld);
    }

    public Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxForce); 
    }

    /*public void AddForce(Vector3 force)
    {
        Vector3 avoidance = _avoid.ChangeVelocity(_velocity) * _avoidanceWeight;

        _velocity += avoidance;

        _velocity = Vector3.ClampMagnitude(_velocity + force, _maxSpeed);
    }*/

    public void AddForce(Vector3 force)
    {
        Vector3 desiredVelocity = _velocity + force;

        Vector3 avoidance = _avoid.ChangeVelocity(desiredVelocity) * _avoidanceWeight;

        _velocity = Vector3.ClampMagnitude(desiredVelocity + avoidance, _maxSpeed);
    }

    public void Move()
    {
        if (_velocity.sqrMagnitude < 0.0001f)
            return;

        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;

        _velocity *= 0.95f; // damping
    }
}