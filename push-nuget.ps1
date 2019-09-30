# parameters
$key = '__KEY__'
$version = '0.1.23'

# apply version

setversion -r $version

# build GivenSpecs

Push-Location

cd "GivenSpecs"
dotnet build --configuration=Release -p:Version=$version
nuget.exe pack -OutputDirectory ./nupkg -IncludeReferencedProjects -Version $version

Pop-Location

# build GivenSpecs.CommandLine

Push-Location

cd "GivenSpecs.CommandLine"
dotnet build --configuration=Release -p:Version=$version
dotnet pack -c Release
#nuget.exe pack -OutputDirectory ./nupkg -IncludeReferencedProjects -Version $version

Pop-Location

# push GivenSpecs

Push-Location

cd "GivenSpecs/nupkg"
dotnet nuget push GivenSpecs.$version.nupkg -k $key -s https://api.nuget.org/v3/index.json

Pop-Location

# push GivenSpecs.CommandLine

Push-Location

cd "GivenSpecs.CommandLine/nupkg"
dotnet nuget push GivenSpecs.CommandLine.$version.nupkg -k $key -s https://api.nuget.org/v3/index.json

Pop-Location