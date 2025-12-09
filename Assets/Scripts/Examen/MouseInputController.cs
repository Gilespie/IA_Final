using UnityEngine;

public class MouseInputController
{
    Vector3 _position;
    public Vector3 Position => _position;

    LayerMask _mask;
    bool _hasClick = false;
    bool _newClick = false;
    public bool HasClick => _hasClick;

    public MouseInputController(LayerMask mask)
    {
        _mask = mask;
    }

    public void InputUpdate()
    {
        _newClick = false;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _mask))
            {
                _position = hit.point;
                _hasClick = true;
                _newClick = true;
            }
        }
    }

    public void ClearClick()
    {
        _hasClick = false;
    }

    public bool HasNewClick => _newClick;
}
