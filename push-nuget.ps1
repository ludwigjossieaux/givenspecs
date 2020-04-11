# parameters
$key = '__KEY__'
$version = '0.1.28'

# clean
dotnet clean .\GivenSpecs.sln

# apply version
setversion $version .\GivenSpecs\GivenSpecs.csproj
setversion $version .\GivenSpecs.Application\GivenSpecs.Application.csproj
setversion $version .\GivenSpecs.Application.Tests\GivenSpecs.Application.Tests.csproj
setversion $version .\GivenSpecs.CommandLine\GivenSpecs.CommandLine.csproj
setversion $version .\GivenSpecs.Tests\GivenSpecs.Tests.csproj

# build GivenSpecs

Push-Location

cd "GivenSpecs"
dotnet build --configuration=Release -p:Version=$version
#dotnet pack -c Release
nuget.exe pack -OutputDirectory ./nupkg -IncludeReferencedProjects -Version $version -Prop Configuration=Release

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