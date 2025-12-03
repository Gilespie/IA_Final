using UnityEngine;

public class BorderLimiter : MonoBehaviour
{
    [SerializeField] LevelBoundary _levelBoundary;

    void Update()
    {
        if (transform.position.magnitude > _levelBoundary.Radius)
        {
            if (_levelBoundary.LimitMode == BorderType.Limit)
            {
                transform.position = transform.position.normalized * _levelBoundary.Radius;
            }

            if (_levelBoundary.LimitMode == BorderType.Teleport)
            {
                transform.position = -transform.position.normalized * _levelBoundary.Radius;
            }
        }
    }
}