using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private Transform _transform;

    public void RotateObject()
    {
        _transform.Rotate(0, 0, 90);
    }
}
