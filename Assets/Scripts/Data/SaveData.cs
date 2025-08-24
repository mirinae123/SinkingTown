using UnityEngine;

/// <summary>
/// 건물 세이브 데이터
/// </summary>
[System.Serializable]
public class StructureSaveData
{
    public StructureType StructureType;
    public Vector2Int Coordinate;
}

/// <summary>
/// 소비형 건물 세이브 데이터
/// </summary>
[System.Serializable]
public class ConsumerSaveData : StructureSaveData
{
    public bool IsEnabled;

    public float CurrentHappiness;
    public bool IsIncreasing;
}

/// <summary>
/// 능동 생산형 건물 세이브 데이터
/// </summary>
[System.Serializable]

public class ActiveProducerSaveData : StructureSaveData
{
    public float Elapsed;
}

/// <summary>
/// 수동 생산형 건물 세이브 데이터
/// </summary>
[System.Serializable]

public class PassiveProducerSaveData : StructureSaveData
{
}

/// <summary>
/// 물에 잠긴 건물 세이브 데이터
/// </summary>
[System.Serializable]
public class SunkenStructureSaveData
{
    public StructureType StructureType;
    public Vector2Int Coordinate;
}

/// <summary>
/// 해적 세이브 데이터
/// </summary>
[System.Serializable]
public class PirateSaveData
{
    public Vector3 Position;
    public Quaternion Rotation;

    public Vector2Int CurrentCoordinate;
    public Vector2Int TargetCoordinate;

    public PirateState CurrentState;
    public float Elapsed;

    public int CurrentHealth;
    public int AttackingFortressCount;
}

/// <summary>
/// 대포알 세이브 데이터
/// </summary>
[System.Serializable]
public class CannonballSaveData
{
    public Vector3 Position;
    public Vector3 Velocity;

    public float Duration;
    public float Elapsed;

    public int TargetPirateIndex;
}

/// <summary>
/// 세이브 데이터
/// </summary>
[System.Serializable]
public class SaveData
{
    // 게임 설정
    public int Seed;

    public int MapSize;
    public float OceanRisePeriod;
    public float PirateSpawnPeriod;
    public bool CanPause;

    // 카메라 상태
    public Vector2 CameraTarget = new Vector2(-1.0f, -1.0f);
    public int CameraRotation = 0;
    public float CameraZoom = 11.0f;

    // 게임 상태
    public bool HasEnded = false;

    public float OceanRiseCooldown;
    public float ResearchCooldown = 0.0f;

    public float PirateSpawnCooldown;
    public int PirateSpawnProbabilityIndex = 0;

    public int CurrentDay = 1;
    public float CurrentTime = 480.0f;

    public float TimeSinceOceanRise = 0.0f;

    public int CurrentWoods = 25;
    public int CurrentStones = 5;
    public int CurrentResearchPoint = 0;

    // 맵 상태
    public int OceanHeight = 6;

    public ConsumerSaveData[] ConsumerStructures = new ConsumerSaveData[] { };
    public PassiveProducerSaveData[] PassiveProducerStructures = new PassiveProducerSaveData[] { };
    public ActiveProducerSaveData[] ActiveProducerStructures = new ActiveProducerSaveData[] { };
    public SunkenStructureSaveData[] SunkenStructures = new SunkenStructureSaveData[] { };
    public Vector2Int[] Decks = new Vector2Int[] { };

    // 해적 상태
    public PirateSaveData[] Pirates = new PirateSaveData[] { };
    public CannonballSaveData[] Cannonballs = new CannonballSaveData[] { };
}
