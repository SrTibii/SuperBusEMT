using UnityEngine;
using UnityEngine.EventSystems;

public class NoteView : MonoBehaviour, IPointerDownHandler
{
    [Header("References")]
    [SerializeField] private RectTransform approachRing;

    [HideInInspector] public int lane;
    [HideInInspector] public double hitDspTime;
    [HideInInspector] public bool active;

    private RhythmGameManager manager;

    // Ajusta estos valores a ojo (OSU feel)
    [SerializeField] private float startScale = 2.2f;
    [SerializeField] private float endScale = 1.0f;

    public void Init(RhythmGameManager mgr, int laneIndex, double dspTime, float leadTimeSeconds)
    {
        manager = mgr;
        lane = laneIndex;
        hitDspTime = dspTime;
        active = true;

        gameObject.SetActive(true);

        if (approachRing != null)
            approachRing.localScale = Vector3.one * startScale;
    }

    public void SetApproach(float t01)
    {
        if (approachRing == null) return;

        float s = Mathf.Lerp(startScale, endScale, t01);
        approachRing.localScale = Vector3.one * s;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!active) return;
        if (manager == null) return;

        manager.TryHitNote(this);
    }

    public void Despawn()
    {
        active = false;
        gameObject.SetActive(false);

        if (manager != null)
            manager.ReturnToPool(this);
    }
}