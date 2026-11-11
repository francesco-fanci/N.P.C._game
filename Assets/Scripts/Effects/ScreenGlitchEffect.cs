using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Triggers temporary screen glitch impulses (heavy chromatic aberration,
/// camera shake, and color flash) when the player performs glitch exploits
/// like Wall Phasing or falls into the void.
/// </summary>
public class ScreenGlitchEffect : MonoBehaviour
{
    public static ScreenGlitchEffect Instance { get; private set; }

    [Header("Volume Reference")]
    [SerializeField]
    private Volume _postProcessVolume;

    private ChromaticAberration _chromaticAberration;
    private LensDistortion _lensDistortion;
    private Coroutine _glitchRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_postProcessVolume == null)
        {
            _postProcessVolume = GetComponent<Volume>();
        }

        if (_postProcessVolume != null && _postProcessVolume.profile != null)
        {
            _postProcessVolume.profile.TryGet(out _chromaticAberration);
            _postProcessVolume.profile.TryGet(out _lensDistortion);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Triggers a brief glitch burst (chromatic tear + distortion).
    /// </summary>
    public void TriggerGlitch(float duration = 0.4f, float intensity = 1f)
    {
        if (_glitchRoutine != null)
        {
            StopCoroutine(_glitchRoutine);
        }
        _glitchRoutine = StartCoroutine(GlitchRoutine(duration, intensity));
    }

    private IEnumerator GlitchRoutine(float duration, float intensity)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float pulse = Mathf.Sin(t * Mathf.PI); // 0 -> 1 -> 0

            if (_chromaticAberration != null)
            {
                _chromaticAberration.intensity.value = Mathf.Lerp(0.1f, intensity, pulse);
            }

            if (_lensDistortion != null)
            {
                _lensDistortion.intensity.value = Mathf.Lerp(0f, -0.35f * intensity, pulse);
            }

            yield return null;
        }

        if (_chromaticAberration != null) _chromaticAberration.intensity.value = 0.1f;
        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;
    }
}
