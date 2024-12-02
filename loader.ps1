#usage: powershell -ExecutionPolicy RemoteSigned -File loader.ps1 [command-name] [args-base64]
$c=$args
$a='$c[1]';
if ($null -eq $c[1]) {$a=''}else{$c[1]=[System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($c[1]));}
Add-Type -Path "$PSScriptRoot\_helper.cs", "$PSScriptRoot\$($c[0]).cs" | Out-Null
$a = Invoke-Expression "[Program]::$($c[0])($a);"