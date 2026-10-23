using UnityEngine;

// Contenedor del circuito: cada hijo de este objeto es un waypoint, en el orden de la jerarquía.
public class WaypointCircuit : MonoBehaviour
{
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private float gizmoRadius = 1.0f;
    [SerializeField] private bool closedLoop = true;

    public int Count
    {
        get { return transform.childCount; }
    }

    // El índice se "envuelve", así que GetPosition(Count) devuelve el primer waypoint
    public Vector3 GetPosition(int index)
    {
        return transform.GetChild(index % transform.childCount).position;
    }

    private void OnDrawGizmos()
    {
        int count = transform.childCount;
        if (count == 0)
            return;

        Gizmos.color = gizmoColor;

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
