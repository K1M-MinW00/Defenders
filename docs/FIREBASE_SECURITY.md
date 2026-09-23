# Firebase 보안 규칙 운영

## 적용 범위

`firestore.rules`는 현재 클라이언트 데이터 경로에 다음 정책을 적용한다.

- `users/{userId}`는 로그인 UID와 문서 ID가 같은 사용자만 읽고 쓸 수 있다.
- 사용자 문서의 최상위 필드와 기본 타입을 검사한다.
- `CreatedAt`은 생성 후 변경할 수 없다.
- 모든 사용자 데이터 갱신은 서버 타임스탬프로 `UpdatedAt`을 변경해야 한다.
- 스키마 버전을 이전 버전으로 낮출 수 없다.
- `mailboxes/{userId}/mails/{mailId}`는 해당 사용자만 읽을 수 있다.
- 클라이언트는 우편을 생성할 수 없으며, 미수령 우편의 `Claimed`만 `true`로 변경할 수 있다.
- 수령했거나 만료된 우편만 클라이언트에서 삭제할 수 있다.
- 명시하지 않은 모든 문서 접근은 거부한다.

Firebase Console과 Admin SDK는 Firestore 보안 규칙을 우회한다. 따라서 운영 보상 우편은 Firebase Console에서 직접 추가하거나, 향후 신뢰할 수 있는 Admin SDK 환경에서 생성한다.

## 배포

Firebase CLI를 설치하고 로그인한 뒤 실제 Firebase 프로젝트 ID를 명시해 배포한다.

```powershell
firebase login
firebase deploy --only firestore:rules,firestore:indexes --project <PROJECT_ID>
```

프로젝트 ID를 저장소의 `.firebaserc`에 고정하면 실수로 다른 Firebase 프로젝트에 배포할 수 있으므로 현재는 포함하지 않는다.

규칙은 파일을 커밋하는 것만으로 Firebase Console에 적용되지 않는다. 배포 전까지는 기존 Console 규칙이 계속 사용된다.

## 이 구조의 한계

현재 게임 경제 로직은 클라이언트가 계산하고 `users/{userId}`를 직접 갱신한다. 규칙은 다른 사용자의 데이터 접근과 문서 구조 훼손은 막지만, 수정 APK가 본인 문서에 유효한 형태의 거짓 재화 값을 쓰는 것은 완전히 판별할 수 없다.

서버 권위 경제가 필요해지면 가챠, 우편 보상, 구매, 스테이지 보상처럼 가치가 있는 변경을 Cloud Functions 또는 별도 Admin SDK 서버로 이전해야 한다.
