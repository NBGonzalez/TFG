using UnityEngine;

// Fíjate que NO hereda de MonoBehaviour. No se arrastra a ningún objeto en Unity.
public static class ItineraryCrossSceneData
{
    // Esta es la "nota" que dejaremos en el tablón.
    // Si vale null, significa que queremos Crear uno nuevo.
    // Si tiene texto (ej: "custom-12345"), significa que queremos Editar ese.
    public static string itineraryIdToEdit = null;
}