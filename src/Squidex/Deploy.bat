SET configuration=Release

REM Restore all Packages
dotnet restore

REM publish packages
dotnet publish -c %configuration%

REM Deploy the Publish Folder to IIS
"C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe" ^
	-verb:sync -source:contentPath="%~dp0bin\%configuration%\netcoreapp1.1\publish" ^
	-enableRule:AppOffline ^
	-allowUntrusted:true ^
	-dest:contentPath=squidex,ComputerName=https://5.175.5.234:8172/msdeploy.axd?site=squidex,UserName=DeploymentUser,Password=1q2w3e$R,AuthType='Basic ^