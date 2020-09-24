param($projectName)

Get-ChildItem -Path .\..\..\Libraries\$projectName\bin\Release\ -Include *.* -File -Recurse | foreach { try { $_.Delete() } catch { } }
dotnet pack .\..\..\Libraries\$projectName\$projectName.csproj -c Release --include-symbols --include-source -p:SymbolPackageFormat=snupkg
nuget push .\..\..\Libraries\$projectName\bin\Release\*.nupkg -Source https://api.nuget.org/v3/index.json -SkipDuplicate
