using UnityEngine;

public class SlowTime : MonoBehaviour
{
    public Transform SelectionPanel;

    public AnimationCurve myAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float transitionSpeed = 1f;
    public float minTimeScale = 0.2f;

    public bool slowTime;
    private float slowTimeTimer;

    void Update()
    {
        float dir = slowTime ? -1f : 1f;

        slowTimeTimer = Mathf.Clamp(slowTimeTimer + Time.unscaledDeltaTime * transitionSpeed * dir, 0, 1);
        float curve = myAnimationCurve.Evaluate(slowTimeTimer);
        Time.timeScale = curve;
        SelectionPanel.localScale = Vector3.one * (1-Mathf.Clamp((curve-.8f) *2, 0, 1))/2;
    }
}
