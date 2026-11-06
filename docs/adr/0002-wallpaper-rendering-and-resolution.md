# ADR 0002: ການແຕ້ມຂໍ້ຄວາມພາສາລາວ ແລະ ການປັບຂະໜາດຮູບພື້ນຫຼັງຕາມ Resolution

## ສະຖານະ (Status)
**Accepted**

## ບໍລິບົດ (Context)
ໃນການຕັ້ງຄ່າຮູບພື້ນຫຼັງ (Wallpaper) ສຳລັບຄອມພິວເຕີຫ້ອງຮຽນ:
1. ພາສາລາວມີສະຣະເທິງ, ສະຣະລຸ່ມ, ແລະ ວັນນະຍຸດ (ເຊັ່ນ: ຫ້ອງ, ເຄື່ອງ, ໝາຍ). ການແຕ້ມດ້ວຍ GDI+ `Graphics.DrawString` ແບບປົກກະຕິໃນບາງເວີຊັນຂອງ Windows ອາດເຮັດໃຫ້ສະຣະເລື່ອນ ຫຼື ວັນນະຍຸດຊ້ອນກັນ.
2. ຈໍຄອມພິວເຕີໃນຫ້ອງຮຽນແຕ່ລະເຄື່ອງອາດມີ Resolution ແຕກຕ່າງກັນ (ເຊັ່ນ 1366x768, 1600x900, 1920x1080). ຖ້າແຕ້ມຂໍ້ຄວາມໃສ່ຮູບ `bg.png` ຕົ້ນສະບັບໂດຍກົງ ໂດຍບໍ່ປັບຕາມຂະໜາດຈໍ, Windows ຈະເຮັດການ Scale/Stretch ຮູບ ເຊິ່ງອາດເຮັດໃຫ້ຕົວໜັງສືມົວ ຫຼື ຖືກບັງ/ຕົກຂອບຈໍ.

## ການຕັດສິນໃຈ (Decision)
1. **Rendering Engine (Phase 0 Spike)**:
   - ໃນ Phase 0 ຈະທົດສອບສົບທຽບທັງ **`Graphics.DrawString` (GDI+)** ແລະ **`TextRenderer.DrawText` (GDI / Uniscribe)** ໂດຍໃຊ້ຟອນ `Phetsarath OT`.
   - ເລືອກ Engine ທີ່ສະແດງວັນນະຍຸດ ແລະ ສະຣະພາສາລາວໄດ້ຖືກຕ້ອງ, ບໍ່ມີບັນຫາ overlapping ຫຼື clipping.
2. **Dynamic Screen Resolution Fitting**:
   - ແທນທີ່ຈະແຕ້ມໃສ່ຂະໜາດຮູບເດີມ, ແອັບຈະກວດສອບ Resolution ຂອງຈໍຫຼັກ (`Screen.PrimaryScreen.Bounds`).
   - Scale/Crop ຮູບພື້ນຫຼັງໃຫ້ເຕັມຈໍພໍດີ, ຈາກນັ້ນແຕ້ມຂໍ້ຄວາມ (ຊື່ຫ້ອງ + ເລກເຄື່ອງ) ລົງເທິງ Canvas ທີ່ເປັນຂະໜາດຈໍແທ້.
   - ວິທີນີ້ເຮັດໃຫ້ຕົວໜັງສືຄົມຊັດທີ່ສຸດ 100% ບໍ່ຜ່ານການ interpolate ຂອງ Windows Desktop Wallpaper subsystem.

## ຜົນກະທົບ (Consequences)
- ຮູບ `wallpaper.png` ທີ່ຖືກ export ໄປຍັງ `C:\ProgramData\ClassroomChecker\wallpaper.png` ຈະມີ Resolution ເທົ່າກັບຈໍແທ້ສະເໝີ.
- ຕຳແໜ່ງ margin 40px top-right ຈະຢູ່ມຸມຂວາເທິງຂອງຈໍສະເໝີ ບໍ່ຫຼຸດຂອບ ຫຼື ບັງ Desktop Icons ທີ່ຢູ່ເບື້ອງຊ້າຍ.
