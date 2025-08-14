<h1>Source Generators</h1>

<p>Attributes required by source generators are provided through the <code>Bluehill.Analyzers.Attributes</code> package. To install this package, add the following to csproj:</p>

```xml
<ItemGroup>
  <PackageReference Include="Bluehill.Analyzers.Attributes" Version="<version>" PrivateAssets="all" ExcludeAssets="runtime" />
</ItemGroup>
```

<p>There is no reason to include the Bluehill.Analyzers.Attributes assembly in the output directory because the attributes are removed at compile time.</p>

<ul>
    <li><a href="EnumExtensionsGenerator">EnumExtensionsGenerator</a></li>
</ul>
