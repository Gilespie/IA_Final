using UnityEngine;

public class FOV : MonoBehaviour
{
    [SerializeField] float _angleRange;
    [SerializeField] float _visionRange;
    [SerializeField] LayerMask _layerMask;

    public bool InFOV(Vector3 pos)
    {
        return InRange(pos) && InAngle(pos) && InSight(pos);    
    }

    public bool InRange(Vector3 pos)
    {
        var dir = (pos - transform.position);
        return  dir.sqrMagnitude < _visionRange * _visionRange;
    }

    public bool InAngle(Vector3 pos)
    {
        var dir = (pos - transform.position);
        var halfAngle = _angleRange / 2;
        return Vector3.Angle(transform.forward, dir) < halfAngle;
    }

    public bool InSight(Vector3 pos)
    {
        return !Physics.Linecast(transform.position, pos, _layerMask);
    }

    private Vector3 GetVectorFromAngle(float angle)
    {
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 lineA = GetVectorFromAngle(_angleRange/2 + transform.eulerAngles.y);
        Vector3 lineB = GetVectorFromAngle(-_angleRange/2 + transform.eulerAngles.y);

        Gizmos.DrawLine(transform.position, transform.position + lineA * _visionRange);
        Gizmos.DrawLine(transform.position, transform.position + lineB * _visionRange);

        Gizmos.color = Color.white;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(Vector3.zero, _visionRange);
    }
}