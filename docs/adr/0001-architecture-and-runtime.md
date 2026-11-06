# ADR 0001: ການເລືອກ Runtime, UI Framework, ແລະ ການແພັກ Single-Executable

## ສະຖານະ (Status)
**Accepted** (ໄດ້ຮັບການຢືນຢັນຜ່ານການ Grill ກັບຜູ້ພັດທະນາແລ້ວ)

## ບໍລິບົດ (Context)
ໂຄງການ Classroom Checker ມີເປົ້າໝາຍສ້າງເຄື່ອງມື Desktop ສຳລັບຜູ້ດູແລລະບົບຫ້ອງຮຽນຄອມພິວເຕີ:
- ແລ່ນເທິງ Windows 10 ແລະ Windows 11 (64-bit ເທົ່ານັ້ນ).
- ຕ້ອງແຈກຢາຍຜ່ານ USB drive ເປັນ **Single Portable Executable (`.exe` ດຽວ)** ໂດຍບໍ່ຕ້ອງມີຂັ້ນຕອນຕິດຕັ້ງ (no installer, zero-install).
- ເຄື່ອງສ່ວນໃຫຍ່ບໍ່ມີອິນເຕີເນັດ (Offline-first).
- ຜູ້ໃຊ້ເປັນ Local Administrator ຢູ່ແລ້ວ.
- ຕ້ອງສະແດງຜົນ ແລະ ແຕ້ມຟອນພາສາລາວ (Phetsarath OT) ໄດ້ຢ່າງຖືກຕ້ອງ 100% ໂດຍສະເພາະ ສະຣະ ແລະ ວັນນະຍຸດ ບໍ່ໃຫ້ຊ້ອນກັນ ຫຼື ເຄື່ອນຕຳແໜ່ງ.

## ທາງເລືອກທີ່ພິຈາລະນາ (Options Considered)

### ທາງເລືອກ 1: .NET Framework 4.8 (WinForms) + Costura.Fody (ຕາມທີ່ລະບຸໃນ `cm-spec.md`)
- **ຂໍ້ດີ:**
  - .NET Framework 4.8 ຖືກຕິດຕັ້ງມາກັບ Windows 10 (version 1903 ຂຶ້ນໄປ) ແລະ Windows 11 ທຸກລຸ້ນຢູ່ແລ້ວ → ບໍ່ຕ້ອງມີ runtime ຕິດໄປນຳ, ຂະໜາດ exe ຈະນ້ອຍ (ປະມານ 5–15 MB).
  - WinForms + GDI+ / Uniscribe ຮອງຮັບການແຕ້ມພາສາລາວໄດ້ດີ.
- **ຂໍ້ທ້າທາຍທີ່ພົບ:**
  - ເຄື່ອງ Dev ປັດຈຸບັນຍັງບໍ່ມີ .NET Framework 4.8 Reference Assemblies ຫຼື .NET SDK ໃນ PATH ສຳລັບ `dotnet build` (ມີ MSBuild ຈາກ Visual Studio 18 BuildTools). ຕ້ອງໃຊ້ `Microsoft.NETFramework.ReferenceAssemblies` NuGet package ເພື່ອ build ແບບ SDK-style.
  - `Costura.Fody` ມັກມີບັນຫາກັບ `ClosedXML` ແລະ `DocumentFormat.OpenXml` ໃນການແພັກ Single-exe ( assembly resolve failure ໃນ runtime).

### ທາງເລືອກ 2: .NET 8 / .NET 9 Desktop (WinForms) + Self-Contained Single-File Publish
- **ຂໍ້ດີ:**
  - ໃຊ້ຄຸນສົມບັດ build-in ຂອງ .NET SDK (`/p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true`) ໂດຍບໍ່ຕ້ອງເພິ່ງພາ plugin ພາຍນອກຄື Costura.Fody.
  - ຮອງຮັບ C# ລຸ້ນໃໝ່, API ທັນສະໄໝ, WMI ແລະ Windows Management API ໄວຂຶ້ນ.
- **ຂໍ້ເສຍ:**
  - ຖ້າແພັກແບບ Self-Contained (ເພື່ອໃຫ້ແລ່ນໄດ້ offline ໂດຍບໍ່ຕ້ອງລົງ .NET runtime ເພີ່ມໃນເຄື່ອງຫ້ອງຮຽນ), ຂະໜາດ exe ຈະໃຫຍ່ຂຶ້ນເປັນ 60–80 MB.

### ທາງເລືອກ 3: .NET Framework 4.8 ໂດຍແຍກ DLL ຫຼື ໃຊ້ ILMerge / Costura ພ້ອມ Fallback CSV
- ຖ້າ Costura ລົ້ມເຫຼວກັບ ClosedXML, fallback ໄປໃຊ້ CSV export ຫຼື ລວມ DLL ດ້ວຍວິທີ resource embedding custom `AppDomain.AssemblyResolve`.

## ການຕັດສິນໃຈເບື້ອງຕົ້ນ (Preliminary Decision)
ຮັກສາເປົ້າໝາຍຕາມ `cm-spec.md` ຄື **.NET Framework 4.8 WinForms** ເພື່ອຮັບປະກັນວ່າແລ່ນໄດ້ທັນທີເທິງ Windows 10/11 ທຸກເຄື່ອງ ໂດຍຂະໜາດໄຟລ໌ນ້ອຍ:
1. ໃຊ້ SDK-style project ພ້ອມເພີ່ມ NuGet `Microsoft.NETFramework.ReferenceAssemblies` ເພື່ອໃຫ້ສາມາດ build ໄດ້ໂດຍບໍ່ຕ້ອງຕິດຕັ້ງ .NET 4.8 Developer Pack ເຕັມ.
2. ສຳລັບ Single Exe: ທົດສອບ Costura.Fody ກ່ອນ; ຖ້າມີບັນຫາກັບ ClosedXML, ຈະໃຊ້ embedded resource assembly resolver ທີ່ຂຽນເອງໃນ `Program.cs` ເຊິ່ງມີຄວາມປອດໄພ ແລະ debug ງ່າຍກວ່າ.
3. ສຳລັບການແຕ້ມຂໍ້ຄວາມລາວ: ໃຊ້ `TextRenderer.DrawText` (GDI/Uniscribe) ຄຽງຄູ່ກັບ `Graphics.DrawString` (GDI+) ໃນ Phase 0 ເພື່ອທຽບຄຸນນະພາບສະຣະ/ວັນນະຍຸດ.

## ຜົນກະທົບ ແລະ ຄວາມສ່ຽງ (Consequences & Risks)
- ຕ້ອງທົດສອບ Phase 0 ທັນທີເພື່ອພິສູດການແຕ້ມຟອນລາວ ແລະ ການຕັ້ງ wallpaper ໃນ Windows 10/11.
- ຕ້ອງມີການກວດສອບວ່າเครื่อง dev ຈະ build ຜ່ານ CLI ໃດ (ຕິດຕັ້ງ .NET SDK ຫຼື ໃຊ້ MSBuild ຈາກ BuildTools).
