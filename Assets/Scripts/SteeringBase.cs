using UnityEngine;

public class SteeringBase : MonoBehaviour
{
    [Header("Steering")]
    [SerializeField] protected float _maxSpeed;
    [SerializeField] protected float _maxForce;
    [SerializeField] protected float _slowingRange = 3f;
    [SerializeField] protected float _timePrediction = 3f;


    [SerializeField] protected float _rotationForce;
    [SerializeField] protected float _radius;
    [SerializeField] protected float _personalArea;
    [SerializeField] protected float _avoidanceWeight;
    [SerializeField] LayerMask _obstacleMask;
    protected Vector3 _velocity;
    public Vector3 Velocity => _velocity;
    protected ObstacleAvoidance _avoid;

    protected virtual void Awake()
    {
        _avoid = new ObstacleAvoidance(transform, _radius, _personalArea, _obstacleMask);
    }

    protected virtual void Update()
    {
        if (_velocity.sqrMagnitude > 0.0001f)
        {
            transform.position += _velocity * Time.deltaTime;
            transform.forward = _velocity.normalized;
        }
    }

    protected Vector3 Seek(Vector3 target)
    {
        Vector3 desired = target - transform.position;
        desired = desired.normalized * _maxSpeed;
        return  CalculateSteering(desired);
    }

    public Vector3 Arrive(Transform target, Vector3 velocity)
    {
        Vector3 offsetToTarget = (target.position - transform.position).NoY();
        float distance = offsetToTarget.magnitude;
        float rampedSpeed = _maxSpeed * (distance / _slowingRange);
        rampedSpeed = Mathf.Min(rampedSpeed, _maxSpeed);

        Vector3 directionToTarget = offsetToTarget.normalized;

        Vector3 desiredVelocity = directionToTarget * rampedSpeed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += steering * Time.deltaTime * _rotationForce;

        return velocity;
    }

    public Vector3 Evade(Transform target, Vector3 velocity)
    {
        float currentTimePrediction = _timePrediction * (target.position - transform.position).magnitude;
        Vector3 futurePos = target.position + target.GetComponent<Hunter>().Velocity * currentTimePrediction;

        Vector3 directionToFuture = (transform.position - futurePos).NoY().normalized;

        Vector3 desiredVelocity = directionToFuture * _maxSpeed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += steering * Time.deltaTime * _rotationForce;

        return velocity;
    }

    protected Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxForce * Time.deltaTime); 
    }

    protected void AddForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, _maxSpeed);
    }

    protected void Move()
    {
        if (_velocity == Vector3.zero) return;

        transform.forward = _velocity;
        transform.position += _velocity * Time.deltaTime;
    }
}