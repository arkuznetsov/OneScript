@echo off

chcp 866 > nul

setlocal
set pathdir=%~dp0

rem echo ᠬ� ���� %CD%
rem echo �ਯ�� ���஢���� %pathdir%

for /f "tokens=*" %%i in ('where oscript') do set OSCRIPT=%%i
if NOT "%OSCRIPT%"=="" GOTO run

set OS_EXE=\OneScript\bin\oscript.exe
set OSCRIPT=%ProgramFiles(x86)%%OS_EXE%
if NOT EXIST "%OSCRIPT%" set OSCRIPT=%ProgramFiles%%OS_EXE%
if NOT EXIST "%OSCRIPT%" set OSCRIPT=%ProgramW6432%%OS_EXE%

:run
@echo on
"%OSCRIPT%" "%pathdir%testrunner.os" -runall %1 %2 %3 %4 %5
@echo off

rem echo ��� ������ %ERRORLEVEL%
if %ERRORLEVEL%==2 GOTO pending_exit
if NOT %ERRORLEVEL%==0 GOTO bad_exit

:success_exit

exit /B 0

:pending_exit

exit /B 2

:bad_exit
if "-1"=="%success%" GOTO success_exit
echo .
echo ��᪮�쪮 ��⮢ 㯠��
echo ��㤠�.  ��᭠� �����

if ".%1"=="." pause
exit /B 1