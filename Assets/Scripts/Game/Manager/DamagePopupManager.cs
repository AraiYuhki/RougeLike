using UnityEngine;
using UnityEngine.Pool;

public class DamagePopupManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private DamagePopup original;

    private ObjectPool<DamagePopup> popups = null;

    private void Awake()
    {
        popups = new ObjectPool<DamagePopup>(
            () =>
            {
                var instance = Instantiate(original, transform);
                instance.SetPool(popups);
                return instance;
            },
            null,
            target => target.gameObject.SetActive(false),
            target => Destroy(target.gameObject),
            true, 10, 30
            );
    }

    public void Create(Unit target, int value, Color color)
    {
        if (!IsInSight(target))
            return;
        var popup = popups.Get();
        popup.transform.position = WorldToPoint(target);
        popup.transform.position += new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), 0);
        popup.Initialize(value, color);
    }

    private bool IsInSight(Unit target)
    {
        var viewPortPosition = mainCamera.WorldToViewportPoint(target.transform.position);
        return 0f <= viewPortPosition.x && viewPortPosition.x <= 1f && 0f <= viewPortPosition.y && viewPortPosition.y <= 1f;
    }

    private Vector3 WorldToPoint(Unit target)
    {
        return mainCamera.WorldToScreenPoint(target.transform.position);
    }
}
