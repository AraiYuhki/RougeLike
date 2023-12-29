using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private bool autoRotation = false;

    public void JumpTo()
    {
        try
        {
            var token = this.GetCancellationTokenOnDestroy();
            transform
                .DOLocalJump(transform.localPosition, 1, 1, 0.4f)
                .ToUniTask(cancellationToken: token)
                .Forget();
        }
        finally
        {
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (autoRotation)
            transform.rotation *= Quaternion.Euler(0f, 90f * Time.deltaTime, 0f);
    }
}
