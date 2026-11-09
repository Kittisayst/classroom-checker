# Classroom Checker (ເຄື່ອງມືກວດສອບ ແລະ ຄຸ້ມຄອງຄອມພິວເຕີຫ້ອງຮຽນ)

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011%20(x64)-blue.svg)]()
[![Framework](https://img.shields.io/badge/Framework-.NET%20Framework%204.8-green.svg)]()
[![Type](https://img.shields.io/badge/Type-Portable%20%2F%20Single%20Executable-orange.svg)]()
[![Language](https://img.shields.io/badge/Language-Lao%20%7C%20English-purple.svg)]()
[![Dependencies](https://img.shields.io/badge/Dependencies-Zero%20External%20NuGet-success.svg)]()

> **Classroom Checker** ເປັນໂປຣແກຣມ Portable ແບບ Single Executable ຂະໜາດກະທັດຮັດ ທີ່ອອກແບບມາສະເພາະສຳລັບ ຄູ-ອາຈານ ແລະ ຜູ້ດູແລລະບົບຫ້ອງຄອມພິວເຕີ (IT Admin) ໃຊ້ງານຜ່ານ **USB Flash Drive** ໂດຍກົງ ໂດຍບໍ່ຕ້ອງຕິດຕັ້ງລົງໃນເຄື່ອງ ເພື່ອຕັ້ງຄ່າ Wallpaper, ກວດສອບຄວາມພ້ອມຂອງ Hardware & Software, ລັອກປ້ອງກັນເກມ/USB, ແລະ ຕິດຕັ້ງຊອບແວພື້ນຖານແບບ Offline ອັດຕະໂນມັດ.

---

## 📑 ສາລະບານ (Table of Contents)

1. [ຈຸດເດັ່ນຫຼັກຂອງລະບົບ (Core Features)](#1-ຈຸດເດັ່ນຫຼັກຂອງລະບົບ-core-features)
2. [ໂຄງສ້າງໂຟນເດີໃນ USB (USB Drive Structure)](#2-ໂຄງສ້າງໂຟນເດີໃນ-usb-usb-drive-structure)
3. [ຄູ່ມືການນຳໃຊ້ລະບົບລະອຽດແຕ່ລະສ່ວນ (User Manual)](#3-ຄູ່ມືການນຳໃຊ້ລະບົບລະອຽດແຕ່ລະສ່ວນ-user-manual)
   - [⚡ ຂັ້ນຕອນທີ່ 1: ຕັ້ງຄ່າດ່ວນ (Quick Setup)](#31-ຕັ້ງຄ່າດ່ວນ-quick-setup-ໃນຄລິກດຽວ)
   - [🖼️ ຂັ້ນຕອນທີ່ 2: ກຳນົດພື້ນຫຼັງ & ເລກເຄື່ອງ (Wallpaper & Labeling)](#32-ກຳນົດພື້ນຫຼັງ--ເລກເຄື່ອງ-wallpaper--labeling)
   - [🔍 ຂັ້ນຕອນທີ່ 3: ກວດສອບລະບົບ 12 ລາຍການ (System Inspection & Auto-Fix)](#33-ກວດສອບລະບົບ-12-ລາຍການ-system-inspection--auto-fix)
   - [🛡️ ຂັ້ນຕອນທີ່ 4: ລະບົບປ້ອງກັນ & ລັອກເກມ/USB (Security Policies)](#34-ລະບົບປ້ອງກັນ--ລັອກເກມusb-security-policies)
   - [📦 ຂັ້ນຕອນທີ່ 5: ຕິດຕັ້ງຊອບແວ Offline 11 ໂປຣແກຣມ (Software Installer)](#35-ຕິດຕັ້ງຊອບແວ-offline-11-ໂປຣແກຣມ-software-installer)
   - [📊 ຂັ້ນຕອນທີ່ 6: Export Spec & ລວມສະເປັກທຸກເຄື່ອງ (Master Summary)](#36-export-spec--ລວມສະເປັກທຸກເຄື່ອງ-master-summary)
4. [ລາຍການຊອບແວພື້ນຖານ 11 ໂປຣແກຣມ (Software Catalog)](#4-ລາຍການຊອບແວພື້ນຖານ-11-ໂປຣແກຣມ-software-catalog)
5. [ຄວາມທົນທານ ແລະ ຄວາມປອດໄພ (Resilience & Security)](#5-ຄວາມທົນທານ-ແລະ-ຄວາມປອດໄພ-resilience--security)
6. [ການ Compile & Run Unit Tests (Build from Source)](#6-ການ-compile--run-unit-tests-build-from-source)

---

## 1. ຈຸດເດັ່ນຫຼັກຂອງລະບົບ (Core Features)

* **🚀 Portable 100%:** ໄຟລ໌ດຽວ `ClassroomChecker.exe` ສຽບ USB ແລ້ວ Run ໄດ້ທັນທີ, ບໍ່ມີຂັ້ນຕອນຕິດຕັ້ງ.
* **⚡ Quick Setup (1-Click):** ຕັ້ງ Wallpaper + Never Sleep + Font ລາວ + ແປ້ນພິມລາວ + ລ້າງ Temp + Export Spec ໃນຄລິກດຽວ ພ້ອມລະບົບ Auto-increment ເລກເຄື່ອງ (01 -> 02 -> 03...).
* **🖼️ Crisp Lao Typography:** ຝັງ Font ມາດຕະຖານ **Phetsarath OT** ໃນຕົວໂປຣແກຣມ Render ປ້າຍຊື່ຫ້ອງ ແລະ ເລກເຄື່ອງລົງ Wallpaper ຄົມຊັດສວຍງາມ.
* **🔍 Deep 12-Point Inspection:** ກວດສອບ MS Office, Windows License, Font, ແປ້ນພິມ, Power, Temp, Driver, Disk, Defender, App Lock, Spec Export ພ້ອມປຸ່ມ Auto-Fix.
* **🛡️ Smart Security Lock:** ບລັອກບໍ່ໃຫ້ນັກຮຽນ Run ເກມ ຫຼື ໂປຣແກຣມຈາກ USB ແລະ Downloads ໂດຍມີ **Smart Whitelist** ຍົກເວັ້ນໃຫ້ `ClassroomChecker.exe` ໃນ USB ໃຊ້ງານໄດ້ສະເໝີ.
* **📦 Offline Software Installer:** ຕິດຕັ້ງ 11 ໂປຣແກຣມຍອດນິຍົມແບບ Silent ບໍ່ຕ້ອງຕໍ່ເນັດ, ພ້ອມຮອງຮັບ **Portable RAMMap64** (Auto Copy + Desktop Shortcut + Auto Accept EULA).
* **📊 Multi-Sheet Excel Spec Export:** Export ສະເປັກເຄື່ອງ (CPU, RAM, Mainboard, Storage, GPU, Network) ເປັນ `.xlsx` ໂດຍບໍ່ຕ້ອງລົງ Microsoft Excel ໄວ້ໃນເຄື່ອງ.
* **🌐 Bilingual Support:** ຮອງຮັບ 2 ພາສາ (ພາສາລາວ ແລະ English) ປ່ຽນໄດ້ທັນທີ.

---

## 2. ໂຄງສ້າງໂຟນເດີໃນ USB (USB Drive Structure)

```text
USB_FLASH_DRIVE:\
  │
  ├── ClassroomChecker.exe      <-- ໄຟລ໌ໂປຣແກຣມຫຼັກ Single Portable Executable
  ├── config.json               <-- ໄຟລ໌ຕັ້ງຄ່າລະບົບ (ຫ້ອງຮຽນ, ນະໂຍບາຍລັອກ, ເລກເຄື່ອງ)
  ├── bg.png                    <-- ຮູບພື້ນຫຼັງ Wallpaper (ສາມາດປ່ຽນເປັນຮູບໂຮງຮຽນໄດ້)
  ├── README.md                 <-- ຄູ່ມືການນຳໃຊ້ລະບົບ
  │
  ├── installers\               <-- ໂຟນເດີເກັບໄຟລ໌ Setup ຂອງໂປຣແກຣມຕ່າງໆ (.exe/.msi)
  │     ├── 7z2408-x64.exe
  │     ├── SumatraPDF-3.5.2-64-install.exe
  │     ├── vlc-3.0.21-win64.exe
  │     ├── BraveBrowserSetup64.exe
  │     ├── ChromeStandaloneSetup64.exe
  │     ├── npp.8.7.Installer.x64.exe
  │     ├── laoscript8.exe
  │     ├── VSCodeUserSetup-x64.exe
  │     ├── gimp-2.10.38-setup.exe
  │     ├── ShareX-16.1.0-setup.exe
  │     ├── RAMMap64.exe
  │     └── README.txt
  │
  ├── specs\                    <-- ໂຟນເດີເກັບໄຟລ໌ສະເປັກ Excel ທີ່ Export ອັດຕະໂນມັດ
  │     ├── ຫ້ອງຄອມພິວເຕີ 1-01.xlsx
  │     ├── ຫ້ອງຄອມພິວເຕີ 1-02.xlsx
  │     └── MasterSummary_ຫ້ອງຄອມພິວເຕີ 1.xlsx  <-- ໄຟລ໌ສະຫຼຸບລວມທຸກເຄື່ອງ
  │
  └── reports\                  <-- ໂຟນເດີເກັບໄຟລ໌ລາຍງານຜົນການກວດສອບ (.txt)
        ├── ຫ້ອງຄອມພິວເຕີ 1-01_Report.txt
        └── ...
```

---

## 3. ຄູ່ມືການນຳໃຊ້ລະບົບລະອຽດແຕ່ລະສ່ວນ (User Manual)

### 3.1 ຕັ້ງຄ່າດ່ວນ (Quick Setup) ໃນຄລິກດຽວ
ປຸ່ມສີຂຽວ **`⚡ ຕັ້ງຄ່າດ່ວນ`** ຢູ່ມຸມຂວາເທິງຂອງໜ້າຈໍ ໃຊ້ສຳລັບຈັດການຄອມພິວເຕີຫ້ອງຮຽນຈຳນວນຫຼາຍໃຫ້ໄວທີ່ສຸດ:
1. ສຽບ USB ໃສ່ເຄື່ອງຄອມພິວເຕີຫ້ອງຮຽນ -> ເປີດ `ClassroomChecker.exe`.
2. ກົດປຸ່ມ **`⚡ ຕັ້ງຄ່າດ່ວນ`**.
3. ກວດເບິ່ງຫ້ອງຮຽນ ແລະ ເລກເຄື່ອງ (ເລກເຄື່ອງຈະບວກເພີ່ມ 1 ອັດຕະໂນມັດສະເໝີ).
4. ກົດ **`⚡ ເລີ່ມຕັ້ງຄ່າດ່ວນ`**: ລະບົບຈະດຳເນີນການ 6 ຂັ້ນຕອນອັດຕະໂນມັດ:
   * ຕັ້ງ Wallpaper ພ້ອມເລກເຄື່ອງ
   * ຕັ້ງຄ່າ Never Sleep (ປ້ອງກັນຈໍດັບ)
   * ຕິດຕັ້ງ Font Phetsarath OT
   * ຕິດຕັ້ງແປ້ນພິມພາສາລາວ (0454)
   * ລ້າງໄຟລ໌ຂີ້ເຫຍື້ອ Temp/Cache
   * Export Spec ເຄື່ອງເປັນ Excel ເກັບໄວ້ໃນ USB
5. ຖອດ USB ໄປສຽບເຄື່ອງຕໍ່ໄປໄດ້ທັນທີ!

---

### 3.2 ກຳນົດພື້ນຫຼັງ & ເລກເຄື່ອງ (Wallpaper & Labeling)
* **ຫ້ອງຮຽນ:** ເລືອກຫ້ອງຄອມພິວເຕີ 1 ຫາ 8 (ຫຼື ກົດ `+ ເພີ່ມຫ້ອງ` ເພື່ອໃສ່ຊື່ຫ້ອງຕາມຕ້ອງການ).
* **ເລກເຄື່ອງ:** ເລືອກເລກເຄື່ອງ (1 ຫາ 99), ລະບົບຈະສະແດງເລກ 2 ຫຼັກ (01, 02...).
* **ຮູບພື້ນຫຼັງ:** ກົດ `ເລືອກຮູບ...` ເພື່ອເລືອກຮູບພື້ນຫຼັງຂອງໂຮງຮຽນ ຫຼື ກົດ `ໃຊ້ຄ່າເລີ່ມຕົ້ນ` ເພື່ອກັບຄືນສູ່ຮູບຕົ້ນສະບັບ.
* **ກົດປຸ່ມ `ກຳນົດພື້ນຫຼັງ (Set Wallpaper)`:** ລະບົບຈະ Render ຮູບພາບໃໝ່ ແລະ ຕັ້ງຄ່າ Wallpaper ທັນທີ.
* **ຕັ້ງຄ່າຂັ້ນສູງ (ໃນ ⚙️ ຕັ້ງຄ່າ):** ສາມາດປັບຕຳແໜ່ງປ້າຍຊື່, ຂະໜາດຕົວໜັງສື, ສີຕົວໜັງສື (Color Picker), ເງົາຕົວໜັງສື ແລະ ເປີດໃຊ້ງານ "Apply to All Users" ເພື່ອໃຫ້ມີຜົນກັບທຸກບັນຊີຜູ້ໃຊ້ໃນ Windows.

---

### 3.3 ກວດສອບລະບົບ 12 ລາຍການ (System Inspection & Auto-Fix)
* ກົດປຸ່ມ **`ສະແກນ (Scan)`** ເພື່ອກວດສອບ 12 ໂມດູນ:
  1. `MS Office`: ກວດ Word, Excel, PowerPoint ແລະ ສະຖານະ Activation
  2. `Windows License`: ກວດສະຖານະລິຂະສິດ Windows ແທ້
  3. `Essential Software`: ກວດ Chrome/Brave, 7-Zip, SumatraPDF
  4. `Font Phetsarath OT`: ກວດ ແລະ ຕິດຕັ້ງຟອນລາວ
  5. `Lao Keyboard`: ກວດ ແລະ ຕິດຕັ້ງແປ້ນພິມລາວ
  6. `Power Scheme`: ກວດ ແລະ ຕັ້ງຄ່າ Never Sleep
  7. `Temp Cleanup`: ກວດ ແລະ ລ້າງໄຟລ໌ຂີ້ເຫຍື້ອປອດໄພ
  8. `Device Drivers`: ກວດຫາໄດຣເວີອຸປະກອນທີ່ມີບັນຫາ
  9. `Disk Free Space`: ກວດເນື້ອທີ່ວ່າງ Drive C:
  10. `Windows Defender`: ກວດສະຖານະ Antivirus Real-time
  11. `App Lock`: ກວດສະຖານະການບລັອກ USB ແລະ ເກມ
  12. `Export Machine Spec`: Export ສະເປັກເຄື່ອງລະອຽດເປັນ `.xlsx`
* **ປຸ່ມ `ແກ້ໄຂທີ່ເລືອກ (Fix)`:** ແກ້ໄຂທຸກຂໍ້ຜິດພາດທີ່ເລືອກໄວ້ໃຫ້ອັດຕະໂນມັດໃນຄລິກດຽວ.
* **ປຸ່ມ `ບັນທຶກລາຍງານ (.txt)`:** ບັນທຶກຜົນສະແກນເປັນ Text File ລົງໂຟນເດີ `reports/`.

---

### 3.4 ລະບົບປ້ອງກັນ & ລັອກເກມ/USB (Security Policies)
* **`🔒 ຫ້າມ Run ໂປຣແກຣມ/ເກມ ຈາກ Flash Drive USB`:**
  * ໃຊ້ Windows Safer API Level 0 ບລັອກທຸກໄຟລ໌ .exe ໃນ USB.
  * ລະບົບມີ **Smart Whitelist**: ຍົກເວັ້ນ `ClassroomChecker.exe` ໃນ USB ໃຫ້ເປີດໃຊ້ງານໄດ້ສະເໝີ.
* **`📁 ຫ້າມ Run ໂປຣແກຣມ ຈາກໂຟນເດີ Downloads ແລະ Temp`:**
  * ປ້ອງກັນບໍ່ໃຫ້ນັກຮຽນດາວໂຫຼດເກມ ຫຼື ໂປຣແກຣມມາ Run ໃນເຄື່ອງ.
* **`🚫 ບລັອກເກມຍອດຮິດອັດຕະໂນມັດ`:**
  * ບລັອກ Roblox, Steam, Garena, Epic Games, Minecraft, Discord, Valorant...
* **ປຸ່ມສັ່ງການ:**
  * **`🔒 ເປີດການລັອກ`:** ເປີດລະບົບບລັອກທັນທີ (ມີຜົນກັບທຸກ User ໃນເຄື່ອງ)
  * **`🔓 ປິດການລັອກ`:** ປົດລັອກລະບົບທັງໝົດກັບຄືນສູ່ສະພາບປົກກະຕິ
  * **`🔄 ກວດສອບ`:** ກວດສອບສະຖານະການລັອກຄືນໃໝ່
  * **`💾 ບັນທຶກ`:** ບັນທຶກການຕັ້ງຄ່າລົງ `config.json`

---

### 3.5 ຕິດຕັ້ງຊອບແວ Offline 11 ໂປຣແກຣມ (Software Installer)
1. ດາວໂຫຼດໄຟລ໌ Setup ມາໄວ້ໃນໂຟນເດີ `installers/` ໃນ USB.
2. ເຂົ້າແທັບ **"ຕິດຕັ້ງຊອບແວ"**: ຕາຕະລາງຈະບອກສະຖານະວ່າໂປຣແກຣມໃດຕິດຕັ້ງແລ້ວ (`✓ ຕິດຕັ້ງແລ້ວ`) ຫຼື ຍັງຂາດ (`✗ ຍັງບໍ່ທັນຕິດຕັ້ງ`) ແລະ ພົບໄຟລ໌ໃນ USB ຫຼື ບໍ່.
3. ກົດ **`📥 ຕິດຕັ້ງ`** ເທື່ອລະຕົວ ຫຼື ກົດ **`⚡ ຕິດຕັ້ງທີ່ຂາດທັງໝົດ`** ເພື່ອຕິດຕັ້ງທຸກໂປຣແກຣມທີ່ຍັງຂາດແບບ Silent (ບໍ່ຕ້ອງກົດ Next, Next).
4. **ພິເສດສຳລັບ RAMMap64 (Portable):**
   * ລະບົບຈະຄັດລອກໄຟລ໌ລົງ `C:\Program Files\RAMMap\`
   * ສ້າງ Desktop Shortcut ຊື່ **`RAMMap (Clear RAM).lnk`** ໄວ້ໜ້າຈໍ Desktop
   * ລົງທະບຽນ App Paths (ກົດ Win+R ແລ້ວພິມ `rammap64` ໄດ້ທັນທີ)
   * ຕັ້ງຄ່າ Auto-Accept EULA ໃນ Registry ໃຫ້ພ້ອມໃຊ້ງານທັນທີ!

---

### 3.6 Export Spec & ລວມສະເປັກທຸກເຄື່ອງ (Master Summary)
* **Export ແຕ່ລະເຄື່ອງ:** ເມື່ອກວດສອບ ຫຼື ກົດ Quick Setup, ລະບົບຈະສ້າງໄຟລ໌ Excel ຊື່ `<ຫ້ອງຮຽນ>-<ເລກເຄື່ອງ>.xlsx` ໄວ້ໃນໂຟນເດີ `specs/` ໃນ USB.
* **ລວມສະເປັກທຸກເຄື່ອງ (Master Excel):**
  * ເມື່ອເອົາ USB ໄປສຽບ ແລະ Export ຄົບທຸກເຄື່ອງໃນຫ້ອງແລ້ວ:
  * ເຂົ້າແທັບ **"ກວດສອບລະບົບ"** -> ກົດປຸ່ມ **`📊 ລວມສະເປັກ (Master Excel)`**
  * ລະບົບຈະລວມໄຟລ໌ `.xlsx` ທຸກເຄື່ອງໃນໂຟນເດີ `specs/` ເຂົ້ານຳກັນເປັນ 1 ໄຟລ໌ໃຫຍ່ (ເຊັ່ນ: `MasterSummary_ຫ້ອງຄອມພິວເຕີ 1.xlsx`) ພ້ອມຕາຕະລາງສະຫຼຸບສະເປັກຮວມທັງໝົດ ສະດວກຕໍ່ການເຮັດລາຍງານສົ່ງຜູ້ບໍລິຫານ!

---

## 4. ລາຍການຊອບແວພື້ນຖານ 11 ໂປຣແກຣມ (Software Catalog)

| ລຳດັບ | ຊື່ຊອບແວ | ໝວດໝູ່ | ລິຂະສິດ | ໄຟລ໌ Setup ທີ່ຮອງຮັບ | ຮູບແບບການຕິດຕັ້ງ |
|---|---|---|---|---|---|
| 1 | **7-Zip (64-bit)** | ບີບອັດ/ແຕກໄຟລ໌ | Open Source (LGPL) | `*7z*.exe` | Silent (`/S`) |
| 2 | **SumatraPDF** | ອ່ານເອກະສານ PDF | Open Source (GPLv3) | `*sumatra*.exe` | Silent (`/S`) |
| 3 | **VLC Media Player** | ເຄື່ອງຫຼິ້ນສື່ວິດີໂອ/ສຽງ | Open Source (GPL) | `*vlc*.exe` | Silent (`/S`) |
| 4 | **Brave Browser** | ເວັບບຣາວເຊີ | Open Source (MPL) | `*brave*.exe` | Silent (`/silent /install`) |
| 5 | **Google Chrome** | ເວັບບຣາວເຊີ | Freeware (Google) | `*chrome*.exe` | Silent (`/silent /install`) |
| 6 | **Notepad++** | ແກ້ໄຂໂຄ້ດ/ຂໍ້ຄວາມ | Open Source (GPL) | `*npp*.exe` | Silent (`/S`) |
| 7 | **LaoScript 8** | ແປ້ນພິມ ແລະ ຟອນລາວ | Freeware (Lao Script) | `*laoscript*.exe` | Silent (`/VERYSILENT`) |
| 8 | **VS Code** | ເຄື່ອງມືຂຽນໂປຣແກຣມ | Free / Open Source | `*vscode*.exe` | Silent (`/VERYSILENT`) |
| 9 | **GIMP** | ຕັດຕໍ່/ຕົກແຕ່ງຮູບພາບ | Open Source (GPL) | `*gimp*.exe` | Silent (`/VERYSILENT`) |
| 10 | **ShareX** | ຖ່າຍພາບ/ອັດວິດີໂອໜ້າຈໍ | Open Source (GPLv3) | `*sharex*.exe` | Silent (`/VERYSILENT`) |
| 11 | **RAMMap64** | Clear RAM / Memory Cleaner | Freeware (Sysinternals) | `*rammap64*.exe` | **Auto Copy + Desktop Shortcut** |

---

## 5. ຄວາມທົນທານ ແລະ ຄວາມປອດໄພ (Resilience & Security)

1. **Corrupt Config Resilience:**
   * ຖ້າໄຟລ໌ `config.json` ເສຍຫາຍ ຫຼື ຂຽນບໍ່ຄົບ, ລະບົບຈະສຳຮອງເປັນ `config.json.bad` ແລະ ສ້າງຄ່າເລີ່ມຕົ້ນທີ່ປອດໄພໃຫ້ທັນທີ ໂດຍບໍ່ Error ຫຼື Crash.
2. **Read-Only USB Redirection:**
   * ຖ້າ USB Flash Drive ຕິດສະຖານະ Write-Protected ຫຼື Read-Only, ໂປຣແກຣມຈະປ່ຽນເສັ້ນທາງບັນທຶກໄປທີ່ `%AppData%\ClassroomChecker\` ອັດຕະໂນມັດ.
3. **Log Rotation:**
   * ໄຟລ໌ Log ຈະຖືກຈຳກັດຂະໜາດບໍ່ເກີນ 1 MB ຖ້າເກີນຈະ Rotate ເປັນ `.log.old` ອັດຕະໂນມັດ ເພື່ອປະຢັດພື້ນທີ່ໃນ USB.
4. **Zero NuGet Dependencies:**
   * ໂປຣແກຣມໃຊ້ສະເພາະ Base Class Library ມາດຕະຖານຂອງ .NET Framework 4.8 GAC ເທົ່ານັ້ນ, ບໍ່ມີ DLL ພາຍນອກ ເຮັດໃຫ້ມີຄວາມປອດໄພສູງ ແລະ ບໍ່ມີບັນຫາ Dependency ຂາດຫາຍ.

---

## 6. ການ Compile & Run Unit Tests (Build from Source)

### ຄວາມຕ້ອງການຂອງລະບົບ:
* Windows 10 ຫຼື Windows 11 (64-bit)
* .NET Framework 4.8 Developer Pack ຫຼື MSBuild / Visual Studio Build Tools

### ວິທີ Build ໂປຣແກຣມ:
```cmd
build.bat
```
* ໄຟລ໌ Binary ຈະຖືກສ້າງຂຶ້ນທີ່: `src\ClassroomChecker\bin\Release\ClassroomChecker.exe`
* ສຳເນົາພ້ອມໃຊ້ງານຈະຖືກເກັບໄວ້ທີ່: `dist\ClassroomChecker.exe`

### ວິທີ Run Unit Tests:
```cmd
tests\run_tests.bat
```
* ທົດສອບຄົບທັງ 26 Unit Tests (Config, Resilience, OpenXML Xlsx Writer, I18n, Security, Software Installer, COM Reflection Shortcut).

### ວິທີ Run Phase 5 Verification Suite:
```powershell
powershell -ExecutionPolicy Bypass -File test_phase5.ps1
```
* ທົດສອບຄວາມສົມບູນຂອງລະບົບທັງ 28 ລາຍການ ພ້ອມກວດສອບ Embedded Resources, WMI, Font Rendering ແລະ Excel Multi-sheet Structure.

---

## 📄 ໃບອະນຸຍາດ (License)

ໂຄງການນີ້ພັດທະນາຂຶ້ນເພື່ອເປັນສາທາລະນະປະໂຫຍດສຳລັບການສຶກສາ ແລະ ການຄຸ້ມຄອງຫ້ອງຮຽນຄອມພິວເຕີ ພາຍໃຕ້ສັນຍາອະນຸຍາດ **MIT License**.
