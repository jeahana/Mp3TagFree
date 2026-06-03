# Git 작업 내용 요약

본 문서에는 Mp3TagFree 프로젝트의 신규 기능 개발 및 UI 개선 사항에 대한 Git 작업 기록을 요약하여 기록합니다.

---

## 1. Git 저장소 설정 및 초기화

로컬 작업 디렉토리를 Git 저장소로 구성하고 원격 GitHub 저장소에 연동하기 위해 다음 명령어를 수행했습니다.

* **저장소 초기화**: 
  ```bash
  git init
  ```
* **원격 저장소 추가**: 원격 주소 `origin`을 설정하였습니다.
  ```bash
  git remote add origin https://github.com/jeahana/Mp3TagFree.git
  ```

---

## 2. 파일 제외 설정 (.gitignore 생성)

빌드 컴파일 산출물 및 개발 환경 설정 정보가 원격 저장소에 포함되지 않도록 프로젝트 루트 디렉토리에 `.gitignore` 파일을 생성하여 관리 대상에서 제외했습니다.

* **제외 항목**:
  * 빌드 산출물 폴더: `bin/`, `obj/`
  * IDE 개발 설정 폴더: `.vs/`, `.vscode/`
  * 개발 환경 프로젝트 파일: `*.user`, `*.suo`
  * 로컬 AI 에이전트 생성 파일: `.agents/`, `.antigravitycli/`

---

## 3. 커밋 (Commit) 내역

신규 구현 및 수정 사항을 반영하여 첫 번째 커밋을 작성했습니다.

* **커밋 메시지**: 
  `feat: Add batch file renaming, double-click playback, and dark theme UI fixes`
* **반영된 신규/수정 파일 목록**:
  * **[NEW]** `.gitignore`: 빌드 산출물 제외 규칙 파일
  * **[NEW]** `Models/RenamePreviewItem.cs`: 파일명 변경 미리보기 바인딩용 데이터 모델
  * **[NEW]** `ViewModels/RenameViewModel.cs`: 이름 패턴 변환 알고리즘, 특수문자 정제 및 중복/파일 존재 유무 검증 모델
  * **[NEW]** `RenameWindow.xaml`: ModernWPF 스타일을 입힌 파일명 변경 모달 팝업 레이아웃
  * **[NEW]** `RenameWindow.xaml.cs`: 패턴 텍스트 상자 커서 삽입 이벤트 헨들러 및 변경 트리거 로직
  * **[MODIFY]** `MainWindow.xaml`: 메인 화면 상단 툴바 내 "파일명 변경..." 액션 버튼 추가 및 그리드 셀의 다크 테마 배경/폰트 테마 색상 연동
  * **[MODIFY]** `MainWindow.xaml.cs`: 그리드 더블클릭 이벤트 발생 시 OS 기본 미디어 플레이어로 오디오를 즉시 실행해주는 `Process.Start` 재생 코드 구현
  * **[MODIFY]** `ViewModels/MainViewModel.cs`: 파일명 변경 버튼 클릭 시 팝업창을 띄우고 데이터 바인딩을 연계하는 명령 추가

---

## 4. 원격 저장소 푸시 (Push)

로컬의 `main` 브랜치를 원격(GitHub) 저장소로 푸시 완료했습니다.

```bash
git push -u origin main
```
* **결과**: GitHub 원격 저장소의 `main` 브랜치로 모든 프로젝트 소스코드가 성공적으로 안전하게 푸시되었습니다.
