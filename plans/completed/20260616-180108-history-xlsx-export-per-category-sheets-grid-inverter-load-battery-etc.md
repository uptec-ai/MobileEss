# History xlsx export per category sheets Grid Inverter Load Battery Etc

- Task ID: 20260616-180108-history-xlsx-export-per-category-sheets-grid-inverter-load-battery-etc
- Status: active
- Created: 2026-06-16 18:01:08

## Goal

Export xlsx를 카테고리별 시트(Grid/Inverter/Load/Battery/Etc)로 분리. 각 시트는
collected_at + 해당 카테고리 컬럼만 포함 -> 탭(목록) 선택 시 관련 컬럼만 표시.
(진짜 PivotTable은 라이브러리 없이 비현실적 -> 멀티시트로 동등 목적 달성)

## Scope

- HistoryViewModel:
  - BuildCategorySheets(columns): PcsSpecs Grid/Inv/Load/Battery/Etc Data로 컬럼 분류,
    미분류는 Etc로, 비어있는 시트는 생략, 전부 비면 PCS 단일.
  - WriteXlsx를 멀티시트로 확장(시트별 Content_Types/workbook/rels 동적 생성, WriteSheet).
- 제외: 데이터/필터 로직, txt 경로 변경 없음.

## Impacted Files

- EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- 구조 검증: 동일 멀티시트 OpenXML 샘플 생성 -> zip 엔트리/XML well-formed 확인.
- E2E(수동): Excel에서 탭 5개(Grid/Inverter/Load/Battery/Etc), 각 탭 관련 컬럼만.

## Rollback

- HistoryViewModel.cs 원복.

## Notes

- 카테고리 = PcsSpecs.{GridData,InvData,LoadData,BatteryData,EtcData} 필드명 기준.
- 각 시트 첫 컬럼 collected_at 공통.
