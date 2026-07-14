# PCS 32Bit Endian Toggle Semantics

- Task ID: 20260602-093444-pcs-32bit-endian-toggle-semantics
- Status: active
- Created: 2026-06-02 09:34:44

## Goal

PCSView의 32bit Read 토글을 IsChecked=true일 때 Big Endian, false일 때 Little Endian으로 동작하도록 변경한다.

## Scope

Included:
- PCS 32bit Read 토글 바인딩 속성명/동작 의미 변경
- PCSView 토글 표시 문구 변경

Excluded:
- 32bit Write 순서 변경
- HomeView 로드 선택 기능 추가 변경
- PCS 프로토콜 주소 변경

## Impacted Files

- EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs
- EMS_PJT_Hamburger/Views/PCSView.xaml

## Test Strategy

- Unit: harness unit gate; 테스트 프로젝트 부재 시 경고 확인
- Integration: harness integration gate; 테스트 프로젝트 부재 시 경고 확인
- Static analysis: harness static analysis gate
- Build: MSBuild Release Any CPU
- E2E: harness WPF artifact validation

## Rollback

이번 태스크의 PcsModel.cs, PCSView.xaml 변경만 되돌리면 이전 토글 의미로 복귀한다.

## Notes

- Harness guard must pass before app code edits.
