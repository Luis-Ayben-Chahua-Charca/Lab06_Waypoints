# Lab 06 – Waypoints

Laboratorio de Desarrollo de Software para Juegos (DSJ) 2026B, Escuela de Ingeniería de Sistemas, UNSA. Sesión 6: Waypoints.

Proyecto de Unity en el que vehículos controlados por la computadora recorren un circuito de waypoints. Incluye la práctica guiada con tanques, el ejercicio del progress tracker y un circuito de carreras de autos.

## Contenido

| Escena | Descripción |
| --- | --- |
| `Assets/Scenes/TankRacing.unity` | Práctica guiada y Ejercicio 1: cuatro tanques recorren un circuito formado por 9 palmeras. Con `Use Tracker` activado siguen a un progress tracker. |
| `Assets/Scenes/CarRacing.unity` | Ejercicio 2: carrera de autos sobre una pista armada con el Racing Kit de Kenney, con carriles, distancia de seguridad y conteo de vueltas. |

## Requisitos

- Unity `6000.6.2f1`
- Universal Render Pipeline `17.6.0`
- Input System `1.20.0` (el proyecto usa solo el Input System nuevo)

## Cómo ejecutarlo

1. Clona el repositorio y ábrelo desde Unity Hub con la versión indicada arriba.
2. Abre una escena de `Assets/Scenes`.
3. Pulsa Play.

Para ocultar o mostrar la ruta amarilla de los waypoints en `CarRacing`, usa el casillero `Show Path` del componente `WaypointCircuit` o la tecla **G** durante la ejecución.

## Scripts (`Assets/Scripts`)

| Script | Función |
| --- | --- |
| `FollowWP` | Hace que un tanque recorra un arreglo de waypoints. Con `Use Tracker` sigue a un tracker que recorre el circuito delante de él, en vez de perseguir los waypoints directamente. |
| `WaypointCircuit` | Contenedor del circuito: cada hijo es un waypoint, en el orden de la jerarquía. Dibuja la ruta con una línea y gizmos, y permite mostrarla u ocultarla. |
| `CarFollowWP` | Auto que recorre un `WaypointCircuit`. Usa el progress tracker, ignora la altura, tiene carril propio (`Lane Offset`), frena si hay otro auto delante y cuenta vueltas. |
| `RaceManager` | Muestra en pantalla la vuelta de cada auto y el orden de llegada con su tiempo. |

### Cómo funciona

En cada fotograma el vehículo:

1. Pasa al siguiente waypoint cuando está a menos de `wpRadius` del actual. Al llegar al último vuelve al primero.
2. Gira hacia el objetivo con `Quaternion.Slerp`.
3. Avanza en su eje Z local con `Translate`.

Con el progress tracker, el objetivo no es el waypoint sino un objeto auxiliar que recorre el circuito por su cuenta, sin física, y espera si el vehículo se atrasa más de `lookAhead`.

### Parámetros importantes

- **`wpRadius`:** radio de llegada al waypoint. Debe ser mayor que el radio de giro del vehículo, aproximadamente `speed / rotSpeed`, o el vehículo orbita el waypoint sin alcanzarlo.
- **`lookAhead`:** distancia máxima a la que el tracker se aleja del vehículo.
- **`laneOffset`:** desplazamiento lateral de cada auto respecto a la ruta. Usa uno distinto por auto.
- **`totalLaps`:** número de vueltas de la carrera (0 = infinitas).

Las distancias están en unidades del mundo, así que dependen de la escala de la escena. En `CarRacing` los autos tienen escala 0,2 y los valores son mucho menores que en `TankRacing`.

## Créditos

- Recursos del tutorial "Waypoints" (tanques, palmeras y escena `TankRacing`): [Unity Learn](https://learn.unity.com/tutorial/waypoints?uv=2019.4&courseId=5dd851beedbc2a1bf7b72bed&projectId=5e0b6dd4edbc2a00200e3641).
- Pista, autos y decorados: Racing Kit de [Kenney](https://kenney.nl), licencia CC0 1.0.
- Millington, I., & Funge, J. (2019). *Artificial Intelligence for Games* (3.ª ed.). CRC Press.

## Autor

Luis Ayben Chahua Charca, UNSA.
