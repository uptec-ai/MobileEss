# History raw export to native xlsx via OpenXML writer

- Task ID: 20260616-165816-history-raw-export-to-native-xlsx-via-openxml-writer
- Status: active
- Created: 2026-06-16 16:58:16

## Goal

History raw export를 고정폭 txt -> 진짜 .xlsx로 변경. 외부 스프레드시트 라이브러리가
없으므로 System.IO.Compression으로 최소 OpenXML(.xlsx) 패키지를 직접 생성한다.
(셀 분리 -> Excel에서 컬럼 자동 정렬)

## Scope

- HistoryViewModel.ExportRawData:
  - SaveFileDialog .txt -> .xlsx.
  - 고정폭 텍스트 작성 로직 제거, WriteXlsx 호출.
  - PcsTime 제외/payload 파싱 로직은 유지.
- 신규 헬퍼: WriteXlsx(Content_Types/rels/workbook/sheet1 직접 작성, 행 스트리밍),
  AddXlsxEntry, WriteCell(숫자 v / 문자 inlineStr), XlsxColumnLetter, XlsxEscape.
- using System.IO, System.IO.Compression 추가.

## Impacted Files

- EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): Export -> .xlsx 저장 -> Excel에서 열어 컬럼 분리/정렬 확인, 숫자/시간 셀 정상.

## Rollback

- HistoryViewModel.cs 원복.

## Notes

- sheet1.xml은 ZipArchive 엔트리에 StreamWriter로 스트리밍(대용량 메모리 절감).
- 값은 InvariantCulture 숫자면 <v>, 아니면 inlineStr. 시간은 텍스트 셀.
