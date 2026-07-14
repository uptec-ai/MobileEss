# Exclude PcsTime fields and fixed width align history raw export txt

- Task ID: 20260616-163850-exclude-pcstime-fields-and-fixed-width-align-history-raw-export-txt
- Status: active
- Created: 2026-06-16 16:38:50

## Goal

1. PcsTime*(Year/MonthDay/HourMinute/SecondMs) 4개 필드를 저장(payload)과 export에서 제외.
2. Export txt를 탭 -> 고정폭(공백 패딩) 정렬로 변경해 텍스트 편집기에서 컬럼 정렬.
   (가로 스크롤은 편집기의 Word Wrap off로 제공 — 파일 자체엔 스크롤바 불가)

## Scope

- PcsModel.SavePcsRawData: excluded 집합에 PcsSpecs.TimeData 추가(저장 중단).
- HistoryViewModel.ExportRawData:
  - PcsSpecs.TimeData 이름을 export 컬럼에서 제외(기존 데이터 대응).
  - 컬럼별 최대폭 계산 후 PadRight로 고정폭 출력(컬럼 간 2칸).
- 제외: 데이터 의미/그 외 export 형식 변경 없음.

## Impacted Files

- EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs
- EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): export txt에 PcsTime* 없음, 컬럼이 고정폭으로 정렬, 편집기 Word Wrap off 시 가로 스크롤.

## Rollback

- 두 파일 원복.

## Notes

- 고정폭 정렬은 monospace 폰트에서 보임. 기존 저장분의 PcsTime*는 export 필터로 숨김.
