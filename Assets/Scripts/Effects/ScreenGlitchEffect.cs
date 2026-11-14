// N.P.C. (Null Pointer Character) - Screen glitch effect using URP volume overrides
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenGlitchEffect : MonoBehaviour
{
    public static ScreenGlitchEffect Instance;

    [Header("Volume Reference")]
    [SerializeField]
    private Volume _postProcessVolume;

    private ChromaticAberration _chromaticAberration;
    private LensDistortion _lensDistortion;
    private Coroutine _glitchRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (_postProcessVolume == null)
        {
            _postProcessVolume = GetComponent<Volume>();
        }

        // Cache the post processing overrides from the volume profile
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

    // Call this to trigger a brief screen glitch burst
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
            float pulse = Mathf.Sin(t * Mathf.PI); // Smooth curve: 0 -> 1 -> 0

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

        // Reset to default baseline values
        if (_chromaticAberration != null) _chromaticAberration.intensity.value = 0.1f;
        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;
    }
}
