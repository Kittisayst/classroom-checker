$exePath = (Resolve-Path "dist\ClassroomChecker.exe").Path
$bytes = [System.IO.File]::ReadAllBytes($exePath)
$asm = [System.Reflection.Assembly]::Load($bytes)
$type = $asm.GetType("ClassroomChecker.Phase5Verifier")
$method = $type.GetMethod("Run")
$result = $method.Invoke($null, @())
exit $result
