<!-- 새 화면 추가 시 이 파일을 .claude/rules/views/<name>.md 로 복사하고 아래 paths 를 그 화면 파일들로 바꾼다.
     paths 블록이 로딩 스코프를 결정한다(해당 화면 편집 시에만 이 문서가 로드됨). -->
---
paths:
  - "EMS_PJT_Hamburger/Views/<Name>View.xaml"
  - "EMS_PJT_Hamburger/Views/<Name>View.xaml.cs"
  - "EMS_PJT_Hamburger/ViewModels/<Name>ViewModel.cs"
---
# View: <Name>

## 목적
<이 화면이 무엇을 보여주고/제어하는가>

## 상태
<구현 완료 / 진행 중 / TODO>

## 담당 ViewModel / 소유
<VM 클래스, App.xaml.cs 에서의 인스턴스(App.XxxVm 등)>

## 데이터 & 외부 I/O
<Modbus/CAN/Serial 태그, DB 테이블, App.config 키 등>

## UI 표면
<주요 바인딩·커맨드·컨트롤>

## 주의사항 / 규칙
<이 화면 특유의 함정, 스레딩, 리소스 해제 등>

## 관련 문서
<.claude/docs/... , docs/... 링크>
