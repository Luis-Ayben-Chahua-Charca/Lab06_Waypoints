using UnityEngine;

public class FollowWP : MonoBehaviour
{
    // Waypoints (por ejemplo, las palmeras) que forman el circuito
    [SerializeField] private GameObject[] waypoints;

    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float rotSpeed = 10.0f;

    // Distancia a la que se considera que el waypoint fue alcanzado
    [SerializeField] private float wpRadius = 3.0f;

    [Header("Progress Tracker (Ejercicio 1)")]
    // Desactivado: el tanque sigue directamente los waypoints (práctica guiada).
    // Activado: el tanque sigue a un tracker que recorre el circuito delante de él.
    [SerializeField] private bool useTracker = false;
    [SerializeField] private bool showTracker = true;
    // Distancia máxima a la que el tracker puede alejarse del tanque
    [SerializeField] private float lookAhead = 10.0f;

    // Índice del waypoint actual
    private int currentWP = 0;
    private GameObject tracker;

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Vector3 target;

        if (useTracker)
        {
            if (tracker == null)
                CreateTracker();

            ProgressTracker();
            target = tracker.transform.position;
        }
        else
        {
            AdvanceWaypoint(transform.position);
            target = waypoints[currentWP].transform.position;
        }

        // Gira suavemente hacia el objetivo (waypoint o tracker)
        Vector3 direction = target - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion lookatTarget = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookatTarget, rotSpeed * Time.deltaTime);
        }

        // Avanza hacia adelante en su eje local Z
        transform.Translate(0, 0, speed * Time.deltaTime);
    }

    // Pasa al siguiente waypoint cuando 'from' está a menos de wpRadius (y vuelve al inicio al terminar)
    private void AdvanceWaypoint(Vector3 from)
    {
        if (Vector3.Distance(from, waypoints[currentWP].transform.position) < wpRadius)
        {
            currentWP++;
            if (currentWP >= waypoints.Length)
                currentWP = 0;
        }
    }

    // El tracker recorre el circuito por su cuenta, sin física, y espera si el tanque se atrasa
    private void ProgressTracker()
    {
        if (Vector3.Distance(tracker.transform.position, transform.position) > lookAhead)
            return;

        AdvanceWaypoint(tracker.transform.position);

        tracker.transform.LookAt(waypoints[currentWP].transform);
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

    private void OnDestroy()
    {
        if (tracker != null)
            Destroy(tracker);
    }
}
