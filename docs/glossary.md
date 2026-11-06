# Classroom Checker — Domain Model & Glossary (ຄຳສັບສະເພາະ)

ເອກະສານນີ້ກຳນົດຄຳສັບສະເພາະ (Ubiquitous Language) ແລະ Domain Model ທີ່ໃຊ້ໃນລະບົບ Classroom Checker ທັງພາສາລາວ ແລະ ພາສາອັງກິດ ເພື່ອໃຫ້ທີມງານ ແລະ ລະບົບໂຄ້ດໃຊ້ຊື່ກົງກັນ.

---

## 1. Domain Entities & Core Concepts

| ຄຳສັບ (ລາວ) | ຄຳສັບ (English) | ຄຳອະທິບາຍ (Definition & Context) |
|---|---|---|
| **ຫ້ອງຮຽນ** | `Room` | ຊື່ ຫຼື ລະຫັດຫ້ອງຮຽນຄອມພິວເຕີ ເຊັ່ນ "ຫ້ອງ A", "Lab 1", ໃຊ້ຈັດກຸ່ມເຄື່ອງ. |
| **ເລກເຄື່ອງ** | `ComputerNumber` | ໝາຍເລກລຳດັບປະຈຳເຄື່ອງໃນຫ້ອງຮຽນ (ປົກກະຕິ 1–45). |
| **ຮູບພື້ນຫຼັງ** | `Wallpaper / Background` | ຮູບ Desktop background ທີ່ຖືກ generate ພ້ອມຝັງຊື່ຫ້ອງ + ເລກເຄື່ອງລົງໄປ. |
| **ຂໍ້ຄວາມກຳກັບ** | `Label Template` | ຮູບແບບຂໍ້ຄວາມທີ່ຂຽນລົງຮູບພື້ນຫຼັງ (ຮອງຮັບ placeholder ເຊັ່ນ `{room}`, `{number}`). |
| **ໂມດູນກວດສອບ** | `CheckModule` | ໜ່ວຍການກວດສອບແຕ່ລະດ້ານ (ເຊັ່ນ Power, Office, Driver, Cleanup) ທີ່ implement `ICheckModule`. |
| **ສະຖານະການກວດ** | `CheckStatus` | ຜົນການກວດສອບ: `Pass` (ຜ່ານ), `Fail` (ບໍ່ຜ່ານ), `Warning` (ເຕືອນ), `Unknown` (ກວດບໍ່ໄດ້). |
| **ການແກ້ໄຂອັດຕະໂນມັດ** | `Fix / Remediation` | ການສັ່ງໃຫ້ລະບົບແກ້ໄຂຄ່າທີ່ບໍ່ຜ່ານ (ສຳລັບ module ທີ່ `CanFix = true`). |
| **ການລ້າງໄຟລ໌** | `Cleanup / Disk Purge` | ການກວາດລ້າງ temporary files, update cache ແລະ recycle bin ແບບປອດໄພ. |
| **ແຜນການໃຊ້ພະລັງງານ** | `Power Scheme / Plan` | ການຕັ້ງຄ່າ timeouts ຂອງ Sleep, Hibernate, Monitor, Disk ໃຫ້ເປັນ 0 (Never). |
| **ຂໍ້ມູນສະເປັກເຄື່ອງ** | `Machine Spec` | ຂໍ້ມູນຮາດແວຣ໌ ແລະ ລະບົບປະຕິບັດການ (CPU, RAM, Disk, GPU, OS, Network) ທີ່ດຶງຜ່ານ WMI. |
| **ໄຟລ໌ລາຍງານສະເປັກ** | `Spec Export File` | ໄຟລ໌ Excel (`.xlsx`) ຊື່ `{room}-{number}.xlsx` ທີ່ບັນທຶກສະເປັກແຕ່ລະເຄື່ອງລົງ USB/specs folder. |

---

## 2. Technical Terms & System Enums

### CheckStatus
- `Pass` (✓): ລະບົບກວດສອບແລ້ວຖືກຕ້ອງຕາມເກນມາດຕະຖານ.
- `Fail` (✗): ບໍ່ຖືກຕ້ອງ ແລະ ຕ້ອງໄດ້ຮັບການແກ້ໄຂ (ຫຼື ຍັງບໍ່ໄດ້ຕິດຕັ້ງ/ເປີດໃຊ້).
- `Warning` (⚠): ພົບຂໍ້ຜິດປົກກະຕິ ແຕ່ບໍ່ເຖິງຂັ້ນເສຍຫາຍຮ້າຍແຮງ (ເຊັ່ນ Driver error ລະຫັດອື່ນທີ່ບໍ່ແມ່ນ 28).
- `Unknown` (?): ລະບົບບໍ່ສາມາດອ່ານຄ່າ ຫຼື ບໍ່ສາມາດຢືນຢັນໄດ້ແນ່ນອນ (ເຊັ່ນ Office license status ຈາກ ospp.vbs).

### Hardware & Storage Terms
- `MediaType`: ປະເພດຂອງ Disk (`HDD` = 3, `SSD` = 4, `SCM`, `Unspecified`) ອ່ານຜ່ານ `MSFT_PhysicalDisk`.
- `ConfigManagerErrorCode`: ລະຫັດສະຖານະ driver ຈາກ `Win32_PnPEntity` (28 = ຍັງບໍ່ຕິດຕັ້ງ driver).
- `P/Invoke`: ການເອີ້ນໃຊ້ native Windows C/C++ API ຈາກ .NET (ເຊັ່ນ `SystemParametersInfo`, `SHEmptyRecycleBin`, `InstallLayoutOrTip`).

---

## 3. Configuration Terms (`config.json`)

- `computerNumberRange`: ຂອບເຂດເລກເຄື່ອງ (min / max).
- `selected`: ຄ່າປັດຈຸບັນທີ່ເລືອກໃນ UI (ຫ້ອງ ແລະ ເລກເຄື່ອງ).
- `requiredFonts`: ລາຍຊື່ Font ທີ່ຕ້ອງມີໃນລະບົບ (ຄ່າເລີ່ມຕົ້ນ: `Phetsarath OT`).
- `excludePaths`: ລາຍການ path ທີ່ຫ້າມລຶບໃນ Cleanup module.
- `specOutputDir`: ໂຟນເດີປາຍທາງສຳລັບບັນທຶກ Excel spec.
