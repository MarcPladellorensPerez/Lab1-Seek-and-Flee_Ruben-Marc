using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El prefab de tu araña (tiene que llevar el componente SpiderFlock)")]
    public GameObject spiderPrefab;
    [Tooltip("Cantidad de arañas a generar")]
    public int numSpiders = 20;
    [Tooltip("Área de generación inicial (mitad del ancho en X y en Z)")]
    public Vector3 spawnLimits = new Vector3(5, 0, 5);

    [Header("Ajustes del Enjambre (Flock)")]
    [Range(0.0f, 10.0f)] public float minSpeed = 2.0f;
    [Range(0.0f, 10.0f)] public float maxSpeed = 5.0f;
    [Tooltip("Radio en el que una araña considera a otra su vecina (cohesión y alineación)")]
    [Range(1.0f, 10.0f)] public float neighbourDistance = 3.0f;
    [Tooltip("Radio, más pequeño que el anterior, en el que se separan unas de otras")]
    [Range(0.1f, 5.0f)] public float separationDistance = 1.5f;
    [Range(0.0f, 10.0f)] public float rotationSpeed = 4.0f;

    [Header("Peso de cada regla")]
    public float cohesionWeight = 1f;
    public float alignWeight = 1f;
    public float separationWeight = 1.5f;

    [Header("Extras")]
    [Tooltip("Cada cuántos segundos recalcula cada araña las reglas. 0 = cada frame, como en las diapos")]
    [Range(0f, 1f)] public float rulesInterval = 0.2f;
    [Tooltip("Pequeña componente aleatoria para que el movimiento sea menos mecánico")]
    [Range(0f, 1f)] public float randomness = 0.2f;
    [Tooltip("Si está activo, las arañas que se salen de roamLimits vuelven al centro")]
    public bool bounded = true;
    [Tooltip("Mitad del ancho en X y en Z de la zona por la que pueden moverse")]
    public Vector3 roamLimits = new Vector3(15, 0, 15);
    [Tooltip("Opcional: un líder o punto al que el grupo tiende a ir (por ejemplo, el jugador)")]
    public Transform goal;
    public float goalWeight = 0.5f;

    // Guardamos directamente el componente SpiderFlock de cada araña:
    // así los vecinos no tienen que hacer GetComponent en cada frame
    [HideInInspector]
    public SpiderFlock[] allSpiders;

    void Start()
    {
        if (spiderPrefab == null || spiderPrefab.GetComponent<SpiderFlock>() == null)
        {
            Debug.LogError("FlockManager: falta el prefab o no tiene el componente SpiderFlock.", this);
            return;
        }

        allSpiders = new SpiderFlock[numSpiders];
        for (int i = 0; i < numSpiders; i++)
        {
            // Posición aleatoria dentro de los límites (manteniendo la altura del spawner)
            Vector3 pos = this.transform.position + new Vector3(
                Random.Range(-spawnLimits.x, spawnLimits.x),
                0, // 0 para mantenerlas en el suelo
                Random.Range(-spawnLimits.z, spawnLimits.z));

            // Rotación inicial aleatoria solo en el eje Y. Con Euler nunca nos sale un vector de dirección a cero
            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject go = Instantiate(spiderPrefab, pos, rot);

            // Le pasamos la referencia del manager a cada araña
            allSpiders[i] = go.GetComponent<SpiderFlock>();
            allSpiders[i].myManager = this;
        }
    }

    // Evita que la velocidad mínima sea mayor que la máxima al tocar los valores en el Inspector
    void OnValidate()
    {
        if (maxSpeed < minSpeed) maxSpeed = minSpeed;
    }

    // Dibuja las zonas al seleccionar el manager: amarillo = spawn, cian = zona por la que se mueven
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnLimits.x * 2f, 0.1f, spawnLimits.z * 2f));

        if (bounded)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(roamLimits.x * 2f, 0.1f, roamLimits.z * 2f));
        }
    }
}