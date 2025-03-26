using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using static Define;
using static Utility;
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

    public event Action<int, int> ChargeBarEnevnt;
    public void UseActiveItem(int currentGage, int coolTime)
    {
        ChargeBarEnevnt?.Invoke(currentGage, coolTime);
    }

    #region MAP_GENERATING

    public RNGManager RNG;

    private string SeedString = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public string Seed { get; private set; }

    public int N { get; set; }

    public int StageNumber { get; set; } = 1;

    private int _baseRoomCountMax = 15;
    private int _baseRoomCountMin = 10;


    public void Init()
    {
        Seed = GenerateSeed();
        RNG = new RNGManager(Seed);
        Debug.Log(Seed);
        StageNumber = 1;
        //0. N(스테이지에 만들 방의 개수) 설정
        //N = (int)(Sn % ((_baseRoomCountMax - _baseRoomCountMin) + 1 + StageNumber * 2) + _baseRoomCountMin);
        //https://gist.github.com/bladecoding/d75aef7e830c738ad5e3d66d146a095c
        //위 링크와는 다르게 특수방의 개수가 늘어났기 때문에 적절한 수치 변경
        N = Math.Min(_baseRoomCountMax, RNG.RandInt(0, 1) + 5 + ((StageNumber * _baseRoomCountMin) / 5));
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

    #endregion


    public void GoToNextRoom(string dir)
    {
        RoomClass currentRoom = Managers.Map.CurrentRoom;
        Vector3 newPos = new();
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

        Managers.Object.DespawnMonsters(Managers.Map.CurrentRoom);

        Managers.Map.CurrentRoom = currentRoom._adjacencentRooms[index];
        //Managers.UI.PlayingUI.BossHpActive(true);

        //ItemHolder에 있는 아이템의 비중을 줄인다.
        if (Managers.Map.CurrentRoom.ItemHolder != null)
        {
            int TemplateId = Managers.Map.CurrentRoom.ItemHolder.GetComponent<ItemHolder>().ItemOfItemHolder.TemplateId;
            Managers.Data.ItemDic[TemplateId].Weight = 0;
        }

        foreach (var temp in Managers.Object.MainCharacters)
        {
            if (temp.OneTimeActive)
                WithdrawOneTimeItemEffect(temp);
            temp.CanMove = true;
        }

        if (Managers.Map.CurrentRoom.IsClear == false)
        {
            Managers.Map.SpawnMonsterAndBossInRoom(Managers.Map.CurrentRoom);
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

        //Clear and load new map
        Managers.Map.DestroyMap();
        while (true)
        {
            if (Managers.Map.GenerateStage() == 1)
                break;
        }
        Managers.Map.LoadMap();

        //Clear Base Objects All
        {
            Managers.Object.ClearObjectManager(true);
        }

        //Move Camera to new starting room
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
        Vector3 newPos = new();
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
        List<RoomClass> list = new();
        foreach (RoomClass r in Managers.Map.Rooms)
        {
            if (r.RoomType == RoomClass.ERoomType.Normal && r != Managers.Map.CurrentRoom)
            {
                list.Add(r);
            }
        }

        RoomClass chosen = RNG.Choice(list);
        Vector3 newPos = chosen.Transform.position + new Vector3(-0.5f, -0.5f, 0);
        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.Collider.enabled = false;
            mc.CanMove = false;
        }

        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.transform.position = newPos;
        }

        Managers.Object.DespawnMonsters(Managers.Map.CurrentRoom);

        Managers.Map.CurrentRoom = chosen;

        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.CanMove = true;
            mc.Collider.enabled = true;
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
            TemplateId = RNG.RandInt(45001, 45044);
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

        //TODO
        if (curRoom.RoomType == RoomClass.ERoomType.Normal)
            SpawnClearAward(curRoom.AwardSeed);
    }

    public void RoomConditionCheck()
    {
        // already clear
        if (Managers.Map.CurrentRoom.IsClear == true) return;

        if (Managers.Object.Monsters.Count == 0 && Managers.Object.Bosses.Count == 0)
            RoomClear();
        return;
    }

    public void ClearGame()
    {
        Managers.Map.DestroyMap();
        SceneManager.LoadScene("MainScene");
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        //TODO OpenUI
        Managers.UI.GameOverUI.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        Init();

        {
            Managers.Object.ClearObjectManager();
        }

        Managers.Map.DestroyMap();
        while (true)
        {
            if (Managers.Map.GenerateStage() == 1)
                break;
        }
        Managers.Map.LoadMap();

        Cam.MoveCameraWithoutLerp(new Vector3(-0.5f, -0.5f, -10f));

        //for (int i=0; i< Managers.Object.MainCharacters.Count; i++)
        //{
        //    Managers.Object.Despawn(Managers.Object.MainCharacters.);
        //}
        MainCharacter[] players = Managers.Object.MainCharacters.ToArray();
        Managers.Object.MainCharacters.Clear();

        for (int i=0; i<players.Length; i++)
        {
            Managers.Object.Despawn(players[i]);
        }
        //foreach (var temp in Managers.Object.MainCharacters)
        //{
        //    //temp.gameObject.SetActive(true);
        //    //temp.transform.position = new Vector3(-0.5f, -0.5f, 0);
        //    //temp.CanMove = true;
        //    Managers.Object.Despawn(temp);
        //}

        Managers.Object.Spawn<MainCharacter>(new Vector3(-0.5f, -0.5f, 0),0,"Player");
        RoomConditionCheck();
        
        Managers.UI.GameOverUI.gameObject.SetActive(false);

    }

    public void SpawnClearAward(long seed)
    {
        EPICKUP_TYPE pickupAward = EPICKUP_TYPE.PICKUP_NULL;
        //EPICKUP_TYPE pickupAward = EPICKUP_TYPE.PICKUP_CHEST;
        int pickupCount = 1;

        SelectClearAwardTypeAndCount(ref pickupCount, ref pickupAward, seed);

        if (pickupCount > 0 && pickupAward != EPICKUP_TYPE.PICKUP_NULL)
        {
            Vector2 roomCenterPos = Managers.Map.CurrentRoom.WorldCenterPos;
            Transform pickupsTransform = FindChildByName(Managers.Map.CurrentRoom.Transform, "Pickups");

            List<Vector3Int> spawnPoses = SpriralPos(roomCenterPos, pickupCount);
            int done = 0;
            foreach (var spawnPos in spawnPoses)
            {
                if (done >= pickupCount) break;
                if (Managers.Map.CanGo(spawnPos))
                {
                    //spawn
                    Vector3 newSpawnPos = (spawnPos - (Vector3)roomCenterPos);
                    Managers.Object.Spawn<Pickup>(newSpawnPos, pickupAward, pickupsTransform);
                    done++;
                }

            }
        }
    }

    public void SelectClearAwardTypeAndCount(ref int pickupCount, ref EPICKUP_TYPE pickupAward, long seed)
    {
        RNGManager rng = new(seed);
        float pickupPercent = rng.RandFloat();

        //TODO 나중에 luck추가
        //현재는 luck이 5라고 가정
        //원작에서는 XorShift라는 RNG 사용
        pickupPercent = rng.RandFloat() * 5f * 0.1f + pickupPercent;


        if (pickupPercent < 0.22f)
        {
            if (pickupPercent < 0.3f)
            {
                //if (rng.RandInt(3) == 0)
                //   pickupAward = EPICKUP_TYPE.PICKUP_TAROT_CARD;
                //else if (rng.RandInt(2) == 0)
                //    pickupAward = EPICKUP_TYPE.PICKUP_TRINKET;
                //else
                //    pickupAward = EPICKUP_TYPE.PICKUP_PILL;
            }
        }
        else if (pickupPercent < 0.45f)
        {
            pickupAward = EPICKUP_TYPE.PICKUP_COIN;
        }
        //TODO TRINKET_RIB_OF_GREED
        else if (pickupPercent < 0.5f && false)
        {
            pickupAward = EPICKUP_TYPE.PICKUP_COIN;
        }
        //TODO TRINKET_DAEMONS_TAIL
        else if (pickupPercent < 0.6f && !(true && rng.RandInt(5) == 0))
        {
            pickupAward = EPICKUP_TYPE.PICKUP_HEART;
        }
        else if (pickupPercent < 0.8f)
        {
            pickupAward = EPICKUP_TYPE.PICKUP_KEY;
        }
        else if (pickupPercent < 0.95f)
        {
            pickupAward = EPICKUP_TYPE.PICKUP_BOMB;
        }
        else
        {
            pickupAward = EPICKUP_TYPE.PICKUP_CHEST;
        }

        //TODO TRINKET_WATCH_BATTERY
        if (rng.RandInt(20) == 0 || (rng.RandInt(15) == 0 && false))
        {
            //pickupAward = EPICKUP_TYPE.PICKUP_LIL_BATTERY;
        }

        if (rng.RandInt(50) == 0)
            pickupAward = EPICKUP_TYPE.PICKUP_GRAB_BAG;

        /*
        if (player:HasTrinket(TRINKET_ACE_SPADES) and rng:RandomInt(10) == 0) 
		    pickupAward = PICKUP_TAROTCARD
	    else if (player:HasTrinket(TRINKET_SAFETY_CAP) and rng:RandomInt(10) == 0) 
		    pickupAward = PICKUP_PILL
	    else if (player:HasTrinket(TRINKET_MATCH_STICK) and rng:RandomInt(10) == 0) 
		    pickupAward = PICKUP_BOMB
	    else if (player:HasTrinket(TRINKET_CHILDS_HEART) and rng:RandomInt(10) == 0 and (not player:HasTrinket(TRINKET_DAEMONS_TAIL) or rng:RandomInt(5) == 0)) 
		    pickupAward = PICKUP_HEART
	    else if (player:HasTrinket(TRINKET_RUSTED_KEY) and rng:RandomInt(10) == 0) 
		    pickupAward = PICKUP_KEY


        if (player:HasCollectible(COLLECTIBLE_SMELTER) and rng:RandomInt(50) == 0) then
		    pickupAward = PICKUP_TRINKET

        if (player:HasCollectible(COLLECTIBLE_GUPPYS_TAIL)) then
	        if (rng:RandomInt(3) != 0) then
		        if (rng:RandomInt(3) == 0) then
			        pickupAward = PICKUP_NULL
            else
                if (rng:RandomInt(2) != 0) then
			        pickupAward = PICKUP_LOCKEDCHEST
		        else
			        pickupAward = PICKUP_CHEST

        if (player:HasCollectible(COLLECTIBLE_CONTRACT_FROM_BELOW) and pickupAward != PICKUP_TRINKET)
	        pickupCount = player:GetCollectibleNum(COLLECTIBLE_CONTRACT_FROM_BELOW) + 1
	        
            --The chance of getting nothing goes down with each contract exponentially
	        local nothingChance = math.pow(0.666, pickupCount - 1)
	        if (nothingChance * 0.5 > rng:NextFloat()) then
		        pickupCount = 0


         */
        // 난이도 hard
        if (false && pickupAward == EPICKUP_TYPE.PICKUP_HEART)
            if (rng.RandInt(100) >= 35)
                pickupAward = EPICKUP_TYPE.PICKUP_NULL;

        /*
         if (player:HasCollectible(COLLECTIBLE_BROKEN_MODEM) and rng:RandomInt(4) == 0 and pickupCount >= 1 and
		(pickupAward == PICKUP_COIN or pickupAward == PICKUP_HEART or pickupAward == PICKUP_KEY or pickupAward == PICKUP_GRAB_BAG or pickupAward == PICKUP_BOMB)
	        pickupCount = pickupCount + 1

         */

    }

    public void SpawnChestAndGrabBagAward(Pickup pickup)
    {
        List<EPICKUP_TYPE> pickupAward = new();
        List<int> pickupCount = new();

        SelectChestAndGrabBagAwardTypeAndCount(pickupCount, pickupAward, pickup.PickupType);

        if (pickupAward.Count == 0 && pickupCount.Count == 0) return;

        if (pickupAward.Count != pickupCount.Count)
        {
            Debug.Log("SpawnChestAndGrabBagAward Err, Counts are diffrent");
            return;
        }

        float[] dx = { 0.5f, 0, -0.5f, 0 };
        float[] dy = { 0, -0.5f, 0, 0.5f };

        Transform pickupsTransform = FindChildByName(Managers.Map.CurrentRoom.Transform, "Pickups");
        Transform callingTF = pickup.transform;

        for (int i = 0; i < pickupCount.Count; i++)
        {
            for (int j = 0; j < pickupCount[i]; j++)
            {
                float nx = callingTF.localPosition.x + dx[Random.Range(0, 4)];
                float ny = callingTF.localPosition.y + dy[Random.Range(0, 4)];

                Debug.Log(pickupAward[i]);
                Managers.Object.Spawn<Pickup>(new Vector3(nx, ny, 0), pickupAward[i], pickupsTransform);
            }
        }

    }

    public void SelectChestAndGrabBagAwardTypeAndCount(List<int> pickupCount, List<EPICKUP_TYPE> pickupAward, EPICKUP_TYPE epickupType)
    {
        RNGManager rng = new(Managers.Map.CurrentRoom.AwardSeed);

        //Chest

        if (epickupType == EPICKUP_TYPE.PICKUP_CHEST)
        {
            float pickupPercent = rng.RandFloat();
            // 2 - 3 times
            if (pickupPercent < 0.78)
            {
                for (int i = 0; i < 2 + rng.RandInt(1); i++)
                {
                    float percentageForBasePickup = rng.RandFloat();
                    if (percentageForBasePickup < 0.35f)
                    {
                        pickupAward.Add(EPICKUP_TYPE.PICKUP_COIN);
                        pickupCount.Add(rng.RandInt(3) + 1);
                        continue;
                    }
                    else if (percentageForBasePickup < 0.55f)
                    {
                        pickupAward.Add(EPICKUP_TYPE.PICKUP_HEART);
                    }
                    else if (percentageForBasePickup < 0.7f)
                    {
                        pickupAward.Add(EPICKUP_TYPE.PICKUP_KEY);
                    }
                    else
                    {
                        pickupAward.Add(EPICKUP_TYPE.PICKUP_BOMB);
                    }
                    
                    pickupCount.Add(1);
                }
            }
            else if (pickupPercent < 0.88)
            {
                pickupAward.Add(EPICKUP_TYPE.PICKUP_TRINKET);
                pickupCount.Add(1);
            }
            else if (pickupPercent < 0.89)
            {
                pickupAward.Add(EPICKUP_TYPE.PICKUP_CHEST);
                pickupCount.Add(1);
            }
            //TODO Locked Chest
            else if (pickupPercent < 0.90)
            {
                pickupAward.Add(EPICKUP_TYPE.PICKUP_CHEST);
                pickupCount.Add(1);
            }
            else
            {
                pickupAward.Add(EPICKUP_TYPE.PICKUP_PILL);
                pickupCount.Add(1);
            }
        }

        //Grab Bag
        else if (epickupType == EPICKUP_TYPE.PICKUP_GRAB_BAG)
        {
            // 1 - 4 times
            for (int i = 0; i < 1 + rng.RandInt(3); i++)
            {
                int percentage = rng.RandInt(1, 100);

                if (percentage <= 33)
                {
                    pickupAward.Add(EPICKUP_TYPE.PICKUP_COIN);
                }
                else if (percentage <= 66)
                {
                    pickupAward.Add(EPICKUP_TYPE.PICKUP_KEY);
                }
                else
                {
                    pickupAward.Add(EPICKUP_TYPE.PICKUP_BOMB);
                }

                pickupCount.Add(1);

            }
        }
    }
}
