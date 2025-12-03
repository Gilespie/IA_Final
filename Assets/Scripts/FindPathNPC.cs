using System.Collections.Generic;
using UnityEngine;

public class FindPathNPC : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] float _speed;
    [SerializeField] LayerMask _obstacleMask;
    [SerializeField] FOV _fov;
    List<Graph> _path = new List<Graph>();
    //Transform _myTransform;
    Vector3 _dir;

    void Start()
    {
        //_myTransform = transform;    
    }

    void Update()
    {
/*        if (Input.GetKeyDown(KeyCode.Q))
        {
           _path = PathManager.Instance.GetPath(transform.position, _target.position);
        }

        if (_path.Count > 0)
        {
            var dir = _path[0].transform.position - transform.position;
            transform.position += dir.normalized * Time.deltaTime * _speed;
            if(dir.magnitude < 0.3f)
            {
                _path.RemoveAt(0);
            }
        }*/

        //float x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            /*_path = PathManager.Instance.GetPath(transform.position, _target.position);

            if (!Physics.Linecast(transform.position, _path[1].transform.position, _obstacleMask))
                _path.RemoveAt(0);*/
        }

        //var dist = _target.position - transform.position;

        if (_fov.InFOV(_target.position))
        {
            _dir = _target.position - transform.position;
        }
        else if (_path.Count > 0)
        {
            _dir = _path[0].transform.position - transform.position;

            if (_dir.magnitude < 0.3f)
                _path.RemoveAt(0);
        }
        else
        {
            _dir = Vector3.zero;
        }

        Move();
    }

    private void Move()
    {
        if (_dir == Vector3.zero) return;

        transform.position += _dir.normalized * Time.deltaTime * _speed;
        transform.forward = _dir;
    }
}