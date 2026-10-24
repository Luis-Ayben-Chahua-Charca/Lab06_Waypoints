using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Contenedor del circuito: cada hijo de este objeto es un waypoint, en el orden de la jerarquía.
// La ruta (líneas amarillas) se puede mostrar u ocultar con el checkbox "Show Path" o con una tecla.
public class WaypointCircuit : MonoBehaviour
{
    [Header("Visualización de la ruta")]
    [SerializeField] private bool showPath = true;

#if ENABLE_INPUT_SYSTEM
    // Tecla para mostrar/ocultar la ruta durante la ejecución
    [SerializeField] private Key toggleKey = Key.G;
#else
    [SerializeField] private KeyCode toggleKey = KeyCode.G;
#endif

    [FormerlySerializedAs("gizmoColor")]
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private float lineWidth = 0.1f;
    // Elevación de la línea sobre los waypoints para que no se mezcle con la pista
    [SerializeField] private float lineHeight = 0.05f;
    [SerializeField] private float gizmoRadius = 1.0f;
    [SerializeField] private bool closedLoop = true;

    private LineRenderer line;

    public int Count
    {
        get { return transform.childCount; }
    }

    // El índice se "envuelve", así que GetPosition(Count) devuelve el primer waypoint
    public Vector3 GetPosition(int index)
    {
        return transform.GetChild(index % transform.childCount).position;
    }

    private void Awake()
    {
        CreateLine();
    }

    private void Update()
    {
        if (TogglePressed())
            showPath = !showPath;

        RefreshLine();
    }

    private bool TogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return toggleKey != Key.None && Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame;
#else
        return Input.GetKeyDown(toggleKey);
#endif
    }

    // Línea visible en la vista Game (los Gizmos solo se ven si están activados en el editor)
    private void CreateLine()
    {
        line = GetComponent<LineRenderer>();
        if (line == null)
            line = gameObject.AddComponent<LineRenderer>();

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");

        line.material = new Material(shader);
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    private void RefreshLine()
    {
        if (line == null)
            return;

        int count = transform.childCount;
        line.enabled = showPath && count > 1;

        if (!line.enabled)
            return;

        line.loop = closedLoop;
        line.startColor = pathColor;
        line.endColor = pathColor;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = count;

        for (int i = 0; i < count; i++)
            line.SetPosition(i, transform.GetChild(i).position + Vector3.up * lineHeight);
    }

    private void OnDrawGizmos()
    {
        int count = transform.childCount;
        if (!showPath || count == 0)
            return;

        Gizmos.color = pathColor;

        for (int i = 0; i < count; i++)
        {
            Vector3 current = transform.GetChild(i).position;
            Gizmos.DrawWireSphere(current, gizmoRadius);

            if (i < count - 1)
                Gizmos.DrawLine(current, transform.GetChild(i + 1).position);
            else if (closedLoop && count > 1)
                Gizmos.DrawLine(current, transform.GetChild(0).position);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(current + Vector3.up * (gizmoRadius + 0.5f), i.ToString());
#endif
        }
    }

#if UNITY_EDITOR
    // Clic derecho sobre el componente > "Agregar waypoint". También sirve duplicar un waypoint con Ctrl+D.
    [ContextMenu("Agregar waypoint")]
    private void AddWaypoint()
    {
        GameObject go = new GameObject("WP_" + transform.childCount);
        go.transform.SetParent(transform, false);

        if (transform.childCount > 1)
            go.transform.position = transform.GetChild(transform.childCount - 2).position + Vector3.forward * 5.0f;

        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Agregar waypoint");
        UnityEditor.Selection.activeGameObject = go;
    }
#endif
}
