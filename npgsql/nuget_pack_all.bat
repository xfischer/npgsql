SET output_dir=edb_dotnet_nugetpackages

dotnet build EF.core\EFCore.PG.slnx -c Release
dotnet build edb-dotnet.slnx -c Release
dotnet pack -c Release -p:IncludeSymbols=false edb-dotnet.slnx -o %output_dir%
dotnet pack -c Release -p:IncludeSymbols=false EF.core\EFCore.PG.slnx -o %output_dir%
pause

