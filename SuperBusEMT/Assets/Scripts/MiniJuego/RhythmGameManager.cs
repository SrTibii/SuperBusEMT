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
    

    [Header("Judgement Windows (seconds)")]
    [SerializeField] private float perfectWindow = 0.10f;
    [SerializeField] private float goodWindow = 0.22f;

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

        // 2) Actualizar visual OSU (encoger approach ring)
        for (int i = 0; i < activeNotes.Count; i++)
        {
            var n = activeNotes[i];
            if (!n.active) continue;

            float t = 1f - (float)((n.hitDspTime - now) / leadTime);
            t = Mathf.Clamp01(t);

            n.SetApproach(t);
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
                n.ShowJudgement("MISS");
                activeNotes.RemoveAt(i);
                n.DespawnAfter(0.25f);
            }
        }

        // 4) Fin canción (si hay clip)
        // if (audioSource != null && audioSource.clip != null && songTime > audioSource.clip.length + 0.5)
        //     playing = false;
        if (nextNoteIndex >= beatMap.notes.Count && activeNotes.Count == 0)
            playing = false;
    }

    private void Spawn(int lane, double noteTimeSecondsFromSongStart)
    {
        if (lane < 0 || lane >= laneHitPoints.Length) return;

        if (pool.Count == 0)
        {
            // En móvil mejor no instanciar en runtime
            return;
        }

        var n = pool.Dequeue();

        // Colocar visualmente en el HIT POINT (OSU-style: posición final)
        RectTransform nrt = (RectTransform)n.transform;

        // Asegura que está dentro del Canvas/NotesParent
        if (notesParent != null)
            nrt.SetParent(notesParent, false);

        nrt.anchoredPosition = laneHitPoints[lane].anchoredPosition;
        nrt.localRotation = Quaternion.identity;
        nrt.localScale = Vector3.one;

        // Convertir noteTime (tiempo de canción) a dsp absoluto
        double hitDsp = songStartDsp + noteTimeSecondsFromSongStart;

        // Inicializa nota (si tu Init aún no recibe leadTime, déjalo como lo tenías)
        n.Init(this, lane, hitDsp, leadTime);

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
        
    }

    private void RegisterMiss()
    {
        combo = 0;
        if (comboText) comboText.text = $"Combo: {combo}";
        
    }

    public void ReturnToPool(NoteView n)
    {
        pool.Enqueue(n);
    }

    public void TryHitNote(NoteView note)
    {
        if (!playing || note == null || !note.active) return;

        double now = AudioSettings.dspTime;
        double signedError = now - note.hitDspTime; // <0 temprano, >0 tarde
        double absError = System.Math.Abs(signedError);

        // Muy temprano: feedback sin castigo
        if (signedError < -goodWindow)
        {
            note.ShowJudgement("EARLY"); // o "ANTES"
            return;
        }

        // PERFECT
        if (absError <= perfectWindow)
        {
            RegisterHit(300);
            note.ShowJudgement("PERFECT");
            activeNotes.Remove(note);
            note.DespawnAfter(0.25f);
            return;
        }

        // GOOD (mantiene combo)
        if (absError <= goodWindow)
        {
            RegisterHit(250); // más friendly que 100 (ajústalo a gusto)
            note.ShowJudgement("GOOD");
            activeNotes.Remove(note);
            note.DespawnAfter(0.25f);
            return;
        }

        // Fuera de ventana:
        // - Si es tarde de verdad -> MISS (rompe combo)
        // - Si está cerca pero fuera (un pelín tarde) -> feedback sin castigo
        if (signedError > goodWindow)
        {
            RegisterMiss();
            note.ShowJudgement("MISS");
        }
        else
        {
            // Está un poco tarde pero no lo suficiente para castigar
            note.ShowJudgement("LATE"); // o "TARDE"
        }
    }
}