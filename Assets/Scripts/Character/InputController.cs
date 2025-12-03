using UnityEngine;

public class InputController
{
    Vector3 _inputDirection;

    public void UpdateArtificial()
    {
        _inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    public Vector3 GetInput()
    {
        return _inputDirection;
    }
}