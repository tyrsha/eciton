using UnityEngine;
using Unity.Entities;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>
    /// 게임 테스트를 위한 간단한 부트스트랩 컴포넌트.
    /// 빈 씬에 이 컴포넌트를 가진 GameObject를 추가하면 자동으로 테스트 시나리오가 실행됩니다.
    /// 
    /// 사용 방법:
    /// 1. 빈 씬을 엽니다
    /// 2. 빈 GameObject를 생성합니다
    /// 3. 이 컴포넌트를 추가합니다
    /// 4. Play 버튼을 누릅니다
    /// 
    /// FireballScenarioBootstrapSystem이 자동으로 실행되어 테스트 시나리오를 시작합니다.
    /// </summary>
    public class EcitonTestBootstrap : MonoBehaviour
    {
        [Header("테스트 설정")]
        [Tooltip("게임 시작 시 테스트 시나리오를 실행할지 여부")]
        public bool enableTestScenario = true;

        [Tooltip("ECS World가 제대로 초기화되었는지 확인")]
        public bool logWorldStatus = true;

        [Tooltip("초기화 시점을 자세히 로그로 출력")]
        public bool logInitializationTiming = true;

        private void Awake()
        {
            if (logInitializationTiming)
            {
                Debug.Log($"[EcitonTestBootstrap] Awake 호출됨 - Time: {Time.time}, Frame: {Time.frameCount}");
                
                // Awake 시점에 ECS World 확인
                var world = World.DefaultGameObjectInjectionWorld;
                if (world != null && world.IsCreated)
                {
                    Debug.Log($"[EcitonTestBootstrap] Awake 시점: ECS World 이미 초기화됨 - {world.Name}");
                }
                else
                {
                    Debug.Log("[EcitonTestBootstrap] Awake 시점: ECS World 아직 초기화되지 않음");
                }
            }
        }

        private void Start()
        {
            if (logInitializationTiming)
            {
                Debug.Log($"[EcitonTestBootstrap] Start 호출됨 - Time: {Time.time}, Frame: {Time.frameCount}");
            }

            if (!enableTestScenario)
                return;

            // ECS World 확인
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                Debug.LogError("[EcitonTestBootstrap] ECS World가 초기화되지 않았습니다. " +
                    "Unity Entities 패키지가 제대로 설치되어 있는지 확인하세요.");
                return;
            }

            if (logWorldStatus)
            {
                Debug.Log($"[EcitonTestBootstrap] ECS World 초기화 완료: {world.Name}");
                Debug.Log($"[EcitonTestBootstrap] 시스템 수: {world.Systems.Count}");
                Debug.Log($"[EcitonTestBootstrap] 엔티티 수: {world.EntityManager.GetAllEntities().Length}");
            }

            Debug.Log("[EcitonTestBootstrap] 테스트 시나리오가 곧 시작됩니다. " +
                "FireballScenarioBootstrapSystem이 자동으로 실행됩니다.");
        }

        // 씬 로드 전에도 확인 가능하도록 정적 메서드 추가
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CheckWorldBeforeSceneLoad()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null && world.IsCreated)
            {
                Debug.Log($"[EcitonTestBootstrap] BeforeSceneLoad: ECS World 초기화됨 - {world.Name}, 시스템 수: {world.Systems.Count}");
            }
            else
            {
                Debug.Log("[EcitonTestBootstrap] BeforeSceneLoad: ECS World 아직 초기화되지 않음");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CheckWorldAfterSceneLoad()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null && world.IsCreated)
            {
                Debug.Log($"[EcitonTestBootstrap] AfterSceneLoad: ECS World 초기화됨 - {world.Name}, 시스템 수: {world.Systems.Count}");
            }
        }
    }
}
