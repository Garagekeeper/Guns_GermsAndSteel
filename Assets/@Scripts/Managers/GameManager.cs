using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameManager
{
    private CameraController _cam;
    public CameraController Cam
    {
        get
        {
            if (_cam == null)
            {
                _cam = Object.FindObjectOfType<CameraController>();
            }

            return _cam;
        }
    }

    public event Action<int, int, string> ChargeBarEnevnt;
    public void UseActiveItem(int currentGage, int coolTime, string type)
    {
        ChargeBarEnevnt?.Invoke(currentGage, coolTime, type);
    }

    #region MAP_GENERATING

    private string SeedString = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public string Seed { get; private set; }
    public long Sn { get; set; }
    //백준 30043의 수치를 참고하였음
    public int A { get; private set; } = 1103515245;
    public int C { get; private set; } = 12345;
    public long M { get; private set; } = (long)1 << 31;

    public int Rand_Cnt { get; private set; } = 0;

    public int N { get; set; }

    public int StageNumber { get; set; } = 1;

    int _baseRoomCountMax = 20;
    int _baseRoomCountMin = 10;


    public void Init()
    {
        Seed = GenerateSeed();
        SeedToInt();
        Debug.Log(Seed);
        Rand_Cnt = 0;
        StageNumber = 1;
        //0. N(스테이지에 만들 방의 개수) 설정
        //N = (int)(Sn % ((_baseRoomCountMax - _baseRoomCountMin) + 1 + StageNumber * 2) + _baseRoomCountMin);
        //https://gist.github.com/bladecoding/d75aef7e830c738ad5e3d66d146a095c
        //위 링크와는 다르게 특수방의 개수가 늘어났기 때문에 적절한 수치 변경
        N = Math.Min(_baseRoomCountMax, RandInt(0, 1) + 7 + ((StageNumber * _baseRoomCountMin) / 3));
        //Debug.Log(N);
    }

    private string GenerateSeed()
    {
        string temp = "";
        for (int i = 0; i < 9; i++)
        {
            int tempInt = UnityEngine.Random.Range(0, SeedString.Length);
            if (i == 4)
                temp += "-";
            else
                temp += SeedString[tempInt];
        }

        return temp;
    }

    private void SeedToInt()
    {
        int temp = 0;
        long res = 0;
        long power = 1;
        for (int i = 8; i >= 0; i--)
        {
            if (Seed[i] == '-') continue;
            if (Seed[i] < 'A')
                temp += Seed[i] - '0';
            if (Seed[i] >= 'A')
                temp += 10 + (Seed[i] - 'A');

            res = res + temp * power;
            power = power * 36;
            temp = 0;
        }
        Sn = res % M;
    }

    #region RAND_FUNCTIONS
    public void Rand()
    {
        // Sn+1 = (A x Sn + C ) Mod M
        Sn = (A * Sn + C) % M;
        Rand_Cnt++;
    }

    public int RandInt(int left, int right)
    {
        Rand();
        return (int)((Sn % (right - left + 1)) + left);
    }

    // P보다 작을 경우 true;
    public bool Chance(int p)
    {

        return (RandInt(1, 100) <= p);
    }

    public T Choice<T>(List<T> rooms)
    {
        int cnt = rooms.Count();
        int temp = RandInt(0, cnt - 1);
        return rooms[temp];
    }


    #endregion

    #endregion


    public void GoToNextRoom(string dir)
    {
        RoomClass currentRoom = Managers.Map.CurrentRoom;
        Vector3 newPos = new Vector3();
        int index = 0;
        if (currentRoom == null)
        {
            Debug.Log("Err Current Room is Null!");
        }


        switch (dir)
        {
            case "Right":
                index = 0;
                break;
            case "Down":
                index = 1;
                break;
            case "Left":
                index = 2;
                break;
            case "Up":
                index = 3;
                break;
        }
        newPos.x = currentRoom._adjacencentRooms[index].XPos;
        newPos.y = currentRoom._adjacencentRooms[index].YPos;


        //playerMove
        MovePlayerToNextRoom(index);
        //CameraMove
        MoveCameraToNextRoom(currentRoom._adjacencentRooms[index]);

        Managers.Map.CurrentRoom = currentRoom._adjacencentRooms[index];
        //Managers.UI.PlayingUI.BossHpActive(true);

        //ItemHolder에 있는 아이템의 비중을 줄인다.
        if (Managers.Map.CurrentRoom.ItemHolder != null)
        {
            int TemplateId = Managers.Map.CurrentRoom.ItemHolder.GetComponent<ItemHolder>().ItemId;
            Managers.Data.ItemDic[TemplateId].Weight = 0;
        }

        foreach (var temp in Managers.Object.MainCharacters)
        {
            if (temp.OneTimeActive)
                WithdrawOneTimeItemEffect(temp);
            temp.CanMove = true;
        }

        if (currentRoom.IsClear == false)
        {
            Managers.Map.SpawnMonsterAndBossInRoom(currentRoom);
        }

        RoomConditionCheck();
    }

    public void GoToNextStage()
    {
        foreach (var temp in Managers.Object.MainCharacters)
        {
            temp.CanMove = false;
            temp.gameObject.SetActive(false);
        }

        StageNumber++;
        Managers.Map.DestroyMap();
        while (true)
        {
            if (Managers.Map.GenerateStage() == 1)
                break;
        }
        Managers.Map.LoadMap();

        Cam.MoveCameraWithoutLerp(new Vector3(-0.5f, -0.5f, -10f));
        foreach (var temp in Managers.Object.MainCharacters)
        {
            temp.gameObject.SetActive(true);
            temp.transform.position = new Vector3(-0.5f, -0.5f, 0);
            temp.CanMove = true;
        }

        RoomConditionCheck();
    }

    public void MovePlayerToNextRoom(int index)
    {
        Vector3 newPos = new Vector3();
        switch (index)
        {
            case 0:
                newPos = new Vector3(5.0f, 0);
                break;
            case 1:
                newPos = new Vector3(0, -5.5f);
                break;
            case 2:
                newPos = new Vector3(-5.0f, 0);
                break;
            case 3:
                newPos = new Vector3(0, 5.5f);
                break;
        }

        foreach (var temp in Managers.Object.MainCharacters)
        {
            temp.transform.position = temp.transform.position + newPos;
        }
    }

    public void MoveCameraToNextRoom(RoomClass nextRoom)
    {
        Vector3 newPos = nextRoom.Transform.position;
        newPos.z = -10;
        Managers.Game.Cam.TargetPos = newPos + new Vector3(-0.5f, -0.5f, 0);
    }

    public void TPToNormalRandom()
    {
        List<RoomClass> list = new List<RoomClass>();
        foreach (RoomClass r in Managers.Map.Rooms)
        {
            if (r.RoomType == RoomClass.ERoomType.Normal && r != Managers.Map.CurrentRoom)
            {
                list.Add(r);
            }
        }

        RoomClass chosen = Choice(list);
        Vector3 newPos = chosen.Transform.position + new Vector3(-0.5f, -0.5f, 0);
        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.CanMove = false;
        }

        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.transform.position = newPos;
        }

        Managers.Map.CurrentRoom = chosen;

        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.CanMove = true;
        }

        if (Managers.Map.CurrentRoom.IsClear == false)
        {
            Managers.Map.SpawnMonsterAndBossInRoom(Managers.Map.CurrentRoom);
        }

        newPos.z = -10f;
        Cam.MoveCameraWithoutLerp(newPos);
        RoomConditionCheck();
    }

    public int SlectItem()
    {
        int TemplateId;
        while (true)
        {
            TemplateId = RandInt(45001, 45044);
            if (Managers.Data.ItemDic.ContainsKey(TemplateId) && Managers.Data.ItemDic[TemplateId].Weight != 0)
                break;
        }
        //TODO 하드코딩 수정
        return TemplateId;
    }

    public void WithdrawOneTimeItemEffect(MainCharacter player)
    {
        player.OneTimeActive = false;
        player.Hp -= player.SpaceItem.Hp;
        player.AttackDamage -= player.SpaceItem.AttackDamage;
        player.Tears -= player.SpaceItem.Tears;
        player.Range -= player.SpaceItem.Range;
        player.ShotSpeed -= player.SpaceItem.ShotSpeed;
        player.Speed -= player.SpaceItem.Speed;
        player.Luck -= player.SpaceItem.Luck;
        player.Life -= player.SpaceItem.Life;
        //item.SetItem;
        //item.ShotType;
    }

    //방의 클리어 조건을 충족했을 때 실행되는 함수
    //DoorTile이 열린 모양으로 변경됨
    public void RoomClear()
    {
        var curRoom = Managers.Map.CurrentRoom;
        curRoom.IsClear = true;

        Managers.Map.ChangeDoorSprite(curRoom);
        foreach (var player in Managers.Object.MainCharacters)
        {
            if (player.OneTimeActive)
                Managers.Game.WithdrawOneTimeItemEffect(player);
        }

        if (curRoom.RoomType == RoomClass.ERoomType.Boss)
        {
            //Managers.Map.CurrentRoom.ItemHolder.SetActive(true);
        }
    }

    public void RoomConditionCheck()
    {
        if (Managers.Object.Monsters.Count == 0 && Managers.Object.Bosses.Count == 0)
            RoomClear();
        return;
    }

    public void ClearGame()
    {
        Managers.Map.DestroyMap();
        SceneManager.LoadScene("Title");
    }

    public void RestartGame()
    {
        Init();
        Managers.Map.DestroyMap();
        while (true)
        {
            if (Managers.Map.GenerateStage() == 1)
                break;
        }
        Managers.Map.LoadMap();

        Cam.MoveCameraWithoutLerp(new Vector3(-0.5f, -0.5f, -10f));
        foreach (var temp in Managers.Object.MainCharacters)
        {
            temp.gameObject.SetActive(true);
            temp.transform.position = new Vector3(-0.5f, -0.5f, 0);
            temp.CanMove = true;
        }
        RoomConditionCheck();

    }
}
