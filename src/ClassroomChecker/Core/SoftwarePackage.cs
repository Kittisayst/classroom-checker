using System;

namespace ClassroomChecker.Core
{
    /// <summary>
    /// ໂຄງສ້າງຂໍ້ມູນຊອບແວພື້ນຖານ (Software Package) ສຳລັບລະບົບຕິດຕັ້ງອັດຕະໂນມັດ
    /// </summary>
    public class SoftwarePackage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string LicenseType { get; set; }
        public string ExeSearchName { get; set; }
        public string[] FallbackPaths { get; set; }
        public string[] InstallerFilePatterns { get; set; }
        public string SilentArguments { get; set; }
        public bool IsMsi { get; set; }

        public SoftwarePackage()
        {
            FallbackPaths = new string[0];
            InstallerFilePatterns = new string[0];
            SilentArguments = "/S";
        }
    }
}
