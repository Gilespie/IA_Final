using UnityEngine;

public class MouseInputController
{
    Vector3 _position;
    public Vector3 Position => _position;

    LayerMask _mask;
    bool _hasClick = false;
    public bool HasClick => _hasClick;

    public MouseInputController(LayerMask mask)
    {
        _mask = mask;
    }

    public void InputUpdate()
    {
        _hasClick = false;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _mask))
            {
                _position = hit.point;
                _hasClick = true;
            }
        }
    }
}
