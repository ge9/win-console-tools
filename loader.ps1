#usage: powershell -ExecutionPolicy RemoteSigned -File loader.ps1 /// [command-name] [args]
$c = [Environment]::CommandLine
$c=$c.Substring($c.IndexOf("///") + 3).TrimStart().Split(" ",2);
$c[1]=$c[1].TrimStart();
$a='$c[1]';
if ($null -eq $c[1]) {$a=''}
Add-Type -Path "$PSScriptRoot\_helper.cs", "$PSScriptRoot\$($c[0]).cs" | Out-Null
$a = Invoke-Expression "[Program]::$($c[0])($a);"