using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{

    private int _initialPoolsize;
    private Queue<GameObject> _sfxPool;
    private GameObject _parent;

    public void Init()
    {
        _parent = GameObject.Find("@SoundPool");

        _initialPoolsize = 30;
        _sfxPool = new Queue<GameObject>();
        InitPool();
    }

    public void InitPool()
    {
        for (int i = 0; i < _initialPoolsize; i++)
            AddNewSFXSource();
    }

    public void AddNewSFXSource()
    {
        GameObject sfxobj = Managers.Resource.Instantiate("sfx",_parent.transform);
        ReturnSFXToPool(sfxobj);
    }

    public void ReturnSFXToPool(GameObject go)
    {
        if (go == null) return;
        if (go.activeSelf == false) return;
        _sfxPool.Enqueue(go);
        go.SetActive(false);
    }

    public GameObject GetSFXFromPool()
    {
        if (_sfxPool.Count == 0)
            AddNewSFXSource();

        GameObject sfx = _sfxPool.Dequeue();
        sfx.SetActive(true);

        return sfx;
    }

    public GameObject PlaySFX(AudioClip clip, float volume = 1f, bool isLoop = false)
    {
        GameObject sfx = GetSFXFromPool();
        sfx.GetComponent<SFXSource>().Play(clip, volume, isLoop);
        return sfx;
    }

    public void ReturnSFXToPoolAll()
    {
        foreach (Transform childTransform in _parent.transform)
        {
            if (_sfxPool.Count > _initialPoolsize) break;
            ReturnSFXToPool(childTransform.gameObject);
        }
    }
}
