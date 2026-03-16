using System;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType { Tap /*, Hold, Swipe*/ }

[CreateAssetMenu(menuName = "Rhythm/Beat Map", fileName = "BeatMap")]
public class BeatMapSO : ScriptableObject
{
    public AudioClip song;
    [Tooltip("Offset en segundos para ajustar sincronía (positivo retrasa notas, negativo las adelanta).")]
    public float offsetSeconds = 0f;

    public List<NoteData> notes = new();

    [Serializable]
    public struct NoteData
    {
        public float time;      // segundos desde inicio de la canción (sin offset)
        public int lane;        // 0..3
        public NoteType type;   // Tap por ahora
    }
}