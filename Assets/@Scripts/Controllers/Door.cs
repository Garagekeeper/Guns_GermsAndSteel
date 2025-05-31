using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utility;
using static Define;
using System;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using Unity.VisualScripting;
using UnityEngine.UI;

public class Door : BaseObject, IExplodable
{
    // RDLU
    public GameObject[] DoorsGameObject { get; private set; } = new GameObject[4];

    public Animator[] Animators { get; private set; } = new Animator[4];


    public ERoomType[] _eDoorType { get; private set; } = new ERoomType[4];
    public EDoorState[] eDoorState { get; private set; } = { EDoorState.Closed, EDoorState.Closed, EDoorState.Closed, EDoorState.Closed };

    private Dictionary<string, int> _dirIndex = new Dictionary<string, int> { { "Right", 0 }, { "Down", 1 }, { "Left", 2 }, { "Up", 3 } };

    public override void Init()
    {
        for (int i = 0; i < 4; i++)
        {
            DoorsGameObject[i] = transform.GetChild(i).gameObject;
            Animators[i] = DoorsGameObject[i].GetComponent<Animator>();
        }
    }

    // Door에 사용될 애니메이터 설정
    public void SetAnimator(int index, ERoomType doorType, ERoomType roomType, string animatorName)
    {
        RuntimeAnimatorController animController;
        if ((animController = Managers.Resource.Load<RuntimeAnimatorController>(animatorName)) == null) return;

        _eDoorType[index] = doorType;
        //Animators[index].runtimeAnimatorController = new AnimatorOverrideController(animController);
        //Animators[index].runtimeAnimatorController = Instantiate(animController);
        Animators[index].runtimeAnimatorController = animController;

        EDoorState state = EDoorState.Closed;



        if (doorType == ERoomType.Curse) DoorsGameObject[index].tag = "SpikeDoor";
        if (doorType == ERoomType.Secret)
        {
            state = EDoorState.Hidden;
            DoorsGameObject[index].tag = "HoleInWall";
        }

        if (Managers.Game.StageNumber > 1)
        {
            if (roomType == ERoomType.Normal || roomType == ERoomType.Start)
            {
                if (doorType == ERoomType.Gold || doorType == ERoomType.Shop)
                {
                    Animators[index].Play("KeyClosed");
                    state = EDoorState.KeyClosed;
                }
                //TODO Coin Closed
            }
        }

        eDoorState[index] = state;
    }

    // 모든 문 열기
    public void TryOpenAll()
    {
        for (int i = 0; i < 4; i++)
        {
            if (DoorsGameObject[i].activeSelf == false) continue;

            if (eDoorState[i] == EDoorState.Closed)
                Open(i);
        }
        FindChildByName(transform, "TrapDoor")?.gameObject.SetActive(true);
        FindChildByName(transform, "ClearBox")?.gameObject.SetActive(true);
    }

    // 문 열기
    public void Open(int index, bool forced = false, MainCharacter Player = null)
    {
        string clipName = "";
        string audioClipNmae = "";
        AudioClip audioClip = null;

        if (eDoorState[index] == EDoorState.Closed)
        {
            clipName = "Open";
            audioClipNmae = "door heavy open";
        }
        // 코인을 사용해서 열어야하는 방
        else if (eDoorState[index] == EDoorState.CoinClosed)
        {
            // 아이템등에 의해서 강제로 열릴 때
            if (forced)
            {
                clipName = "CoinOpen";
                eDoorState[index] = EDoorState.Opened;
                audioClipNmae = "door heavy open";
            }
            // 플레이어에 의해서 열릴때
            else if (Player != null)
            {
                // 보유한 코인이 있어야 열림
                if (Player.Coin > 0)
                {
                    clipName = "CoinOpen";
                    audioClipNmae = "unlock";
                    Player.AddCoin(-1);
                    eDoorState[index] = EDoorState.Opened;
                }
            }
        }
        // 열쇠를 사용해서 열어야하는 방
        else if (eDoorState[index] == EDoorState.KeyClosed)
        {

            // 아이템등에 의해서 강제로 열릴 때
            if (forced)
            {
                clipName = "KeyOpen";
                audioClipNmae = "door heavy open";
                eDoorState[index] = EDoorState.Opened;
            }
            // 플레이어에 의해서 열릴때
            else if (Player != null)
            {
                //TODO GoldenKey
                // 보유한 열쇠가 있어야 열림
                if (Player.KeyCount > 0)
                {
                    clipName = "KeyOpen";
                    audioClipNmae = "unlock";
                    Player.AddKey(-1);
                    eDoorState[index] = EDoorState.Opened;
                }
            }
        }


        if (clipName == "") return;
        if (audioClipNmae != "")
        {
            audioClip = Managers.Resource.Load<AudioClip>(audioClipNmae);
            Managers.Sound.PlaySFX(audioClip, 0.2f);
        }

        Animators[index].Play(clipName);
        StartCoroutine(Copened(index));
    }

    public void Opened(int index)
    {
        Animators[index].Play("Opened");
        eDoorState[index] = EDoorState.Opened;
    }

    IEnumerator Copened(int index)
    {
        yield return null;
        float delay = Animators[index].GetCurrentAnimatorClipInfo(0)[0].clip.length;
        yield return new WaitForSeconds(delay);
        Animators[index].Play("Opened");
        eDoorState[index] = EDoorState.Opened;
    }

    // 
    public void ClosedAll()
    {
        string clipName = "Closed";
        for (int index = 0; index < 4; index++)
        {
            // 문이 없으면 넘어감
            if (Animators[index].gameObject.activeSelf == false) continue;
            // 비밀방인경우 클립이 다름
            if (_eDoorType[index] == ERoomType.Secret)
            {
                clipName = "Close";
            }
            Animators[index].Play(clipName);

            eDoorState[index] = EDoorState.Closed;
        }
    }

    public void CloseAll()
    {
        for (int i = 0; i < 4; i++)
        {
            if (DoorsGameObject[i].activeSelf == false) return;

            Close(i);
        }
    }

    public void Close(int index)
    {
        string audioClipNmae = "door heavy close";
        string clipName = "Close";
        AudioClip audioClip = null;
        if (eDoorState[index] == EDoorState.Hidden) return;

        if (audioClipNmae != "")
        {
            audioClip = Managers.Resource.Load<AudioClip>(audioClipNmae);
            Managers.Sound.PlaySFX(audioClip, 0.1f);
        }

        StartCoroutine(CClosed(index));
        Animators[index].Play(clipName);
    }

    IEnumerator CClosed(int index)
    {
        yield return null;
        float delay = Animators[index].GetCurrentAnimatorClipInfo(0)[0].clip.length;
        yield return new WaitForSeconds(delay);
        Animators[index].Play("Closed");
        eDoorState[index] = EDoorState.Opened;
    }

    // 폭탄 등에 의해서 문이 파괴
    public void Break(string dir)
    {
        int index = _dirIndex[dir];
        string clipName = "";

        if (eDoorState[index] == EDoorState.Opened) clipName = "BrokenOpen";
        if (eDoorState[index] == EDoorState.Closed) clipName = "Break";
        if (eDoorState[index] == EDoorState.Hidden)
        {
            if (Managers.Map.CurrentRoom.IsClear)
            {
                clipName = "Opened";

                // 넘어갈 다음 방 문을 열기
                var adjRoom = Managers.Map.CurrentRoom._adjacencentRooms[index];
                adjRoom.Doors.GetComponent<Door>().Open((index + 2) % 4, true);

                if (Managers.Map.CurrentRoom.RoomType != ERoomType.Secret)
                {
                    AudioClip audioClip = Managers.Resource.Load<AudioClip>("secret room find v2_07");
                    Managers.Sound.PlaySFX(audioClip, 0.1f);
                }
            }
        }

        Animators[index].Play(clipName);
        StartCoroutine(CBroken(index));
    }

    IEnumerator CBroken(int index)
    {
        yield return null;
        float delay = Animators[index].GetCurrentAnimatorClipInfo(0)[0].clip.length;
        yield return new WaitForSeconds(delay);
        if (eDoorState[index] == EDoorState.Hidden) 
        { 
            eDoorState[index] = EDoorState.Opened;
            // 비밀방을 열면
            // 반대편 문도 열어주기
            var adjRoom = Managers.Map.CurrentRoom._adjacencentRooms[index];
            adjRoom.Doors.GetComponent<Door>().Opened((index + 2) % 4);

            //미니맵도 밝혀주기
            string[] cellSprite = { "minimap1_4", "minimap1_3", "minimap1_2", };
            var minimap = Managers.UI.PlayingUI.GetMinimapPannel();
            var cell = minimap.transform.Find(adjRoom.RoomObject.name);
            string spriteName = "";

            if (adjRoom.RoomType != ERoomType.Normal && adjRoom.RoomType != ERoomType.Start)
            {
                //roomIcon
                GameObject temp = cell.GetChild(0).gameObject;
                temp.SetActive(true);
                temp.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("minimap_icons_" + (int)adjRoom.RoomType);
            }

            if (adjRoom.IsClear)
                spriteName = cellSprite[(int)ECellType.Visited];
            else
                spriteName = cellSprite[(int)ECellType.UnVisited];

            Managers.Map.ChangeMinimapCellSprite(cell.gameObject, spriteName);


            yield break; 
        }
        Animators[index].Play("BrokenOpen");
        eDoorState[index] = EDoorState.BrokenOpen;
    }

    public void OnExplode(Creature own)
    {
        
    }

    // 폭탄에 의해서 문이 파괴될때
    public void OnExplode(Creature own, object args)
    {
        Break(args as string);
    }
}
