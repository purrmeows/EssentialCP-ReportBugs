# RiposteExplorer Configuration

> Report Bugs feature configuration for RiposteExplorer system

---

##  Global Command Definition

**Path:** `/Configurations/GlobalCommandDefinition`

**Create Object:** `ReportBugs`

```text
<Data:
    <Icon:Report>
    <Caption:THP_ReportBugs>
    <HotKeyCode:F11>
    <WorkflowName:Std.ReportBugs>
    <RegionName:Toolbar>
    <RunMode:Always>
    <AbortCurrent:0>
    <Order:6>
>
```

---

##  Workflow Definition

**Path:** `/Configurations/Workflow/Definitions`

**Create Object:** `Std.ReportBugs`

```text
<Data:
    <Sequence:
        <Item:
            <ModuleName:THP.Extention.ReportBugs>
            <ActionName:ReportBugs>
        >
    >
>
```

---

##  Text Resources (EN)

**Path:** `/Configurations/Text/EN`

```text
-THP_ReportBug_Delete           <Data:<Value:Delete Data>>
-THP_ReportBug_Description      <Data:<Value:Description of the issue>>
-THP_ReportBug_Grid_Date        <Data:<Value:Date>>
-THP_ReportBug_Grid_Description <Data:<Value:Description>>
-THP_ReportBug_Grid_No          <Data:<Value:No>>
-THP_ReportBug_Grid_Status      <Data:<Value:Status>>
-THP_ReportBug_HistoryTitle     <Data:<Value:Problem reporting history>>
-THP_ReportBug_Image            <Data:<Value:Image Imported>>
-THP_ReportBug_Title            <Data:<Value:Report a system error>>
-THP_ReportBugs                 <Data:<Value:Report Bugs>>
```

---

##  Text Resources (TH)

**Path:** `/Configurations/Text/TH`

```text
-THP_ReportBug_Delete           <Data:<Value:ลบข้อมูล>>
-THP_ReportBug_Description      <Data:<Value:รายละเอียดปัญหาที่พบเจอ>>
-THP_ReportBug_Grid_Date        <Data:<Value:วัน/เวลา>>
-THP_ReportBug_Grid_Description <Data:<Value:รายละเอียด>>
-THP_ReportBug_Grid_No          <Data:<Value:ลำดับ>>
-THP_ReportBug_Grid_Status      <Data:<Value:สถานะ>>
-THP_ReportBug_HistoryTitle     <Data:<Value:ประวัติการแจ้งปัญหา>>
-THP_ReportBug_Image            <Data:<Value:รูปภาพที่นำเข้า>>
-THP_ReportBug_Title            <Data:<Value:แจ้งรายงานข้อผิดพลาดของระบบ>>
-THP_ReportBugs                 <Data:<Value:รายงานข้อผิดพลาด>>
```