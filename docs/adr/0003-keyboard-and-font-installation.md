# ADR 0003: ການຕິດຕັ້ງແປ້ນພິມ ແລະ ຟອນລາວ ແບບປອດໄພ (Keyboard & Font Installation)

## ສະຖານະ (Status)
**Accepted**

## ບໍລິບົດ (Context)
ໃນໂມດູນ `SoftwareCheckModule` ຂໍ້ 5.2 ຂອງ spec:
- ແອັບຕ້ອງສາມາດກວດ ແລະ ຕິດຕັ້ງແປ້ນພິມລາວ (`0454:00000454` / `lo-LA`) ແລະ ຟອນ `Phetsarath OT`.
- ໃນ Windows 10 ແລະ Windows 11, ຄຳສັ່ງ PowerShell ເຊັ່ນ `Set-WinUserLanguageList` ຖ້າບໍ່ລະວັງ ອາດຂຽນທັບລາຍການພາສາເດີມ (ເຊັ່ນ EN, TH) ເຮັດໃຫ້ແປ້ນພິມອື່ນໆ ຫາຍໄປຈາກ Taskbar.
- ບາງເຄື່ອງບໍ່ມີອິນເຕີເນັດ (Offline) ເຮັດໃຫ້ Windows Update ບໍ່ສາມາດດາວໂຫຼດ Language Feature On Demand ໄດ້.

## ການຕັດສິນໃຈ (Decision)
1. **Font Installation**:
   - ແຍກໄຟລ໌ຟອນ `PhetsarathOT.ttf` ທີ່ຝັງເປັນ Embedded Resource ອອກໄປທີ່ `%WINDIR%\Fonts`.
   - ຂຽນ Registry `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts` ສຳລັບລະດັບລະບົບ.
   - ເອີ້ນ P/Invoke `AddFontResourceW` ແລະ Broadcast `WM_FONTCHANGE` ເພື່ອໃຫ້ແອັບອື່ນໆ ເຫັນຟອນທັນທີໂດຍບໍ່ຕ້ອງ Restart ເຄື່ອງ.
2. **Keyboard Layout Installation (Non-destructive Append)**:
   - ລຳດັບທີ 1: ລອງໃຊ້ P/Invoke `InstallLayoutOrTip("0454:00000454", 0)` (ຈາກ `input.dll`) ເຊິ່ງເປັນວິທີມາດຕະຖານຂອງ Windows ທີ່ເພີ່ມ layout ໂດຍບໍ່ກະທົບພາສາອື່ນ.
   - ລຳດັບທີ 2 (Fallback): ຖ້າໃຊ້ PowerShell, ຕ້ອງອ່ານລາຍການພາສາປັດຈຸບັນຜ່ານ `Get-WinUserLanguageList` ມາກ່ອນ ແລ້ວ Append `lo-LA` ເຂົ້າໄປໃນ List ແລ້ວຈຶ່ງ `Set-WinUserLanguageList -Force` (ຫ້າມ overwrite ດ້ວຍ `lo-LA` ພຽງຢ່າງດຽວ).
   - ຖ້າເຄື່ອງ offline ແລະ Windows ບໍ່ມີ layout binary ມາກັບ ISO, ໃຫ້ລາຍງານສະຖານະ `Warning` ພ້ອມຄຳອະທິບາຍແທນການ crash.

## ຜົນກະທົບ (Consequences)
- ປອດໄພຕໍ່ເຄື່ອງຄອມພິວເຕີຫ້ອງຮຽນ: ພາສາ English / Thai / ອື່ນໆ ຍັງຄົງຢູ່ຄົບຖ້ວນ.
- ບໍ່ຕ້ອງການ restart ເຄື່ອງໃນກໍລະນີທົ່ວໄປ.
