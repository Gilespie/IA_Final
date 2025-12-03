using UnityEngine;

public class TargetChangeColor : MonoBehaviour
{
    public void ChangeColor(Collider col, Color color)
    {
        col.GetComponentInChildren<MeshRenderer>().material.color = color;
    }
}