# GateHelper Project - Agent Handover Context

이 문서는 Antigravity Agent가 다른 PC나 새로운 대화 세션에서도 프로젝트의 현재 상태와 맥락을 곧바로 파악할 수 있도록 기록된 인수인계(Handover) 문서입니다. Agent는 새로운 대화가 시작될 때 자동으로 이 문서를 읽고 이전 상황을 이어갑니다.

## 📌 최근 작업 내역 (최종 업데이트: 2026-08-11)

**[Excel Analyzer UX 개선 및 프로그램 아이콘 복구]**
1. **Excel Analyzer 드래그 앤 드롭 오버레이 클릭 지원 (`ExcelAnalyzerForm.cs`)**
   - 기존 드래그 앤 드롭 방식 외에도, 화면 덮개 패널이나 텍스트를 클릭 시 `OpenFileDialog`가 열려 파일 탐색기에서 직접 엑셀 파일을 선택할 수 있도록 UX를 개선했습니다.
   - 처음 사용하는 사용자를 위해 오버레이 상단에 "💡 MGMT로 수집된 Variable Data를 분석해주는 기능입니다." 라는 설명 문구를 추가했습니다.
2. **프로그램 대표 아이콘(ApplicationIcon) 비율 복구 (`GateHelper.csproj`, `ico_gateone_fixed.ico`)**
   - 과거 잘못 생성된 정사각형 형태의 아이콘(`ico_gateone.ico`) 때문에 로고가 세로로 길쭉하게 찌그러져 보이던 현상을 해결했습니다.
   - 원본 PNG 이미지의 가로 직사각형 비율을 유지한 채 위아래 투명 여백(Padding)을 덧댄 정품 256x256 아이콘(`ico_gateone_fixed.ico`)을 새로 생성하여 프로젝트에 적용했습니다.

---

## 📌 이전 작업 내역 (2026-07-29)

**[코드 컨벤션 포맷 통일 및 ReleaseNotes 갱신]**
1. **Region 주석 포맷 영문화 및 통일 (`Util.cs`, `ChromeDriverManager.cs`, `LogValidator` 등 전역)**
   - 소스코드 내 파편화되어 있던 한글 `#region` 블록들을 `[Category Name]` 형태의 직관적인 쉬운 영문 포맷으로 일괄 변경하여 가독성을 높였습니다.
2. **ReleaseNotes 업데이트 (`Util.cs`)**
   - UI 화면 원복 버그 픽스, Region 포맷 통일, Excel Analyzer 탭 추가 등의 최신 작업 내역을 릴리즈 노트 텍스트(v2.4.5)에 반영했습니다.

**[NuGet 패키지 의존성 충돌 해결 및 Excel Analyzer 규칙 우클릭 메뉴 구현]**
1. **.NET 9.0 패키지 의존성 런타임 충돌 문제 완벽 해결 (`packages.config`, `.csproj`, `App.config`)**
   - NuGet 업데이트 시 유입된 `.NET 9.0` 프리뷰 기반 패키지(`System.Memory 4.6.x` 등)가 `.NET Framework 4.8` 및 `ClosedXML`과 충돌하여 파일 드롭 시 어셈블리 불일치 에러(FUSION_E_REF_DEF_MISMATCH)를 내뿜는 현상을 해결했습니다.
   - 꼬여있던 `System.Memory` 등을 안정 버전으로 롤백하고, 보안 취약점이 픽스된 `System.Security.Cryptography.Xml (8.0.4)` 단일 패키지만 안전하게 업데이트했습니다.
   - `App.config` 내 하드코딩된 바인딩 리다이렉트 정보를 전면 삭제하고, MSBuild의 `AutoGenerateBindingRedirects` 속성을 활용해 빌드 시점에 무결점인 `GateHelper.exe.config`가 자동 생성되도록 구조를 개선했습니다.

2. **Excel Analyzer 규칙 관리용 우클릭 컨텍스트 메뉴(ContextMenu) 도입 (`ExcelAnalyzerForm.cs`)**
   - 사용자 편의성 극대화를 위해 규칙 리스트박스(`LstScenarios`) 안에서만 동작하는 전용 `ContextMenuStrip`을 신규 구현했습니다.
   - **주요 기능**: `위로 이동`, `아래로 이동`, `이름 변경`(전용 Input 다이얼로그 호출), `복제`(룰 복사본 즉시 생성), `삭제`(경고창을 통한 실수 방지)
   - 모든 메뉴 액션은 내부 데이터 동기화(`_config.Profiles`), UI 새로고침 최적화(`_isUpdatingUI`), 그리고 영구 저장(`SaveConfig()`)을 원스톱으로 처리합니다.

**[메인 UI 원복 기능 추가 및 테마 버그 픽스]**
1. **UI 강제 원복(Reset) 기능 구현 (`MainUI.cs`, `MainUI.Designer.cs`)**
   - 윈도우 디스플레이 배율 변경 등으로 폼 레이아웃이 화면 밖으로 벗어나거나 내부 컨트롤이 깨졌을 때, 프로그램 재시작 없이(크롬 드라이버 연결 유지) 창 크기를 복구하는 기능을 추가했습니다.
   - 우측 상단 타이틀 바의 버전 텍스트 바로 옆에 조그만 🔄 아이콘(라벨)을 배치했으며, 버전 텍스트 길이가 동적으로 변하더라도 항상 우측에 딱 맞게 앵커링되도록 `SizeChanged` 이벤트를 바인딩했습니다.
   - 복구 버튼 클릭 시 즉각 동작하지 않고 `MessageBox`로 `확인/취소` 절차를 거치게 하여 사용자의 오조작을 방지했습니다.
   - 단순 폼 사이즈만 줄이는 것이 아니라 내부 패널/그룹 박스의 위치와 크기까지 함께 되돌리기 위해 `Util_Control.ToggleFormLayout`의 축소 로직을 강제로 태우도록 구현했습니다.

2. **MaterialSkin 타이틀 바 투명 배경색(BackColor) 충돌 버그 픽스 (`MainUI.cs`)**
   - 디자이너 툴에서는 정상적으로 보이나 실제 런타임 시 폼 배경(흰색/회색)이 드러나 흰색 상자가 생기는 버그를 해결했습니다.
   - `ThemeManager`에 의해 테마(Dark/Light)가 변경될 때마다 `MaterialSkinManager`가 라벨들의 `BackColor`를 폼 바탕색으로 강제 덮어쓰는 것을 확인하고, `ThemeChanged` 이벤트 안에서 타이틀 바 텍스트들의 배경색을 `DarkPrimaryColor`로 즉각 원상복구(Override) 시키는 방어 로직을 추가했습니다.

---

## 📌 이전 작업 내역 (2026-07-24)

**[Excel Analyzer 기능 신규 구현 및 Rule Setup UI 고도화]**
1. **Excel Analyzer 기초 뼈대 및 데이터 파싱 (`Util_ExcelAnalyzer.cs`)**
   - Drag & Drop으로 엑셀 파일(.xlsx)을 받아 ClosedXML 라이브러리를 통해 비동기(Async) 파싱하는 로직을 구축했습니다.
   - 다중 시트(Multi-sheet) 처리를 위해 시트명, 호기명, 변수명, 설정값, 설명(Description)을 동적으로 매핑할 수 있도록 구현했습니다.

2. **스마트 컬럼 매핑 및 UX 최적화 (`ExcelAnalyzerForm.cs`)**
   - 과거 사용자의 컬럼 매핑 이력을 기억하여 자동 선택하고, 없을 경우 EQP, Item, Value 등 실무 키워드를 기반으로 콤보박스를 자동 유추 매핑합니다.
   - 불필요한 팝업을 제거하고, 엑셀 파일 없이도 기존에 저장된 프로필로 직접 Rule Setup에 진입할 수 있는 기능을 추가했습니다.

3. **Rule Setup 폼 및 툴팁 가독성 개선**
   - `MaterialCheckedListBox`의 중복 버그를 우회하여 시나리오(RuleProfile)별로 호기/고유/공통 변수 체크 항목이 안전하게 로드 및 덮어쓰기되도록 구조를 분리했습니다.
   - 설정값이나 설명(Description) 텍스트가 매우 길어 잘리는 현상을 방지하기 위해 70자 기준 자동 줄바꿈(`WrapText`) 로직을 내장한 스마트 ToolTip을 적용했습니다.

4. **Excel Analyzer 검증 결과 UI 시인성 및 테마 최적화**
   - 다크모드와 라이트모드 환경에 맞춰 오류(Red)와 정상(Green) 텍스트 색상을 분리 적용했습니다. (다크모드: 코랄레드/밝은그린, 라이트모드: 기본레드/포레스트그린)
   - 데이터그리드뷰(DataGridView)의 배경색을 연하게 조정하고 세로선(GridLine)을 추가하여 표 데이터를 읽을 때의 피로도를 낮추고 가독성을 대폭 개선했습니다.
   - 결과 상태 아이콘을 기존 박스형(✅)에서 깔끔한 일반 체크마크(✔)로 교체하여 에러 아이콘(❌)과 디자인 통일성을 맞추었습니다.
   
---

## 📌 이전 작업 내역 (2026-07-23)

**[UI 설정 자동 저장 및 백그라운드 타이머 버그 픽스]**
1. **옵션 설정 자동 저장 기능 구현 (`user_options.json`)**
   - 기존 설정 파일(`settings.config`)을 손상시키지 않기 위해 UI 체크박스 옵션 상태를 `_meta/user_options.json`에 별도로 저장하고 불러오는 기능을 추가했습니다. (`Util_Option.cs`, `OptionForm.cs`)
   - `Auto Screen Unlock` 등 사용자 맞춤 옵션이 프로그램 재시작 시에도 유지되도록 개선되었습니다.

2. **자동 로그인 타이머 인터락 방어 범위 전체 확장 (`MainUI.cs`)**
   - 백그라운드 타이머가 포커스를 강탈하여 자동 로그인의 키보드 입력(SendKeys)이 씹히는 잠재적 충돌을 방지하기 위해, `PerformGateOneAutoLogin` 전체 구간을 `IsAuthInProgress` 인터락으로 보호하도록 재배치했습니다.

3. **MGMT 타겟 URL 빈 문자열 매칭(오인) 버그 픽스 (`Util_BackgroundMonitor.cs`)**
   - 설정 파일에서 MGMT 주소를 입력하지 않았을 때(`""`), 모든 팝업이 MGMT 탭으로 오인 감지되는 현상을 수정했습니다. (`!string.IsNullOrEmpty` 조건 추가)

**[자동 로그인 오류 알림 세분화 및 Toast UI 도입]**
1. **오류 시퀀스 세분화 (`Util_Connect.cs`)**
   - 기존의 자동 로그인(OTP 인증) 6개 단계 메서드(`AutoConnect_1` ~ `AutoConnect_6`)가 모두 `out string errorMessage`를 반환하도록 리팩토링했습니다.
   - 각 구간마다 실패 원인에 맞춰 세분화된 문구(예: URL 누락, 메일 못 찾음, 추출 실패 등)를 생성하여 반환합니다.

2. **모던 토스트 알림 팝업 (`ToastNotification.cs`) 신규 제작**
   - 백그라운드 구동에 적합하지 않은 투박한 `MessageBox`를 대체하기 위해 직접 `ToastNotification` 윈폼 UI를 제작했습니다.
   - 우측 하단에서 부드럽게 나타나며(Fade-in), 사용자가 돌아와서 내용을 확인한 후 클릭하면 자연스럽게 사라지도록(Manual Dismiss) 구현했습니다.
   - 알림 창에 `[GateHelper]` 접두사를 붙여 스팸 알림 등과 혼동되지 않도록 조치했습니다.

3. **`MainUI.cs` 에러 핸들링 연동 및 버그 수정**
   - `PerformGateOneAutoLogin()`에서 위 반환된 에러 메시지들을 받아 신규 도입된 `ToastNotification.Show(...)`를 띄워주도록 로직을 일괄 교체했습니다.
   - 6단계 마지막 키보드 Action 타이핑 실패 시 멈추지 않고 성공 처리되던 논리적 버그(Logical Bug)를 픽스했습니다.

4. **비동기화(`async/await`) 처리 및 중복 실행 방지**
   - `PerformGateOneAutoLogin()` 로직 전체를 `Task.Run()`을 사용해 백그라운드 스레드로 넘겨, 15초 대기 중 윈도우(UI)가 "응답 없음"에 빠지는 현상을 완전히 해결했습니다.
   - `BtnStart1` 버튼이 실행 시점에 즉각 `Enabled = false`가 되도록 안전 장치를 확인하여, 광클릭으로 인한 이중 실행을 원천 차단했습니다.

---

## 📌 이전 작업 내역 (2026-07-22)

**[백그라운드 타이머 리팩토링 및 안정화]**
1. **`Util_BackgroundMonitor.cs` 신규 분리**
   - 기존 `MainUI.cs`에 얽혀 있던 거대한 `TimerStatusChecker_Tick` 로직을 독립적인 클래스로 완전히 분리했습니다.
   - 탭 갯수를 캐싱(`_lastWindowCount`)하여 변화가 있을 때만 브라우저 탭(WindowHandles)을 정밀 순회하도록 최적화했습니다. (부하 획기적 감소)
   - 5초마다 하던 URL 파싱 작업을 생성자 1회로 줄였습니다.
   - 타이머 중복 실행을 막기 위한 동시성 Lock 처리를 가장 바깥쪽에 배치했습니다.

2. **포커스 강탈 버그 수정**
   - 공지사항이나 구글 등 다른 사이트 팝업이 떴을 때, 5초마다 타이머가 포커스를 강제로 가져가는 현상을 수정했습니다.
   - 타겟 관리 사이트가 아닌 탭은 `_ignoredHandles`에 등록되어 이후 탭 검사에서 완벽히 무시됩니다.

3. **`MainUI.cs` 호환성 유지 적용**
   - 기존에 사용되던 `managementHandle`, `_isManagementActive` 전역 변수를 `_bgMonitor` 객체를 바라보는 **프로퍼티(Property)** 로 변경했습니다. 
   - 덕분에 다른 파일이나 기존 버튼 로직들을 뜯어고치지 않고도 완벽하게 호환을 유지하며 컴파일 에러(0개)를 달성했습니다.
   - 세션 강제 종료 시 예외를 잡아 드라이버를 `null`로 초기화하는 방어 코드를 적용했습니다.

## 🚀 다음 에이전트(나)에게 남기는 메모
- **현재 상태**: 모든 코드가 Release/Debug 환경에서 성공적으로 빌드(`에러 0개`)되며, `master` 브랜치에 커밋 및 푸시 되었습니다. (이전 22일자 작업 내역 포함)
- **사용자(User)의 다음 행동**: 사용자는 변경된 최신 릴리즈 빌드판을 통해 다른 환경에서 테스트하거나 실사용할 예정입니다.
- **행동 지침**: 향후 자동 로그인 프로세스에서 추가로 예외 상황이 발생한다면 `Util_Connect.cs`의 각 단계별 `try-catch` 또는 분기문에 에러 문구를 추가하고, 새로운 UI 요소 알림이 필요하다면 `ToastNotification.cs`를 재사용하세요. 이전 백그라운드 타이머 로직과 관련해서는 `Util_BackgroundMonitor.cs`를 참고하십시오.
