# printcd
print the current directory
# pecho
print the command line argument as it is
# startrun
Executes the given commandline with new console window (use default terminal emulator).
# hiderun
Executes the given commandline with no console window (useless with GUI applicatons).
# adminrun
Executes the given commandline as administrator
# uacrun
like startrun, but this executable requires administrator privilege when invoked, so equivalent to adminrun.
# cmdc
Executes the given commandline as an argument to `cmd /c`, with "%" correctly escaped
# errlogrun
Executes the given commandline, redirecting the stderr to the given file
- Usage: errlogrun stderr.txt [commandline]
# waitrun
Executes the given commandline and wait a key press
# andrun
Executes the given two commandlines in order. "&" in first_command should be escaped as "&&".
- usage: `andrun first_command & second_command``
# looprun
Executes the given command repeatedly
# piperun
Executes the given two commandlines with a pipe. "|" in first_command should be escaped as "||".  
- usage: `piperun first_command | second_command``
# piperunex
like piperun but can execute any number of commandlines. "|" in all commandlines should be escaped as "||".  
- usage: `piperunex command_1 | command_2 | ... | command_n``
# evalrun
Gets the output of the given commandline and executes it as a commandline.
# hererun
Executes the commandline, replacing the specified string with the executable's directory. The executable has an icon because it may be renamed to use.
- Usage: `hererun // cmd "//\..\script.bat"`
# "runother" family
Make commandline from its "companion" text file and the passed argument, and execute it. The companion file is determined by the full path of the executable, changing the last and the third last character to "t" (`*.txt` for `*.exe` and `*.tot` for `*.com` etc.). Each executable has an icon because it will be renamed to use. The versions with `-gui` is much like those without it, but compiled as Win32 application (no console window).
## runother(-gui)
Simply concatenating the content of the companion file and the passed argument and execute it.
## runotherex(-gui)
The first line of the companion file is recognized as "prefix" (here we assume it is `$$`), and the following lines are executed, with replacement:
- prefix+`c` (here `$$c`)...the passed argument
- prefix+`a` (here `$$a`)...the passed argument, with "&" replaced by "&&" (for andrun)
- prefix+`b` (here `$$b`)...the passed argument, with "|" replaced by "||" (for piperun)
- prefix+`d` (here `$$d`)...the directory containing the executable
# Note
the following commands quit without waiting for the given commandline to terminate.
- startrun, adminrun, uacrun, hiderun