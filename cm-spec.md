# cm-spec.md — Classroom Checker (ເຄື່ອງມືກວດສອບຄອມພິວເຕີຫ້ອງຮຽນ)

> ເອກະສານນີ້ແມ່ນ spec ສຳລັບ Claude Code ອ່ານ ແລະ ພັດທະນາຕາມເປັນເຟສ.
> ພາສາ: ລາວ, ຄຳສັບເທັກນິກຮັກສາເປັນ English.

---

## 1. ພາບລວມ

Desktop app ສຳລັບ Windows ທີ່ຜູ້ດູແລ (admin) ເປີດໃສ່ແຕ່ລະເຄື່ອງໃນຫ້ອງຮຽນ ເພື່ອ:
1. ຕັ້ງຮູບພື້ນຫຼັງ (wallpaper) ທີ່ມີ **ຊື່ຫ້ອງ + ເລກເຄື່ອງ** ຂຽນລົງໃນຮູບ
2. ກວດສອບໂປຣແກຣມ, font, ແປ້ນພິມລາວ
3. ຕັ້ງ Power ເປັນ never sleep
4. ລ້າງໄຟລ໌ຂີ້ເຫຍື້ອ / temp
5. ກວດ driver ທີ່ຂາດ
6. Export spec ເຄື່ອງເປັນ Excel
7. ປຸ່ມ "ສະແກນ" ກວດທຸກຂໍ້ 1-6 ແລ້ວບອກວ່າຂໍ້ໃດຜ່ານ/ບໍ່ຜ່ານ ແລະ ແກ້ໄຂຂໍ້ທີ່ເລືອກ

### ຂໍ້ກຳນົດການໃຊ້ງານ (ຢືນຢັນແລ້ວ)
- Windows 10 / 11, **64-bit ທັງໝົດ** (Win7 ບໍ່ຮອງຮັບ)
- ທຸກເຄື່ອງຜູ້ໃຊ້ເປັນ **Administrator** ຢູ່ແລ້ວ
- ບໍ່ມີ Deep Freeze / Group Policy / domain
- ບາງເຄື່ອງມີເນັດ ບາງເຄື່ອງບໍ່ມີ → ທຸກຄຸນສົມບັດຫຼັກຕ້ອງເຮັດວຽກ **offline**
- ເຜີຍແຜ່ຜ່ານ **USB** → ເປັນ **portable exe** ດຽວ, ກົດແລ້ວເຮັດວຽກ, ບໍ່ຕ້ອງ install
- UI ສອງພາສາ: ລາວ + ອັງກິດ (default: ລາວ)
- ຜູ້ພັດທະນາບຳລຸງຮັກສາໂຄ້ດເອງ → ໂຄ້ດຕ້ອງອ່ານງ່າຍ, ແຍກ module ຊັດ, ມີ comment ສຳຄັນ

---

## 2. Tech Stack

| ລາຍການ | ເລືອກ |
|---|---|
| ພາສາ | C# |
| Framework | .NET Framework 4.8, Windows Forms (WinForms) |
| Project style | SDK-style `.csproj`, `TargetFramework = net48`, `PlatformTarget = x64` |
| JSON | Newtonsoft.Json |
| Excel export | ClosedXML |
| ລວມ DLL ເຂົ້າ exe ດຽວ | Costura.Fody |
| ຮູບ / ຂຽນຂໍ້ຄວາມ | System.Drawing (GDI+) |
| ກວດ hardware | WMI (`System.Management`) |
| Admin | `app.manifest` → `requestedExecutionLevel level="requireAdministrator"` |

ເຫດຜົນ: .NET 4.8 ມີໃນ Win10 (ລຸ້ນໃໝ່) ແລະ Win11 ຢູ່ແລ້ວ, GDI+ ສະແດງສະຣະ/ວັນນະຍຸດລາວຖືກຕ້ອງ.

---

## 3. ໂຄງສ້າງໂປຣເຈັກ

```
ClassroomChecker/
├─ ClassroomChecker.sln
├─ cm-spec.md                  ← ໄຟລ໌ນີ້
├─ FILES.md                    ← ລາຍການໄຟລ໌ທີ່ສ້າງ + ໜ້າທີ່ (Claude Code ຕ້ອງອັບເດດທຸກຄັ້ງ)
├─ src/ClassroomChecker/
│  ├─ ClassroomChecker.csproj
│  ├─ app.manifest
│  ├─ Program.cs
│  ├─ Config/
│  │  ├─ AppConfig.cs          ← class ຂອງ config.json
│  │  └─ ConfigService.cs      ← load/save
│  ├─ Core/
│  │  ├─ ICheckModule.cs       ← interface ຮ່ວມ
│  │  ├─ CheckResult.cs
│  │  └─ ScanRunner.cs         ← ລັນຫຼາຍ module
│  ├─ Modules/
│  │  ├─ WallpaperModule.cs
│  │  ├─ SoftwareCheckModule.cs   ← Office, font, keyboard
│  │  ├─ PowerModule.cs
│  │  ├─ CleanupModule.cs
│  │  ├─ DriverCheckModule.cs
│  │  └─ SpecExportModule.cs
│  ├─ Native/
│  │  └─ NativeMethods.cs      ← P/Invoke ທັງໝົດ
│  ├─ UI/
│  │  ├─ MainForm.cs
│  │  ├─ WallpaperPanel.cs
│  │  └─ ScanPanel.cs
│  ├─ I18n/
│  │  ├─ Localizer.cs
│  │  ├─ lo.json
│  │  └─ en.json
│  └─ Assets/
│     ├─ bg.png                ← ພື້ນຫຼັງ default (ຮູບລ້າໆ)
│     └─ Fonts/PhetsarathOT.ttf  ← ຝັງເປັນ EmbeddedResource
└─ tests/                      ← (ທາງເລືອກ) unit test ສ່ວນ logic ທີ່ບໍ່ແຕະ Windows
```

### ກົດຂອງ module
ທຸກ module ເຮັດ interface ດຽວກັນ ເພື່ອໃຫ້ເພີ່ມຂໍ້ໃໝ່ງ່າຍ:

```csharp
public interface ICheckModule
{
    string Id { get; }                 // "power", "cleanup", ...
    string DisplayNameKey { get; }     // key ໃນ lo.json/en.json
    bool CanFix { get; }
    CheckResult Check();               // ກວດ, ຫ້າມປ່ຽນຄ່າລະບົບ
    CheckResult Fix();                 // ແກ້ໄຂ (ຖ້າ CanFix)
}

public enum CheckStatus { Pass, Fail, Warning, Unknown }

public class CheckResult
{
    public CheckStatus Status;
    public string Summary;             // ຂໍ້ຄວາມສັ້ນສະແດງໃນ UI
    public List<string> Details;       // ລາຍລະອຽດ
}
```

---

## 4. config.json

ເກັບຢູ່ **ໂຟນເດີດຽວກັບ exe** (portable). ຖ້າບໍ່ມີ → ສ້າງໃໝ່ດ້ວຍຄ່າ default. ຖ້າໂຟນເດີ write ບໍ່ໄດ້ (USB read-only) → fallback ໄປ `%APPDATA%\ClassroomChecker\config.json` ແລະ ແຈ້ງເຕືອນ.

```json
{
  "version": 1,
  "language": "lo",
  "rooms": ["ຫ້ອງ A", "ຫ້ອງ B"],
  "computerNumberRange": { "min": 1, "max": 45 },
  "selected": { "room": "ຫ້ອງ A", "number": 1 },
  "background": {
    "imagePath": "bg.png",
    "style": "fill"
  },
  "label": {
    "template": "{room} - {number}",
    "fontFamily": "Phetsarath OT",
    "fontSizePx": 72,
    "color": "#FFFFFF",
    "shadow": true,
    "position": "top-right",
    "marginPx": 40
  },
  "requiredFonts": ["Phetsarath OT"],
  "cleanup": {
    "userTemp": true,
    "windowsTemp": true,
    "recycleBin": true,
    "windowsUpdateCache": true,
    "browserCache": false,
    "excludePaths": []
  },
  "specOutputDir": "specs"
}
```

ກົດ:
- `rooms`: ເລືອກຈາກ list ຫຼື ເພີ່ມໃໝ່ໃນ UI → ບັນທຶກກັບ json ທັນທີ. ຄວນມີປຸ່ມລຶບ/ແກ້ຊື່ຫ້ອງນຳ.
- `number`: ເລືອກໄດ້ 1–45 (ມາຈາກ `computerNumberRange`, ປ່ຽນໄດ້ໃນ json).
- `imagePath`: relative ຈາກ exe ຫຼື absolute. ຜູ້ໃຊ້ອັບໂຫຼດຮູບໃໝ່ໄດ້ຜ່ານ UI → copy ໃສ່ໂຟນເດີ `backgrounds/` ຂ້າງ exe ແລ້ວອັບເດດ path. ປຸ່ມ "ໃຊ້ຄ່າເລີ່ມຕົ້ນ" ກັບຄືນ `bg.png`.
- `bg.png` default ຝັງເປັນ EmbeddedResource ເພື່ອໃຫ້ exe ໃຊ້ໄດ້ ແມ່ນແຕ່ບໍ່ມີໄຟລ໌ຂ້າງ.
- `template` ຮອງຮັບ placeholder: `{room}`, `{number}`, `{hostname}`, `{ip}`, `{date}`.
- ຖ້າ config ເສຍ (JSON ຜິດ) → backup ເປັນ `config.json.bad` ແລ້ວສ້າງໃໝ່, ຫ້າມ crash.

---

## 5. ຄຸນສົມບັດລະອຽດ

### 5.1 ກຳນົດຮູບພື້ນຫຼັງ (Wallpaper)

**UI**: dropdown ຫ້ອງ (+ ປຸ່ມເພີ່ມຫ້ອງ), dropdown ເລກ 1–45, ປຸ່ມເລືອກຮູບພື້ນຫຼັງ, ປຸ່ມ "ກຳນົດພື້ນຫຼັງ", ມີ preview ຮູບກ່ອນ apply.

**ຂັ້ນຕອນ**:
1. ໂຫຼດຮູບພື້ນຖານຈາກ config (ຫຼື bg.png ຝັງ)
2. ສ້າງ `Bitmap` ຂະໜາດເທົ່າຮູບຕົ້ນສະບັບ, ວາດຮູບລົງ
3. ແທນ placeholder ໃນ `template` → ຂຽນຂໍ້ຄວາມລົງຕຳແໜ່ງທີ່ກຳນົດ
   - `Graphics.TextRenderingHint = AntiAliasGridFit` (ຫຼື `ClearTypeGridFit`)
   - ໃຊ້ `StringFormat` ແລະ `MeasureString` ເພື່ອຈັດຕຳແໜ່ງ
   - shadow = ວາດຂໍ້ຄວາມສີດຳເບື້ອງຫຼັງ offset 2-3 px
   - ຖ້າ font ບໍ່ມີໃນເຄື່ອງ → ໂຫຼດຈາກ font ທີ່ຝັງ (`PrivateFontCollection`) ເພື່ອໃຫ້ຮູບອອກມາຖືກສະເໝີ ແມ່ນແຕ່ຍັງບໍ່ໄດ້ຕິດຕັ້ງ font
4. ບັນທຶກເປັນ PNG ໃນ `C:\ProgramData\ClassroomChecker\wallpaper.png` (ທຸກ user ອ່ານໄດ້)
5. ຕັ້ງ wallpaper:
   - registry `HKCU\Control Panel\Desktop`: `WallpaperStyle` + `TileWallpaper` ຕາມ `style` (fill=10/0, fit=6/0, stretch=2/0, center=0/0, tile=0/1)
   - `SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE)`
6. ບັນທຶກ `selected` ໃສ່ config

**ໝາຍເຫດ**: wallpaper ຕັ້ງໃຫ້ສະເພາະ user ທີ່ login ຢູ່ ເພາະ `HKCU`. ໃຫ້ສະແດງຂໍ້ຄວາມເຕືອນໃນ UI ວ່າ "ຕັ້ງໃຫ້ບັນຊີ: {username}".

### 5.2 ກວດໂປຣແກຣມ / Font / ແປ້ນພິມ (SoftwareCheckModule)

**MS Office** ກວດ Word, Excel, PowerPoint ແຍກກັນ:
- ຊອກ `WINWORD.EXE`, `EXCEL.EXE`, `POWERPNT.EXE` ຈາກ registry `App Paths`
  - `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\<exe>`
  - ແລະ `HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\<exe>`
- ອ່ານເວີຊັນຈາກ `FileVersionInfo` ຂອງ exe (ສະແດງ 2016/2019/2021/365 ຕາມ build ຖ້າແປໄດ້)
- ກວດວ່າ exe ມີຢູ່ແທ້ (`File.Exists`)

**License ຂອງ Office** (best-effort):
- ຊອກ `ospp.vbs` ໃນ `C:\Program Files\Microsoft Office\Office1x` ແລະ `Program Files (x86)`
- ລັນ `cscript //nologo ospp.vbs /dstatus` → parse ຫາ `LICENSE STATUS: ---LICENSED---`
- ຖ້າຊອກ `ospp.vbs` ບໍ່ເຫັນ ຫຼື parse ບໍ່ໄດ້ → status = `Unknown` (ບໍ່ແມ່ນ Fail)

**License ຂອງ Windows**:
- WMI `SoftwareLicensingProduct` where `PartialProductKey IS NOT NULL` ແລະ `Name LIKE 'Windows%'` → `LicenseStatus == 1` ແມ່ນ Licensed

**Font**:
- ກວດຈາກ `InstalledFontCollection` ວ່າມີ family ໃນ `requiredFonts` ບໍ່ (default: `Phetsarath OT`)
- ຖ້າບໍ່ມີ → ສະແດງປຸ່ມ "ຕິດຕັ້ງ font" (Fix):
  - ຖອດ font ທີ່ຝັງ (`Assets/Fonts/*.ttf`) ໄປ `C:\Windows\Fonts`
  - ລົງທະບຽນ `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts`
  - `AddFontResource` + broadcast `WM_FONTCHANGE`
- ຮອງຮັບ font ເພີ່ມຜ່ານ `requiredFonts` ແລະ ໂຟນເດີ `fonts/` ຂ້າງ exe
- ⚠ ກວດ license ຂອງ font Phetsarath OT ກ່ອນຝັງໃນ exe ແຈກຢາຍ

**ແປ້ນພິມລາວ**:
- ກວດວ່າມີ layout ລາວ (LANGID `0x0454`, KLID `00000454`) ຜ່ານ `GetKeyboardLayoutList` ຫຼື registry `HKCU\Keyboard Layout\Preload`
- ຖ້າບໍ່ມີ → ປຸ່ມ "ຕິດຕັ້ງແປ້ນພິມລາວ" (Fix):
  - ວິທີທີ 1: `InstallLayoutOrTip("0454:00000454", 0)` (P/Invoke ຈາກ `input.dll`)
  - ວິທີ fallback: PowerShell `Set-WinUserLanguageList` ເພີ່ມ `lo`
  - ຕ້ອງທົດສອບໃນ Win10 ແລະ Win11 ແທ້ (ບາງລຸ້ນອາດຕ້ອງການເນັດ → ຖ້າລົ້ມເຫຼວໃຫ້ບອກເຫດຜົນຊັດເຈນ)

### 5.3 Power → Never sleep (PowerModule)

ຕັ້ງ **ທັງ AC ແລະ DC (ແບັດ)** ເປັນ never:
- ປິດຈໍ (monitor timeout)
- Sleep (standby)
- Hibernate (timeout) — ທາງເລືອກ: `powercfg -h off`
- ປິດ disk (disk timeout)

ການຕັ້ງ: `powercfg /change monitor-timeout-ac 0`, `standby-timeout-ac 0`, `hibernate-timeout-ac 0`, `disk-timeout-ac 0` ແລະ `-dc` ຄູ່ກັນ.

ການກວດ (Check): ຢ່າ parse ຜົນຂອງ `powercfg /q` ເພາະຂໍ້ຄວາມປ່ຽນຕາມພາສາ Windows. ໃຫ້ໃຊ້ P/Invoke `PowerGetActiveScheme` + `PowerReadACValueIndex` / `PowerReadDCValueIndex` ແລ້ວປຽບທຽບກັບ 0.

### 5.4 ລ້າງຂີ້ເຫຍື້ອ (CleanupModule)

ລົບສະເພາະສິ່ງທີ່ປອດໄພ ຕາມ checkbox ໃນ config:
- User temp: `%TEMP%`
- `C:\Windows\Temp`
- Recycle Bin: `SHEmptyRecycleBin`
- Windows Update cache: `C:\Windows\SoftwareDistribution\Download` (ຢຸດ service `wuauserv` ກ່ອນ ແລ້ວເລີ່ມໃໝ່)
- Browser cache: ປິດໄວ້ເປັນ default

ກົດຄວາມປອດໄພ:
- ລົບສະເພາະ **ເນື້ອໃນ** ຂອງໂຟນເດີ ບໍ່ລົບໂຟນເດີແມ່
- ໄຟລ໌ທີ່ຖືກລັອກ / ສິດບໍ່ພໍ → **ຂ້າມ** (try/catch ແຕ່ລະໄຟລ໌) ຢ່າໃຫ້ໂປຣແກຣມຢຸດ
- ຫ້າມ follow symlink / junction ອອກນອກໂຟນເດີເປົ້າໝາຍ
- ຮອງຮັບ `excludePaths`
- Check() = ຄິດໄລ່ຂະໜາດທີ່ລ້າງໄດ້ (ບໍ່ລົບ); Fix() = ລົບຈິງ ແລະ ລາຍງານຂະໜາດທີ່ລ້າງໄດ້ + ຈຳນວນໄຟລ໌ທີ່ຂ້າມ
- ໃຊ້ `Task`/`BackgroundWorker` ເພື່ອບໍ່ໃຫ້ UI ຄ້າງ

### 5.5 ກວດ Driver (DriverCheckModule)

- WMI `Win32_PnPEntity` where `ConfigManagerErrorCode <> 0`
- `ConfigManagerErrorCode = 28` = ຍັງບໍ່ໄດ້ຕິດຕັ້ງ driver → ລາຍງານວ່າ **ຂາດ driver**
- code ອື່ນ (10, 22, 43 ...) → ລາຍງານເປັນ Warning ພ້ອມລະຫັດ
- ສະແດງ: ຊື່ອຸປະກອນ, Hardware ID, ລະຫັດ error
- **ເຟສນີ້ກວດ ແລະ ລາຍງານເທົ່ານັ້ນ** (`CanFix = false`) — ການດາວໂຫຼດ driver ອັດຕະໂນມັດ ເປັນເຟສຕໍ່ໄປ ເພາະບາງເຄື່ອງບໍ່ມີເນັດ
- ຕົວເລືອກເພີ່ມ (ເຟສຕໍ່ໄປ): ປຸ່ມ "ຮ້ອງຂໍ Windows Update ຫາ driver" ຖ້າມີເນັດ

### 5.6 Export Spec ເຄື່ອງ (SpecExportModule)

ດຶງຂໍ້ມູນຜ່ານ WMI:
- ຫ້ອງ + ເລກເຄື່ອງ (ຈາກ config), hostname, username, ວັນທີ/ເວລາ
- ຍີ່ຫໍ້ / model (`Win32_ComputerSystem`), serial (`Win32_BIOS`), mainboard (`Win32_BaseBoard`)
- CPU: ຊື່, ຈຳນວນ core/thread (`Win32_Processor`)
- RAM: ຂະໜາດລວມ, ແຕ່ລະ slot (ຂະໜາດ, ຄວາມໄວ, ປະເພດ) (`Win32_PhysicalMemory`)
- Disk: model, ຂະໜາດ, ປະເພດ SSD/HDD (`MSFT_PhysicalDisk` ໃນ namespace `root\Microsoft\Windows\Storage`), free space ແຕ່ລະ drive
- GPU (`Win32_VideoController`)
- Network: MAC, IP (`Win32_NetworkAdapterConfiguration` where IPEnabled)
- OS: ຊື່, ເວີຊັນ, build, architecture
- ຜົນຂອງ module ອື່ນ (Office, font, keyboard, driver ທີ່ຂາດ) ເພີ່ມເປັນ sheet ທີ່ 2 (ທາງເລືອກ)

Output: Excel (`.xlsx`) **ໄຟລ໌ລະເຄື່ອງ** ຊື່ `{ຫ້ອງ}-{ເລກ}.xlsx` ໃນໂຟນເດີ `specs/` ຂ້າງ exe (ຢູ່ໃນ USB). ຖ້າເຄື່ອງທີ່ມີຊື່ດຽວກັນມີແລ້ວ → ຂຽນທັບ ແຕ່ຖາມຢືນຢັນ ຫຼື ເພີ່ມ timestamp.
ໝາຍເຫດ: ຊື່ໄຟລ໌ຕ້ອງ sanitize ຕົວອັກສອນທີ່ Windows ຫ້າມ (`\ / : * ? " < > |`).

### 5.7 ປຸ່ມສະແກນ (ScanPanel)

- ມີ **checkbox** ແຕ່ລະຂໍ້ (1–6) ໃຫ້ຕິກເລືອກວ່າຈະສະແກນ/ແກ້ຂໍ້ໃດ
- ປຸ່ມ **"ສະແກນ"** → ລັນ `Check()` ຂອງ module ທີ່ເລືອກ ຕາມລຳດັບ (background thread, ມີ progress)
- ສະແດງຜົນແຕ່ລະຂໍ້: ✓ ຜ່ານ / ✗ ບໍ່ຜ່ານ / ⚠ ເຕືອນ / ? ບໍ່ຮູ້, ພ້ອມ Summary ແລະ ກົດຂະຫຍາຍເບິ່ງ Details
- ປຸ່ມ **"ແກ້ໄຂທີ່ເລືອກ"** → ລັນ `Fix()` ສະເພາະຂໍ້ທີ່ຕິກ ແລະ `CanFix = true` ແລະ ສະຖານະບໍ່ຜ່ານ
- ຫຼັງແກ້ → ສະແກນຊ້ຳອັດຕະໂນມັດເພື່ອຢືນຢັນ
- ສະຫຼຸບທ້າຍ: "ຜ່ານ X / Y ຂໍ້"
- ຄວນມີປຸ່ມ "ບັນທຶກລາຍງານ" (.txt) (ທາງເລືອກ)

---

## 6. UI ແລະ ສອງພາສາ

- ໜ້າຕ່າງດຽວ ແບ່ງ 2 ສ່ວນ (ຫຼື Tab): **ພື້ນຫຼັງ** | **ກວດສອບ**; ມຸມບນມີ dropdown ປ່ຽນພາສາ ລາວ/English
- ຂໍ້ຄວາມ UI ທັງໝົດມາຈາກ `lo.json` / `en.json` ຜ່ານ `Localizer.T("key")` — ຫ້າມ hardcode ຂໍ້ຄວາມໃນ Form
- ປ່ຽນພາສາແລ້ວ refresh ທັນທີ ແລະ ບັນທຶກໃນ `config.language`
- Font ຂອງ UI: ໃຊ້ `Phetsarath OT` ຖ້າມີ, ບໍ່ມີກໍໃຊ້ `Leelawadee UI` (ມີໃນ Windows ຢູ່ແລ້ວ ແລະ ຮອງຮັບລາວ), ສຸດທ້າຍ `Segoe UI`
- ຮອງຮັບ DPI scaling (`AutoScaleMode = Dpi`) ເພາະຄອມຫ້ອງຮຽນອາດມີຈໍຫຼາຍຂະໜາດ
- ເວລາເຮັດວຽກຍາວ (ລ້າງໄຟລ໌, ສະແກນ) ຕ້ອງບໍ່ໃຫ້ UI ຄ້າງ

---

## 7. ການ Build ແລະ Publish

```
dotnet build -c Release
```
- ໃຊ້ Costura.Fody ລວມ DLL (Newtonsoft.Json, ClosedXML, ແລະ dependencies) ເຂົ້າ exe ດຽວ
- ຜົນ: `ClassroomChecker.exe` ໄຟລ໌ດຽວ ກ໊ອບລົງ USB ໄດ້ເລີຍ
- ຖ້າ Costura ເກີດບັນຫາກັບ ClosedXML → fallback: export `.csv` ເປັນຄ່າຕົ້ນ ແລ້ວແກ້ພາຍຫຼັງ (ແຈ້ງຜູ້ໃຊ້ກ່ອນປ່ຽນ)

---

## 8. ຂໍ້ຈຳກັດ ແລະ ຄວາມສ່ຽງ (ຕ້ອງເຄົາລົບ)

1. exe ຕ້ອງຂໍສິດ admin (UAC) ທຸກຄັ້ງທີ່ເປີດ — ຖືກຕ້ອງຕາມທີ່ຕ້ອງການ
2. Wallpaper ຕັ້ງໃຫ້ສະເພາະ user ທີ່ login ຢູ່ (HKCU)
3. Antivirus ອາດເຕືອນ exe ທີ່ບໍ່ມີ digital signature ທີ່ແກ້ power / ລຶບໄຟລ໌ / ຕິດຕັ້ງ font → ຕ້ອງທົດສອບ
4. ການຕິດຕັ້ງແປ້ນພິມລາວ offline ອາດບໍ່ສຳເລັດໃນບາງລຸ້ນ Windows → ຕ້ອງທົດສອບ ແລະ ລາຍງານ error ຊັດເຈນ
5. License check ຂອງ Office ເປັນ best-effort (ຂຶ້ນກັບ `ospp.vbs`) → ຫ້າມສະແດງ "ບໍ່ຖືກລິຂະສິດ" ຖ້າກວດບໍ່ໄດ້, ໃຫ້ສະແດງ "ກວດບໍ່ໄດ້"
6. ຫ້າມ crash ເມື່ອ WMI / registry ອ່ານບໍ່ໄດ້ — ທຸກ module ຕ້ອງ catch exception ແລ້ວສົ່ງ `Unknown` ພ້ອມເຫດຜົນ
7. ຫ້າມລົບໄຟລ໌ນອກລາຍການທີ່ກຳນົດໄວ້ໃນ 5.4

---

## 9. ແຜນພັດທະນາເປັນເຟສ

ເຮັດເທື່ອລະເຟສ, ຈົບແຕ່ລະເຟສຕ້ອງ build ໄດ້ ແລະ ທົດສອບໄດ້ ກ່ອນໄປເຟສຕໍ່ໄປ.

| ເຟສ | ເນື້ອໃນ | ເກນຜ່ານ (Acceptance) |
|---|---|---|
| 0 | Spike: ຂຽນຂໍ້ຄວາມລາວລົງຮູບ + ຕັ້ງ wallpaper (console/form ງ່າຍໆ) | ສະຣະ/ວັນນະຍຸດລາວສະແດງຖືກ, wallpaper ປ່ຽນໄດ້ຈິງໃນເຄື່ອງແທ້ |
| 1 | Project skeleton, config.json, MainForm, ແຖບ Wallpaper ເຕັມ (5.1), i18n ພື້ນຖານ | ເລືອກຫ້ອງ/ເລກ → ກົດ → wallpaper ປ່ຽນ, ປິດ-ເປີດ app ຄ່າຍັງຢູ່, ເພີ່ມຫ້ອງໃໝ່ໄດ້ |
| 2 | ໂຄງ `ICheckModule` + ScanRunner + ກວດ Office/Font/ແປ້ນພິມ (5.2) + ກວດ Driver (5.5) | ປຸ່ມສະແກນສະແດງ ✓/✗ ຖືກຕ້ອງໃນເຄື່ອງທີ່ຮູ້ຄ່າ |
| 3 | Fix: ຕິດຕັ້ງ font/ແປ້ນພິມ, PowerModule (5.3), CleanupModule (5.4), checkbox + ປຸ່ມ "ແກ້ໄຂທີ່ເລືອກ" | ແກ້ແລ້ວສະແກນຊ້ຳຜ່ານ; ລ້າງໄຟລ໌ແລ້ວບໍ່ crash ເມື່ອເຈິໄຟລ໌ຖືກລັອກ |
| 4 | SpecExportModule (5.6), UI ສອງພາສາຄົບ | ໄດ້ໄຟລ໌ xlsx ຖືກຊື່ ໃນໂຟນເດີ specs/ ແລະ ຂໍ້ມູນຖືກຕ້ອງ |
| 5 | Costura (exe ດຽວ), ທົດສອບ Win10 + Win11, ທົດສອບຈາກ USB, ເຄື່ອງບໍ່ມີເນັດ | ກົດ exe ຈາກ USB ແລ້ວເຮັດວຽກທຸກຂໍ້ໃນເຄື່ອງທີ່ບໍ່ເຄີຍຕິດຕັ້ງຫຍັງ |

---

## 10. ຂໍ້ຕົກລົງທີ່ຍັງເປີດ (ຖາມຜູ້ໃຊ້ກ່ອນເຮັດ ຫຼື ໃຊ້ຄ່າ default)

| ຫົວຂໍ້ | ຄ່າ default ທີ່ໃຊ້ຖ້າຍັງບໍ່ໄດ້ຄຳຕອບ |
|---|---|
| ຂໍ້ຄວາມໃນຮູບພື້ນຫຼັງ | `{room} - {number}` ຢ່າງດຽວ (ຕັ້ງຄ່າໄດ້ຜ່ານ `template`) |
| ຕຳແໜ່ງຂໍ້ຄວາມ | `top-right`, margin 40px (ຫຼີກ icon desktop ທີ່ຢູ່ເບື້ອງຊ້າຍ ແລະ taskbar ເບື້ອງລຸ່ມ) |
| ຂະໜາດຮູບ bg.png | ໃຊ້ຂະໜາດຕາມຮູບຕົ້ນສະບັບ, ບໍ່ resize |
| License font Phetsarath OT | ຜູ້ໃຊ້ຕ້ອງຢືນຢັນວ່າແຈກຢາຍໄດ້ ກ່ອນຝັງໃນ exe |
| ບັນຊີ user ທີ່ຈະຕັ້ງ wallpaper | user ທີ່ login ຢູ່ (ສະແດງຊື່ໃນ UI) |

---

## 11. ກົດສຳລັບ Claude Code (ອ່ານກ່ອນເລີ່ມ)

1. ອ່ານ `cm-spec.md` ທັງໝົດກ່ອນ, ເຮັດ **ເທື່ອລະເຟສ** ຕາມຕາຕະລາງໃນຂໍ້ 9. ຢ່າໂດດຂ້າມເຟສ.
2. ກ່ອນເລີ່ມແຕ່ລະເຟສ ບອກແຜນສັ້ນໆ (ໄຟລ໌ທີ່ຈະສ້າງ/ແກ້) ແລ້ວຈຶ່ງເລີ່ມ.
3. **ທຸກຄັ້ງທີ່ສ້າງ ຫຼື ແກ້ໄຂໄຟລ໌** ໃຫ້ອັບເດດ `FILES.md` (ຊື່ໄຟລ໌ + ໜ້າທີ່ໃນ 1 ແຖວ) ເພື່ອໃຫ້ຜູ້ພັດທະນາຮູ້ວ່າມີໄຟລ໌ຫຍັງແດ່.
4. ໂຄ້ດຕ້ອງອ່ານງ່າຍ ມີ comment ເປັນພາສາລາວ ຫຼື ອັງກິດ ໃນຈຸດທີ່ບໍ່ຊັດເຈນ (ໂດຍສະເພາະ P/Invoke ແລະ registry).
5. ທຸກ module ຕ້ອງ catch exception, ຫ້າມ crash ທັງແອັບ.
6. ຢ່າລຶບ ຫຼື ແກ້ໄຂ ລະບົບນອກຂອບເຂດທີ່ spec ກຳນົດ.
7. ຖ້າ spec ບໍ່ຊັດເຈນ ຫຼື ຂັດກັນ → **ຖາມຜູ້ໃຊ້** ແທນການເດົາ.
8. ເມື່ອ debug: ອ່ານໄຟລ໌ຕົ້ນສະບັບໃນໂປຣເຈັກໂດຍກົງ ແລະ ລັນ build ເພື່ອເບິ່ງ error ຈິງ, ຢ່າເດົາຈາກຂໍ້ຄວາມທີ່ຜູ້ໃຊ້ copy ມາ.
9. ຈົບແຕ່ລະເຟສ: ລັນ `dotnet build -c Release`, ລາຍງານສັ້ນໆວ່າເຮັດຫຍັງແລ້ວ ແລະ ຜູ້ໃຊ້ຕ້ອງທົດສອບຫຍັງໃນເຄື່ອງແທ້.

### ຕົວຢ່າງ prompt ເລີ່ມຕົ້ນ (ໃຫ້ຜູ້ພັດທະນາ copy ໃຊ້ໃນ Claude Code)

```
ອ່ານ cm-spec.md ທັງໝົດ ແລ້ວເລີ່ມ "ເຟສ 0" ຕາມຂໍ້ 9.
ສ້າງເປັນໂປຣເຈັກ C# WinForms .NET Framework 4.8 ຕາມໂຄງສ້າງໃນຂໍ້ 3.
ເມື່ອເຮັດແລ້ວ ອັບເດດ FILES.md ແລະ ບອກຂ້ອຍວ່າຕ້ອງທົດສອບຫຍັງໃນເຄື່ອງແທ້.
ຢ່າເລີ່ມເຟສ 1 ຈົນກວ່າຂ້ອຍຈະບອກວ່າເຟສ 0 ຜ່ານ.
```
