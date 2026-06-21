using UnityEngine;

// Play basladiginda UI yerlesimini otomatik uygular; Setup Main UI menusune gerek kalmaz.
public static class GameUiBootstrap
{
    private static bool hasScheduledDeferredApply;

    /// <summary>Canvas uzerinde GameUILayout kurar ve temayi uygular.</summary>
    public static void EnsureApplied()
    {
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameUILayout layout = canvas.GetComponent<GameUILayout>();
        if (layout == null)
            layout = canvas.gameObject.AddComponent<GameUILayout>();

        layout.ApplyLayout();
    }

    /// <summary>Sahne yuklenince bir sonraki karede layout uygular (Awake zinciri tamamlansin diye).</summary>
    public static void ScheduleDeferredApply()
    {
        if (hasScheduledDeferredApply || !Application.isPlaying) return;
        hasScheduledDeferredApply = true;

        GameObject runner = new GameObject("GameUiBootstrapRunner");
        runner.hideFlags = HideFlags.HideAndDontSave;
        runner.AddComponent<DeferredApplyRunner>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterSceneLoad()
    {
        hasScheduledDeferredApply = false;
        if (Application.isPlaying)
            ScheduleDeferredApply();
    }

    /// <summary>Bir kare bekleyip layout uygulayan gecici runner.</summary>
    private sealed class DeferredApplyRunner : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(ApplyNextFrame());
        }

        private System.Collections.IEnumerator ApplyNextFrame()
        {
            yield return null;
            EnsureApplied();
            Destroy(gameObject);
        }
    }
}
