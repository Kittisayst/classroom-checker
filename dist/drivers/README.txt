[ພາສາລາວ]
ໃສ່ driver ທີ່ຕ້ອງການຕິດຕັ້ງແບບ offline ໄວ້ໃນໂຟນເດີນີ້ (ເປັນໂຟນເດີຍ່ອຍແຍກຕາມອຸປະກອນກໍໄດ້).
ແຕ່ລະ driver ຕ້ອງມີໄຟລ໌ .inf ພ້ອມໄຟລ໌ປະກອບ (.sys, .cat, ...) ຢູ່ຮ່ວມກັນ.

ວິທີໃຊ້: ເປີດ Classroom Checker -> ແຖບກວດສອບ -> ສະແກນ -> ຕິກຂໍ້ Driver -> "ແກ້ໄຂທີ່ເລືອກ"
ໂປຣແກຣມຈະຕິດຕັ້ງດ້ວຍ pnputil ແລ້ວກວດຊ້ຳ. ຜົນລະອຽດເບິ່ງໃນ "ເບິ່ງລາຍລະອຽດ".

ໝາຍເຫດ:
- driver ທີ່ບໍ່ກົງກັບ hardware ຂອງເຄື່ອງນັ້ນຈະຖືກຂ້າມ (ເປັນເລື່ອງປົກກະຕິ).
- ບາງ driver ຕ້ອງ Restart ເຄື່ອງ ຈຶ່ງຈະເຮັດວຽກ.
- ໃສ່ສະເພາະ driver ທີ່ມາຈາກແຫຼ່ງທີ່ເຊື່ອຖືໄດ້.
- ໂຟນເດີນີ້ວ່າງໄດ້. ຖ້າບໍ່ມີ .inf ຂໍ້ Driver ຈະສະແດງ "ບໍ່ພົບໄຟລ໌ driver" ແລະ ບໍ່ແຕະຕ້ອງລະບົບ.

[English]
Put drivers for offline installation in this folder (subfolders per device are fine).
Each driver needs its .inf file together with its payload files (.sys, .cat, ...).

Usage: open Classroom Checker -> Scan tab -> Scan -> tick the Driver row -> "Fix Selected".
The app installs them with pnputil and re-checks. See "View details" for the full output.

Notes:
- Drivers that do not match the machine's hardware are skipped (this is normal).
- Some drivers require a restart.
- Only add drivers from trusted sources.
- This folder may be empty. Without any .inf the Driver row reports "no driver files found" and changes nothing.
