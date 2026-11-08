# WorkLog Studio — Folder-Scoped Architecture & UI Rules

이 규칙은 `WorkLog/` 디렉토리 내부 파일을 열람하거나 수정할 때 자동으로 활성화되는 전용 설계 수칙입니다.

## 1. 픽셀 퍼펙트(Pixel-Perfect) 레이아웃 & 좌표 불변 규칙
- **상단 HUD (`pnlTopHud`, `Y = 8`, `Height = 30`)**:
  - **좌측 통계 & 액션**: 통계 라벨 동적 체이닝 ➔ 구분선(`pnlStatsSep`) ➔ `btnAddNew` (**110×30**, `➕ 새 업무`, 에메랄드 그린 `#00E676`).
  - **우측 본문 줌 & 리포트 (`Anchor = Top | Right`)**: `btnDetailZoomIn` (26×30) ➔ 4px ➔ `btnDetailZoomOut` (26×30) ➔ 6px ➔ `btnDailyReport` (**110×30**, `📋 일일보고 복사`, 네온 시안 `#00E5FF`).
- **좌측 상단 필터바 (`pnlLeftTop`, `Height = 78`)**:
  - 1행: 검색창 `txtSearch` (`Dock = Top`).
  - 2행: `flowQuickFilter` (전체, 진행, 대기, 오늘, 긴급, 완료 숨김) + **우상단 목록 줌 버튼 `pnlListZoom` (`Dock = Right`)**:
    - `btnZoomIn` (26×26) & `btnZoomOut` (26×26)이 완료 건 우측, 목록 바로 위 우상단에 항시 고정 배치되어 목록 글자 크기를 즉각 제어.
- **우측 상세 헤더 1열·2열 수직 정렬 & 모던 카드 UI**:
  - **1열 트렌디 라운드 칩 뱃지 (`X = 1`, `Width = 68`, `Height = 28`, `Radius = 4px`)**:
    - `lblDetailNo` (`#번호`, Consolas Bold, 일렉트릭 시안)와 `lblTitleBadge` (`📌 제목`, 맑은 고딕 Bold)는 부드러운 안티에일리어싱 라운드 카드 칩(`Color.FromArgb(20, 28, 40)`)으로 일체감 있게 렌더링.
  - **2열 컨트롤 열 (`X = 75`)**:
    - 1행: `btnStatusToggle` (`X=75`), `btnPriorityToggle` (`X=169`), `dtpDate` (`X=257`), `lblLastUpdated` (`X=414`, `수정: MM-dd HH:mm`), 우측 끝 액션 패널 `pnlDetailActions` (`Dock = Right`, `Width = 148`: `btnSaveEntry` 68px, `btnDeleteEntry` 68px 잘림 방지 완벽 격리).
    - 2행: `pnlTitleWrapper` (`X=75`, `Height=30`, `Anchor = Top|Left|Right`, 라운드 코너 카드형 인풋 래퍼, 포커스 시 네온 시안 글로우 하이라이트 전환).

## 2. 폰트 줌(Zoom) & GDI+ 제로 리소스 규칙
- **좌측 목록(`OlvWorkLog`) 폰트 줌**:
  - `UpdateFontSize` 호출 시 반드시 `OlvWorkLog.BuildList(true)` 및 `OlvWorkLog.Refresh()`를 동기 호출하여 마우스 호버 없이도 즉시 행 높이(`RowHeight`)와 폰트가 갱신되게 합니다.
  - `DrawSubItem` 내부에서는 `e.SubItem.Font` 대신 폼의 캐시된 `_regularFont` / `_boldFont`를 직접 참조합니다.
- **우측 상세(`txtContent`, `txtMemo`) 폰트 줌**:
  - 기본 14pt Regular (`맑은 고딕`, 한글 가독성 대폭 향상), 범위 `8f ~ 24f` 클램프, 구버전 12pt 저장값은 최초 로드 시 14pt로 자동 승격 후 `WorkLogData.DetailFontSize`에 자동 영속화.
  - `Ctrl + 마우스 휠` 입력 시 `HandledMouseEventArgs.Handled = true`로 원치 않는 스크롤을 차단하고 폰트 교체 시 이전 폰트를 즉시 `Dispose()`합니다.
- **상태·중요도별 행 배경 및 우선순위 캐시 (DONE > 긴급/지연 > ING > OPEN)**:
  - **1순위 (DONE/FIXED)**: 항상 차분한 파스텔 쿨 슬레이트 그레이(`42,50,62`) + 소프트 그레이 텍스트 (긴급/지연 여부와 무관하게 완료 최우선).
  - **2순위 (미완료 긴급 또는 7일 경과 지연)**: 소프트 파스텔 로즈레드 음영(`64,26,32`) + 코럴 레드(`255,120,120`) 볼드 텍스트.
  - **3순위 (미완료 ING..)**: 부드러운 파스텔 웜 옐로우(`70,58,22`) + 크림 옐로우 텍스트.
  - **4순위 (대기 OPEN 보통/낮음)**: 별도 음영 없는 기본 다크(`13,17,23`).
  - **중요도 표기**: 긴급(`HIGH`)만 `🔥긴급` 불꽃 이모지 적용, 보통(`NORMAL`)과 낮음(`LOW`)은 이모지 없이 텍스트(`보통`, `낮음`)로 정갈하게 표기.
  - `ObjectListView.FormatRow` 및 `DrawSubItem` 2중 동기화로 렌더링 무결성을 보장하며 모든 브러시는 정적 캐시(`static readonly SolidBrush`)로 유지합니다.

## 3. 데이터 무결성 & 하위 호환성 (Backward Compatibility)
- **저장 경로**: `_meta/WorkLog.json`, 백업 `_meta/WorkLog.json.bak`, 캡처 폴더 `_meta/WorkLog_Images/`.
- **구버전 호환(`WorkLogRepository.cs`)**:
  - 2025년 초기 원시 JSON 배열(`[ { ... } ]`) 포맷과 현행 `WorkLogData` 래퍼 오브젝트 포맷을 모두 자동 감지하여 역직렬화합니다.
  - 과거 상태값(`"Open"`, `"Done"`, `"Blocked"`)은 대문자(`"OPEN"`, `"DONE"`)로 자동 정규화하며, 기존 `Tags` 및 `Memo` 필드를 절대 유실하지 않습니다.
- **메모리 격리 및 UX 안전장치**:
  - `WorkLogImagePopupForm` (팝아웃 새 창)은 원본 이미지를 `new Bitmap(img)`으로 독립 복제하여 메인 창 이미지 전환 시 GDI+ 크래시를 원천 방지합니다.
  - 새 업무(`btnAddNew`) 클릭 시 활성 필터가 새 항목을 가리지 않도록 퀵 필터를 `"ALL"`로 전환하고 검색어를 초기화합니다.

