SET output_dir=edb_dotnet_nugetpackages

dotnet pack -c Release -p:IncludeSymbols=false edb-dotnet-v9.sln -o %output_dir%
dotnet pack -c Release -p:IncludeSymbols=false EF.core\EFCore.PG.sln -o %output_dir%
pause

