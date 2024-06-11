# a

``` bash
dotnet new sln
dotnet new console --output folder1 --langVersion 7.3
dotnet sln add folder1
```

<TargetFramework>net7.0</TargetFramework>
to
<TargetFramework>**net481**</TargetFramework>

Por fim, para você compilar programas compatíveis com o Clarion, é necessário referenciar a DLL do ClarionLibrary.dll.
Para isso, crie um diretório lib e coloque a biblioteca ClarionLibrary.dll lá. Para referenciá-la no projeto, você
precisa incluir o seguinte no seu arquivo .csproj

``` bash
<ItemGroup>
<Reference Include="CLARIONLibrary">
<HintPath>..\lib\ClarionLibrary.dll</HintPath>
</Reference>
</ItemGroup>
```

msbuild
mono *.exe