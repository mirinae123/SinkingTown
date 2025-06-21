using UnityEngine;

/// <summary>
/// 건물 데이터
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/StructureData", order = 1)]
public class StructureData : ScriptableObject
{
    public StructureType StructureType;     // 건물 종류

    public Sprite StructureImage;           // 건물 이미지
    public string StructureNameKey;         // 건물 이름 키

    public GameObject StructurePrefab;          // 건물 프리팹
    public GameObject BuildingStructurePrefab;  // 건설 중인 건물 프리팹
    public GameObject DisabledStructurePrefab;  // 비활성화된 건물 프리팹
    public GameObject SunkenStructurePrefab;    // 물에 잠긴 건물 프리팹

    public Resource Needs;      // 요구량
    public Resource Produces;   // 생산량
    public bool RequireOcean;   // 근처에 바다가 필요한지 여부

    public int WoodCost;        // 목재 비용
    public int StoneCost;       // 석재 비용
    public int Radius;          // 효과 범위

    public float MaxHappiness;  // 최대 행복도
    public float IncreaseSpeed; // 행복도 증가량
    public float DecreaseSpeed; // 행복도 감소량

    public float TimeToProduce; // 자원 생산 시간
}
