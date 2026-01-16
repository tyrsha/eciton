# 몬스터 AI (DOTS Behavior Tree) 가이드

이 문서는 `com.tyrsha.eciton` 패키지에서 제공하는 **몬스터 AI 기능**(Perception + Behavior Tree + Ability 실행 파이프라인)을 “나중에 보기 쉬운 형태”로 정리한 것입니다.

## 목표

- **PerceptionSystem(DOTS)**: 몬스터가 주변의 타겟을 찾고(가까운 적), 사거리 안인지 판정해 `BehaviorTreeBlackboard`를 갱신
- **BehaviorTreeTickSystem(DOTS)**: 블롭(BlobAsset)으로 정의된 BT를 매 프레임 평가하고, 액션은 ECS 요청/컴포넌트로만 표현
- **Ability 실행**: BT가 `PressAbilityInputRequest`를 발행 → `AbilityInputSystem`이 `TryActivateAbilityRequest`로 변환 → 게이트/실행 시스템이 처리

## 구성요소(런타임)

### 1) Perception(인지)

- **컴포넌트**
  - `Faction`: 팀/진영 구분(같은 팀은 타겟 제외)
  - `Targetable`: 타겟 후보 마커(플레이어/소환수/다른 몬스터 등)
  - `PerceptionSensor`: 인지 반경(Radius)
  - `AttackRange`: “인레인지” 판정용 거리
  - `BehaviorTreeBlackboard`: `Target`, `TargetInRange` 등을 보관(런타임 가변)
- **시스템**
  - `PerceptionSystem`: 가장 가까운 적(Targetable & 다른 Faction)을 반경 내에서 선택해서 블랙보드를 갱신

### 2) Behavior Tree

- **블롭(정의)**
  - `BehaviorTreeBlob`: 노드 배열 + children 인덱스 배열
- **컴포넌트**
  - `BehaviorTreeAgent`: 블롭 참조
  - `BehaviorTreeBlackboard`: 타겟/사거리 같은 런타임 가변 상태
  - `BehaviorTreeLastResult`: 디버그/테스트용 마지막 결과
- **시스템**
  - `BehaviorTreeTickSystem`: 루트부터 “stateless”로 평가

현재 지원하는 노드(스텁):
- Composite: `Selector`, `Sequence`
- Condition: `HasTarget`, `TargetInRange`, `HasGameplayTag`, `NotHasGameplayTag`
- Action: `PressAbilitySlot`, `MoveToTarget`, `ClearMoveRequest`

### 3) Ability 파이프라인(연결)

BT 액션이 내는 것은 **게임플레이 결정**이 아니라 “요청”입니다.

- `PressAbilitySlot` → `PressAbilityInputRequest` 버퍼에 추가
- `AbilityInputSystem` → `TryActivateAbilityRequest`로 변환
- 이후 `AbilityActivationGateSystem`(태그/마나/쿨다운) → `AbilityExecutionSystem`(ExecutionType 기반 실행)

## Authoring(에디터에서 세팅)

### A) PerceptionAuthoring

`PerceptionAuthoring`를 붙이면 Baker가 아래를 베이크합니다:
- `Faction`
- `Targetable`(옵션)
- `PerceptionSensor`/`AttackRange`(옵션)

**권장**
- 플레이어/타겟: `IsTargetable=true`, `IsSensor=false`
- 몬스터: `IsTargetable=true`, `IsSensor=true`

### B) MonsterAiAuthoring

`MonsterAiAuthoring`에 `BehaviorTreeAsset`를 연결하면:
- `BehaviorTreeBlob` 생성 + `BehaviorTreeAgent` 부착
- `BehaviorTreeBlackboard` 기본값 부착

### C) MonsterAbilityLoadoutAuthoring

몬스터에게 능력을 주고 “슬롯 바인딩”까지 자동으로 하려면:
- `MonsterAbilityLoadoutAuthoring`에서 Slot → AbilityId/Level을 입력
- Baker가 `GrantAbilityRequest` + `AbilityInputBindingByAbilityId`를 베이크
- 런타임에서 `AbilityGrantSystem`이 능력을 부여하고,
- `AbilityInputAutoBindSystem`이 Slot → Handle 바인딩을 완성합니다.

## BT 템플릿 생성/검증(에디터 메뉴)

### 템플릿 생성

- 메뉴: `Eciton/AI/Create Behavior Tree Templates`
- 생성 위치: `Assets/Eciton/AI/BehaviorTrees/`

기본 제공 템플릿(스텁):
- `BT_Ranged_ChaseAndCast`
- `BT_Melee_ChaseAndAttack`
- `BT_Idle_IfNoTarget`

### BT 검증

- 메뉴: `Eciton/Validate/Behavior Tree Assets`
- child index 범위, null 노드 등 기본 오류를 잡습니다.

## 운영 팁(확장 포인트)

- Perception은 현재 “가장 가까운 적 1명”만 고릅니다.
  - 시야각, LOS(라인오브사이트), 위협도(aggro), 가중치, 기억(최근 타겟) 등은 프로젝트에서 확장하세요.
- BT는 stateless 평가라서 장기 Running 노드(예: 이동 완료까지 유지) 같은 것은
  - `MoveToTargetRequest` 같은 **지속 컴포넌트**를 두고,
  - 다른 이동 시스템이 “완료 시 컴포넌트를 제거”하는 방식으로 구성하는 것을 추천합니다.

