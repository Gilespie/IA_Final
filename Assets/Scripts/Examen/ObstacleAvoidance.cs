using UnityEngine;

public class ObstacleAvoidance : ISteering
{
    private Transform _npc;
    private float _radius;
    private float _personalArea;
    private LayerMask _obstacleMask;

    public ObstacleAvoidance(Transform npc, float radius, float personalArea, LayerMask mask)
    {
        _npc = npc;
        _radius = radius;
        _personalArea = personalArea;
        _obstacleMask = mask;
    }

    public Vector3 ChangeVelocity(Vector3 velocity)
    {
        if (velocity.sqrMagnitude < 0.0001f)
            return velocity;

        // Используем SphereCastNonAlloc для лучшей производительности
        RaycastHit[] hits = new RaycastHit[10];
        int hitCount = Physics.SphereCastNonAlloc(_npc.position, _personalArea, velocity.normalized,
                                                  hits, _radius, _obstacleMask);

        if (hitCount > 0)
        {
            // Находим ближайшее препятствие (игнорируя самого себя)
            RaycastHit closestHit = default;
            float closestDistance = float.MaxValue;
            
            for (int i = 0; i < hitCount; i++)
            {
                // Игнорируем попадание в самого себя
                if (hits[i].transform == _npc) continue;
                
                if (hits[i].distance < closestDistance)
                {
                    closestHit = hits[i];
                    closestDistance = hits[i].distance;
                }
            }

            if (closestDistance < float.MaxValue)
            {
                Vector3 hitPoint = closestHit.point;
                float hitDistance = closestHit.distance;

                Vector3 relativePos = _npc.InverseTransformPoint(hitPoint);
                Vector3 dirToPoint = (hitPoint - _npc.position).normalized;
                Vector3 newDir;

                if (relativePos.x < 0)
                {
                    newDir = Vector3.Cross(_npc.transform.up, dirToPoint);
                }
                else
                {
                    newDir = -Vector3.Cross(_npc.transform.up, dirToPoint);
                }

                float clampedDistance = Mathf.Clamp(hitDistance - _personalArea, 0, _radius);
                float invertedClampDistance = _radius - clampedDistance;
                float proportionalDistance = invertedClampDistance / _radius;

                float avoidanceStrength = Mathf.Clamp01(proportionalDistance * 2f);
                return Vector3.Lerp(velocity.normalized, newDir.normalized, avoidanceStrength).normalized * velocity.magnitude;
            }
        }

        return velocity;
    }
}
