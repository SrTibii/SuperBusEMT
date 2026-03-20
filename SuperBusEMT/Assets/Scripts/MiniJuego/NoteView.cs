using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class NoteView : MonoBehaviour, IPointerDownHandler
{
    [Header("References")]
    [SerializeField] private RectTransform approachRing;
    [SerializeField] private TMP_Text judgementText;

    [HideInInspector] public int lane;
    [HideInInspector] public double hitDspTime;
    [HideInInspector] public bool active;

    private RhythmGameManager manager;

    [Header("OSU Feel")]
    [SerializeField] private float startScale = 2.2f;
    [SerializeField] private float endScale = 1.0f;

    [Header("Judgement UI")]
    [SerializeField] private float judgementDuration = 0.4f;

    private float judgementHideTime;
    private bool despawnScheduled;

    public void Init(RhythmGameManager mgr, int laneIndex, double dspTime, float leadTimeSeconds)
    {
        StopAllCoroutines();
        despawnScheduled = false;
        manager = mgr;
        lane = laneIndex;
        hitDspTime = dspTime;
        active = true;

        gameObject.SetActive(true);

        if (approachRing != null)
            approachRing.localScale = Vector3.one * startScale;

        // reset judgement
        if (judgementText != null)
            judgementText.text = "";

        judgementHideTime = 0f;
    }

    public void SetApproach(float t01)
    {
        if (approachRing == null) return;

        float s = Mathf.Lerp(startScale, endScale, t01);
        approachRing.localScale = Vector3.one * s;
    }

    public void ShowJudgement(string msg)
    {
        if (judgementText == null) return;

        judgementText.text = msg;
        judgementHideTime = Time.unscaledTime + judgementDuration;
    }

    public void DespawnAfter(float seconds)
    {
        if (!active || despawnScheduled) return;
        despawnScheduled = true;
        StartCoroutine(DespawnRoutine(seconds));
    }

    private System.Collections.IEnumerator DespawnRoutine(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Despawn();
    }

    private void Update()
    {
        // Esto es muy barato: solo hace algo si hay un judgement activo
        if (judgementHideTime <= 0f) return;

        if (Time.unscaledTime >= judgementHideTime)
        {
            if (judgementText != null) judgementText.text = "";
            judgementHideTime = 0f;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!active || manager == null) return;
        manager.TryHitNote(this);
        Debug.Log("Note clicked");
    }

    public void Despawn()
    {
        active = false;
        gameObject.SetActive(false);

        if (manager != null)
            manager.ReturnToPool(this);
    }
}