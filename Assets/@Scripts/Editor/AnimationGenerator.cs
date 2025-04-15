using DG.Tweening.Plugins.Core.PathCore;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimationGenerator : MonoBehaviour
{
    [MenuItem("Tools/AnimationGenerator")]
    static void GenerateAnimation()
    {

        string savePath = $"Assets/@Resources/Animation/Generated";

        //해당 디렉토리가 존재하지 않으면 디렉토리 생성
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        // 프로젝트 뷰에서 선택된 것 중 Texture2d 항목들을 불러옴
        // DeepAssets모드로 폴더를 선택하면 내부의 파일까지 선택가능
        Object[] texture = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        List<Sprite> sprites = new();

        foreach (var tex in texture)
        {

            // texture2d는 jpg png 같은 파일
            // sprite는 texture 내부의 하위 에셋
            // texture2d 만으로는 내부 에셋에 접근 할 수 었음
            /*
             character.png (Texture2D)
                ├─ Sprite: character_idle_1
                ├─ Sprite: character_idle_2
             */
            string path = AssetDatabase.GetAssetPath(tex);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite)
                    sprites.Add(sprite);
            }
        }

        Debug.Log(sprites.Count);
        if (sprites.Count == 0) return;

        // LINQ 문법
        // C# 언어에 직접 쿼리 기능을 통합하는 방식을 기반으로 하는 기술 집합 이름입니다. 
        // 쿼리 구문을 사용하면 최소한의 코드로 데이터 원본에 대한 필터링, 정렬 및 그룹화 작업을 수행할 수 있습니다. 
        // 이 부분을 필요할때 고쳐서 적절하게 그룹화
        var groups = sprites
            .OrderBy(e => e.name)
            .GroupBy(e => e.name.Split('_')[0])
            // 위에서 나눠진 그룹들을 나눠진 기준을 Key로 딕셔너리에 등록
            // 위에서 e.name.Split('_')[0]가 key
            .ToDictionary(g => g.Key, g=>g.ToList());

        List<AnimationClip> clips = new List<AnimationClip>();

        foreach (var group in groups)
        {
            string clipName = $"{group.Key}";
            AnimationClip clip = new();
            // 클립 프레임 조절
            clip.frameRate = 12;
            // 반복여부
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime == false)
            {
                settings.loopTime = true;
                settings.keepOriginalOrientation = true;
                settings.keepOriginalPositionXZ = true;
                settings.keepOriginalPositionY = true;
                settings.loopBlend = true;
                settings.loopBlendOrientation = true;
                settings.loopBlendPositionXZ = true;
                settings.loopBlendPositionY = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            // 애니메이션에서 제어될 타겟 지정
            // UnityEngine.UI.Image 타입의 m_Sprite ( image.sprite)
            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(UnityEngine.UI.Image),
                path = "",
                propertyName = "m_Sprite"
            }; ;


            // 키프레임에 값을 지정
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[group.Value.Count];
            for (int i = 0; i< group.Value.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = (i * (2)) / clip.frameRate,
                    value = group.Value[i]
                };

            }

            // clip의 spriteBindin(제어 목표)를 key프레임 값에 따라 제어하도록 붙여줌
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);
            AssetDatabase.CreateAsset(clip, $"{savePath}/{clipName}.anim");
            clips.Add( clip );
        }

        // Animator 생성
        // 필요시 이름 바꾸기
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath($"{savePath}/RenamingReQuired.controller");
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        int columns = 5; // 몇 개씩 가로로 배치할지
        int spacing = 200; // 스테이트 간 거리

        stateMachine.AddState("None");

        for (int i=0; i<clips.Count; i++)
        {
            int x = (i % columns) * spacing + 400;
            int y = -(i / columns) * spacing + 1200; // y는 위로 올라가게 하려면 -부호

            var clip = clips[i];
            var state = stateMachine.AddState(clip.name, new Vector3(x, y, 0));
            state.motion = clip;
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Complete");


        Object folder = AssetDatabase.LoadAssetAtPath<Object>(savePath);

        if (folder != null)
        {
            Selection.activeObject = folder;
            EditorUtility.FocusProjectWindow(); // 프로젝트 창에 포커스
        }
        else
        {
            Debug.LogWarning("지정한 경로에 폴더가 없습니다: " + savePath);
        }
    }
}
