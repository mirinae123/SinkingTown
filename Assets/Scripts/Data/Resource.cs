using System;

/// <summary>
/// 자원을 나타내는 클래스.
/// 1) 각 타일에 제공되는 자원을 나타낸다.
/// 2) 각 구조물의 기본 요구 사항, 생산량을 나타낸다.
/// 3) 자원 변동 사항을 이웃에 알릴 때 상황을 나타낸다.
/// </summary>
[Serializable]
public struct Resource
{
    /// <summary>
    /// 인구
    /// </summary>
    public int population;

    /// <summary>
    /// 물고기
    /// </summary>
    public int fish;

    /// <summary>
    /// 식량
    /// </summary>
    public int food;

    /// <summary>
    /// 목재
    /// </summary>
    public int wood;

    /// <summary>
    /// 석재
    /// </summary>
    public int stone;

    /// <summary>
    /// 면화
    /// </summary>
    public int cotton;

    /// <summary>
    /// 의복
    /// </summary>
    public int clothe;

    /// <summary>
    /// 추가 범위
    /// </summary>
    public bool rangeBonus;

    /// <summary>
    /// 생성자
    /// </summary>
    public Resource(int population = 0, int fish = 0, int food = 0, int wood = 0, int stone = 0, int cotton = 0, int clothe = 0, int efficiencyBonus = 0, bool rangeBonus = false)
    {
        this.population = population;

        this.fish = fish;
        this.food = food;

        this.wood = wood;
        this.stone = stone;

        this.cotton = cotton;
        this.clothe = clothe;

        this.rangeBonus = rangeBonus;
    }

    /// <summary>
    /// 복사 생성자
    /// </summary>
    public Resource(Resource a)
    {
        this.population = a.population;

        this.fish = a.fish;
        this.food = a.food;

        this.wood = a.wood;
        this.stone = a.stone;

        this.cotton = a.cotton;
        this.clothe = a.clothe;

        this.rangeBonus = a.rangeBonus;
    }

    /// <summary>
    /// needs가 provided의 도움을 받는지 확인
    /// </summary>
    public static bool IsNeeded(Resource needs, Resource provided)
    {
        return (needs.population > 0 && provided.population > 0) || (needs.fish > 0 && provided.fish > 0) ||
               (needs.food > 0 && provided.food > 0) || (needs.cotton > 0 && provided.cotton > 0) ||
               (needs.clothe > 0 && provided.clothe > 0) || provided.rangeBonus;
    }

    /// <summary>
    /// 더하기 연산 오버로딩
    /// 자원 변동 값으로 전달받은 내용을 실제 타일의 자원 정보에 반영할 때 쓰임
    /// </summary>
    public static Resource operator +(Resource a, Resource b)
    {
        return new Resource(population: a.population + b.population > 0 ? a.population + b.population : 0,
                                fish: a.fish + b.fish > 0 ? a.fish + b.fish : 0,
                                food: a.food + b.food > 0 ? a.food + b.food : 0,
                                wood: a.wood + b.wood > 0 ? a.wood + b.wood : 0,
                                stone: a.stone + b.stone > 0 ? a.stone + b.stone : 0,
                                cotton: a.cotton + b.cotton > 0 ? a.cotton + b.cotton : 0,
                                clothe: a.clothe + b.clothe > 0 ? a.clothe + b.clothe : 0,
                                rangeBonus: a.rangeBonus | b.rangeBonus);
    }

    /// <summary>
    /// 비교 오버로딩
    /// 요구 사항 충족 여부를 판단할 때 쓰임
    /// </summary>
    public static bool operator <(Resource a, Resource b)
    {
        return a.population < b.population || a.fish < b.fish || a.food < b.food || a.wood < b.wood || a.stone < b.stone || a.cotton < b.cotton || a.clothe < b.clothe;
    }

    public static bool operator >(Resource a, Resource b)
    {
        return a.population > b.population || a.fish > b.fish || a.food > b.food || a.wood > b.wood || a.stone > b.stone || a.cotton > b.cotton || a.clothe > b.clothe;
    }
}
