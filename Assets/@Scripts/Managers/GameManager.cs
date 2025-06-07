using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
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

    public RNGManager RNG;

    private string SeedString = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public string Seed { get; private set; }

    public int N { get; set; }

    public int StageNumber { get; set; } = 1;
    public int MAXStageNumber { get; private set; } = 5;

    private int _baseRoomCountMax = 15;
    private int _baseRoomCountMin = 10;

    public GameScene GameScene { get; set; }

    public void Init()
    {
        Seed = GenerateSeed();
        RNG = new RNGManager(Seed);
        Debug.Log(Seed);
        StageNumber = 1;
        //0. N(스테이지에 만들 방의 개수) 설정
        //https://gist.github.com/bladecoding/d75aef7e830c738ad5e3d66d146a095c
        //위 링크와는 다르게 특수방의 개수가 늘어났기 때문에 적절한 수치 변경
        N = Math.Min(_baseRoomCountMax, RNG.RandInt(0, 1) + 5 + ((StageNumber * _baseRoomCountMin) / 5));
    }

    /// <summary>
    /// 게임에서 사용될 시드문자 생성
    /// </summary>
    /// <returns>시드가 문자열로 반환</returns>
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

    /// <summary>
    /// (클리어된 상태의) 문에 접촉하면 호출되는 함수
    /// </summary>
    /// <param name="dir">접촉한 문의 이름</param>
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
        // 미클리어후 폭탄, 아이템등 어떤 이유로 나가면 문을 닫힌상태로
        if (currentRoom.IsClear == false)
        {
            currentRoom.Doors.GetComponent<Door>().ClosedAll();
        }

        // 현재 맵 변경
        Managers.Map.CurrentRoom = currentRoom._adjacencentRooms[index];
        //Managers.UI.PlayingUI.BossHpActive(true);

        //ItemHolder에 있는 아이템의 비중을 줄인다.
        if (Managers.Map.CurrentRoom.ItemHolder != null && Managers.Map.CurrentRoom.ItemHolder.GetComponent<ItemHolder>().ItemOfItemHolder != null)
        {
            int TemplateId = Managers.Map.CurrentRoom.ItemHolder.GetComponent<ItemHolder>().ItemOfItemHolder.TemplateId;
            Managers.Data.ItemDic[TemplateId].Weight = 0;
        }


        if (Managers.Map.CurrentRoom.IsClear == false)
        {
            Managers.Map.SpawnPickUp(Managers.Map.CurrentRoom);
            Managers.Map.SpawnMonsterAndBossInRoom(Managers.Map.CurrentRoom, () =>
            {
                CoroutineHelper.Instance.StartMyCoroutine(WaitSpawn());
            });
            // stage bgm
            GameScene.PlayStageBGM();
        }
        else
        {
            CoroutineHelper.Instance.StartMyCoroutine(WaitSpawn());
        }
    }

    // 다음 방을 넘어갈때 캐릭터를 0.5초 컨트롤 할 수 없음
    // 몬스터 소환등을 기다리기 위함
    public IEnumerator WaitSpawn()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        foreach (var temp in Managers.Object.MainCharacters)
        {
            if (temp.OneTimeActive)
                WithdrawOneTimeItemEffect(temp);
            temp.CanMove = true;
            temp.CanAttack = true;
            temp.Collider.enabled = true;
        }

        RoomConditionCheck();
    }

    /// <summary>
    /// 다음 스테이지 이동 (플레이어, 카메라)
    /// </summary>
    public void GoToNextStage()
    {
        foreach (var temp in Managers.Object.MainCharacters)
        {
            temp.CanMove = false;
            temp.CanAttack = false;
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
            Managers.Object.ClearObjectManager(false);
        }

        //Move Camera to new starting room
        Cam.MoveCameraWithoutLerp(new Vector3(0.5f, 0.5f, -10f));
        foreach (var temp in Managers.Object.MainCharacters)
        {
            temp.gameObject.SetActive(true);
            temp.transform.position = new Vector3(0.5f, 0.5f, 0);
            temp.CanMove = true;
            temp.CanAttack = true;
        }

        RoomConditionCheck();
    }

    /// <summary>
    /// 다음 방 이동 (플레이어)
    /// </summary>
    /// <param name="index">이동 방향</param>
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

    /// <summary>
    /// 다음 방 이동 (카메라)
    /// </summary>
    /// <param name="nextRoom">이동할 방</param>
    public void MoveCameraToNextRoom(RoomClass nextRoom)
    {
        Vector3 newPos = nextRoom.Transform.position;
        newPos.z = -10;
        Managers.Game.Cam.TargetPos = newPos + new Vector3(0.5f, 0.5f);
    }

    /// <summary>
    /// ItemHolder에 소환될 아이템 설정,
    /// roomType에 따라서 아이템 pool이 달라짐
    /// </summary>
    /// <param name="roomType">아이템이 소환될 방의 타입</param>
    public int SelectItem(ERoomType roomType)
    {
        int TemplateId;

        if (roomType == ERoomType.Gold)
        {
            // 해당 아이템 배열이 빈 경우 "Breakfast" 아이템으로 대체
            if (Managers.Data.GoldArray.Count == 0)
                TemplateId = 45025;
            else
            {
                TemplateId = Managers.Data.GoldArray[RNG.RandInt(Managers.Data.GoldArray.Count - 1)];
                Managers.Data.GoldArray.Remove(TemplateId);
            }
        }
        else if (roomType == ERoomType.Shop)
        {
            if (Managers.Data.ShopArray.Count == 0)
                TemplateId = 45025;
            else
            {
                TemplateId = Managers.Data.ShopArray[RNG.RandInt(Managers.Data.ShopArray.Count - 1)];
                Managers.Data.ShopArray.Remove(TemplateId);
            }
        }
        else if (roomType == ERoomType.Boss)
        {
            if (Managers.Data.BossArray.Count == 0)
                TemplateId = 45025;
            else
            {
                TemplateId = Managers.Data.BossArray[RNG.RandInt(Managers.Data.BossArray.Count - 1)];
                Managers.Data.BossArray.Remove(TemplateId);
            }
        }
        else if (roomType == ERoomType.Secret)
        {
            if (Managers.Data.SecretArray.Count == 0)
                TemplateId = 45025;
            else
            {
                TemplateId = Managers.Data.SecretArray[RNG.RandInt(Managers.Data.SecretArray.Count - 1)];
                Managers.Data.SecretArray.Remove(TemplateId);
            }
        }
        else if (roomType == ERoomType.Curse)
        {
            if (Managers.Data.CurseArray.Count == 0)
                TemplateId = 45025;
            else
            {
                TemplateId = Managers.Data.CurseArray[RNG.RandInt(Managers.Data.CurseArray.Count - 1)];
                Managers.Data.CurseArray.Remove(TemplateId);
            }
        }
        else
        {
            Debug.Log("err in selectItem, non exist RoomType");
            return 0;
        }

        if (!Managers.Data.ItemDic.ContainsKey(TemplateId)) throw new Exception($"err nonexist itemID:{TemplateId}");
        return TemplateId;
    }

    /// <summary>
    /// 일회성 아이템 능력치 회수
    /// </summary>
    /// <param name="player">대상 플레이어</param>
    public void WithdrawOneTimeItemEffect(MainCharacter player)
    {
        player.OneTimeActive = false;
        player.Hp -= player.SpaceItem.Hp;
        player.TotalDmgUp -= player.SpaceItem.DmgUp;
        player.FlatDmgUp -= player.SpaceItem.FlatDmgUp;
        player.Multiplier = player.SpaceItem.Multiplier;
        player.Tears -= player.SpaceItem.Tears;
        player.Range -= player.SpaceItem.Range;
        player.ShotSpeed -= player.SpaceItem.ShotSpeed;
        player.Speed -= player.SpaceItem.Speed;
        player.Luck -= player.SpaceItem.Luck;
        player.Life -= player.SpaceItem.Life;
        //item.SetItem;
        //item.ShotType;
    }

    /// <summary>
    /// 방이 클리어 조건이 충족되면 호출하는 함수
    /// </summary>
    public void RoomClear()
    {
        var curRoom = Managers.Map.CurrentRoom;
        var existingValue = curRoom.IsClear;
        curRoom.IsClear = true;

        //curRoom.Doors.GetComponent<Door>().OpenAll();
        // 1회성 아이템이 있으면 능력치 회수
        foreach (var player in Managers.Object.MainCharacters)
        {
            if (player.OneTimeActive)
                Managers.Game.WithdrawOneTimeItemEffect(player);
        }

        if (curRoom.RoomType == ERoomType.Boss)
        {
            Managers.Map.CurrentRoom.ItemHolder?.SetActive(true);
            GameScene.PlayStageBGM();
            //보스방의 ItemHolder는 클리어 한 후 나타남
        }

        // 보상
        // 기존에 클리어한 방은 안줌
        if (existingValue == false)
        {
            // 원래 몬스터가 없던 방은 보상을 주지 않는다.
            if (FindChildByName(curRoom.Transform, "Monster").childCount > 0)
            {
                // 보스방은 픽업 안줌
                if (curRoom.RoomType != ERoomType.Boss)
                    SpawnClearAward(curRoom.AwardSeed);
                foreach (var player in Managers.Object.MainCharacters)
                {
                    if (player.SpaceItem == null) continue;
                    ChangeItemGage(player.SpaceItem, 1);
                }
            }

        }

        // DoorAnim
        // 클리어하지 않은 방만 Door의 열리는 모션을 재생한다
        if (existingValue == false)
        {
            curRoom.Doors.GetComponent<Door>().TryOpenAll();
        }
    }

    /// <summary>
    /// 방의 클리어 조건을 충족하는지 확인,
    /// 충족하면 RoomClear() 호출
    /// </summary>
    public void RoomConditionCheck()
    {
        //0. 이미 클리어한 방은 건너뜀
        if (Managers.Map.CurrentRoom.IsClear == true) return;

        //1. 몬스터가 없는 경우에 클리어 판정
        if (Managers.Object.Monsters.Count == 0 && Managers.Object.Bosses.Count == 0)
            RoomClear();
        return;
    }

    /// <summary>
    /// 게임 클리어
    /// </summary>
    public void ClearGame()
    {
        Managers.Map.DestroyMap();
        SceneManager.LoadScene("Title");
    }

    /// <summary>
    /// 게임 종료 (플레이어 죽음, ESC)
    /// </summary>
    public void GameOver()
    {
        Time.timeScale = 0;
        //TODO OpenUI
        Managers.UI.GameOverUI.gameObject.SetActive(true);
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        Init();

        // 아이템 배열 초기화
        Managers.Data.SetItemArray();

        {
            // 소환된 오브젝트 정리 (플레이어 제외)
            Managers.Object.ClearObjectManager();
        }

        // 맵 제거후 재생성
        Managers.Map.DestroyMap();
        while (true)
        {
            if (Managers.Map.GenerateStage() == 1)
                break;
        }
        Managers.Map.LoadMap();

        Managers.UI.PlayingUI.gameObject.SetActive(false);
        Managers.UI.PlayingUI.BossHpActive(false);

        Cam.MoveCameraWithoutLerp(new Vector3(0.5f, 0.5f, -10f));

        // 멀티플레이인 경우
        //for (int i=0; i< Managers.Object.MainCharacters.Count; i++)
        //{
        //    Managers.Object.Despawn(Managers.Object.MainCharacters.);
        //}
        MainCharacter[] players = Managers.Object.MainCharacters.ToArray();
        Managers.Object.MainCharacters.Clear();

        for (int i = 0; i < players.Length; i++)
        {
            Managers.Object.Despawn(players[i]);
        }
        //멀티 플레이인 경우
        //foreach (var temp in Managers.Object.MainCharacters)
        //{
        //    //temp.gameObject.SetActive(true);
        //    //temp.transform.position = new Vector3(-0.5f, -0.5f, 0);
        //    //temp.CanMove = true;
        //    Managers.Object.Despawn(temp);
        //}

        Managers.Object.Spawn<MainCharacter>(Vector3.zero, 0, "Player");
        RoomConditionCheck();

        Managers.UI.GameOverUI.gameObject.SetActive(false);
        Managers.UI.PlayingUI.gameObject.SetActive(true);

        GameScene.RestartAuodioSource();
    }

    /// <summary>
    /// 방을 클리어 했을 때 보상 설정
    /// </summary>
    /// <param name="seed">보상 선정을 위한 시드</param>
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

            // 픽업이 1개 이상인 경우 나선형 모양으로 소환
            List<Vector3Int> spawnPoses = SpriralPos(roomCenterPos, pickupCount);
            int done = 0;
            foreach (var spawnPos in spawnPoses)
            {
                if (done >= pickupCount) break;
                if (Managers.Map.CanGo(spawnPos))
                {
                    //spawn
                    Vector3 newSpawnPos = (spawnPos - (Vector3)roomCenterPos);
                    Managers.Object.Spawn<Pickup>(newSpawnPos, pickupAward, pickupsTransform, default, true);
                    done++;
                }

            }
        }
    }

    /// <summary>
    /// 희생방 보상 소환
    /// </summary>
    /// <param name="pickupCount">개수</param>
    /// <param name="sacrificeAward">보상 종류</param>
    public void SpawnSacrificeAward(int pickupCount, EPICKUP_TYPE sacrificeAward)
    {
        if (pickupCount <= 0) return;

        Vector2 roomCenterPos = Managers.Map.CurrentRoom.WorldCenterPos;
        Transform pickupsTransform = FindChildByName(Managers.Map.CurrentRoom.Transform, "Pickups");
        List<Vector3Int> spawnPoses = SpriralPos(roomCenterPos, pickupCount);

        int done = 0;
        foreach (var pos in spawnPoses)
        {
            if (done >= pickupCount) break;
            if (Managers.Map.CanGo(pos))
            {
                //spawn
                Vector3 newSpawnPos = ((pos + new Vector3(0.5f, 0.5f)) - (Vector3)roomCenterPos);
                Managers.Object.Spawn<Pickup>(newSpawnPos, sacrificeAward, pickupsTransform, default, true);
                done++;
            }
        }
    }

    /// <summary>
    /// 방 클리어 보상 종류 및 개수 선정
    /// </summary>
    /// <param name="pickupCount">(참조)보상 개수</param>
    /// <param name="pickupAward">(참조)보상 종류</param>
    /// <param name="seed">보상 시드</param>
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
                pickupAward = EPICKUP_TYPE.PICKUP_COIN;
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

    /// <summary>
    /// 상자, 가방의 보상 소환
    /// , 픽업의 타입에 따라서 테이블 달라짐
    /// </summary>
    /// <param name="pickupt">접촉한 픽업</param>
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

        float[] dx = { 1f, 0, -1f, 0 };
        float[] dy = { 0, 1f, 0, 1f };

        Transform pickupsTransform = FindChildByName(Managers.Map.CurrentRoom.Transform, "Pickups");

        for (int i = 0; i < pickupCount.Count; i++)
        {
            for (int j = 0; j < pickupCount[i]; j++)
            {
                float nx = Random.Range(-1, 1);
                float ny = Random.Range(-1, 1);

                Managers.Object.Spawn<Pickup>(pickup.transform.localPosition, pickupAward[i], pickupsTransform, new Vector3(nx, ny, 0).normalized * 4, true);
            }
        }

    }

    /// <summary>
    /// 상자, 가방의 보상 설정
    /// </summary>
    /// <param name="pickupCount">개수를 담은 List</param>
    /// <param name="pickupAward">보상 종류를 담은 Lsit</param>
    /// <param name="epickupType">접촉한 pickup 타입</param>
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
                //pickupAward.Add(EPICKUP_TYPE.PICKUP_TRINKET);
                //pickupCount.Add(1);
                pickupAward.Add(EPICKUP_TYPE.PICKUP_COIN);
                pickupCount.Add(rng.RandInt(3) + 1);
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
                //pickupAward.Add(EPICKUP_TYPE.PICKUP_PILL);
                //pickupCount.Add(1);
                pickupAward.Add(EPICKUP_TYPE.PICKUP_COIN);
                pickupCount.Add(10);
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

        Managers.Map.CurrentRoom.AwardSeed = rng.Next();
    }

    /// <summary>
    /// 희생방 보상 설정
    /// </summary>
    /// <param name="count">희생(spike 접촉) 횟수</param>
    public void GetSacrificeReward(int count)
    {
        long awardSeed = Managers.Map.CurrentRoom.AwardSeed;
        RNGManager rng = new RNGManager(awardSeed);

        int pickupCount = 0;
        EPICKUP_TYPE sacrificeAward = EPICKUP_TYPE.PICKUP_COIN;

        if (count <= 2)
        {
            // 50 nothing, 50 penny
            if (rng.Chance(50))
            {
                pickupCount = 1;
                sacrificeAward = EPICKUP_TYPE.PICKUP_COIN;
            }

        }
        else if (count <= 3)
        {
            // 67p incerse devil angel

            // 67 3penny

            if (rng.Chance(67))
            {
                pickupCount = 3;
                sacrificeAward = EPICKUP_TYPE.PICKUP_COIN;
            }
        }
        else if (count <= 4)
        {
            if (rng.Chance(50))
            {
                pickupCount = 1;
                sacrificeAward = EPICKUP_TYPE.PICKUP_CHEST;
            }
        }
        else if (count <= 5)
        {
            // 33 3penny
            if (rng.Chance(33))
            {
                pickupCount = 3;
                sacrificeAward = EPICKUP_TYPE.PICKUP_COIN;
            }
            else
            {
                // 67p incerse devil angel

                // 67 1heart
                pickupCount = 1;
                sacrificeAward = EPICKUP_TYPE.PICKUP_HEART;
            }

        }

        /* custom */
        else if (count <= 6)
        {
            //50 10 penny 50 3soulheart
            if (rng.Chance(50))
            {
                pickupCount = 10;
                sacrificeAward = EPICKUP_TYPE.PICKUP_COIN;
            }
            else
            {
                // TODO soul hear
                //50 3"soul"heart
                pickupCount = 3;
                sacrificeAward = EPICKUP_TYPE.PICKUP_HEART;
            }
        }

        else
        {
            return;
        }
        SpawnSacrificeAward(pickupCount, sacrificeAward);

        Managers.Map.CurrentRoom.AwardSeed = rng.Sn;
    }

    /// <summary>
    /// 상점 아이템 소환 (맵을 생성하는 과정에서 호출)
    /// </summary>
    /// <param name="pos">ShopItem 타일이 존재하는 위치</param>
    /// <param name="room">해당 방의 room class</param>
    public void SpawnShopItem(Vector3Int pos, RoomClass room)
    {
        RNGManager rng = new RNGManager(room.AwardSeed);
        var parent = FindChildByName(room.Transform, "ShopItems");
        Tilemap tm = room.TilemapCollisionPrefab.GetComponent<Tilemap>();

        // 첫번쨰 소환은 무조건 ItemHolder 아이템
        if (parent.childCount == 0)
        {
            //SPAWN PASSIVE or ACtiveITem
            room.ItemHolder = Managers.Resource.Instantiate("ItemHolder");
            room.ItemHolder.GetComponent<ItemHolder>().Init(room, pos);

            // for collision data
            tm.SetTile(pos, Managers.Resource.Load<Tile>("ItemHolder_Tile"));
        }
        else
        {
            //Spawn random prickup

            Pickup pickup;
            EPICKUP_TYPE pickupType = EPICKUP_TYPE.PICKUP_COIN;
            int price = 0;
            var value = rng.RandInt(100);
            if (value < 25)
            {
                pickupType = EPICKUP_TYPE.PICKUP_HEART;
                price = 3;
            }
            else if (value > 50)
            {
                pickupType = EPICKUP_TYPE.PICKUP_KEY;
                price = 5;
            }
            else if (value < 75)
            {
                pickupType = EPICKUP_TYPE.PICKUP_BOMB;
                price = 5;
            }
            else
            {
                pickupType = EPICKUP_TYPE.PICKUP_GRAB_BAG;
                price = 7;
            }

            if (pickupType == EPICKUP_TYPE.PICKUP_COIN)
            {
                throw new Exception($"EPICKUP_TYPE err while spawning shop item");
            }

            pickup = Managers.Object.Spawn<Pickup>(pos, pickupType, FindChildByName(room.Transform, "ShopItems"));
            pickup.GetComponent<Collider2D>().enabled = true;
            FindChildByName(pickup.transform, "ShopItemPrice").GetComponent<TextMeshPro>().gameObject.SetActive(true);
            FindChildByName(pickup.transform, "ShopItemPrice").GetComponent<TextMeshPro>().text = price.ToString();
            pickup.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

            // for collision data
            tm.SetTile(pos, Managers.Resource.Load<Tile>("CanGo"));
        }


        room.AwardSeed = rng.Sn;
    }

    public int SelectRandomMonster()
    {
        List<int> monsterList = new List<int>();
        // id가 제일작은 몬스터 (첫번째 몬스터)
        int firstId = Managers.Data.MonsterDic.First().Key - 1;

        foreach (var monsterData in Managers.Data.MonsterDic)
        {
            if (monsterData.Key / firstId == 1)
            {
                monsterList.Add(monsterData.Key);
            }
        }

        int selectedindex = Random.Range(0, monsterList.Count());
        return monsterList[selectedindex];
    }


    #region Active effect
    /// <summary>
    /// 액티브 아이템의 게이지를 변화시키는 함수
    /// </summary>
    /// <param name="item">대상 아이템</param>
    /// <param name="value">변화 값</param>
    public void ChangeItemGage(Item item, int value)
    {
        // 꽉차지 않은 상태에서는 비프음 재생
        if (item.CurrentGage < item.CoolTime)
        {
            AudioClip audioClip = Managers.Resource.Load<AudioClip>("beep");
            Managers.Sound.PlaySFX(audioClip, 0.3f);
        }

        item.CurrentGage = Math.Clamp(item.CurrentGage + value, 0, item.CoolTime);

        Managers.UI.PlayingUI.ChangeChargeGage(item);
    }

    /// <summary>
    /// 스테이지의 일반방 중에서 한곳으로 랜덤 텔레포트
    /// </summary>
    public void TPToNormalRandom()
    {
        // TP할 방 선택
        List<RoomClass> list = new();
        foreach (RoomClass r in Managers.Map.Rooms)
        {
            if (r.RoomType == ERoomType.Normal && r != Managers.Map.CurrentRoom)
            {
                list.Add(r);
            }
        }

        RoomClass chosen = RNG.Choice(list);

        // 플레이어 이동
        Vector3 newPos = chosen.Transform.position + new Vector3(0.5f, 0.5f, 0);
        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.Collider.enabled = false;
            mc.CanMove = false;
            mc.CanAttack = false;
        }

        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.transform.position = newPos;
        }

        // 카메라 이동
        newPos.z = -10f;
        Cam.MoveCameraWithoutLerp(newPos);

        // 기존 방에 소환된 몬스터 디스폰
        Managers.Object.DespawnMonsters(Managers.Map.CurrentRoom);

        // 아이템 비중 조정 
        if (Managers.Map.CurrentRoom.ItemHolder != null)
        {
            int TemplateId = Managers.Map.CurrentRoom.ItemHolder.GetComponent<ItemHolder>().ItemOfItemHolder.TemplateId;
            Managers.Data.ItemDic[TemplateId].Weight = 0;
        }

        // 현재방 정보 변경
        Managers.Map.CurrentRoom = chosen;


        // 소환될 몬스터 있는경우 소환ㅇㄴ
        if (Managers.Map.CurrentRoom.IsClear == false)
        {
            Managers.Map.SpawnMonsterAndBossInRoom(Managers.Map.CurrentRoom, () =>
            {
                CoroutineHelper.Instance.StartMyCoroutine(WaitSpawn());
            });
        }
        else
        {
            CoroutineHelper.Instance.StartMyCoroutine(WaitSpawn());
        }
    }

    /// <summary>
    /// Item roll(아이템 바꾸기)
    /// </summary>
    /// <param name="player">혹시 모를 사고예방, 아이템을 들고있는지 확인용도</param>
    /// <param name="target">돌리려는 타겟</param>
    public void Roll(MainCharacter player, string target = "item")
    {
        if (target == "item")
        {
            if (player.SpaceItem == null) return;
            // 아이템 홀더가 없으면 리턴
            if (Managers.Map.CurrentRoom.ItemHolder == null) return;
            // 아이템 홀더에 패시브 아이템을 이미 먹은 경우
            if (Managers.Map.CurrentRoom.ItemHolder.GetComponent<ItemHolder>().ItemOfItemHolder ==  null) return;
            Managers.Map.CurrentRoom.ItemHolder.GetComponent<ItemHolder>().SetItem(Managers.Map.CurrentRoom);
        }
        else if (target == "pickup")
        {

        }
        else if (target == "all")
        {

        }
    }

    #endregion

}
