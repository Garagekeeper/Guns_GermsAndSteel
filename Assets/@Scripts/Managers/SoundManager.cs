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
        // GameScene으로 넘어오면서 @SoundPool 오브젝트가 DontDestroyOnLoad에 생성됨
        _parent = GameObject.Find("@SoundPool");
        _initialPoolsize = 30;
        _sfxPool = new Queue<GameObject>();
        InitPool();
    }

    public void InitPool()
    {
        if (_sfxPool.Count >= _initialPoolsize) return;

        // Pool에 미리 생성
        for (int i = 0; i < _initialPoolsize; i++)
            AddNewSFXSource();
    }

    public void AddNewSFXSource()
    {
        //오브젝트 인스턴스화 후에 큐에 집어넣음
        GameObject sfxobj = Managers.Resource.Instantiate("sfx",_parent.transform);
        ReturnSFXToPool(sfxobj);
    }

    public void ReturnSFXToPool(GameObject go)
    {
        // 큐에 넣을때는 active false로
        if (go == null) return;
        if (go.activeSelf == false) return;
        _sfxPool.Enqueue(go);
        go.SetActive(false);
    }

    public GameObject GetSFXFromPool()
    {
        // 큐에 남아있지 않으면 추가
        if (_sfxPool.Count == 0)
            AddNewSFXSource();

        // 큐에서 뽑아내서 반환
        GameObject sfx = _sfxPool.Dequeue();
        sfx.SetActive(true);

        return sfx;
    }

    // 오디오 클립 재생
    public GameObject PlaySFX(AudioClip clip, float volume = 1f, bool isLoop = false)
    {
        GameObject sfx = GetSFXFromPool();
        sfx.GetComponent<SFXSource>().Play(clip, volume, isLoop);
        return sfx;
    }

    // 게임 초기화등에 사용
    public void ReturnSFXToPoolAll()
    {
        foreach (Transform childTransform in _parent.transform)
        {
            if (_sfxPool.Count <= _initialPoolsize)
                ReturnSFXToPool(childTransform.gameObject);
            else 
                GameObject.Destroy(childTransform.gameObject);
        }
    }
}
