using System.Collections.Generic;
using UnityEngine;

// Auto que recorre un circuito de waypoints, con progress tracker opcional, carril propio,
// frenado al tener otro auto delante y conteo de vueltas.
public class CarFollowWP : MonoBehaviour
{
    [SerializeField] private WaypointCircuit circuit;

    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float rotSpeed = 6.0f;

    // Distancia a la que se considera que el waypoint fue alcanzado
    [SerializeField] private float wpRadius = 1.0f;

    // Ignora la diferencia de altura entre el auto y los waypoints
    [SerializeField] private bool ignoreHeight = true;

    [Header("Progress Tracker")]
    [SerializeField] private bool useTracker = true;
    [SerializeField] private bool showTracker = false;
    // Distancia máxima a la que el tracker puede alejarse del auto
    [SerializeField] private float lookAhead = 2.5f;

    [Header("Carril y distancia entre autos")]
    // Desplazamiento lateral respecto al centro de la ruta (positivo = derecha). Usa uno distinto por auto.
    [SerializeField] private float laneOffset = 0.0f;
    // Si hay un auto delante a menos de esta distancia, empieza a frenar
    [SerializeField] private float followDistance = 1.5f;
    // A esta distancia o menos, el auto se detiene hasta que el de adelante se aleje
    [SerializeField] private float stopDistance = 0.6f;
    // Ancho del "pasillo" delante del auto donde se detectan otros autos
    [SerializeField] private float laneWidth = 0.4f;

    [Header("Vueltas")]
    // 0 = infinitas
    [SerializeField] private int totalLaps = 3;

    private static readonly List<CarFollowWP> allCars = new List<CarFollowWP>();

    private int currentWP = 0;
    private int lapsCompleted = 0;
    private bool finishing = false;
    private bool finished = false;
    private float startTime;
    private float finishTime;
    private GameObject tracker;

    public int LapsCompleted { get { return lapsCompleted; } }
    public int TotalLaps { get { return totalLaps; } }
    public bool Finished { get { return finished; } }
    public float FinishTime { get { return finishTime; } }

    private void OnEnable()
    {
        allCars.Add(this);
    }

    private void OnDisable()
    {
        allCars.Remove(this);
    }

    private void Start()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        if (finished || circuit == null || circuit.Count == 0)
            return;

        Vector3 target;

        if (useTracker)
        {
            if (tracker == null)
                CreateTracker();

            if (!finishing)
                ProgressTracker();

            // Al terminar las vueltas el tracker se detiene y el auto lo alcanza
            if (finishing && Distance(transform.position, tracker.transform.position) < wpRadius + Mathf.Abs(laneOffset))
            {
                Finish();
                return;
            }

            target = ApplyLane(tracker.transform.position, tracker.transform.forward);
        }
        else
        {
            AdvanceWaypoint(transform.position);

            if (finishing)
            {
                Finish();
                return;
            }

            Vector3 wp = circuit.GetPosition(currentWP);
            target = ApplyLane(wp, wp - transform.position);
        }

        MoveTowards(target);
    }

    private void MoveTowards(Vector3 target)
    {
        Vector3 direction = Flat(target - transform.position);

        if (direction != Vector3.zero)
        {
            Quaternion lookatTarget = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookatTarget, rotSpeed * Time.deltaTime);
        }

        transform.Translate(0, 0, speed * GetSpeedFactor() * Time.deltaTime);
    }

    // Desplaza el punto objetivo hacia el lado de su carril
    private Vector3 ApplyLane(Vector3 point, Vector3 pathDirection)
    {
        Vector3 flatDirection = pathDirection;
        flatDirection.y = 0;

        if (flatDirection == Vector3.zero || laneOffset == 0)
            return point;

        Vector3 right = Vector3.Cross(Vector3.up, flatDirection.normalized);
        return point + right * laneOffset;
    }

    // 1 = velocidad normal, 0 = detenido. Baja si hay otro auto delante, en el mismo pasillo.
    private float GetSpeedFactor()
    {
        float factor = 1.0f;

        Vector3 forward = Flat(transform.forward).normalized;
        Vector3 right = Flat(transform.right).normalized;

        foreach (CarFollowWP other in allCars)
        {
            if (other == this || other.finished)
                continue;

            Vector3 toOther = Flat(other.transform.position - transform.position);
            float dist = toOther.magnitude;

            if (dist < 0.0001f || dist > followDistance)
                continue;

            // Debe estar delante y dentro del pasillo
            if (Vector3.Dot(forward, toOther / dist) < 0.7f)
                continue;

            if (Mathf.Abs(Vector3.Dot(right, toOther)) > laneWidth)
                continue;

            factor = Mathf.Min(factor, Mathf.InverseLerp(stopDistance, followDistance, dist));
        }

        return factor;
    }

    // Pasa al siguiente waypoint; al volver al primero se completa una vuelta
    private void AdvanceWaypoint(Vector3 from)
    {
        if (Distance(from, circuit.GetPosition(currentWP)) < wpRadius)
        {
            currentWP++;

            if (currentWP >= circuit.Count)
            {
                currentWP = 0;
                lapsCompleted++;

                if (totalLaps > 0 && lapsCompleted >= totalLaps)
                    finishing = true;
            }
        }
    }

    private void ProgressTracker()
    {
        if (Distance(tracker.transform.position, transform.position) > lookAhead)
            return;

        AdvanceWaypoint(tracker.transform.position);

        Vector3 direction = Flat(circuit.GetPosition(currentWP) - tracker.transform.position);
        if (direction != Vector3.zero)
            tracker.transform.rotation = Quaternion.LookRotation(direction);

        tracker.transform.Translate(0, 0, (speed + 2) * Time.deltaTime);
    }

    private void CreateTracker()
    {
        tracker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tracker.name = name + "_Tracker";
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.GetComponent<MeshRenderer>().enabled = showTracker;
        tracker.transform.position = transform.position;
        tracker.transform.rotation = transform.rotation;
    }

    private void Finish()
    {
        finished = true;
        finishTime = Time.time - startTime;
    }

    private Vector3 Flat(Vector3 v)
    {
        if (ignoreHeight)
            v.y = 0;
        return v;
    }

    private float Distance(Vector3 a, Vector3 b)
    {
        return Flat(a - b).magnitude;
    }

    private void OnDestroy()
    {
        if (tracker != null)
            Destroy(tracker);
    }
}
