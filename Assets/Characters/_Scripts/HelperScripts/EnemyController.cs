using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject PlayerTarget { get; set; }
    [SerializeField]
    private GameObject m_playerReference;
    [SerializeField]
    private LayerMask m_playerLayerMask;
    [SerializeField]
    private float m_detectionRange = 10.0f;
    public Vector3 LastKnownPlayerPosition { get; set; }

    private void Start()
    {
        LastKnownPlayerPosition = transform.position;
    }
    private void Update()
    {
        RaycastHit hit;
        Vector3 direction = m_playerReference.transform.position - transform.position;
        Physics.Raycast(transform.position, direction, out hit, m_detectionRange, m_playerLayerMask);
        if (hit.collider != null && hit.collider.gameObject == m_playerReference)
        {
            PlayerTarget = hit.collider.gameObject;
            LastKnownPlayerPosition = PlayerTarget.transform.position;
        }
        else
        {
            PlayerTarget = null;
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (Application.isPlaying)
        {
            if (PlayerTarget != null)
                Gizmos.color = Color.green;
            if (m_playerReference != null)
            {
                Vector3 direction = m_playerReference.transform.position - transform.position;
                Gizmos.DrawLine(transform.position, m_playerReference.transform.position);
            }
        }
        Gizmos.DrawWireSphere(transform.position, m_detectionRange);
    }
}
