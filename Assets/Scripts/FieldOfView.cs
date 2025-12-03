using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [SerializeField, Range(0.01f, 50f)] float _radius;
    [SerializeField, Range(0.01f, 50f)] float _speed;
    [SerializeField, Range(0.01f, 360f)] float _fov;
    [SerializeField, Range(0.01f, 360f)] float _fovWide;
    [SerializeField] LayerMask _targetMask;
    [SerializeField] LayerMask _obstacleMask;
    [SerializeField] Collider[] _targets = new Collider[10];
    
    Collider _desired;
    

    void FixedUpdate()
    {
        if(_desired)
        {
            transform.forward = _desired.transform.position - transform.position;
            transform.position += transform.forward * Time.deltaTime * _speed;
        }

        FindTargets();
    }
    
    void FindTargets()
    {
        Physics.OverlapSphereNonAlloc(transform.position, _radius, _targets, _targetMask);

        for (int i = 0; i < _targets.Length; i++)
        {
            if(_targets[i] == null) continue;

            _targets[i].GetComponent<TargetChangeColor>().ChangeColor(_targets[i], Color.white);

            Vector3 direction = _targets[i].transform.position - transform.position;

            if (Vector3.Angle(direction.normalized, transform.forward) > _fovWide * 0.5f) continue;

            if (!Physics.Raycast(transform.position, direction.normalized, direction.magnitude, _obstacleMask))
            {
                if (Vector3.Angle(direction.normalized, transform.forward) > _fov * 0.5f)
                {
                    _targets[i].GetComponent<TargetChangeColor>().ChangeColor(_targets[i], Color.white);
                    _desired = null;
                }
                else
                {
                    _desired = _targets[i];
                    _targets[i].GetComponent<TargetChangeColor>().ChangeColor(_targets[i], Color.red);
                }
                
                Debug.DrawLine(transform.position, _targets[i].transform.position);
            }
        }
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Quaternion rotationPositive = Quaternion.AngleAxis(_fov/2, transform.up);
    //    Quaternion rotationNegative = Quaternion.AngleAxis(-_fov/2, transform.up);
    //    Vector3 angleDirPositive = rotationPositive * transform.forward; 
    //    Vector3 angleDirNegative = rotationNegative * transform.forward;
    //    Gizmos.DrawLine(transform.position, transform.position + angleDirPositive * _radius);
    //    Gizmos.DrawLine(transform.position, transform.position + angleDirNegative * _radius);

    //    Gizmos.color = Color.red;
    //    Quaternion rotationPositiveWide = Quaternion.AngleAxis(_fovWide / 2, transform.up);
    //    Quaternion rotationNegativeWide = Quaternion.AngleAxis(-_fovWide / 2, transform.up);
    //    Vector3 angleDirPositiveWide = rotationPositiveWide * transform.forward;
    //    Vector3 angleDirNegativeWide = rotationNegativeWide * transform.forward;
    //    Gizmos.DrawLine(transform.position, transform.position + angleDirPositiveWide * _radius);
    //    Gizmos.DrawLine(transform.position, transform.position + angleDirNegativeWide * _radius);

    //    Gizmos.color = Color.white;
    //    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, 0, 1));
    //    Gizmos.DrawWireSphere(Vector3.zero, _radius);
    //}
}