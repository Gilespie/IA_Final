using UnityEngine;

public class SteeringBase : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float _maxSpeed = 4f;
    [SerializeField] protected float _maxForce = 10f;

    [Header("Arrive")]
    [SerializeField] protected float _slowingRange = 3f;
    [SerializeField] protected float _rotationForce = 5f;

    [Header("Obstacle Avoidance")]
    [SerializeField] protected float _radius = 1f;
    [SerializeField] protected float _personalArea = 1f;
    [SerializeField] protected float _avoidanceWeight = 1f;
    [SerializeField] protected LayerMask _obstacleMask;

    protected Vector3 _velocity;
    public Vector3 Velocity => _velocity;

    protected ObstacleAvoidance _avoid;
    public ObstacleAvoidance Avoid => _avoid;
    public float AvoidanceWeight => _avoidanceWeight;

    protected virtual void Awake()
    {
        _avoid = new ObstacleAvoidance(transform, _radius, _personalArea, _obstacleMask);
    }

    protected virtual void Update()
    {
        if (_velocity.sqrMagnitude > 0.0001f)
        {
            transform.forward = _velocity.normalized;
            transform.position += _velocity * Time.deltaTime;
        }
    }

    protected Vector3 Seek(Vector3 target)
    {
        Vector3 desired = (target - transform.position).normalized * _maxSpeed;
        return CalculateSteering(desired);
    }

    public Vector3 Arrive(Transform target, Vector3 velocity)
    {
        Vector3 offset = (target.position - transform.position).NoY();
        float dist = offset.magnitude;

        float speed = _maxSpeed * (dist / _slowingRange);
        speed = Mathf.Min(speed, _maxSpeed);

        Vector3 desiredVelocity = offset.normalized * speed;
        Vector3 steering = desiredVelocity - velocity;

        velocity += steering * Time.deltaTime * _rotationForce;

        return velocity;
    }

    public Vector3 Evade(Transform target, Vector3 velocity)
    {
        Vector3 dir = (transform.position - target.position).NoY();
        Vector3 desired = dir.normalized * _maxSpeed;

        Vector3 steering = desired - velocity;
        velocity += steering * Time.deltaTime * _rotationForce;

        return velocity;
    }

    protected Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxForce * Time.deltaTime);
    }

    public void AddForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, _maxSpeed);
    }

    public void Stop()
    {
        _velocity = Vector3.zero;
    }
}
