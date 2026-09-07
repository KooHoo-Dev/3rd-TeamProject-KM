using UnityEngine;
using Jay;

namespace Combat
{
    public class TestCombat : SingletonBase<TestCombat>
    {
        // HitBox에서 타격 요청 패킷을 보내면 이 함수가 받습니다.
        public void SendHitRequest(HitRequestPacket packet)
        {
            Debug.Log($"[가짜 서버] 패킷 수신 완료 - 공격자: {packet.AttackerId}, 타겟: {packet.TargetId}, 데미지: {packet.Damage}");
            
            // 실제 서버라면 여기서 해킹인지 아닌지 검증을 하겠지만, 
            // 테스트니까 무조건 타격을 인정하고 클라이언트들에게 응답을 뿌려줍니다.
            SendHitResponseToClients(packet.TargetId, packet.Damage);
        }

        private void SendHitResponseToClients(string targetId, int damage)
        {
            Debug.Log($"[가짜 서버] 타격 승인! 클라이언트들에게 데미지 적용 명령 전송");
            
            // 내 클라이언트의 CombatSystem을 찾아 타격 적용 함수를 실행합니다.
            if (CombatSystem.Instance != null)
            {
                CombatSystem.Instance.OnReceiveHitResponse(targetId, damage);
            }
        }
    }
}