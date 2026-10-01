// T01 - Movement Pathfinding in Games
// Recull de codi C# extret de les diapositives del PDF.
//
// IMPORTANT:
// - Aquest fitxer és un recull de fragments de referència, NO un únic script pensat per compilar tal qual.
// - Algunes diapositives mostren fragments parcials, alternatives de codi o variables declarades en altres scripts.
// - He mantingut els noms i l'estructura de les diapositives tant com ha estat possible.


// ============================================================================
// DIAPO 9 - Implementació de Kinematic Seek & Flee en Unity
// ============================================================================

// Seek
Vector3 direction = target.transform.position - transform.position;
direction.y = 0f; // (x, z): position in the floor

// Flee (alternativa a Seek)
Vector3 direction = transform.position - target.transform.position;

Vector3 movement = direction.normalized * maxVelocity;

float angle = Mathf.Rad2Deg * Mathf.Atan2(movement.x, movement.z);
Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up); // up = y


// ============================================================================
// DIAPO 10 - Update de posició/rotació i reducció de freqüència de Steering
// ============================================================================

// Update: rotation and position (dt = Time.deltaTime)
transform.rotation = Quaternion.Slerp(
    transform.rotation,
    rotation,
    Time.deltaTime * turnSpeed
);

transform.position += transform.forward.normalized * maxVelocity * Time.deltaTime;

// Time: how to reduce frequency in steerings calls
float freq = 0f;

void Update()
{
    freq += Time.deltaTime;
    if (freq > 0.5)
    {
        freq -= 0.5f;
        Seek();
    }

    // Update commands
}


// ============================================================================
// DIAPO 11 - Math & Unity Stuff I: distància i angle
// ============================================================================

Vector3.Distance(target.transform.position, transform.position);

Mathf.Abs(Vector3.Angle(transform.forward, movement)); // forward = z


// ============================================================================
// DIAPO 12 - Math & Unity Stuff II: signed angle
// ============================================================================

Vector3.SignedAngle(v, w, transform.forward);


// ============================================================================
// DIAPO 14 - Implementació de Steering Update
// ============================================================================

void Update()
{
    if (Vector3.Distance(target.transform.position, transform.position) < stopDistance)
        return;

    Seek(); // calls to this function should be reduced

    turnSpeed += turnAcceleration * Time.deltaTime;
    turnSpeed = Mathf.Min(turnSpeed, maxTurnSpeed);

    movSpeed += acceleration * Time.deltaTime;
    movSpeed = Mathf.Min(movSpeed, maxSpeed);

    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        rotation,
        Time.deltaTime * turnSpeed
    );

    transform.position += transform.forward.normalized * movSpeed * Time.deltaTime;
}


// ============================================================================
// DIAPO 15 - Implementació de Steering Seek
// ============================================================================

void Seek()
{
    Vector3 direction = target.transform.position - transform.position;
    direction.y = 0f;

    movement = direction.normalized * acceleration;

    float angle = Mathf.Rad2Deg * Mathf.Atan2(movement.x, movement.z);
    rotation = Quaternion.AngleAxis(angle, Vector3.up);
}


// ============================================================================
// DIAPO 22 - NavMesh Agent: implementació de Seek
// ============================================================================

public NavMeshAgent agent;
public GameObject target;

void Seek()
{
    agent.destination = target.transform.position;
}


// ============================================================================
// DIAPO 30 - Implementació simple de Wander
// ============================================================================

// parameters: float radius, offset;
Vector3 localTarget = UnityEngine.Random.insideUnitCircle * radius;
localTarget += new Vector3(0, 0, offset);

Vector3 worldTarget = transform.TransformPoint(localTarget);
worldTarget.y = 0f;

// La diapositiva també indica utilitzar NavMesh.SamplePosition.


// ============================================================================
// DIAPO 32 - Implementació simple de Pursue & Evade
// ============================================================================

Vector3 targetDir = target.transform.position - transform.position;
float lookAhead = targetDir.magnitude / agent.speed;

Seek(target.transform.position + target.transform.forward * lookAhead);

// Flee for evasion


// ============================================================================
// DIAPO 33 - Hide: C# previ (hiding spots, lambdas, LINQ i tuples)
// ============================================================================

// Defining Hiding Spots
GameObject[] hidingSpots;
hidingSpots = GameObject.FindGameObjectsWithTag("hide");

// Anonymous Functions
Func<int, int> inc = (a) => a + 1;
// inc(4) = 5

// Tuples
(int, string) a = (1, "Pep");
(int, string) b = (2, "Anna");
// a.CompareTo(b) = -1

// Linq Select (Queries)
int[] v = { 3, 2, -3, 5 };
// v.Min() = -3
// v.Select((x) => Math.Abs(x)).Min() = 2


// ============================================================================
// DIAPO 34 - Implementació de Hide
// ============================================================================

void Hide()
{
    Func<GameObject, float> distance =
        (hs) => Vector3.Distance(target.transform.position, hs.transform.position);

    GameObject hidingSpot = hidingSpots.Select(
        ho => (distance(ho), ho)
    ).Min().Item2;

    Vector3 dir = hidingSpot.transform.position - target.transform.position;

    Ray backRay = new Ray(hidingSpot.transform.position, -dir.normalized);
    RaycastHit info;

    hidingSpot.GetComponent<Collider>().Raycast(backRay, out info, 50f);

    Seek(info.point + dir.normalized);
}


// ============================================================================
// DIAPO 35 - Follow Path / Patrol amb waypoints
// ============================================================================

public GameObject[] waypoints;
int patrolWP = 0;

// ...

if (!agent.pathPending && agent.remainingDistance < 0.5f)
    Patrol();

// ...

void Patrol()
{
    patrolWP = (patrolWP + 1) % waypoints.Length;
    Seek(waypoints[patrolWP].transform.position);
}


// ============================================================================
// DIAPO 38 - Path Smoothing amb Bézier Path Creator
// ============================================================================

using UnityEngine;
using UnityEngine.AI;
using PathCreation;
using System.Collections;

public class Follow : MonoBehaviour
{
    public GameObject robber;
    public GameObject treasure;
    public NavMeshAgent agent;
    public PathCreator pathCreator;
    public EndOfPathInstruction endOfPathInstruction;
    public float speed = 5;

    float distanceTravelled;

    void Start()
    {
        if (pathCreator != null)
        {
            distanceTravelled =
                pathCreator.path.GetClosestDistanceAlongPath(transform.position);

            agent.destination =
                pathCreator.path.GetPointAtDistance(
                    distanceTravelled,
                    endOfPathInstruction
                );
        }
    }

    void Update()
    {
        if (Vector3.Distance(treasure.transform.position, robber.transform.position) < 10f)
        {
            agent.destination = robber.transform.position;
            agent.isStopped = false;
        }
        else
        {
            if (agent.remainingDistance > 0.2f)
            {
                distanceTravelled =
                    pathCreator.path.GetClosestDistanceAlongPath(transform.position);

                agent.destination =
                    pathCreator.path.GetPointAtDistance(
                        distanceTravelled,
                        endOfPathInstruction
                    );
            }
            else
            {
                agent.isStopped = true;

                if (pathCreator != null)
                {
                    distanceTravelled += speed * Time.deltaTime;

                    transform.position =
                        pathCreator.path.GetPointAtDistance(
                            distanceTravelled,
                            endOfPathInstruction
                        );

                    transform.rotation =
                        pathCreator.path.GetRotationAtDistance(
                            distanceTravelled,
                            endOfPathInstruction
                        );
                }
            }
        }
    }
}


// ============================================================================
// DIAPO 45 - Flocking Manager / creació dels peixos
// ============================================================================

allFish = new GameObject[numFish];

for (int i = 0; i < numFish; ++i)
{
    Vector3 pos = this.transform.position + ...; // random position
    Vector3 randomize = ...;                     // random vector direction

    allFish[i] = (GameObject)Instantiate(
        fishPrefab,
        pos,
        Quaternion.LookRotation(randomize)
    );

    allFish[i].GetComponent<Flock>().myManager = this;
}


// ============================================================================
// DIAPO 46 - Flocking Rule I: Cohesion
// ============================================================================

Vector3 cohesion = Vector3.zero;
int num = 0;

foreach (GameObject go in myManager.allFish)
{
    if (go != this.gameObject)
    {
        float distance = Vector3.Distance(
            go.transform.position,
            transform.position
        );

        if (distance <= myManager.neighbourDistance)
        {
            cohesion += go.transform.position;
            num++;
        }
    }
}

if (num > 0)
    cohesion = (cohesion / num - transform.position).normalized * speed;


// ============================================================================
// DIAPO 47 - Flocking Rule II: Match velocity / Align
// ============================================================================

Vector3 align = Vector3.zero;
int num = 0;

foreach (GameObject go in myManager.allFish)
{
    if (go != this.gameObject)
    {
        float distance = Vector3.Distance(
            go.transform.position,
            transform.position
        );

        if (distance <= myManager.neighbourDistance)
        {
            align += go.GetComponent<Flock>().direction;
            num++;
        }
    }
}

if (num > 0)
{
    align /= num;
    speed = Mathf.Clamp(
        align.magnitude,
        myManager.minSpeed,
        myManager.maxSpeed
    );
}


// ============================================================================
// DIAPO 48 - Flocking Rule III: Separation
// ============================================================================

Vector3 separation = Vector3.zero;

foreach (GameObject go in myManager.allFish)
{
    if (go != this.gameObject)
    {
        float distance = Vector3.Distance(
            go.transform.position,
            transform.position
        );

        if (distance <= myManager.neighbourDistance)
            separation -= (transform.position - go.transform.position) /
                          (distance * distance);
    }
}


// ============================================================================
// DIAPO 49 - Combinació de Flocking + Update
// ============================================================================

// Combination
direction = (cohesion + align + separation).normalized * speed;

// Update
transform.rotation = Quaternion.Slerp(
    transform.rotation,
    Quaternion.LookRotation(direction),
    myManager.rotationSpeed * Time.deltaTime
);

// NOTA: a la diapositiva apareix literalment "Time.deltaTime speed"
// (sense operador entre els dos). Ho deixo documentat aquí tal com surt:
// transform.Translate(0.0f, 0.0f, Time.deltaTime speed);

// Forma C# sintàcticament vàlida que probablement es pretenia:
transform.Translate(0.0f, 0.0f, Time.deltaTime * speed);


// ============================================================================
// DIAPO 58 - Dijkstra + A* sobre un Graph
// DIAPO 60 - La presentació repeteix el mateix script destacant A*
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Graph
{
    public HashSet<string> vertices = new HashSet<string>
    {
        "a", "b", "c", "d", "e", "f"
    };

    public Dictionary<string, Dictionary<string, int>> edges =
        new Dictionary<string, Dictionary<string, int>>
        {
            { "a", new Dictionary<string, int> { { "b", 4 },  { "c", 2 } } },
            { "b", new Dictionary<string, int> { { "c", 5 },  { "d", 10 } } },
            { "c", new Dictionary<string, int> { { "e", 3 } } },
            { "d", new Dictionary<string, int> { { "f", 11 } } },
            { "e", new Dictionary<string, int> { { "d", 4 } } },
            { "f", new Dictionary<string, int> { } }
        };

    public Dictionary<string, int> h = new Dictionary<string, int>
    {
        { "a", 20 },
        { "b", 18 },
        { "c", 12 },
        { "d", 10 },
        { "e", 9 },
        { "f", 0 }
    };
}

public class shortestPath : MonoBehaviour
{
    void Start()
    {
        Graph g = new Graph();

        Show(Dijkstra(g, "a", "f"));
        Show(Astar(g, "a", "f"));
    }

    List<string> Dijkstra(Graph g, string source, string target)
    {
        var d = g.vertices.ToDictionary(v => v, v => 1000);
        var prev = g.vertices.ToDictionary(v => v, v => " ");

        d[source] = 0;

        HashSet<String> Q = new HashSet<string>(g.vertices);

        while (Q.Count() > 0)
        {
            string v = Q.Select(x => (d[x], x)).Min().Item2;
            Q.Remove(v);

            foreach (var pair in g.edges[v])
            {
                int alt = d[v] + pair.Value;

                if (alt < d[pair.Key])
                {
                    d[pair.Key] = alt;
                    prev[pair.Key] = v;
                }
            }
        }

        List<string> path = new List<string>();
        path.Insert(0, target);

        while (prev[target] != " ")
        {
            target = prev[target];
            path.Insert(0, target);
        }

        return path;
    }

    List<string> Astar(Graph g, string source, string target)
    {
        var d = g.vertices.ToDictionary(v => v, v => 1000);
        var prev = g.vertices.ToDictionary(v => v, v => " ");

        d[source] = 0;

        HashSet<String> Q = new HashSet<string>(g.vertices);

        while (Q.Count() > 0)
        {
            string v = Q.Select(x => (d[x] + g.h[x], x)).Min().Item2;
            Q.Remove(v);

            foreach (var pair in g.edges[v])
            {
                int alt = d[v] + pair.Value;

                if (alt < d[pair.Key])
                {
                    d[pair.Key] = alt;
                    prev[pair.Key] = v;
                }
            }
        }

        List<string> path = new List<string>();
        path.Insert(0, target);

        while (prev[target] != " ")
        {
            target = prev[target];
            path.Insert(0, target);
        }

        return path;
    }

    void Show(List<string> l)
    {
        string s = "Path:\n";

        foreach (var x in l)
            s += " " + x;

        Debug.Log(s);
    }
}
