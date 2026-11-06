# ADR 0004: ການແພັກ Single Portable Executable ແລະ ການນຳໃຊ້ຜ່ານ USB Offline

## ສະຖານະ (Status)
**Accepted** (ສຳເລັດ ແລະ ຜ່ານການກວດສອບໃນ Phase 5)

## ບໍລິບົດ (Context)
ຕາມຂໍ້ກຳນົດໃນ `cm-spec.md` (ຂໍ້ 7 ແລະ ເຟສ 5):
- ໂປຣແກຣມ Classroom Checker ຕ້ອງແຈກຢາຍຜ່ານ USB Flash Drive ເພື່ອໄປໃຊ້ກັບຄອມພິວເຕີຫ້ອງຮຽນທີ່:
  1. ບໍ່ມີອິນເຕີເນັດ (Offline 100%).
  2. ບໍ່ມີການຕິດຕັ້ງ runtime ຫຼື ຊອບແວພາຍນອກເພີ່ມ (Zero-install).
  3. ເປັນ Single Portable Executable (`.exe` ໄຟລ໌ດຽວ ບໍ່ມີ loose `.dll` ຕິດພັນ).
  4. ເຮັດວຽກໄດ້ຢ່າງປອດໄພ ເຖິງແມ່ນວ່າ USB ຈະຕິດສະຖານະ Read-Only.

ໃນ `cm-spec.md` ໄດ້ສະເໜີໃຫ້ໃຊ້ Costura.Fody ເພື່ອລວມ `Newtonsoft.Json` ແລະ `ClosedXML` ເຂົ້າເປັນ exe ດຽວ, ແຕ່ພ້ອມດຽວກັນກໍໄດ້ເຕືອນໄວ້ວ່າ: *"ຖ້າ Costura ເກີດບັນຫາກັບ ClosedXML → fallback... (ແຈ້ງຜູ້ໃຊ້ກ່ອນປ່ຽນ)"*.

## ການວິເຄາະ ແລະ ທາງເລືອກ (Analysis & Options)

### ທາງເລືອກ 1: ນຳໃຊ້ Costura.Fody + Third-party NuGet (ClosedXML + Newtonsoft.Json)
- **ຂໍ້ເສຍ:**
  - `ClosedXML` ຂຶ້ນກັບ `DocumentFormat.OpenXml` (ຂະໜາດໃຫຍ່ >10 MB) ແລະ `ExcelNumberFormat`.
  - ເມື່ອຖືກ bundle ຜ່ານ Costura.Fody, ມັກເກີດບັນຫາ `TypeInitializationException` ຫຼື `FileNotFoundException` ໃນ runtime ເພາະ OpenXml ໃຊ້ dynamic assembly loading.
  - ເພີ່ມຂະໜາດ `.exe` ຈາກ 380 KB ຂຶ້ນໄປເປັນ 15–20 MB.
  - ຕ້ອງເພິ່ງພາ Fody build task ເຊິ່ງອາດມີບັນຫາກັບ MSBuild environment ທີ່ແຕກຕ່າງກັນ.

### ທາງເລືອກ 2: Zero-Dependency Architecture ໂດຍໃຊ້ .NET 4.8 Built-in GAC + Native OpenXML Writer (ທາງເລືອກທີ່ເລືອກ)
- **ວິທີການ:**
  1. ໃຊ້ `System.Web.Extensions` (`JavaScriptSerializer`) ທີ່ຕິດມາກັບ .NET Framework 4.8 ແທນ `Newtonsoft.Json`.
  2. ສ້າງ `SimpleXlsxWriter` ດ້ວຍ `System.IO.Compression` (ZipArchive) ເພື່ອສ້າງໄຟລ໌ `.xlsx` ຕາມມາດຕະຖານ OpenXML ແທ້ 100% ແທນ `ClosedXML`.
  3. ຝັງ assets ທັງໝົດ (Font `PhetsarathOT.ttf`, ຮູບ `bg.png`, ພາສາ `lo.json`, `en.json`) ເປັນ `EmbeddedResource` ໂດຍກົງໃນ `.csproj`.
- **ຜົນໄດ້ຮັບ:**
  - ໂປຣເຈັກບໍ່ມີ dependency ພາຍນອກແມ່ນແຕ່ອັນດຽວ!
  - ຜົນການ build ຈະໄດ້ `ClassroomChecker.exe` ທີ່ເປັນ **Native Single Portable Executable 100% ຕັ້ງແຕ່ຕົ້ນ** ໂດຍບໍ່ຕ້ອງຜ່ານ IL weaving ຫຼື Costura.Fody.
  - ຂະໜາດ `.exe` ພຽງແຕ່ **385 KB** (ນ້ອຍກວ່າ 0.4 MB)!
  - ບໍ່ມີຄວາມສ່ຽງເລື່ອງ assembly loading failure ຫຼື Antivirus flag ເນື່ອງຈາກ memory extraction.

## ໂຄງສ້າງ Package ສຳລັບ USB (USB Distribution Layout)
ສ້າງ script `package.bat` ເພື່ອ build Release ແລະ ຈັດຕັ້ງໂຟນເດີ `dist/` ພ້ອມນຳໄປ copy ລົງ USB:
```
dist/
├── ClassroomChecker.exe           # ໂປຣແກຣມດ່ຽວ 385 KB (Portable, Zero-dependency)
├── README.txt                     # ຄູ່ມືການນຳໃຊ້ສອງພາສາ (Lao & English)
├── backgrounds/                   # ໂຟນເດີເກັບຮູບພາບພື້ນຫຼັງເພີ່ມເຕີມ (.png, .jpg)
│   └── README.txt                 # ຄຳແນະນຳຂະໜາດຮູບ (1080p, 1440p)
└── specs/                         # ໂຟນເດີເກັບຜົນ Excel {room}-{number}.xlsx ທີ່ export ອອກມາ
    └── .gitkeep
```

## ຜົນກະທົບ ແລະ ການກວດສອບ (Consequences & Verification)
- **Offline 100%**: ທຸກ resource ຝັງຢູ່ໃນ binary, ບໍ່ມີ network call.
- **Portability**: ແລ່ນໄດ້ທັນທີເທິງ Windows 10 (1903+) ແລະ Windows 11 x64 ທຸກລຸ້ນ.
- **Read-Only USB Resilience**: `ConfigService` ມີ fallback ໄປ `%LOCALAPPDATA%\ClassroomChecker\config.json` ຖ້າ USB ຂຽນບໍ່ໄດ້.
