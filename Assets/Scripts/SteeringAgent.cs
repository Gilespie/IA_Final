using UnityEngine;

public class SteeringAgent : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _timePrediction;
    [SerializeField] private float _slowingRange;
    //[SerializeField] private Steerings _steering;
    [SerializeField] private float _avoidanceDetectionRange;
    [SerializeField] private float _personalArea;
    [SerializeField] private LayerMask _obstacleMask;
    private ISteering _currentSteering;
    private ObstacleAvoidance _obstacleAvoidance;
    private Vector3 _velocity;

    private void Start()
    {
        //switch(_steering)
        //{
        //    case Steerings.Flee:
        //        _currentSteering = new Flee(transform, _target, _maxSpeed);
        //        break;
        //        case Steerings.Seek:
        //        _currentSteering = new Seek(transform, _target, _maxSpeed);
        //        break;
        //        case Steerings.Persuit:
        //        _currentSteering = new Persuit(transform, _target, _maxSpeed, _timePrediction);
        //        break;
        //        case Steerings.Arrive:
        //        _currentSteering = new Arrive(transform, _target, _maxSpeed, _slowingRange);
        //        break;
        //        case Steerings.Evade:
        //        _currentSteering = new Evade(transform, _target, _maxSpeed, _timePrediction);
        //        break;
        //    default: break;

        //}

        _currentSteering = new Seek(transform, _target, _maxSpeed);
        _obstacleAvoidance = new ObstacleAvoidance(transform, _avoidanceDetectionRange, _personalArea, _obstacleMask);
    }

    private void Update()
    {
        _velocity = _currentSteering.ChangeVelocity(_velocity);
        transform.forward = _velocity;
        _velocity = _obstacleAvoidance.ChangeVelocity(_velocity);
        transform.position += _velocity * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _avoidanceDetectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _personalArea);
    }
}