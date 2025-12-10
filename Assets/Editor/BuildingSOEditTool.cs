using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class BuildingSOEditTool : EditorWindow
{
    private BuildingSO selectedBuildingSO;
    
    // 입력 필드들
    private string inputBuildingName = "";
    private Sprite inputBuildingIcon;
    private BuildingType inputBuildingType = BuildingType.None;
    private GameObject inputBuildingPrefab;
    private int inputHappiness = 0;
    private int inputPopulation = 0;
    private int inputBuildingSize = 1;
    
    // 삭제용 입력 필드
    private string deleteBuildingName = "";
    
    // 메시지 표시용
    private string message = "";
    private MessageType messageType = MessageType.None;

    [MenuItem("Tools/BuildingSOEditTool")]
    public static void ShowWindow()
    {
        GetWindow<BuildingSOEditTool>("BuildingSOEditTool");
    }

    private void OnGUI()
    {
        GUILayout.Label("BuildingSO Edit Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // BuildingSO 선택
        selectedBuildingSO = (BuildingSO)EditorGUILayout.ObjectField(
            "BuildingSO", 
            selectedBuildingSO, 
            typeof(BuildingSO), 
            false
        );
        EditorGUILayout.Space();

        if (selectedBuildingSO == null)
        {
            EditorGUILayout.HelpBox("BuildingSO를 선택해주세요.", MessageType.Info);
            return;
        }

        // buildingDatas가 null이면 초기화
        if (selectedBuildingSO.buildingDatas == null)
        {
            selectedBuildingSO.buildingDatas = new List<BuildingData>();
        }

        // 메시지 표시
        if (!string.IsNullOrEmpty(message))
        {
            EditorGUILayout.HelpBox(message, messageType);
            EditorGUILayout.Space();
        }

        // 현재 데이터 개수 표시
        GUILayout.Label($"현재 BuildingDatas 개수: {selectedBuildingSO.buildingDatas.Count}", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // ========== 추가 섹션 ==========
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("데이터 추가", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        inputBuildingName = EditorGUILayout.TextField("Building Name", inputBuildingName);
        inputBuildingIcon = (Sprite)EditorGUILayout.ObjectField("Building Icon", inputBuildingIcon, typeof(Sprite), false);
        inputBuildingType = (BuildingType)EditorGUILayout.EnumPopup("Building Type", inputBuildingType);
        inputBuildingPrefab = (GameObject)EditorGUILayout.ObjectField("Building Prefab", inputBuildingPrefab, typeof(GameObject), false);
        inputHappiness = EditorGUILayout.IntField("Happiness", inputHappiness);
        inputPopulation = EditorGUILayout.IntField("Population", inputPopulation);
        inputBuildingSize = EditorGUILayout.IntField("Building Size", inputBuildingSize);

        EditorGUILayout.Space();
        if (GUILayout.Button("Add", GUILayout.Height(30)))
        {
            AddBuildingData();
        }

        EditorGUILayout.Space(20);

        // ========== 삭제 섹션 ==========
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("데이터 삭제", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        deleteBuildingName = EditorGUILayout.TextField("Building Name", deleteBuildingName);

        EditorGUILayout.Space();
        if (GUILayout.Button("Delete", GUILayout.Height(30)))
        {
            RemoveBuildingData();
        }
    }

    private void AddBuildingData()
    {
        // BuildingName이 비어있는지 확인
        if (string.IsNullOrEmpty(inputBuildingName))
        {
            ShowMessage("Building Name을 입력해주세요.", MessageType.Warning);
            return;
        }

        // 이미 존재하는 이름인지 확인
        BuildingData existingData = selectedBuildingSO.buildingDatas.FirstOrDefault(
            data => data.buildingName == inputBuildingName
        );

        if (existingData != null)
        {
            // 기존 데이터 덮어쓰기
            existingData.buildingIcon = inputBuildingIcon;
            existingData.buildingType = inputBuildingType;
            existingData.buildingPrefab = inputBuildingPrefab;
            existingData.Happiness = inputHappiness;
            existingData.Population = inputPopulation;
            existingData.buildingSize = inputBuildingSize;
            
            EditorUtility.SetDirty(selectedBuildingSO);
            ShowMessage($"'{inputBuildingName}' 데이터가 덮어쓰여졌습니다.", MessageType.Info);
        }
        else
        {
            // 새 데이터 추가
            BuildingData newData = new BuildingData
            {
                buildingName = inputBuildingName,
                buildingIcon = inputBuildingIcon,
                buildingType = inputBuildingType,
                buildingPrefab = inputBuildingPrefab,
                Happiness = inputHappiness,
                Population = inputPopulation,
                buildingSize = inputBuildingSize
            };

            selectedBuildingSO.buildingDatas.Add(newData);
            EditorUtility.SetDirty(selectedBuildingSO);
            ShowMessage($"'{inputBuildingName}' 데이터가 추가되었습니다.", MessageType.Info);
        }
        
        // 입력 필드 초기화
        ClearInputFields();
    }

    private void RemoveBuildingData()
    {
        // BuildingName이 비어있는지 확인
        if (string.IsNullOrEmpty(deleteBuildingName))
        {
            ShowMessage("Building Name을 입력해주세요.", MessageType.Warning);
            return;
        }

        // 해당 이름의 데이터 찾기
        BuildingData dataToRemove = selectedBuildingSO.buildingDatas.FirstOrDefault(
            data => data.buildingName == deleteBuildingName
        );

        if (dataToRemove == null)
        {
            ShowMessage($"'{deleteBuildingName}' 이름의 데이터가 존재하지 않습니다.", MessageType.Error);
            return;
        }

        // 데이터 삭제
        selectedBuildingSO.buildingDatas.Remove(dataToRemove);
        EditorUtility.SetDirty(selectedBuildingSO);
        
        ShowMessage($"'{deleteBuildingName}' 데이터가 삭제되었습니다.", MessageType.Info);
        
        // 삭제 입력 필드 초기화
        deleteBuildingName = "";
    }

    private void ShowMessage(string msg, MessageType type)
    {
        message = msg;
        messageType = type;
    }

    private void ClearInputFields()
    {
        inputBuildingName = "";
        inputBuildingIcon = null;
        inputBuildingType = BuildingType.None;
        inputBuildingPrefab = null;
        inputHappiness = 0;
        inputPopulation = 0;
        inputBuildingSize = 1;
    }
}
