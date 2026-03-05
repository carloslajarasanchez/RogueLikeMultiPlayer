using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Cam Settings")]
    [SerializeField] private float _lerpSpeed = 5f;
    [SerializeField] private float _heightOffset = 5f;
    [SerializeField] private float _zOffset = -5f;

    private Transform _target;

    private void Awake()
    {
        Main.CustomEvents.OnLocalPlayerSpawned.AddListener(SetTarget);
    }

    private void Start()
    {
        if (_target == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && player.IsOwner)
                SetTarget(player.transform);
        }
    }

    void FixedUpdate()
    {
        if (_target == null) return;

        Vector3 targetPosition = new Vector3(
            _target.position.x,
            _heightOffset,
            _target.position.z + _zOffset
        );

        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position, targetPosition, _lerpSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }

    private void SetTarget(Transform playerTransform)
    {
        PlayerController pc = playerTransform.GetComponent<PlayerController>();
        if (pc != null && !pc.IsOwner) return;

        _target = playerTransform;
        transform.position = new Vector3(
            _target.position.x,
            _heightOffset,
            _target.position.z + _zOffset
        );
    }

    private void OnDestroy()
    {
        Main.CustomEvents.OnLocalPlayerSpawned.RemoveListener(SetTarget);
    }
}