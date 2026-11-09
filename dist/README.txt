================================================================================
                    CLASSROOM CHECKER v1.0 (PORTABLE USB EDITION)
               ໂປຣແກຣມກວດສອບ ແລະ ຕັ້ງຄ່າຄອມພິວເຕີຫ້ອງຮຽນອັດຕະໂນມັດ
================================================================================

[ພາສາລາວ / Lao]
Classroom Checker ເປັນໂປຣແກຣມແບບ Portable ໄຟລ໌ດຽວ (Zero-install, Single-file executable) 
ທີ່ອອກແບບມາສຳລັບຜູ້ດູແລລະບົບຫ້ອງຮຽນຄອມພິວເຕີ (Classroom Admins) ເພື່ອນຳໃຊ້ຜ່ານ Flash Drive / USB 
ໂດຍສາມາດເຮັດວຽກແບບ Offline 100% ໂດຍບໍ່ຕ້ອງເຊື່ອມຕໍ່ອິນເຕີເນັດ ແລະ ບໍ່ຕ້ອງຕິດຕັ້ງໂປຣແກຣມເພີ່ມ.

--------------------------------------------------------------------------------
1. ຄວາມຕ້ອງການຂອງລະບົບ (System Requirements)
--------------------------------------------------------------------------------
- ລະບົບປະຕິບັດການ: Windows 10 (Version 1903 ຂຶ້ນໄປ) ຫຼື Windows 11 (64-bit)
- Runtime: .NET Framework 4.8 (ມີຕິດຕັ້ງມານຳ Windows 10/11 ຢູ່ແລ້ວ)
- ສິດການໃຊ້ງານ: ຕ້ອງເປີດດ້ວຍສິດ Administrator (Run as administrator)

--------------------------------------------------------------------------------
2. ໂຄງສ້າງໂຟນເດີໃນ USB (Folder Structure)
--------------------------------------------------------------------------------
USB:\
├── ClassroomChecker.exe    - ໂປຣແກຣມຫຼັກ (Portable Single-file)
├── README.txt              - ຄູ່ມືການໃຊ້ງານ (ໄຟລ໌ນີ້)
├── backgrounds\            - ໂຟນເດີເກັບຮູບພາບພື້ນຫຼັງເພີ່ມເຕີມ (.png, .jpg) [ທາງເລືອກ]
├── specs\                  - ໂຟນເດີເກັບໄຟລ໌ລາຍງານ Hardware/System (.xlsx) ທີ່ Export ອອກມາ
├── drivers\                - [ທາງເລືອກ] ໃສ່ driver (.inf + ໄຟລ໌ປະກອບ) ໄວ້ນີ້ ເພື່ອຕິດຕັ້ງແບບ offline (ເບິ່ງ drivers\README.txt)
├── logs\                   - [ສ້າງອັດຕະໂນມັດ] log ຂອງແຕ່ລະເຄື່ອງ {ຫ້ອງ}-{ເລກ}.log
└── reports\                - [ສ້າງອັດຕະໂນມັດ] ລາຍງານສະແກນ (.txt) ທີ່ບັນທຶກເອງຫຼັງສະແກນ/ແກ້ໄຂ

--------------------------------------------------------------------------------
3. ວິທີການໃຊ້ງານ (Quick Start)
--------------------------------------------------------------------------------
ຂັ້ນຕອນທີ 1: ສຽບ USB ເຂົ້າເຄື່ອງຄອມພິວເຕີໃນຫ້ອງຮຽນ.
ຂັ້ນຕອນທີ 2: ຄລິກຂວາໃສ່ໄຟລ໌ `ClassroomChecker.exe` ແລ້ວເລືອກ "Run as administrator".
            (ຫຼື ກົດເປີດໂດຍກົງ ລະບົບຈະຮ້ອງຂໍສິດ Admin UAC ໂດຍອັດຕະໂນມັດ).

ແຖບທີ 1: ພື້ນຫຼັງ (Desktop Wallpaper)
  - ເລືອກຫ້ອງຮຽນ (Room) ຈາກລາຍການ ຫຼື ກົດ "ເພີ່ມ" ເພື່ອສ້າງຫ້ອງໃໝ່ (ເຊັ່ນ: LAB 1, LAB 2).
  - ເລືອກເລກປະຈຳເຄື່ອງ (Number) 1 - 45.
  - ເລືອກຮູບພື້ນຫຼັງ: ໃຊ້ຮູບ Default ທີ່ຝັງມາພ້ອມ ຫຼື ກົດ "ເລືອກຮູບ..." ເພື່ອເລືອກຮູບຂອງຕົນເອງ.
  - ເບິ່ງຕົວຢ່າງ Live Zoom Preview ແລ້ວກົດປຸ່ມ "ຕັ້ງພື້ນຫຼັງ".
  - ລະບົບຈະແຕ້ມຊື່ຫ້ອງ ແລະ ເລກເຄື່ອງ ດ້ວຍ Font Phetsarath OT ທີ່ຄົມຊັດ ແລະ ຕັ້ງເປັນ Desktop ທັນທີ.

ແຖບທີ 2: ກວດສອບ ແລະ ແກ້ໄຂ (Scan & Fix)
  - ຕິກເລືອກຫົວຂໍ້ທີ່ຕ້ອງການກວດສອບ:
    1. Microsoft Office (Word, Excel, PowerPoint, FileVersion, OSPP License)
    2. Windows License (ສະຖານະ Activation ຜ່ານ WMI)
    3. Font ພາສາລາວ (Phetsarath OT)
    4. ແປ້ນພິມພາສາລາວ (Lao Keyboard Layout 0454:00000454)
    5. Power Scheme (ຕັ້ງ Never Sleep: ຈໍ, Standby, Hibernate, Disk = 0)
    6. ລ້າງໄຟລ໌ຂີ້ເຫຍື້ອ (Safe Temp & Cache Cleanup)
    7. Driver ອຸປະກອນ (Win32_PnPEntity Code 28 Missing Drivers)
    8. Export Spec ເຄື່ອງ (ດຶງຂໍ້ມູນ Hardware/OS ອອກເປັນ Excel .xlsx)
    9. ຊອບແວພື້ນຖານ (Chrome, 7-Zip, PDF)
    10. ພື້ນທີ່ວ່າງ Disk (drive ລະບົບ: ເຫຼືອ <10% = ເຕືອນ, <5% = ບໍ່ຜ່ານ)
    11. Windows Defender (Real-time protection + ອາຍຸ definitions; ຖ້າໃຊ້ antivirus ອື່ນ = ກວດບໍ່ໄດ້)
  - ກົດປຸ່ມ "ເລີ່ມສະແກນ" (Scan) ເພື່ອກວດສອບສະຖານະ (✓ ຜ່ານ, ✗ ບໍ່ຜ່ານ, ⚠ ເຕືອນ, ? ບໍ່ຮູ້).
  - ກົດປຸ່ມ "ແກ້ໄຂທີ່ເລືອກ" (Fix Selected) ເພື່ອຕິດຕັ້ງ Font, ແປ້ນພິມ, ຕັ້ງ Power, ແລະ ລ້າງໄຟລ໌ອັດຕະໂນມັດ.
  - ກົດ "Export Spec" ເພື່ອສົ່ງອອກຂໍ້ມູນສະເປັກເຄື່ອງທັງໝົດເປັນໄຟລ໌ Excel ລົງໃນໂຟນເດີ `specs/`.

--------------------------------------------------------------------------------
4. ໝາຍເຫດ ແລະ ຄວາມປອດໄພ
--------------------------------------------------------------------------------
- ຖ້າ USB ເປັນ Read-Only (ລັອກການຂຽນ): ໂປຣແກຣມຈະບັນທຶກ config ລົງໃນ `%LOCALAPPDATA%\ClassroomChecker` ໂດຍອັດຕະໂນມັດ ໂດຍບໍ່ crash.
- ການລ້າງໄຟລ໌ຂີ້ເຫຍື້ອ (Cleanup): ລະບົບຈະຂ້າມ (skip) ໄຟລ໌ທີ່ກຳລັງຖືກລັອກ ຫຼື ບໍ່ມີສິດເຂົ້າເຖິງຢ່າງປອດໄພ.
- ລະບົບສອງພາສາ: ສາມາດປ່ຽນພາສາລະຫວ່າງ ລາວ / English ໄດ້ຕະຫຼອດເວລາຢູ່ມຸມຂວາເທິງ.
- ລາຍງານອັດຕະໂນມັດ: ຫຼັງສະແກນ/ແກ້ໄຂ ລະບົບບັນທຶກລາຍງານລົງ `reports\` ແລະ ຂຽນ log ລົງ `logs\` ໂດຍບໍ່ຕ້ອງເລືອກ path (ຖ້າ USB ຂຽນບໍ່ໄດ້ ໃຊ້ `%APPDATA%\ClassroomChecker\`).
- ເລກເຄື່ອງຊ້ຳ: ຖ້າໄຟລ໌ spec ຂອງເລກນີ້ເປັນຂອງເຄື່ອງອື່ນ (Serial ຕ່າງກັນ) ລະບົບຈະບໍ່ຂຽນທັບ ແຕ່ບັນທຶກເປັນໄຟລ໌ໃໝ່ ແລະ ເຕືອນ.
- ຕິດຕັ້ງ Driver: ກົດ "ແກ້ໄຂທີ່ເລືອກ" ໃນຂໍ້ Driver ເພື່ອຕິດຕັ້ງຈາກ `drivers\` ດ້ວຍ pnputil (ບາງ driver ອາດຕ້ອງ Restart).
- Wallpaper ໃຫ້ທຸກ user (ທາງເລືອກ): ຕັ້ງ `"applyToAllUsers": true` ໃນສ່ວນ `background` ຂອງ config.json. ຄ່າເລີ່ມຕົ້ນ = ປິດ.

================================================================================
[ENGLISH]
Classroom Checker is a standalone, single-file portable utility designed for 
classroom administrators to configure, inspect, and maintain student PCs 
directly from a USB flash drive without internet access or prior installation.

Features:
1. Dynamic Lao Desktop Wallpaper: Draws room name and PC number using Phetsarath OT font.
2. Software & License Verification: Checks Office apps, Windows Activation, Fonts, and Lao keyboard.
3. Automated Power Optimization: Sets AC and DC timeouts (monitor, sleep, hibernate, disk) to Never.
4. Safe Disk Cleanup: Cleans temporary files and caches without crashing on locked handles.
5. Missing Driver Detection: Detects missing device drivers (PnP Code 28).
6. Native OpenXML Excel Export: Gathers full CPU, RAM slots, SSD/HDD, GPU, OS, and BIOS specs into specs/{room}-{number}.xlsx.
7. Dual Language UI: Instant toggle between Lao and English.
================================================================================
