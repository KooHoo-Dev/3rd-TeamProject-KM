namespace BlueMarble
{
    /// <summary>
    /// 출발지 급여 등 특수 타일 관련 밸런스 수치.
    /// 정식 수치는 플레이테스트 후 확정 예정 (기획서 16장). 지금은 영토 가격(100)과 스케일을 맞춘 임시값.
    /// 추후 밸런스 데이터(TSV)로 이전 예정.
    /// </summary>
    public static class SalaryConfig
    {
        public const int PassAmount = 200;
    }
}