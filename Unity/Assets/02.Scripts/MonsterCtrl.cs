using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MonsterCtrl : MonoBehaviour
{
    // 몬스터의 상태 정보
    public enum State
    {
        IDLE,
        TRACE,
        ATTACK,
        DIE
    }
    // 몬스터의 현재 상태
    public State state = State.IDLE;
    // 추적 사정거리
    public float traceDist = 10.0f;
    // 공격 사정거리
    public float attackDist = 2.0f;
    // 몬스터의 사망 여부
    public bool isDie = false;
    // 컴포넌트의 캐시를 처리할 변수
    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent agent;
   
    private Animator anim;

    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        // 몬스터의 Transform 할당
        monsterTr = GetComponent<Transform>();
        // 추적 대상인 Player의 Transform 할당
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        // NavMeshAgent 컴포넌트 할당
        agent = GetComponent<NavMeshAgent>();
        // 추적 대상의 위치를 설정하면 바로 추적 시작
        //agent.destination = playerTr.position;
        // 몬스터의 상태를 체크하는 코루틴 함수 호출
        StartCoroutine(CheckMonsterState());
        // 상태에 따라 몬스터의 행동을 수행하는 코루틴 함수 호출
        StartCoroutine(MonsterAction());
    }

    void OnCollisionEnter(Collision coll)
    {

        if (coll.collider.CompareTag("BULLET"))
        {
            // 충돌한 총알을 삭제
            Destroy(coll.gameObject);
            // 피격 리액션 애니메이션 실행
            anim.SetTrigger(hashHit);
        }

    }

    // 일정한 간격으로 몬스터의 행동 상태를 체크
    IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            // 0.3초 동안 중지(대기)하는 동안 제어권을 메시지 루프에 양보
            yield return new WaitForSeconds(0.3f);
            // 몬스터의 상태가 DIE일 때 코루틴을 종료
            if (state == State.DIE) yield break;
            // 몬스터와 주인공 캐릭터 사이의 거리 측정
            float distance = Vector3.Distance(playerTr.position, monsterTr.position);
            // 공격 사정거리 범위로 들어왔는지 확인
            if (distance <= attackDist)
            {
                state = State.ATTACK;
            }
            // 추적 사정거리 범위로 들어왔는지 확인
            else if (distance <= traceDist)
            {
                state = State.TRACE;
            }
            else
            {
                state = State.IDLE;
            }
        }
    }
    // 몬스터의 상태에 따라 몬스터의 동작을 수행
    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (state)
            {
                // IDLE 상태
                case State.IDLE:
                    // 추적 중지
                    agent.isStopped = true;
                    // Animator의 IsTrace 변수를 false로 설정
                    anim.SetBool(hashTrace, false);
                    break;
                // 추적 상태
                case State.TRACE:
                    // 추적 대상의 좌표로 이동 시작
                    agent.SetDestination(playerTr.position);
                    agent.isStopped = false;
                    // Animator의 IsTrace 변수를 true로 설정
                    anim.SetBool(hashTrace, true);
                    // Animator의 IsAttack 변수를 false로 설정
                    anim.SetBool(hashAttack, false);
                    break;
                // 공격 상태
                case State.ATTACK:
                    // Animator의 IsAttack 변수를 true로 설정
                    anim.SetBool(hashAttack, true);
                    break;
                // 사망
                case State.DIE:
                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }
    void OnDrawGizmos()
    {
        // 추적 사정거리 표시
        if (state == State.TRACE)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, traceDist);
        }
        // 공격 사정거리 표시
        if (state == State.ATTACK)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackDist);
        }
    }
}