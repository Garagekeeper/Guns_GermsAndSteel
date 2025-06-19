using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDeathEffect : MonoBehaviour
{
    // Start is called before the first frame update
    public void PlaySmallBurst()
    {
        AudioClip audioClip = Managers.Resource.Load<AudioClip>($"death burst small {Random.Range(0,3)}");
        Managers.Sound.PlaySFX(audioClip, 0.5f);
    }

    public void LargeBurst()
    {
        AudioClip audioClip = Managers.Resource.Load<AudioClip>($"death_burst_large_{Random.Range(0, 2)}");
        Managers.Sound.PlaySFX(audioClip, 0.5f);
    }
}
