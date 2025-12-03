using System.Collections;
using UnityEngine;

public class Boid : SteeringBase
{
    [SerializeField] float _viewRadius;
    [SerializeField] float _separationRadius;
    FlockingManager _flockingManager => FlockingManager.Instance;

    [SerializeField, Range(0, 3)] float _separationWeight;
    [SerializeField, Range(0, 3)] float _coheshionWeight;
    [SerializeField, Range(0, 3)] float _aligmentWeight;

    [SerializeField] float _detectionRange = 5f;
    [SerializeField] float _distanceOffset = 1f;
    [SerializeField] float _randomDirDelay = 5f;

    [SerializeField] LayerMask _enemyMask;
    [SerializeField] LayerMask _foodMask;
    [SerializeField] LayerMask _boidMask;

    Needs _needs;
    Collider[] _apples = new Collider[1];
    Collider[] _enemy = new Collider[1];
    Collider[] _boid = new Collider[10];

    Vector3 _randomDir = Vector3.zero;

    private void Start()
    {
        _needs = GetComponent<Needs>();

        _flockingManager.AddBoid(this);

        StartCoroutine(CheckRandomDir());

        AddForce(_randomDir);
    }

    private void Update()
    {
        if (_needs.IsHungry())
        {
            int foodCount = Physics.OverlapSphereNonAlloc(transform.position, _detectionRange, _apples, _foodMask);

            if (foodCount > 0)
            {
                AddForce(Arrive(_apples[0].transform, _velocity));
                Debug.Log("Busco la comida");
                float distanceBetweenBoidAndApple = (_apples[0].transform.position - transform.position).magnitude;

                if(distanceBetweenBoidAndApple <= _distanceOffset)
                {
                    _apples[0].GetComponent<Apple>().ConsumeFood(_needs);
                }
            }
        }

        int enemy = Physics.OverlapSphereNonAlloc(transform.position, _detectionRange, _enemy, _enemyMask);

        if (enemy > 0) 
        {
            AddForce(Evade(_enemy[0].transform, _velocity));
            Debug.Log("Escapo del enimigo");
        }

        int boid = Physics.OverlapSphereNonAlloc(transform.position, _detectionRange, _boid, _boidMask);

        if (NearBoid())
        {
            //Debug.Log("Esta en el flow!");
            Flocking();
        }
        else
        {
            Debug.Log("Esta en direccion aleatoria!");
            AddForce(_randomDir);
        }

        AddForce(_avoid.ChangeVelocity(_velocity) * _avoidanceWeight);
        Move();
    }

    private void Flocking()
    {
        AddForce(Separation() * _separationWeight + Cohesion() * _coheshionWeight + Aligment() * _aligmentWeight);
    }

    private Vector3 Separation()
    {
        Vector3 desired = Vector3.zero;

        int count = 0;

        for (int i = 0; i < _flockingManager.AllBoids.Count; i++)
        {
            Boid current = _flockingManager.AllBoids[i];

            if (current == this) continue;

            Vector3 dir = current.transform.position - transform.position;
            if (dir.sqrMagnitude > _separationRadius * _separationRadius) continue;

            count++;

            desired += dir;
        }

        if (count == 0) return desired;

        desired /= count;
        desired *= -1;

        return CalculateSteering(desired.normalized * _maxSpeed);
    }

    private Vector3 Cohesion()
    {
        var desired = Vector3.zero;

        int count = 0;

        foreach (var item in _flockingManager.AllBoids)
        {
            if (item == this) continue;

            if (Vector3.Distance(transform.position, item.transform.position) > _viewRadius) continue;

            count++;
            desired += item.transform.position;
        }

        if (count == 0) return desired;

        desired /= count;

        return Seek(desired);
    }

    private Vector3 Aligment()
    {
        Vector3 desired = Vector3.zero;

        int count = 0;

        foreach (var item in _flockingManager.AllBoids)
        {
            if (item == this) continue;

            if (Vector3.Distance(transform.position, item.transform.position) > _viewRadius) continue;

            count++;
            desired += item._velocity;
        }

        if(count == 0) return desired;

        return CalculateSteering(desired.normalized * _maxSpeed);
        // Normalmente usamos CalculateSteering(desired) solo
        //Pero en este caso quiero que se mantengan en movimiento a velocidad máxima
    }

    public bool NearBoid()
    {
        foreach (var b in _boid)
        {
            if (b != null && b.gameObject != this.gameObject) 
                return true;
        }

        return false;
    }

    private IEnumerator CheckRandomDir()
    {
        while (true)
        {
            _randomDir = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
            yield return new WaitForSeconds(_randomDirDelay);
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _viewRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _separationRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
    }
}