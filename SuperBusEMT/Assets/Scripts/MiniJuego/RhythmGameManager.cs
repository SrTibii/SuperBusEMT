using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    [Header("Beatmap")]
    [SerializeField] private BeatMapSO beatMap;
    [SerializeField] private AudioSource audioSource;

    [Header("Spawn/Visual")]
    [Tooltip("Cuánto antes aparece la nota en pantalla (segundos).")]
    [SerializeField] private float leadTime = 1.2f;

    [Tooltip("Prefab de la nota (muy simple).")]
    [SerializeField] private NoteView notePrefab;

    [Tooltip("Puntos de spawn por lane (4 transforms).")]
    [SerializeField] private RectTransform[] laneSpawnPoints = new RectTransform[4];
    [SerializeField] private RectTransform[] laneHitPoints = new RectTransform[4];

    [SerializeField] private TMPro.TMP_Text comboText;
    [SerializeField] private TMPro.TMP_Text judgeText;

    [Header("Judgement Windows (seconds)")]
    [SerializeField] private float perfectWindow = 0.05f;
    [SerializeField] private float goodWindow = 0.12f;

    [Header("Optimization")]
    [SerializeField] private int poolSize = 64;

    [SerializeField] private RectTransform notesParent; // arrastra aquí el Canvas (o un contenedor dentro)
    

    // runtime
    private readonly Queue<NoteView> pool = new();
    private readonly List<NoteView> activeNotes = new(); // pocas notas activas, vale
    private int nextNoteIndex;
    private double songStartDsp;
    private bool playing;

    // score
    public int combo { get; private set; }
    public int score { get; private set; }

    private void Awake()
    {
        // Pool
        for (int i = 0; i < poolSize; i++)
        {
            var n = Instantiate(notePrefab, notesParent);
            n.gameObject.SetActive(false);
            pool.Enqueue(n);
        }

        // Si aún no hay beatmap, no ordenar
        if (beatMap != null)
            beatMap.notes.Sort((a, b) => a.time.CompareTo(b.time));
    }

    public void StartSong()
    {
        nextNoteIndex = 0;
        activeNotes.Clear();
        combo = 0;
        score = 0;

        songStartDsp = AudioSettings.dspTime + 0.1;
        playing = true;

        if (beatMap != null && beatMap.song != null && audioSource != null)
        {
            audioSource.clip = beatMap.song;
            audioSource.PlayScheduled(songStartDsp);
        }
        // Si no hay song, el juego sigue usando songStartDsp como referencia de tiempo.
    }

    private void Update()
    {
        if (beatMap == null || !playing) return;

        double now = AudioSettings.dspTime;
        double songTime = now - songStartDsp; // segundos desde inicio real

        // 1) Spawn de notas cuando están a leadTime de entrar
        while (nextNoteIndex < beatMap.notes.Count)
        {
            var note = beatMap.notes[nextNoteIndex];
            double noteTime = note.time + beatMap.offsetSeconds;

            if (noteTime - leadTime <= songTime)
            {
                Spawn(note.lane, noteTime);
                nextNoteIndex++;
            }
            else break;
        }

        // 2) Actualizar visual (mover notas hacia el punto de hit)
        // t = 0 cuando spawnea, t = 1 cuando llega al hit
        for (int i = 0; i < activeNotes.Count; i++)
        {
            var n = activeNotes[i];
            if (!n.active) continue;

            float t = 1f - (float)((n.hitDspTime - now) / leadTime);
            t = Mathf.Clamp01(t);

            RectTransform noteRT = (RectTransform)n.transform;

            Vector2 from = laneSpawnPoints[n.lane].anchoredPosition;
            Vector2 to = laneHitPoints[n.lane].anchoredPosition;

            noteRT.anchoredPosition = Vector2.LerpUnclamped(from, to, t);
        }

        // 3) Miss automático: si ya pasó el goodWindow sin pulsar
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            var n = activeNotes[i];
            if (!n.active) { activeNotes.RemoveAt(i); continue; }

            double error = (now - n.hitDspTime);
            if (error > goodWindow)
            {
                RegisterMiss();
                n.Despawn();
                activeNotes.RemoveAt(i);
            }
        }

        // 4) Fin canción (si hay clip)
        // if (audioSource != null && audioSource.clip != null && songTime > audioSource.clip.length + 0.5)
        //     playing = false;
    }

    private void Spawn(int lane, double noteTimeSecondsFromSongStart)
    {
        if (lane < 0 || lane >= laneSpawnPoints.Length) return;

        if (pool.Count == 0)
        {
            // Si te quedas sin pool, mejor no instanciar en móvil: simplemente no spawneas.
            return;
        }

        var n = pool.Dequeue();

        // Colocar visualmente
        n.transform.position = laneSpawnPoints[lane].position;
        n.transform.rotation = laneSpawnPoints[lane].rotation;

        // Convertir noteTime (tiempo de canción) a dsp absoluto
        double hitDsp = songStartDsp + noteTimeSecondsFromSongStart;
        n.Init(this, lane, hitDsp);

        activeNotes.Add(n);
    }

    // Llamado por UI/teclado
    public void Hit(int lane)
    {
        if (!playing) return;

        double now = AudioSettings.dspTime;

        // Busca la nota activa más cercana en esa lane
        NoteView best = null;
        double bestAbsError = double.MaxValue;

        for (int i = 0; i < activeNotes.Count; i++)
        {
            var n = activeNotes[i];
            if (!n.active || n.lane != lane) continue;

            double absError = System.Math.Abs(now - n.hitDspTime);
            if (absError < bestAbsError)
            {
                bestAbsError = absError;
                best = n;
            }
        }

        if (best == null)
        {
            RegisterMiss();
            return;
        }

        if (bestAbsError <= perfectWindow)
        {
            RegisterHit(300);
            RemoveActive(best);
        }
        else if (bestAbsError <= goodWindow)
        {
            RegisterHit(100);
            RemoveActive(best);
        }
        else
        {
            RegisterMiss();
        }
    }

    private void RemoveActive(NoteView n)
    {
        n.Despawn();
        activeNotes.Remove(n);
    }

    private void RegisterHit(int points)
    {
        combo++;
        score += points + combo;

        if (comboText) comboText.text = $"Combo: {combo}";
        if (judgeText) judgeText.text = (points == 300) ? "PERFECT" : "GOOD";
    }

    private void RegisterMiss()
    {
        combo = 0;
        if (comboText) comboText.text = $"Combo: {combo}";
        if (judgeText) judgeText.text = "MISS";
    }

    public void ReturnToPool(NoteView n)
    {
        pool.Enqueue(n);
    }
}