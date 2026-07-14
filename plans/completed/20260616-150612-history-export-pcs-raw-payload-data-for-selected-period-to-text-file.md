# History export PCS raw payload data for selected period to text file

- Task ID: 20260616-150612-history-export-pcs-raw-payload-data-for-selected-period-to-text-file
- Status: active
- Created: 2026-06-16 15:06:12

## Goal

History View에 조회 기간(StartDate~EndDate)의 PCS 측정 raw 데이터를 파일로 내보내는
Export 버튼 추가. tb_ems_raw_data.payload_json(Deflate 압축 JSON)을 해제해 필드별 컬럼으로
탭 구분 텍스트(.txt, Excel에서 열림)로 저장.

## Scope

- HistoryViewModel: Cmd_ExportRaw + ExportRawData()
  - tb_ems_raw_data(source=PCS, 기간) 조회 -> payload_json bytea Decompress -> JSON 파싱
  - 필드 union을 컬럼으로, collected_at + 값들을 탭 구분으로 출력
  - SaveFileDialog(.txt) + File.WriteAllText. 상태 메시지.
  - 헬퍼: ReadBytea, ParseJsonToStringDict.
- HistoryView.xaml: 헤더 필터줄에 Export 버튼(ActionButtonStyle).
- 제외: BMS export(요청은 PCS 측정 데이터), 실제 .xlsx 생성(라이브러리 없음 -> .txt로 제공).

## Impacted Files

- EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs
- EMS_PJT_Hamburger/Views/HistoryView.xaml

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): 기간 조회 후 Export -> 다이얼로그 -> 탭 구분 txt 생성, Excel에서 컬럼 정렬 확인.

## Rollback

- 두 파일 원복.

## Notes

- 기존 ExportAlarm(SaveFileDialog+WriteAllText) 패턴, Decompress(Deflate) 재사용.
- payload는 StatusData 제외 측정 필드만 포함. 대용량 기간은 동기 처리라 잠깐 멈출 수 있음.
