using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El prefab de tu araña")]
    public GameObject spiderPrefab;
    [Tooltip("Cantidad de arañas a generar")]
    public int numSpiders = 20;
    [Tooltip("Área de generación inicial")]
    public Vector3 spawnLimits = new Vector3(5, 0, 5);

    [Header("Ajustes del Enjambre (Flock)")]
    [Range(0.0f, 10.0f)] public float minSpeed = 2.0f;
    [Range(0.0f, 10.0f)] public float maxSpeed = 5.0f;
    [Range(1.0f, 10.0f)] public float neighbourDistance = 3.0f;
    [Range(0.0f, 10.0f)] public float rotationSpeed = 4.0f;

    [HideInInspector]
    public GameObject[] allSpiders;

    void Start()
    {
        allSpiders = new GameObject[numSpiders];
        for (int i = 0; i < numSpiders; i++)
        {
            // Posición aleatoria dentro de los límites (manteniendo la altura del spawner)
            Vector3 pos = this.transform.position + new Vector3(
                Random.Range(-spawnLimits.x, spawnLimits.x),
                0, // 0 para mantenerlas en el suelo
                Random.Range(-spawnLimits.z, spawnLimits.z));

            // Rotación inicial aleatoria (solo en el eje Y para no inclinarlas)
            Vector3 randomizeDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

            allSpiders[i] = Instantiate(spiderPrefab, pos, Quaternion.LookRotation(randomizeDir));

            // Le pasamos la referencia del manager a cada araña
            allSpiders[i].GetComponent<SpiderFlock>().myManager = this;
        }
    }
}
