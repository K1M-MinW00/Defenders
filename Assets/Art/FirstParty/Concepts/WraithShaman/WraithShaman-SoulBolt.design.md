# Wraith Shaman Soul Bolt 디자인 잠금안

상태: v1 꼬리형 디자인 반려 / v2 단일 기준 및 6프레임 비행 루프 승인·Unity 연결

- 용도: 망령 주술사의 단일 대상 중거리 기본 공격 투사체
- 방향: 화면 오른쪽
- 논리 셀: 100x100px, 실제 불투명 영역 약 12x12px
- 중심: 셀 중앙 `x=50, y=50`
- 배경: 완전 투명 RGBA, 그림자 없음
- PPU: 16
- 형태: 작은 청록색 영혼 핵을 비대칭 초승달형 영혼 껍질이 감싸는 구형 마력탄
- 팔레트: 외곽 `#1F1A1A`, 어두운 청록, 저채도 청록, 옅은 민트색 핵
- 금지: 꼬리, 혜성형 잔상, 파란 얼음, 번개, 화살촉, 해골 얼굴, 문자, 원형 마법진, 연기 구름, 네온 발광
- 런타임 이동은 코드가 담당한다. 애니메이션 프레임 안에서 투사체 중심은 이동하지 않는다.
- 충돌·소멸 이펙트는 비행 루프에 포함하지 않는다.

## 1단계 — 단일 기준 스프라이트 프롬프트

Create exactly one gameplay-ready pixel-art projectile reference sprite for the approved Wraith Shaman monster. Transparent RGBA background. Use one logical 100x100 pixel cell, but keep the actual opaque projectile compact at approximately 12x12 pixels and centered at x=50, y=50. Design it as a small condensed soul orb, not a comet: a pale mint inner ember is partially enclosed by one asymmetrical crescent-shaped shell of muted turquoise spectral fire. The crescent opening faces screen-right, making the travel direction readable without using a tail. Add one tiny detached turquoise spark inside the open side of the crescent, no farther than 1 pixel from the main silhouette. Use a dark 1-pixel outline matching #1F1A1A, muted dark teal, desaturated turquoise, and a restrained pale mint core. Match the approved Wraith Shaman gameplay sprite's pixel density, limited palette, upper-left lighting, and crisp integer-pixel rendering. Alpha must be either 0 or 255. No tail, trailing flame, comet streak, arrowhead, wings, skull face, eye, magic circle, smoke, ice crystal, lightning, neon glow, soft halo, blur, antialiasing, shadow, text, guides, impact burst, or multiple variants. Output exactly one isolated projectile, not a sprite sheet.

## 2단계 — 6프레임 비행 루프 프롬프트

Use the approved single Wraith Shaman Soul Bolt v2 reference sprite as the strict authoritative design. Create one horizontal pixel-art flight-loop sprite sheet with exactly 6 frames. Each frame occupies one logical 100x100 pixel cell, producing a final transparent RGBA image exactly 600x100 pixels. Keep the projectile centered at the same x=50, y=50 anchor in every frame; runtime code moves it through the world. Preserve the approximately 12x12 silhouette, pale mint inner ember, one asymmetrical crescent-shaped turquoise shell with its opening facing screen-right, one-pixel #1F1A1A outline, and the fixed muted teal/turquoise/mint palette. Animate a restrained contained-energy cycle: the crescent shell rocks clockwise and counterclockwise by changing only 1–2 boundary pixels, the inner ember alternates between two brightness steps, and the single small spark shifts around the open side by at most 1 pixel. The orb must never grow or shrink by more than 1 pixel, and frame 6 must transition seamlessly into frame 1. Keep its center of mass fixed to prevent pivot jitter. Use only integer pixels and alpha values of 0 or 255. No tail, travel within cells, full rotation, comet streak, new particles, impact flash, explosion, magic circle, skull, eye, text, guides, background color, antialiasing, blur, glow filter, or interpolation.

단일 기준 스프라이트를 먼저 승인한 뒤에만 6프레임 비행 루프를 생성한다.

## Unity 연결

- 비행 시트: `Assets/Art/Generated/Projectiles/WraithShaman/WraithShaman-SoulBolt-Flight.png`
- 규격: 600x100px, 100x100 논리 셀 6개, 프레임별 실제 불투명 영역 11x12px
- 임포트: Sprite Multiple, 16 PPU, Point, 압축 없음, 타이트 슬라이스
- 비행 루프: `Assets/Animations/Projectile/WraithShaman/WraithShaman_SoulBolt_Flight.anim`, 8 FPS 반복
- 컨트롤러: `Assets/Animations/Projectile/WraithShaman/WraithShaman_SoulBolt.controller`
- 프리팹: `Assets/Prefabs/Projectile/WraithShaman_SoulBolt.prefab`
- 망령 주술사의 `RangedMonsterAttack`이 `Body/FirePoint`에서 해당 프리팹을 발사한다.
