using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private Vector3 offset;


    // Update is called once per frame
    void Update()
    {
        transform.localPosition = target.transform.localPosition + offset;
        transform.LookAt(target.transform);
    }
}
