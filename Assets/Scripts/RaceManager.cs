using System.Linq;
using UnityEngine;

// Muestra en pantalla las vueltas de cada auto y el orden de llegada.
public class RaceManager : MonoBehaviour
{
    // Si se deja vacío, busca todos los CarFollowWP de la escena
    [SerializeField] private CarFollowWP[] cars;
    [SerializeField] private int fontSize = 20;

    private GUIStyle style;

    private void Start()
    {
        if (cars == null || cars.Length == 0)
            cars = FindObjectsByType<CarFollowWP>();
    }

    private void OnGUI()
    {
        if (cars == null)
            return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.normal.textColor = Color.white;
        }

        float y = 10;
        int place = 1;

        // Primero los que ya terminaron, ordenados por tiempo; luego los demás
        foreach (CarFollowWP car in cars.Where(c => c.Finished).OrderBy(c => c.FinishTime))
        {
            GUI.Label(new Rect(10, y, 600, 30), place + ". " + car.name + "  -  " + car.FinishTime.ToString("F1") + " s", style);
            y += fontSize + 8;
            place++;
        }

        foreach (CarFollowWP car in cars.Where(c => !c.Finished))
        {
            string laps = car.TotalLaps > 0
                ? (car.LapsCompleted + 1) + "/" + car.TotalLaps
                : (car.LapsCompleted + 1).ToString();

            GUI.Label(new Rect(10, y, 600, 30), car.name + "  -  Vuelta " + laps, style);
            y += fontSize + 8;
        }
    }
}
