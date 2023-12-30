using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private bool autoRotation = false;

    // Update is called once per frame
    private void Update()
    {
        if (autoRotation)
            transform.rotation *= Quaternion.Euler(0f, 90f * Time.deltaTime, 0f);
    }
}
