set CSC_EXE=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe

%CSC_EXE% printcd.cs

%CSC_EXE% _helper.cs pecho.cs
%CSC_EXE% _helper.cs piperun.cs
%CSC_EXE% _helper.cs piperunex.cs

%CSC_EXE% -t:winexe _helper.cs adminrun.cs
%CSC_EXE% -t:winexe _helper.cs hiderun.cs
%CSC_EXE% -t:winexe _helper.cs startrun.cs
%CSC_EXE% -t:winexe _helper.cs -win32icon:icon-G.png.ico runother-gui.cs
%CSC_EXE% -t:winexe _helper.cs -win32icon:iconex-G.png.ico runotherex-gui.cs

%CSC_EXE% _helper.cs _handler.cs andrun.cs
%CSC_EXE% _helper.cs _handler.cs cmdc.cs
%CSC_EXE% _helper.cs _handler.cs evalrun.cs
%CSC_EXE% _helper.cs _handler.cs waitrun.cs
%CSC_EXE% _helper.cs _handler.cs errlogrun.cs
%CSC_EXE% _helper.cs _handler.cs -win32icon:icon-H.png.ico hererun.cs
%CSC_EXE% _helper.cs _handler.cs -win32icon:icon-C.png.ico runother.cs
%CSC_EXE% _helper.cs _handler.cs -win32icon:iconex-C.png.ico runotherex.cs

%CSC_EXE% -out:uacrun.exe -win32manifest:uac.xml startrun.cs _helper.cs
