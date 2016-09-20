REM Create a 'GeneratedReports' folder if it does not exist
if not exist "%~dp0GeneratedReports" mkdir "%~dp0GeneratedReports"
 
REM Remove any previously created test output directories
CD %~dp0
FOR /D /R %%X IN (%USERNAME%*) DO RD /S /Q "%%X"
 
REM Run the tests against the targeted output
call :GenerateCoverage

REM Generate the report output based on the test results
if %errorlevel% equ 0 (
 call :RunReportGeneratorOutput
)

exit /b %errorlevel%

:GenerateCoverage
"%UserProfile%\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" ^
-register:user ^
-target:"C:\Program Files\dotnet\dotnet.exe" ^
-targetargs:"test %~dp0\pinkparrot_infrastructure\PinkParrot.Infrastructure.Tests" ^
-filter:"+[PinkParrot*]*" ^
-skipautoprops ^
-output:"%~dp0\GeneratedReports\Infrastructure.xml" ^
-oldStyle

"%UserProfile%\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" ^
-register:user ^
-target:"C:\Program Files\dotnet\dotnet.exe" ^
-targetargs:"test %~dp0\pinkparrot_write\PinkParrot.Write.Tests" ^
-filter:"+[PinkParrot*]*" ^
-skipautoprops ^
-output:"%~dp0\GeneratedReports\Write.xml" ^
-oldStyle
exit /b %errorlevel%

:RunReportGeneratorOutput
"%UserProfile%\.nuget\packages\ReportGenerator\2.4.5\tools\ReportGenerator.exe" ^
-reports:"%~dp0\GeneratedReports\*.xml" ^
-targetdir:"%~dp0\GeneratedReports\Output"
exit /b %errorlevel%