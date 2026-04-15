public enum SkillTargetFailPolicy
{
    CancelAndRefund,    // 타겟 없으면 스킬 취소, 에너지 유지
    WaitUntilFound,     // 타겟 찾을 때까지 SkillState에서 대기
    CastWithoutTarget   // 타겟 없어도 자기 위치 기준 등으로 시전
}
