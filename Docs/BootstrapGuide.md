# 부트스트랩 컴포넌트 가이드

프로젝트에 여러 부트스트랩 컴포넌트가 있습니다. 각각의 용도와 사용 시나리오를 설명합니다.

## 부트스트랩 종류

### 1. **Eciton Test Bootstrap** (권장: Eciton 패키지 테스트용)

**위치:** `Tyrsha.Eciton.Presentation.EcitonTestBootstrap`

**용도:**
- **Eciton 패키지의 ECS 시스템을 테스트**하기 위한 간단한 부트스트랩
- Fireball 시나리오를 자동으로 실행합니다
- 데이터베이스나 추가 설정 없이 바로 테스트 가능

**언제 사용:**
- ✅ Eciton 패키지의 능력 시스템을 테스트하고 싶을 때
- ✅ ECS 시스템이 제대로 작동하는지 확인하고 싶을 때
- ✅ 간단한 테스트 시나리오를 실행하고 싶을 때
- ✅ 데이터베이스 설정 없이 빠르게 테스트하고 싶을 때

**사용 방법:**
1. 빈 씬을 엽니다
2. 빈 GameObject를 생성합니다
3. `Eciton Test Bootstrap` 컴포넌트를 추가합니다
4. Play 버튼을 누릅니다

**실행되는 시나리오:**
- Actor1과 Actor2 엔티티 생성
- Actor1에게 Fireball 능력 부여
- Actor1이 Actor2를 타겟으로 Fireball 발사
- 투사체 비행 후 충돌
- 즉시 데미지 및 화상 DoT 적용

---

### 2. **Moba Quick Start Bootstrap**

**위치:** `Moba.Runtime.MobaQuickStartBootstrap`

**용도:**
- MOBA 프로젝트의 **빠른 시작** 부트스트랩
- ErPrototype 또는 MobaPrototype 중 선택 가능
- 자동으로 적절한 부트스트랩을 생성합니다

**언제 사용:**
- ✅ MOBA 프로젝트를 빠르게 시작하고 싶을 때
- ✅ ErPrototype 또는 MobaPrototype 중 선택하고 싶을 때
- ✅ 샘플 데이터를 자동 생성하고 싶을 때

**게임 모드:**
- **ErPrototype**: 이터널리턴 스타일 배틀로얄
- **MobaPrototype**: 리그 오브 레전드 스타일 MOBA

---

### 3. **Moba Prototype Bootstrap**

**위치:** `Moba.Runtime.MobaPrototypeBootstrap`

**용도:**
- **MOBA 스타일 게임** 프로토타입 부트스트랩
- 미니언 웨이브, 정글, 오브젝티브 등 MOBA 요소 포함

**언제 사용:**
- ✅ MOBA 스타일 게임을 테스트하고 싶을 때
- ✅ 미니언 웨이브 시스템을 테스트하고 싶을 때
- ✅ 정글 몬스터 시스템을 테스트하고 싶을 때

**필수 조건:**
- `MobaDatabase` 리소스가 필요합니다
- `Tools/MOBA/Generate Sample Data`를 먼저 실행해야 합니다

---

### 4. **Er Prototype Bootstrap**

**위치:** `Moba.RuntimeER.ErPrototypeBootstrap`

**용도:**
- **이터널리턴 스타일 배틀로얄** 프로토타입 부트스트랩
- 루팅, 안전지대 수축, 최후 생존 승리 등 포함

**언제 사용:**
- ✅ 배틀로얄 스타일 게임을 테스트하고 싶을 때
- ✅ 루팅 시스템을 테스트하고 싶을 때
- ✅ 안전지대 수축 시스템을 테스트하고 싶을 때

**필수 조건:**
- `MobaDatabase` 리소스가 필요합니다
- `Tools/MOBA/Generate Sample Data`를 먼저 실행해야 합니다

---

## 선택 가이드

### Eciton 패키지만 테스트하고 싶다면

→ **Eciton Test Bootstrap** 사용

- 가장 간단하고 빠릅니다
- 추가 설정이 필요 없습니다
- ECS 시스템만 테스트할 수 있습니다

### MOBA 프로젝트 전체를 테스트하고 싶다면

→ **Moba Quick Start Bootstrap** 사용

- 게임 모드를 선택할 수 있습니다
- 샘플 데이터를 자동 생성합니다
- MOBA 프로젝트의 모든 기능을 테스트할 수 있습니다

### 특정 프로토타입만 테스트하고 싶다면

→ **Moba Prototype Bootstrap** 또는 **Er Prototype Bootstrap** 직접 사용

- 각 프로토타입의 특정 기능을 테스트할 수 있습니다
- 더 세밀한 제어가 가능합니다

---

## 주의사항

1. **여러 부트스트랩을 동시에 사용하지 마세요**
   - 충돌이 발생할 수 있습니다
   - 한 씬에는 하나의 부트스트랩만 사용하세요

2. **Eciton Test Bootstrap은 독립적으로 사용 가능**
   - MOBA 프로젝트의 데이터베이스가 없어도 작동합니다
   - Eciton 패키지의 ECS 시스템만 테스트합니다

3. **MOBA 부트스트랩들은 데이터베이스가 필요**
   - `MobaDatabase` 리소스가 필요합니다
   - 먼저 `Tools/MOBA/Generate Sample Data`를 실행하세요

---

## 빠른 참조

| 부트스트랩 | 용도 | 데이터베이스 필요 | 복잡도 |
|-----------|------|------------------|--------|
| **Eciton Test Bootstrap** | Eciton 패키지 테스트 | ❌ 불필요 | ⭐ 간단 |
| **Moba Quick Start Bootstrap** | MOBA 프로젝트 빠른 시작 | ✅ 필요 | ⭐⭐ 보통 |
| **Moba Prototype Bootstrap** | MOBA 프로토타입 | ✅ 필요 | ⭐⭐⭐ 복잡 |
| **Er Prototype Bootstrap** | 배틀로얄 프로토타입 | ✅ 필요 | ⭐⭐⭐ 복잡 |
