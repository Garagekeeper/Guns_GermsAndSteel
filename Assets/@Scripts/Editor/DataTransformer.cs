using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Data;
using System.ComponentModel;
using System.Reflection;
using NUnit.Framework.Interfaces;
using static PlasticPipe.Server.MonitorStats;
using System.Xml.Linq;

public class DataTransformer : EditorWindow
{
#if UNITY_EDITOR
    #region Functions

    // F3 키를 눌러 저장된 데이터를 지울 수 있는 메뉴 항목 등록
    [MenuItem("Clear/RemoveSaveData _F3")]
    public static void RemoveSaveData()
    {
        // 영속 데이터 경로에 저장된 SaveData.json 파일 경로
        string path = Application.persistentDataPath + "/SaveData.json";
        if (File.Exists(path))
        {
            // 파일이 존재하면 삭제
            File.Delete(path);
            Debug.Log("SaveFile Deleted");
        }
        else
        {
            // 없으면 로그만 출력
            Debug.Log("No SaveFile Detected");
        }
    }

    #endregion

    // F4 키를 눌러 Excel 데이터를 JSON으로 변환하는 메뉴 항목 등록
    [MenuItem("Tools/ParseExcel _F4")]  // 추가 단축키: Control + K
    public static void ParseExcelDataToJson()
    {
        // 세 가지 CSV(Excel 추출) 파일을 각각 파싱하여 JSON으로 저장
        ParseExcelDataToJson<ItemDataLoader, ItemData>("Item_Data");
        ParseExcelDataToJson<MonsterDataLoader, MonsterData>("Monster_Data");
        ParseExcelDataToJson<RoomDataLoader, RoomData>("Room_Data");
        ParseExcelDataToJson<RoomItemArrayDataLoader, RoomItemArrayData>("Item_Array_Data");

        Debug.Log("Complete DataTransformer");
    }

    #region Helpers

    // 제네릭 헬퍼: Loader 타입 인스턴스 생성 후 첫 번째 필드(리스트)에 파싱 결과 대입 → JSON 직렬화
    private static void ParseExcelDataToJson<Loader, LoaderData>(string filename)
        where Loader : new()
        where LoaderData : new()
    {
        // Loader 인스턴스 생성 (e.g. ItemDataLoader)
        Loader loader = new Loader();
        // Loader 내부 첫 번째 필드(보통 List<LoaderData>) 가져오기
        FieldInfo field = loader.GetType().GetFields()[0];
        // 파싱된 리스트를 해당 필드에 설정
        field.SetValue(loader, ParseExcelDataToList<LoaderData>(filename));

        // JSON 포맷으로 문자열 직렬화
        string jsonStr = JsonConvert.SerializeObject(loader, Formatting.Indented);
        // 에셋 폴더에 JSON 파일로 저장
        File.WriteAllText($"{Application.dataPath}/@Resources/Data/JsonData/{filename}.json", jsonStr);
        // 에디터 내 에셋 갱신
        AssetDatabase.Refresh();
    }

    // CSV 파일을 읽어 List<LoaderData> 형태로 반환하는 메인 파싱 로직
    private static List<LoaderData> ParseExcelDataToList<LoaderData>(string filename)
        where LoaderData : new()
    {
        List<LoaderData> loaderDatas = new List<LoaderData>();

        // CSV 파일 전체를 읽어서 줄 단위로 split
        string[] lines = File.ReadAllText($"{Application.dataPath}/@Resources/Data/ExcelData/{filename}.csv")
                             .Trim()
                             .Split("\n");

        // 첫 줄(헤더) 제외하고 데이터 행만 rows 리스트에 저장
        List<string[]> rows = new List<string[]>();
        int innerFieldCount = 0;
        for (int l = 1; l < lines.Length; l++)
        {
            string[] row = lines[l].Replace("\r", "").Split(',');
            rows.Add(row);
        }

        // 각 데이터 행마다 LoaderData 인스턴스 생성 후 필드별로 값 설정
        for (int r = 0; r < rows.Count; r++)
        {
            // 빈 행 또는 ID(첫 칼럼)가 비어 있으면 건너뜀
            if (rows[r].Length == 0) continue;
            if (string.IsNullOrEmpty(rows[r][0])) continue;

            innerFieldCount = 0;  // multi-row 처리용 커서 초기화
            LoaderData loaderData = new LoaderData();
            Type loaderDataType = typeof(LoaderData);
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            // 상속 계층 순서대로 모든 필드 수집
            var fields = GetFieldsInBase(loaderDataType, bindingFlags);

            // 다음 레코드 시작 인덱스 탐색: ID 칼럼이 채워진 행까지
            int nextIndex;
            for (nextIndex = r + 1; nextIndex < rows.Count; nextIndex++)
            {
                if (!string.IsNullOrEmpty(rows[nextIndex][0]))
                    break;
            }

            // 각 필드에 대해 값을 파싱해서 loaderData에 설정
            for (int f = 0; f < fields.Count; f++)
            {
                FieldInfo field = loaderData.GetType().GetField(fields[f].Name);
                Type type = field.FieldType;

                // 제네릭 타입(List<T>)인 경우: 여러 행 묶음으로 리스트 생성
                if (type.IsGenericType)
                {
                    Type valueType = type.GetGenericArguments()[0];
                    Type genericListType = typeof(List<>).MakeGenericType(valueType);
                    var genericList = Activator.CreateInstance(genericListType) as IList;

                    // r부터 nextIndex-1까지 반복하며 각 행의 값을 리스트에 추가
                    for (int i = r; i < nextIndex; i++)
                    {
                        if (string.IsNullOrEmpty(rows[i][f + innerFieldCount])) continue;
                        bool isCustomClass = valueType.IsClass && !valueType.IsPrimitive && valueType != typeof(string);

                        if (isCustomClass)
                        {
                            // 커스텀 클래스 내부 필드까지 파싱
                            object fieldInstance = Activator.CreateInstance(valueType);
                            FieldInfo[] fieldInfos = fieldInstance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                            for (int k = 0; k < fieldInfos.Length; k++)
                            {
                                FieldInfo innerField = valueType.GetFields()[k];
                                string str = rows[i][f + innerFieldCount + k];
                                object convertedValue = ConvertValue(str, innerField.FieldType);
                                if (convertedValue != null)
                                    innerField.SetValue(fieldInstance, convertedValue);
                            }

                            // 다음 행이 동일 레코드인지 검사하여 innerFieldCount 조정
                            string nextStr = null;
                            if (i + 1 < rows.Count)
                            {
                                if (f + innerFieldCount < rows[i + 1].Length && string.IsNullOrEmpty(rows[i + 1][0]))
                                    nextStr = rows[i + 1][f + innerFieldCount];
                            }
                            if (string.IsNullOrEmpty(nextStr) || i + 1 == nextIndex)
                                innerFieldCount = fieldInfos.Length - 1;

                            genericList.Add(fieldInstance);
                        }
                        else
                        {
                            // 기본 타입이면 바로 값 변환 후 리스트에 추가
                            object value = ConvertValue(rows[i][f], valueType);
                            genericList.Add(value);
                        }
                    }

                    // 리스트가 null이 아니면 필드에 설정
                    if (genericList != null)
                        field.SetValue(loaderData, genericList);
                }
                else
                {
                    // 제네릭이 아닌 경우: 기본 타입 또는 커스텀 클래스 필드 처리
                    bool isCustomClass = type.IsClass && !type.IsPrimitive && type != typeof(string);
                    if (isCustomClass)
                    {
                        // 커스텀 클래스 인스턴스 생성 후 내부 필드 파싱
                        object fieldInstance = Activator.CreateInstance(type);
                        FieldInfo[] fieldInfos = fieldInstance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                        for (int i = 0; i < fieldInfos.Length; i++)
                        {
                            FieldInfo innerField = type.GetFields()[i];
                            string val = rows[r][f + innerFieldCount + i];
                            object converted = ConvertValue(val, innerField.FieldType);
                            if (converted != null)
                                innerField.SetValue(fieldInstance, converted);
                        }

                        innerFieldCount = fieldInfos.Length - 1;
                        field.SetValue(loaderData, fieldInstance);
                    }
                    else
                    {
                        // 기본 타입이면 ConvertValue 사용
                        object value = ConvertValue(rows[r][f], type);
                        if (value != null)
                            field.SetValue(loaderData, value);
                    }
                }
            }

            // 완성된 객체를 리스트에 추가
            loaderDatas.Add(loaderData);
        }

        return loaderDatas;
    }

    // 문자열을 특정 타입으로 변환 (int, float, enum 등)
    private static object ConvertValue(string value, Type type)
    {
        if (string.IsNullOrEmpty(value)) return null;
        TypeConverter converter = TypeDescriptor.GetConverter(type);
        return converter.ConvertFromString(value);
    }

    // CSV 셀에 & 구분자로 여러 값이 들어올 경우 List<T>로 변환(현재 미사용)
    private static object ConvertList(string value, Type type)
    {
        if (string.IsNullOrEmpty(value)) return null;
        Type valueType = type.GetGenericArguments()[0];
        Type genericListType = typeof(List<>).MakeGenericType(valueType);
        var genericList = Activator.CreateInstance(genericListType) as IList;
        foreach (var token in value.Split('&'))
        {
            if (string.IsNullOrWhiteSpace(token)) continue;
            genericList.Add(ConvertValue(token, valueType));
        }
        return genericList;
    }

    // 한 줄의 CSV 데이터를 리스트로 변환하는 유틸 (미사용)
    private static IList ParseCsvDataToList(string csvData, Type itemType)
    {
        var listType = typeof(List<>).MakeGenericType(itemType);
        var list = Activator.CreateInstance(listType) as IList;
        if (string.IsNullOrEmpty(csvData)) return list;
        var items = csvData.Split('\n');
        foreach (var item in items)
        {
            var obj = Activator.CreateInstance(itemType);
            var props = item.Split(',');
            var fields = itemType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length && i < props.Length; i++)
                fields[i].SetValue(obj, Convert.ChangeType(props[i], fields[i].FieldType));
            list.Add(obj);
        }
        return list;
    }

    // 상속 계층을 타고 올라가면서 base → derived 순으로 모든 필드 수집
    public static List<FieldInfo> GetFieldsInBase(Type type, BindingFlags flags)
    {
        List<FieldInfo> fields = new List<FieldInfo>();
        HashSet<string> names = new HashSet<string>();
        Stack<Type> stack = new Stack<Type>();
        while (type != null && type != typeof(object))
        {
            stack.Push(type);
            type = type.BaseType;
        }
        while (stack.Count > 0)
        {
            var t = stack.Pop();
            foreach (var f in t.GetFields(flags))
            {
                if (names.Add(f.Name))
                    fields.Add(f);
            }
        }
        return fields;
    }

    #endregion
#endif
}
