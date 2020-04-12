REM G:\UnityEngine\2019.3.0f6\Editor\Unity.exe -projectPath <G:\Vin129P\LCF> -executeMethod <PackageEditor.BatHelloWorld> 
REM -quit -executeMethod <PackageEditor.BatHelloWorld> 
echo start
REM cd /d G:\Vin129P\PMP
REM git checkout master
REM git pull
G:\UnityEngine\2019.3.0f6\Editor\Unity.exe -quit -batchmode -projectPath "G:\Vin129P\PMP\PMP" -executeMethod "AssetBundleBuilder.Build"
echo over
pause