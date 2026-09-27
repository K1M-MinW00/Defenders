# 상점 시스템

## 현재 범위

- 로비 상점의 한정, 패키지, 충전, 교환소 4개 탭
- Gem을 비용으로 사용하는 상품의 구매, 보상 지급, 구매 이력 저장
- KST 09:00 기준 일간·주간·월간 재고 초기화
- 기간 한정 판매와 계정별 구매 제한
- IAP 상품은 데이터에 등록하고 UI에 준비 중으로 표시하되 실제 결제는 수행하지 않음

## 데이터

ShopProductData가 상품 표시, 비용, 보상, 판매 기간과 재고 정책을 정의한다.
상품 ID는 출시 후 변경하지 않는 영구 키로 사용한다. 기간은 ISO-8601 UTC 문자열로 입력한다.

UserShopData에는 ProductId + PeriodKey별 구매 횟수가 저장된다. 초기화 상품은 현재
기간 키만 조회하므로 과거 이력은 감사 및 운영 분석에 남는다.

| ResetPeriod | 기간 키 | 초기화 |
|---|---|---|
| None | permanent | 없음 |
| Daily | D:yyyyMMdd | 매일 KST 09:00 |
| Weekly | W:yyyyMMdd | 매주 월요일 KST 09:00 |
| Monthly | M:yyyyMM | 매월 1일 KST 09:00 |

## 구매 트랜잭션

GemShopPurchaseUseCase는 상품 유효성, 판매 기간, 재고, Gem 잔액을 검사한 뒤 복제된
사용자 데이터에 보상과 비용을 계산한다. Resource, Inventory, Roster, Shop을
Firestore 한 번의 업데이트로 저장한 경우에만 런타임 사용자 데이터에 반영한다.

클라이언트의 중복 클릭은 UI와 유스케이스에서 차단하며, UserDataManager의 변경 큐가
다른 재화 변경과 구매를 직렬화한다.

현재 Firebase 클라이언트 저장은 원자적 문서 업데이트지만 악의적인 클라이언트를
신뢰할 수는 없다. 실제 서비스 단계에서는 구매 명령을 서버 함수로 이동하고 서버 시간,
재고, 잔액, 보상 지급을 서버 트랜잭션으로 검증해야 한다.

## IAP 확장

ShopPurchaseType.InAppPurchase와 IAPProductId는 유지되어 있다. 스토어 등록 후에는
IAP 결제 어댑터를 추가하고 영수증 서버 검증 성공 시 동일한 보상 계산 경로를 호출한다.
클라이언트 영수증 확인만으로 보상을 지급하지 않는다.

## 에디터 도구

Tools > Defenders > Build Shop UI가 공용 상품 카드 프리팹, 샘플 상품 데이터,
LobbyScene/Shop_Panel의 4개 탭 UI를 재생성한다.
