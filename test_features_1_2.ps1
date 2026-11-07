# Test suite for Feature 1 (Master Summary Excel) and Feature 2 (Quick Setup Runner)
$ErrorActionPreference = "Stop"

Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "         FEATURE 1 & FEATURE 2 VERIFICATION SUITE" -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Cyan

$passed = 0
$total = 0

function Assert-Test([string]$name, [bool]$cond, [string]$detail = "") {
    $script:total++
    if ($cond) {
        $script:passed++
        Write-Host "  [PASS] $name" -ForegroundColor Green -NoNewline
        if ($detail) { Write-Host " ($detail)" } else { Write-Host "" }
    } else {
        Write-Host "  [FAIL] $name" -ForegroundColor Red -NoNewline
        if ($detail) { Write-Host " - $detail" } else { Write-Host "" }
    }
}

# 1. Prepare specs directory with multiple PC files
$specsDir = Join-Path $PSScriptRoot "specs"
if (-not (Test-Path $specsDir)) {
    New-Item -ItemType Directory -Path $specsDir | Out-Null
}

$pc1 = Join-Path $specsDir "ຫ້ອງ A-01.xlsx"
$pc2 = Join-Path $specsDir "ຫ້ອງ A-02.xlsx"
$pc3 = Join-Path $specsDir "ຫ້ອງ A-03.xlsx"

Assert-Test "PC 1 spec file exists" (Test-Path $pc1) ($pc1)

# Create PC 2 and PC 3 copies if not exist
if (Test-Path $pc1) {
    Copy-Item $pc1 $pc2 -Force
    Copy-Item $pc1 $pc3 -Force
}
Assert-Test "PC 2 spec file created" (Test-Path $pc2) ($pc2)
Assert-Test "PC 3 spec file created" (Test-Path $pc3) ($pc3)

# 2. Load assembly
$exePath = (Resolve-Path "dist\ClassroomChecker.exe").Path
$bytes = [System.IO.File]::ReadAllBytes($exePath)
$asm = [System.Reflection.Assembly]::Load($bytes)
Assert-Test "Assembly loaded from dist\ClassroomChecker.exe" ($asm -ne $null)

# 3. Test Feature 1: MasterSummaryService
$masterType = $asm.GetType("ClassroomChecker.Core.MasterSummaryService")
Assert-Test "MasterSummaryService type found" ($masterType -ne $null)

$genMethod = $masterType.GetMethod("GenerateMasterSummary", [System.Reflection.BindingFlags]"Public,Static")
Assert-Test "GenerateMasterSummary method found" ($genMethod -ne $null)

$params = [object[]]@([string]$specsDir, [string]"ຫ້ອງ A")
$masterPath = $genMethod.Invoke($null, $params)
Write-Host "  Generated Master Summary at: $masterPath" -ForegroundColor DarkGray
Assert-Test "Master Excel file generated" (Test-Path $masterPath) (Split-Path $masterPath -Leaf)

# 4. Verify Master Excel Structure & OpenXML compliance
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($masterPath)

$hasSheet1 = ($zip.GetEntry("xl/worksheets/sheet1.xml") -ne $null)
$hasSheet2 = ($zip.GetEntry("xl/worksheets/sheet2.xml") -ne $null)
$hasSheet3 = ($zip.GetEntry("xl/worksheets/sheet3.xml") -ne $null)
$hasWorkbook = ($zip.GetEntry("xl/workbook.xml") -ne $null)
$hasStyles = ($zip.GetEntry("xl/styles.xml") -ne $null)

Assert-Test "Master Excel has Sheet 1 (Lab Overview)" $hasSheet1
Assert-Test "Master Excel has Sheet 2 (Hardware Components)" $hasSheet2
Assert-Test "Master Excel has Sheet 3 (Check Summaries)" $hasSheet3
Assert-Test "Master Excel has Workbook definition" $hasWorkbook
Assert-Test "Master Excel has Styles definition" $hasStyles

# Check sheet names in workbook.xml
$wbEntry = $zip.GetEntry("xl/workbook.xml")
$wbStream = $wbEntry.Open()
$reader = New-Object System.IO.StreamReader($wbStream, [System.Text.Encoding]::UTF8)
$wbXml = $reader.ReadToEnd()
$reader.Close()
$wbStream.Close()

$hasName1 = $wbXml.Contains('name="Lab Overview"')
$hasName2 = $wbXml.Contains('name="Hardware Components"')
$hasName3 = $wbXml.Contains('name="Check Summaries"')

Assert-Test "Workbook contains sheet name 'Lab Overview'" $hasName1
Assert-Test "Workbook contains sheet name 'Hardware Components'" $hasName2
Assert-Test "Workbook contains sheet name 'Check Summaries'" $hasName3

# Check styles.xml contains Phetsarath OT
$styleEntry = $zip.GetEntry("xl/styles.xml")
$styleStream = $styleEntry.Open()
$styleReader = New-Object System.IO.StreamReader($styleStream, [System.Text.Encoding]::UTF8)
$styleXml = $styleReader.ReadToEnd()
$styleReader.Close()
$styleStream.Close()

$hasFont = $styleXml.Contains('Phetsarath OT')
Assert-Test "Styles XML configured with Phetsarath OT font" $hasFont

# Print summary of generated rows in sheet1, sheet2, sheet3
for ($i = 1; $i -le 3; $i++) {
    $entry = $zip.GetEntry("xl/worksheets/sheet$i.xml")
    $stream = $entry.Open()
    $r = New-Object System.IO.StreamReader($stream, [System.Text.Encoding]::UTF8)
    $xml = $r.ReadToEnd()
    $r.Close()
    $stream.Close()
    $rowCount = ([regex]::Matches($xml, "<row ")).Count
    Write-Host "    Sheet $i row count: $rowCount (size: $($entry.Length) bytes)" -ForegroundColor Gray
}

$zip.Dispose()

# 5. Test Feature 2: QuickSetupRunner
$runnerType = $asm.GetType("ClassroomChecker.Core.QuickSetupRunner")
Assert-Test "QuickSetupRunner type found" ($runnerType -ne $null)

$runnerInstance = [System.Activator]::CreateInstance($runnerType)
Assert-Test "QuickSetupRunner instantiated" ($runnerInstance -ne $null)

# Verify QuickSetupDialog type
$dialogType = $asm.GetType("ClassroomChecker.UI.QuickSetupDialog")
Assert-Test "QuickSetupDialog type found" ($dialogType -ne $null)

# Verify Localization keys
$locType = $asm.GetType("ClassroomChecker.I18n.Localizer")
$tMethod = $locType.GetMethod("T", [System.Reflection.BindingFlags]"Public,Static", $null, @([string], [object[]]), $null)

$btnSetupLo = $tMethod.Invoke($null, @("btn_quick_setup", $null))
$mergeLo = $tMethod.Invoke($null, @("btn_merge_specs", $null))
$setupTitleLo = $tMethod.Invoke($null, @("quick_setup_title", $null))

Assert-Test "Lao translation for btn_quick_setup" ($btnSetupLo -ne "btn_quick_setup" -and $btnSetupLo -match "ຕັ້ງຄ່າດ່ວນ") ($btnSetupLo)
Assert-Test "Lao translation for btn_merge_specs" ($mergeLo -ne "btn_merge_specs" -and $mergeLo -match "ລວມສະເປັກ") ($mergeLo)
Assert-Test "Lao translation for quick_setup_title" ($setupTitleLo -ne "quick_setup_title" -and $setupTitleLo -match "ຕັ້ງຄ່າ.*ດ່ວນ") ($setupTitleLo)

# Switch to English and check translations
$setLangMethod = $locType.GetMethod("SetLanguage", [System.Reflection.BindingFlags]"Public,Static", $null, @([string]), $null)
$setLangMethod.Invoke($null, @("en"))

$btnSetupEn = $tMethod.Invoke($null, @("btn_quick_setup", $null))
$mergeEn = $tMethod.Invoke($null, @("btn_merge_specs", $null))
$setupTitleEn = $tMethod.Invoke($null, @("quick_setup_title", $null))

Assert-Test "English translation for btn_quick_setup" ($btnSetupEn -match "Quick Setup") ($btnSetupEn)
Assert-Test "English translation for btn_merge_specs" ($mergeEn -match "Master Excel|Merge") ($mergeEn)
Assert-Test "English translation for quick_setup_title" ($setupTitleEn -match "Quick Setup") ($setupTitleEn)

# Reset language back to lo
$setLangMethod.Invoke($null, @("lo"))

Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "   RESULTS: $passed / $total TESTS PASSED ($([math]::Round($passed / $total * 100))%)" -ForegroundColor $(if ($passed -eq $total) { "Green" } else { "Red" })
Write-Host "================================================================================" -ForegroundColor Cyan

if ($passed -eq $total) {
    exit 0
} else {
    exit 1
}
