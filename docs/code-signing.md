# Code signing (ຂໍ້ 13)

ເປົ້າໝາຍ: ຫຼຸດການເຕືອນຂອງ SmartScreen / antivirus ຕໍ່ `ClassroomChecker.exe`. ແອັບນີ້ແກ້ Power, ລຶບໄຟລ໌,
ຕິດຕັ້ງ font/driver ແລະ ຂໍສິດ admin ສະນັ້ນ exe ທີ່ບໍ່ມີ signature ມັກຖືກຈັບເຕືອນ (ເບິ່ງ cm-spec ຂໍ້ 8.3).

> [!IMPORTANT]
> ການເຊັນຕ້ອງໃຊ້ **certificate ຂອງເຈົ້າຂອງໂປຣເຈັກເອງ**. script ຈັດການແຕ່ຂັ້ນຕອນເຊັນ ບໍ່ໄດ້ອອກ certificate ໃຫ້.
> ຫ້າມ commit ໄຟລ໌ `.pfx` ຫຼື ລະຫັດຜ່ານເຂົ້າ git.

## ທາງເລືອກ certificate

| ທາງເລືອກ | ຜົນ | ໝາຍເຫດ |
|---|---|---|
| Self-signed (ສ້າງເອງ) | ຍັງຂຶ້ນເຕືອນ SmartScreen ຢູ່ ເວັ້ນແຕ່ນຳ cert ໄປ trust ໃນທຸກເຄື່ອງ | ຟຣີ; ໃຊ້ໄດ້ເມື່ອມີສິດ admin ທຸກເຄື່ອງ (ກໍລະນີນີ້ແມ່ນ) |
| OV code-signing (ຈາກ CA) | ຫຼຸດເຕືອນ, reputation ຕ້ອງສະສົມ | ມີຄ່າໃຊ້ຈ່າຍຕໍ່ປີ |
| EV code-signing | ໄດ້ reputation ທັນທີ | ແພງກວ່າ, ມັກຕ້ອງໃຊ້ hardware token |

ເນື່ອງຈາກທຸກເຄື່ອງເປັນ Administrator ຢູ່ແລ້ວ, **self-signed + trust cert ຄັ້ງດຽວໃນແຕ່ລະເຄື່ອງ** ເປັນທາງທີ່ປະຢັດ.
ຖ້າຕ້ອງການໃຫ້ຜູ້ໃຊ້ທົ່ວໄປບໍ່ເຫັນຄຳເຕືອນ ຕ້ອງໃຊ້ OV/EV.

## ສ້າງ self-signed certificate (PowerShell, ແບບ admin)

```powershell
$cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=Classroom Checker" `
        -CertStoreLocation Cert:\CurrentUser\My -NotAfter (Get-Date).AddYears(5)
$pwd = Read-Host -AsSecureString "ລະຫັດຜ່ານ .pfx"
Export-PfxCertificate -Cert $cert -FilePath D:\secure\ClassroomChecker.pfx -Password $pwd
Export-Certificate    -Cert $cert -FilePath D:\secure\ClassroomChecker.cer   # ສຳລັບ trust ໃນເຄື່ອງອື່ນ
```

ເກັບ `.pfx` ໄວ້ບ່ອນປອດໄພ **ນອກ repo ແລະ ນອກ USB ທີ່ແຈກຢາຍ**.

## ເຊັນ exe

```bat
package.bat
set SIGN_PFX=D:\secure\ClassroomChecker.pfx
set SIGN_PASSWORD=********
sign.bat
```

[sign.bat](../sign.bat) ໃຊ້ `signtool sign /fd SHA256` ພ້ອມ timestamp (`SIGN_TIMESTAMP`, ຄ່າເລີ່ມຕົ້ນ DigiCert)
ແລ້ວ `signtool verify /pa`. ຕ້ອງມີເນັດຕອນເຊັນ (ເພື່ອ timestamp) ແຕ່ບໍ່ຕ້ອງມີຕອນນຳໄປໃຊ້ໃນຫ້ອງຮຽນ.

## Trust self-signed cert ໃນເຄື່ອງຫ້ອງຮຽນ (admin, ເຮັດຄັ້ງດຽວຕໍ່ເຄື່ອງ)

```powershell
Import-Certificate -FilePath .\ClassroomChecker.cer -CertStoreLocation Cert:\LocalMachine\TrustedPublisher
Import-Certificate -FilePath .\ClassroomChecker.cer -CertStoreLocation Cert:\LocalMachine\Root
```

> [!WARNING]
> ການໃສ່ cert ໃນ `Root` ໝາຍຄວາມວ່າເຄື່ອງນັ້ນເຊື່ອ cert ນີ້ສຳລັບທຸກຈຸດປະສົງ. ຖ້າ `.pfx` ຮົ່ວ ຜູ້ອື່ນເຊັນມັນແວຣ໌ໄດ້.
> ດັ່ງນັ້ນຕ້ອງປົກປ້ອງ `.pfx` ແລະ ໃຊ້ລະຫັດຜ່ານທີ່ແຂງແຮງ.

## ຂໍ້ຄວນຮູ້

- ຕ້ອງເຊັນ **ໃໝ່ທຸກຄັ້ງທີ່ build/package ໃໝ່** (`package.bat` ຂຽນທັບ exe).
- Signature ບໍ່ຮັບປະກັນວ່າ antivirus ຈະບໍ່ເຕືອນ ແຕ່ຊ່ວຍຫຼາຍ. ຄວນທົດສອບໃນເຄື່ອງແທ້ທີ່ມີ AV ທີ່ໃຊ້ຢູ່.
- ໃຊ້ `Get-AuthenticodeSignature dist\ClassroomChecker.exe` ເພື່ອກວດສະຖານະ.
