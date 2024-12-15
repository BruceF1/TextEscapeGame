@Echo off&SetLocal EnableExtensions EnableDelayedExpansion
rem Copy this file into the bin\release folder of your executable and open the CLI and type merge.bat yourmergedexename.exe

if "%1"=="" (
	echo missing output file name.
	echo usage: Merge.bat "MyMergedExecutable.exe"
	exit /b
) else (
	set outputFile=%1
)

rem if the file exists, let's delete it as we are going to build the file again anyway
IF EXIST "%outputFile%" (
    DEL "%outputFile%"
)

set executable=
for %%e in (*.exe) do (
	set executable=!executable! %%~nxe
)  

set libraries=
for %%i in (*.dll) do (
	set libraries=!libraries! %%~nxi
)

rem echo Looking for ILMerge in "ILMerge\ILMerge.exe"
"c:\ILMerge\ILMerge.exe" /ndebug /targetplatform:"v4,C:\Windows\Microsoft.NET\Framework64\v4.0.30319" /out:%outPutFile% %executable% %libraries%