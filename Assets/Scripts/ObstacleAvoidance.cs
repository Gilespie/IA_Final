using UnityEngine;

public class ObstacleAvoidance : ISteering
{
    private Transform _npc;
    private float _radius;
    private float _personalArea;
    private LayerMask _obstacleMask;

    public ObstacleAvoidance(Transform npc, float radius, float personalArea, LayerMask obstacleMask)
    {
        _npc = npc;
        _radius = radius;
        _personalArea = personalArea;
        _obstacleMask = obstacleMask;
    }

    /*public Vector3 ChangeVelocity(Vector3 velocity)
    {
        if(Physics.SphereCast(_npc.position, _personalArea, velocity.normalized, out RaycastHit hit, _radius, _obstacleMask))
        {
            var hitPoint = hit.point;
            var hitDistance = hit.distance;

            Vector3 relativePos = _npc.InverseTransformPoint(hitPoint);
            Vector3 dirToPoint = (hitPoint - _npc.position).normalized;
            Vector3 newDir;

            if(relativePos.x < 0)
            {
                newDir = Vector3.Cross(_npc.transform.up, dirToPoint);
            }
            else
            {
                newDir = -Vector3.Cross(_npc.transform.up, dirToPoint);
            }

            var clampedDistance = Mathf.Clamp(hitDistance - _personalArea, 0, _radius); //dstance - collider del personaje
            var invertedClampDistance = _radius - clampedDistance; //inverse 
            var proportionalDistance = invertedClampDistance / _radius;

            return Vector3.Lerp(velocity.normalized, newDir.normalized, proportionalDistance * Time.deltaTime).normalized * velocity.magnitude;
        }

        return velocity;
        
    }*/

    public Vector3 ChangeVelocity(Vector3 desiredVelocity)
    {

        if (desiredVelocity.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 dir = desiredVelocity.normalized;

        if (Physics.SphereCast(
            _npc.position,
            _personalArea,
            dir,
            out RaycastHit hit,
            _radius,
            _obstacleMask))
        {

            Vector3 localHit = _npc.InverseTransformPoint(hit.point);


            Vector3 steerDir = (localHit.x < 0)
                ? Vector3.Cross(_npc.up, dir)      
                : -Vector3.Cross(_npc.up, dir);   


            float strength = 1f - (hit.distance / _radius);
            strength = Mathf.Clamp01(strength);


            return steerDir.normalized * strength;
        }

        return Vector3.zero;
    }
}