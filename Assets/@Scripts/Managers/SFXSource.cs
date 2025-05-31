using System.Collections;
using UnityEngine;

public class SFXSource : MonoBehaviour
{
    private AudioSource _audioSource;
    private Coroutine _coroutine;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        // 2D 라서 0
        _audioSource.spatialBlend = 0f;
    }

    public void Play(AudioClip clip, float volume = 1f, bool isLoop = false)
    {
        StopAllCoroutines();

        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.loop = isLoop;
        _audioSource.Play();

        if (!isLoop)
            _coroutine = StartCoroutine(CDisableAfterPlay());
    }

    private IEnumerator CDisableAfterPlay()
    {
        yield return new WaitForSeconds(_audioSource.clip.length + 0.1f);
        Managers.Sound.ReturnSFXToPool(gameObject);
    }

    private void OnDisable()
    {

    }
}
