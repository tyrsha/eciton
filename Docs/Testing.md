# 게임 테스트 방법

Eciton 패키지를 테스트하는 방법을 안내합니다.

## 빠른 시작

### 방법 1: 부트스트랩 컴포넌트 사용 (권장)

1. Unity 에디터에서 빈 씬을 엽니다 (또는 `Assets/Scenes/SampleScene.unity` 사용)
2. Hierarchy에서 빈 GameObject를 생성합니다 (우클릭 > Create Empty)
3. GameObject의 이름을 "TestBootstrap"으로 변경합니다
4. Inspector에서 `Add Component` 버튼을 클릭합니다
5. `Eciton Test Bootstrap` 컴포넌트를 추가합니다
6. Play 버튼을 누릅니다

게임이 시작되면 `FireballScenarioBootstrapSystem`이 자동으로 실행되어 다음 시나리오가 시작됩니다:
- Actor1과 Actor2 엔티티가 생성됩니다
- Actor1에게 Fireball 능력이 부여됩니다
- Actor1이 Actor2를 타겟으로 Fireball을 발사합니다
- 투사체가 비행한 후 Actor2에 충돌합니다
- Actor2에게 즉시 데미지와 화상 DoT가 적용됩니다

### 방법 2: ECS 시스템 자동 실행

Unity ECS는 기본적으로 `DefaultGameObjectInjectionWorld`를 사용하며, 모든 `ISystem`은 자동으로 등록되고 실행됩니다.

- `ExampleDatabaseBootstrapSystem`: 데이터베이스를 생성합니다 (InitializationSystemGroup)
- `FireballScenarioBootstrapSystem`: 테스트 시나리오를 시작합니다 (SimulationSystemGroup)

따라서 빈 씬에서도 Play 버튼만 누르면 자동으로 테스트가 시작됩니다.

## 테스트 시나리오 상세

### Fireball 시나리오

1. **초기화 단계** (`FireballScenarioBootstrapSystem`)
   - Actor1 생성: Health=100, Mana=100, Strength=10, Agility=10
   - Actor2 생성: Health=100, Mana=50, Strength=8, Agility=8
   - Actor1에게 Fireball 능력 부여

2. **능력 활성화** (`AbilityActivationGateSystem`)
   - Actor1이 Actor2를 타겟으로 Fireball 활성화 요청
   - 쿨다운 및 마나 비용 확인

3. **능력 실행** (`AbilityExecutionSystem`)
   - Fireball 투사체 엔티티 생성
   - 투사체에 PrimaryEffect(즉시 데미지)와 SecondaryEffect(화상 DoT) 설정

4. **투사체 비행** (`FireballAbilitySystem`)
   - 투사체의 `RemainingFlightTime` 감소
   - 시간이 0 이하가 되면 충돌 처리

5. **효과 적용** (`EffectRequestSystem`, `ActiveEffectSystem`)
   - 즉시 데미지: Actor2의 Health -30
   - 화상 DoT: 5초 동안 매초 Health -4

## 디버깅

### 콘솔 로그 확인

게임 실행 중 Console 창에서 다음 로그를 확인할 수 있습니다:
- `[EcitonTestBootstrap]` 접두사가 붙은 로그
- ECS World 초기화 상태
- 시스템 실행 상태

### 엔티티 확인

Unity의 **Entities** 창 (Window > Entities > Hierarchy)에서 생성된 엔티티를 확인할 수 있습니다:
- Actor1, Actor2 엔티티
- Fireball 투사체 엔티티
- AbilityEffectDatabase 싱글톤 엔티티

### 컴포넌트 확인

각 엔티티의 컴포넌트를 확인하여 다음을 검증할 수 있습니다:
- `AttributeData`: Health, Mana 값 변화
- `ActiveEffect`: 적용된 효과 목록
- `GameplayTagElement`: Burning 태그 등

## 문제 해결

### 아무 일도 일어나지 않는 경우

1. **ECS World 확인**
   - `EcitonTestBootstrap` 컴포넌트의 `Log World Status` 옵션을 활성화
   - Console에서 ECS World 초기화 메시지 확인

2. **시스템 확인**
   - Window > Entities > Systems에서 시스템 목록 확인
   - `FireballScenarioBootstrapSystem`이 등록되어 있는지 확인

3. **데이터베이스 확인**
   - `ExampleDatabaseBootstrapSystem`이 실행되었는지 확인
   - `AbilityEffectDatabase` 싱글톤이 생성되었는지 확인

### 에러가 발생하는 경우

- `ObjectDisposedException`: 구조적 변경 후 버퍼를 다시 얻어야 합니다 (이미 수정됨)
- `NullReferenceException`: 데이터베이스가 초기화되지 않았을 수 있습니다

## 추가 테스트

### Unity Test Runner 사용

프로젝트에는 포괄적인 테스트 스위트가 포함되어 있습니다:

1. Window > General > Test Runner 열기
2. PlayMode 탭에서 테스트 실행
3. `EcitonAbilitiesTests`, `EcitonBehaviorTreeTests` 등 실행

자세한 내용은 `Packages/com.tyrsha.eciton/Tests/RuntimeTests/README.md`를 참조하세요.
