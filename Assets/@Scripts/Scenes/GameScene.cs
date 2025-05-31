using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;
using static Utility;

public class GameScene : MonoBehaviour
{
    enum Eclips
    {
        TitleMusicJinger,
        TheCellarIntro,
        TheCellar,
        TheCavesIntro,
        TheCaves,
        TheDepthsIntro,
        TheDepths,
        YouDied,
        SecretRoomAlt,
        Casteportcullis,
        BasicBossFight,
        BossFightJingleOutro,
    }


    GameObject LoadImage;
    GameObject Bg;

    AudioSource _audioSource;

    public List<AudioClip> _audioclips;

    private void Awake()
    {
        LoadImage = FindChildByName(transform, "LoadingImage").gameObject;
        Bg = FindChildByName(transform, "BG").gameObject;

        LoadImage.SetActive(true);
        Bg.SetActive(true);

        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0;
        _audioSource.volume = 0.05f;


        LoadImage.GetComponent<Animator>().Play("loadimages-" + Random.Range(1, 57).ToString("D3"));
        _audioSource.clip = _audioclips[(int)Eclips.TitleMusicJinger];
        _audioSource.Play();

        Managers.Game.GameScene = this;

        StartCoroutine(CLoading());
    }

    public void RestartAuodioSource()
    {
        _audioSource.Stop();
        _audioSource.clip = null;
        PlayStageBGM();
    }

    // 로딩 종료
    IEnumerator CEndLoad()
    {
        yield return new WaitForSeconds(1);
        SpawnCharacter();
        Managers.Game.RoomConditionCheck();

        LoadImage.SetActive(false);
        Bg.SetActive(false);

        Managers.UI.PlayingUI.gameObject.SetActive(true);
        float delay = _audioSource.clip.length;
        yield return new WaitForSeconds(delay);
        PlayStageBGM();

    }

    // 로딩 시작
    IEnumerator CLoading()
    {
        yield return null;

        LoadUI();
        Managers.UI.PlayingUI.gameObject.SetActive(false);
        Managers.Map.Init(() =>
        {
            StartCoroutine(CEndLoad());
        });
    }

    public void LoadUI()
    {
        GameObject parent = GameObject.Find("UI");

        GameObject go = Managers.Resource.Instantiate("PlayingUI", parent.transform);
        go.name = "PlayingUI";
        Managers.UI.PlayingUI = go.GetComponent<PlayingUI>();


        go = Managers.Resource.Instantiate("GameOverUI", parent.transform);
        go.name = "GameOverUI";
        Managers.UI.GameOverUI = go.GetComponent<GameOverUI>();

        go = Managers.Resource.Instantiate("PauseUI", parent.transform);
        go.name = "PauseUI";
        Managers.UI.PauseUI = go.GetComponent<PauseUI>();

    }

    public void DeleteUI()
    {
        GameObject parent = GameObject.Find("UI");
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }

        Managers.UI.PlayingUI = null;
        Managers.UI.GameOverUI = null;
        Managers.UI.PauseUI = null;
    }

    public void SpawnCharacter()
    {
        MainCharacter mc = Managers.Object.Spawn<MainCharacter>(Vector3.zero);
    }

    public void LoadMiniMap()
    {
        Managers.Map.GenerateMinimap();
    }

    public void PlayStageBGM()
    {

        int enumIndex = Managers.Game.StageNumber switch
        {
            >= 1 and <= 2 => 2,
            >= 3 and <= 4 => 4,
            >= 5 and <= 6 => 6,
            _ => -1
        };

        if (Managers.Map.CurrentRoom.RoomType == ERoomType.Secret)
        {
            _audioSource.clip = _audioclips[(int)Eclips.SecretRoomAlt]; 
            _audioSource.loop = true;
            _audioSource.Play();
        }
        else if (Managers.Map.CurrentRoom.RoomType == ERoomType.Boss)
        {
            if (Managers.Map.CurrentRoom.IsClear)
            {
                Managers.Sound.PlaySFX(_audioclips[(int)Eclips.BossFightJingleOutro], 0.1f);
                _audioSource.loop = true;
                _audioSource.Stop();
                StartCoroutine(CPlayBGMLoop(enumIndex));
            }
            else
            {
                StopPlayingBG();
                Managers.Sound.PlaySFX(_audioclips[(int)Eclips.Casteportcullis]);
                _audioSource.clip = _audioclips[(int)Eclips.BasicBossFight];
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }
        else
        {
            // 현재 재생중인 오디오 클립의 이름
            string playingNowName = "";
            if (_audioSource.clip != null)
                playingNowName = _audioSource.clip.name;


            if (enumIndex == -1) return;

            AudioClip audioClipIntro = _audioclips[enumIndex - 1];
            AudioClip audioClip = _audioclips[enumIndex];

            // 기존에 재생하던거면 넘어가기
            if (playingNowName == audioClip.name || playingNowName == audioClipIntro.name) return;

            StartCoroutine(CPlayBGMLoop(enumIndex));
        }
    }

    IEnumerator CPlayBGMLoop(int enumIndex)
    {
        float delay = 0;
        yield return new WaitWhile(() => (_audioSource.clip == _audioclips[(int)Eclips.TitleMusicJinger] && _audioSource.isPlaying));


        _audioSource.clip = _audioclips[enumIndex - 1];
        _audioSource.loop = false;
        _audioSource.Play();
        delay = _audioSource.clip.length;
        yield return new WaitForSeconds(delay);
        _audioSource.loop = true;
        _audioSource.clip = _audioclips[enumIndex];
        _audioSource.Play();
    }

    public void StopPlayingBG()
    {
        _audioSource.Stop();
        _audioSource.clip = null;
        _audioSource.loop = false;
        StopAllCoroutines();
    }

    public void OnDisable()
    {

    }

}
