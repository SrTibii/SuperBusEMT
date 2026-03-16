using UnityEngine;

public class NoteView : MonoBehaviour
{
    [HideInInspector] public int lane;
    [HideInInspector] public double hitDspTime;
    [HideInInspector] public bool active;

    // Referencia al manager para devolver al pool
    private RhythmGameManager manager;

    public void Init(RhythmGameManager mgr, int laneIndex, double dspTime)
    {
        manager = mgr;
        lane = laneIndex;
        hitDspTime = dspTime;
        active = true;
        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        active = false;
        gameObject.SetActive(false);
        manager.ReturnToPool(this);
    }
}