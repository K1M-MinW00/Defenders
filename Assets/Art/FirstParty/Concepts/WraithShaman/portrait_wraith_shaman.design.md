# 망령 주술사 디자인 잠금안

상태: 새 v2 단일 Idle 승인 / Idle, Walk, Attack, Die Unity 연결 완료

단일 Idle 기준: `WraithShaman-Idle-Source-v2.png`

최종 화풍·윤곽선 기준: `WraithShaman-Idle-Source-v2-HD.png`

- 셀: 100x100px
- 실제 불투명 영역: 24x29px
- 방향: 화면 오른쪽
- 하단 기준선: y=56
- 배경: 완전 투명, 그림자 없음
- 저채도 20색 팔레트와 프로젝트 공통 외곽선색 `#1F1A1A`을 사용한다.
- 애니메이션은 `Idle → Move → Attack → Hurt → Die` 순서로 한 종류씩만 제작하고 검수한다.

## 디자인 잠금 패키지

- 승인된 HD 기준: `WraithShaman-Idle-Source-v2-HD.png`
- 승인된 16 PPU 단일 기준: `WraithShaman-Idle-Source-v2.png`
- Idle 생성 원본: `WraithShaman-Idle-Animation-Source-v2.png`
- Walk 생성 원본: `WraithShaman-Walk-Animation-Source-v2.png`
- Attack 생성 원본: `WraithShaman-Attack-Animation-Source-v2.png`
- Die 생성 원본: `WraithShaman-Die-Animation-Source-v2.png`
- 구형 초상화, 구형 Idle, 스타일 참조, 실루엣과 팔레트 이미지는 삭제했다.

## 원거리 투사체 레퍼런스

- 상태: 단일 레퍼런스 검토 대기. 승인 전에는 애니메이션·프리팹·공격 컴포넌트에 연결하지 않는다.
- 파일: `WraithShaman-SoulProjectile-Reference.png`
- 논리 셀: 100x100px, 실제 불투명 영역 11x12px, 경계 중심 x=50 / y=49.5
- 형태: 화면 오른쪽이 열린 비대칭 초승달형 영혼불 껍질과 내부의 옅은 민트색 핵.
- 개구부 안쪽에는 본체와 1px 간격을 둔 청록색 단일 스파크만 사용한다.
- 색상: `#1F1A1A`, `#425C6B`, `#607581`, `#A7C8C8` 네 색으로 고정한다.
- 알파는 0 또는 255만 사용하며 꼬리, 잔광, 후광, 그림자와 추가 파티클은 금지한다.
- 비행 루프 시트: `Assets/Art/Generated/Projectiles/WraithShaman/WraithShaman-SoulBolt-Flight.png` — 6프레임, 600x100px.
- 비행 프레임은 승인된 단일 레퍼런스를 직접 복제한 뒤 핵 밝기, 초승달 상·하단 및 개방부를 1px 이내로만 바꾸고 스파크만 위·아래 1px 이동한다.
- 모든 비행 프레임은 논리 셀 중심 x=50 / y=50, 실제 경계 11x12px, 동일 팔레트와 이진 알파를 유지한다. 런타임 이동을 시트 안에 표현하지 않는다.

## Unity 연결

- Idle 시트: `Assets/Art/Generated/Monsters/WraithShaman/WraithShaman-Idle.png` — 6프레임, 실제 점유 21~24x29px
- Walk 시트: `Assets/Art/Generated/Monsters/WraithShaman/WraithShaman-Walk.png` — 8프레임, 실제 점유 21~24x29px
- Attack 시트: `Assets/Art/Generated/Monsters/WraithShaman/WraithShaman-Attack.png` — 8프레임
- Die 시트: `Assets/Art/Generated/Monsters/WraithShaman/WraithShaman-Die.png` — 4프레임
- Sprite Mode: Multiple, 16 PPU, Point, 압축 없음
- 원본 배치는 100x100 논리 셀이며 Unity Sprite Rect는 프레임별 불투명 영역을 타이트하게 등록한다.
- Idle은 하단 기준을 유지하면서 후드와 몸통이 호흡하고 소매·꼬리·영혼불이 한 박자 늦게 따라온다.
- Walk는 유령 꼬리의 압축과 신장으로 이동감을 만들고 본체는 1~2px만 바운스한다.
- Attack은 본체·후드·꼬리 중심과 기준선을 고정하고, 팔과 지팡이만 들어 올리며 지팡이 끝의 작은 영혼불이 밝아졌다가 사라지는 제자리 시전으로 표현한다. 전진·후퇴·돌진·크기 변화는 사용하지 않는다.
- 각 100x100 논리 셀은 가장 큰 본체 연결 영역만 유지하고, 이전·다음 프레임에서 넘어온 손·무기·의상·효과 조각이 없는지 검사한다.
- 얼굴, 눈, 지팡이 구조와 팔레트는 모든 프레임에서 고정한다.
- 타이트 슬라이스에는 논리 100x100 셀 중심을 기준으로 계산한 커스텀 피벗을 사용해 프레임별 실루엣 변화가 화면 떨림으로 변하지 않게 한다.
- Idle 클립: `Assets/Animations/Monster/WraithShaman/WraithShaman_Idle.anim`
- Walk 클립: `Assets/Animations/Monster/WraithShaman/WraithShaman_Walk.anim`
- Attack 클립: `Assets/Animations/Monster/WraithShaman/WraithShaman_Attack.anim` — 비반복, 5번째 프레임(0.4초) `OnAttackHit`, 8번째 프레임(0.7초) `OnAttackFinished`
- Die 클립: `Assets/Animations/Monster/WraithShaman/WraithShaman_Die.anim` — 비반복
- Override Controller: `Assets/Animations/Monster/WraithShaman/WraithShaman.overrideController`
- Monster_Base Variant: `Assets/Prefabs/Monsters/WraithShaman.prefab`
- Body에는 기존 SpriteRenderer와 Animator를 유지하고 `MonsterAnimationEvent`를 추가했다.
- Attack과 Die는 `Monster_Attack`, `Monster_Die` 슬롯에 연결했다.
- `Body/FirePoint`에서 청록색 `WraithShaman_SoulBolt`를 발사하는 `RangedMonsterAttack`을 프리팹 루트에 연결했다.
- 일반 등급 MonsterDataSO `Assets/GameData/Monsters/Wraith_Shaman.asset`을 생성하고 HP 90, 이동속도 2.5, 공격력 12, 사거리 5.5, 초당 공격 0.65, 위협도 3으로 고정했다.

Hurt는 필요할 때 별도 승인 단계에서 생성한다.

## 역할과 인상

- 일반 크기 몬스터인 원거리/보조형 언데드 주술사다.
- 영리하고 불길한 인상을 주되, 과도하게 기괴하거나 장식적인 형태는 피한다.
- 게임용 스프라이트에서는 오른쪽을 바라본다.

## 반드시 유지할 외형

- 넓고 단순한 저채도 슬레이트 블루 후드와 짧은 숄.
- 따뜻한 회색 해골 얼굴과 짧은 아래턱.
- 화면에서 보이는 눈 하나만 탁한 청록색이며 반대 눈구멍은 어둡다.
- 짧은 갈색 지팡이를 몸 가까이 든다.
- 지팡이 끝에는 작고 저채도인 영혼불만 사용한다.
- 다리 대신 넓고 단순한 회청색 유령 꼬리를 사용한다.
- 목걸이, 금색 장식, 동물 해골, 연기 가닥 같은 작은 장식은 사용하지 않는다.

## 고정 색상

- 주색: 저채도 슬레이트 블루, 목탄색
- 뼈: 따뜻한 회색
- 지팡이: 탁한 갈색
- 마법광: 옅고 탁한 청록색. 네온색은 사용하지 않는다.

## 스프라이트 변환 시 단순화 우선순위

1. 후드 실루엣
2. 한쪽 청록색 눈
3. 해골 얼굴
4. 짧은 지팡이와 작은 영혼불
5. 넓고 단순한 유령 꼬리

얼굴, 눈의 개수와 위치, 지팡이를 든 손, 꼬리 방향은 애니메이션 전체에서 바꾸지 않는다.

## 원본 생성 지시문 요약

Wizard, Armored Skeleton, Elite Orc의 픽셀 밀도와 팔레트를 기준으로 한 투명 배경 망령 주술사. 넓은 저채도 후드, 한쪽의 작은 탁한 청록색 눈, 짧은 지팡이, 넓은 회청색 꼬리를 사용하고 순수 검정·흰색·네온색과 작은 장식은 제외한다.
