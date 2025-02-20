using UnityEngine;
using UnityEditor;

public class DoorShakeAnimationCreator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Create Door Shake Animation")]
    static void CreateDoorShakeAnimation()
    {
        // 새로운 애니메이션 클립 생성
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60; // 초당 60프레임

        // 1. 회전 값 흔들기 (기준 -90도)
        AnimationCurve rotationCurve = new AnimationCurve(
            new Keyframe(0f, -90f, 0, 10f),
            new Keyframe(0.1f, -88f, 10f, -10f), // 0.1초 후 약간 이동
            new Keyframe(0.2f, -92f, -10f, 10f), // 반대로 이동
            new Keyframe(0.3f, -88f, 10f, -10f),
            new Keyframe(0.4f, -92f, -10f, 10f),
            new Keyframe(0.5f, -90f, 0, 0),   // 0.5초에 원래 위치
            new Keyframe(0.6f, -88f, 10f, -10f), // 다시 반복
            new Keyframe(0.7f, -92f, -10f, 10f),
            new Keyframe(0.8f, -88f, 10f, -10f),
            new Keyframe(0.9f, -92f, -10f, 10f),
            new Keyframe(1.0f, -90f, 0, 0)   // 1초에 원래 위치
        );

        // 2. 크기 변화 (Scale 조정)
        AnimationCurve scaleCurve = new AnimationCurve(
             new Keyframe(0f, 1f, 0, 3f),
            new Keyframe(0.1f, 1.05f, 3f, -3f),
            new Keyframe(0.2f, 0.95f, -3f, 3f),
            new Keyframe(0.3f, 1.05f, 3f, -3f),
            new Keyframe(0.4f, 0.95f, -3f, 3f),
            new Keyframe(0.5f, 1f, 0, 0),   // 0.5초에 원래 크기
            new Keyframe(0.6f, 1.05f, 3f, -3f),
            new Keyframe(0.7f, 0.95f, -3f, 3f),
            new Keyframe(0.8f, 1.05f, 3f, -3f),
            new Keyframe(0.9f, 0.95f, -3f, 3f),
            new Keyframe(1.0f, 1f, 0, 0)   // 1초에 원래 크기
        );
        // 설정한 애니메이션 키프레임을 적용
        clip.SetCurve("", typeof(Transform), "localEulerAngles.z", rotationCurve); // 회전
        clip.SetCurve("", typeof(Transform), "localScale.x", scaleCurve); // 크기 변화 (X)
        clip.SetCurve("", typeof(Transform), "localScale.y", scaleCurve); // 크기 변화 (Y)

        // 애니메이션 클립을 프로젝트 폴더에 저장
        AssetDatabase.CreateAsset(clip, "Assets/@Resources/Animation/Boss/Mom/DoorShake.anim");
        AssetDatabase.SaveAssets();

        Debug.Log(" DoorShake 애니메이션 생성 완료! ");
    }
#endif
}