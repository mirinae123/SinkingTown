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
    public float population;

    /// <summary>
    /// 물고기
    /// </summary>
    public float fish;

    /// <summary>
    /// 식량
    /// </summary>
    public float food;

    /// <summary>
    /// 목재
    /// </summary>
    public float wood;

    /// <summary>
    /// 석재
    /// </summary>
    public float stone;

    /// <summary>
    /// 면화
    /// </summary>
    public float cotton;

    /// <summary>
    /// 의복
    /// </summary>
    public float clothe;

    /// <summary>
    /// 추가 효율
    /// </summary>
    public float efficiencyBonus;

    /// <summary>
    /// 추가 범위
    /// </summary>
    public int radiusBonus;

    /// <summary>
    /// 생성자
    /// </summary>
    public Resource(float population = 0, float fish = 0, float food = 0, float wood = 0, float stone = 0, float cotton = 0, float clothe = 0, float efficiencyBonus = 0, int radiusBonus = 0)
    {
        this.population = population;

        this.fish = fish;
        this.food = food;

        this.wood = wood;
        this.stone = stone;

        this.cotton = cotton;
        this.clothe = clothe;

        this.efficiencyBonus = efficiencyBonus;
        this.radiusBonus = radiusBonus;
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

        this.efficiencyBonus = a.efficiencyBonus;
        this.radiusBonus = a.radiusBonus;
    }

    /// <summary>
    /// needs가 provided의 도움을 받는지 확인
    /// </summary>
    public static bool IsNeeded(Resource needs, Resource provided)
    {
        return needs.population > 0 && provided.population > 0 ||needs.fish > 0 && provided.fish > 0 ||
               needs.food > 0 && provided.food > 0 || needs.cotton > 0 && provided.cotton > 0 ||
               needs.clothe > 0 && provided.clothe > 0 || needs.wood > 0 && provided.wood > 0 ||
               needs.stone > 0 && provided.stone > 0 || provided.efficiencyBonus > 0 || provided.radiusBonus > 0;
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
                                efficiencyBonus: a.efficiencyBonus + b.efficiencyBonus > 0 ? a.efficiencyBonus + b.efficiencyBonus : 0,
                                0);
    }

    /// <summary>
    /// 빼기 연산 오버로딩
    /// </summary>
    public static Resource operator -(Resource a, Resource b)
    {
        return new Resource(population: a.population - b.population > 0 ? a.population - b.population : 0,
                                fish: a.fish - b.fish > 0 ? a.fish - b.fish : 0,
                                food: a.food - b.food > 0 ? a.food - b.food : 0,
                                wood: a.wood - b.wood > 0 ? a.wood - b.wood : 0,
                                stone: a.stone - b.stone > 0 ? a.stone - b.stone : 0,
                                cotton: a.cotton - b.cotton > 0 ? a.cotton - b.cotton : 0,
                                clothe: a.clothe - b.clothe > 0 ? a.clothe - b.clothe : 0,
                                efficiencyBonus: a.efficiencyBonus - b.efficiencyBonus > 0 ? a.efficiencyBonus - b.efficiencyBonus : 0,
                                0);
    }

    /// <summary>
    /// 음수 기호 오버로딩
    /// </summary>
    public static Resource operator -(Resource a)
    {
        return new Resource(population: -a.population,
                                fish: -a.fish,
                                food: -a.food,
                                wood: -a.wood,
                                stone: -a.stone,
                                cotton: -a.cotton,
                                clothe: -a.clothe,
                                efficiencyBonus: -a.efficiencyBonus,
                                radiusBonus: -a.radiusBonus);
    }

    /// <summary>
    /// float 값과의 곱셈 오버로딩
    /// 생산량에 효율을 곱해 실질 생산량을 구할 때 쓰임
    /// </summary>
    public static Resource operator *(Resource a, float efficiency)
    {
        return new Resource(population: a.population * efficiency,
                                fish: a.fish * efficiency,
                                food: a.food * efficiency,
                                wood: a.wood * efficiency,
                                stone: a.stone * efficiency,
                                cotton: a.cotton * efficiency,
                                clothe: a.clothe * efficiency,
                                efficiencyBonus: a.efficiencyBonus,
                                radiusBonus: a.radiusBonus);
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
