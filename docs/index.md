# Mustache.NETStandard & Mustache.PCL - Logic-less templates for .NET 

These libraries are implementations of Mustache logic-less templates specifications.

Mustache.NETStandard is a .NETStandard Class Library for .NETStandard 2.0.

Mustache.PCL is a .NET Portable Class Library for 
- .NET Framework 4.6
- ASP.NET Core 1.0
- Windows Universal 10.0

Mustache documentation and specifications can be found at [http://mustache.github.io](http://mustache.github.io).

# Compliance to Mustache specifications
This implementation is compliant to Mustache specifications version 1.2.2 *lambdas* inlcuded (at today 2022.08.20 Mustache specifications version 1.2.2 is the last version), optional modules *inheritance* and *dynamic-names* are not implemented. Specifications are available at [https://github.com/mustache/spec](https://github.com/mustache/spec).

# NuGet
Mustache.NETStandard is availabe using NuGet at [https://www.nuget.org/packages/Mustache.NETStandard](https://www.nuget.org/packages/Mustache.NETStandard).

Mustache.PCL is availabe using NuGet at [https://www.nuget.org/packages/Mustache.PCL](https://www.nuget.org/packages/Mustache.PCL).

# How to use the library

## Basic usage
Here is a code sample to explain how to use this library
```csharp
const string templateString = {% raw %}@"Hello {{Name}}
You have just won {{Value}} {{Currency}}!
{{#InCa}}
Well, {{TaxedValue}} {{Currency}}, after taxes.
{{/InCa}}";{% endraw %}

var data = new
{
    Name = "Chris",
    Value = 10000,
    TaxedValue = 6000,
    Currency = "dollars",
    InCa = true
};

string result = Template.Compile(templateString).Render(data);
```

The value of result is
```
Hello Chris
You have just won 10000 dollars!
Well, 6000 dollars, after taxes.
```

It is possible to use `Dictionary` and and `dynamic` objects:
 
```csharp
var data = new Dictionary<string, object>
{
    {"Name", "Chris"},
    {"Value", "10000"},
    {"TaxedValue", 6000},
    {"Currency", "dollars"},
    {"InCa", true}
};
```

```csharp
var data = new
{
    Name = "Chris",
    Value = 10000,
    TaxedValue = 6000,
    Currency = "dollars",
    InCa = true
};
```

A compiled template can be used many times
```csharp
const string templateString = {% raw %}@"Hello {{Name}}
You have just won {{Value}} {{Currency}}!
{{#InCa}}
Well, {{TaxedValue}} {{Currency}}, after taxes.
{{/InCa}}";{% endraw %}

var dictionary = new Dictionary<string, object>
{
    {"Name", "Chris"},
    {"Value", "10000"},
    {"TaxedValue", 6000},
    {"Currency", "dollars"},
    {"InCa", true}
};

var dynamicObject = new
{
    Name = "Chris",
    Value = 20000,
    TaxedValue = 12000,
    Currency = "dollars",
    InCa = true
};

Template template = Template.Compile(templateString);
string result1 = template.Render(dictionary);
string result2 = template.Render(dynamicObject);
```
`result1` has value
```
Hello Chris
You have just won 10000 dollars!
Well, 6000 dollars, after taxes.
```
`result2` has value
```
Hello Mike
You have just won 20000 dollars!
Well, 12000 dollars, after taxes.
```

## Partials
If you need partials use method Template.Render(object, Dictionary<string, string>), example:
```csharp
var data = new
{
    Name = "Chris",
    Value = 10000,
    TaxedValue = 6000,
    Currency = "dollars",
    InCa = true
};

{% raw %}var partials = new Dictionary<string, string> { { "partial", "Well, {{TaxedValue}} {{Currency}}, after taxes.\r\n" } };{% endraw %}

const string templateString = {% raw %}@"Hello {{Name}}
You have just won {{Value}} {{Currency}}!
{{#InCa}}
{{>partial}}
{{/InCa}}";{% endraw %}

string result = Template.Compile(templateString).Render(data, partials);
```

## Lambdas
Lambdas are supported as methods with a `string` argument ad an `object` return type.
```csharp
Template compiledTemplate = Template.Compile({% raw %}"Hello, {{Lambda}}!"{% endraw %});

var dictionary = new Dictionary<string, object>
{
    {
        "Lambda", (Func<string, object>) (rawText => "world")
    }
};

var classObject = new LambdaObject();

dynamic dynamicObject = new
{
    Lambda = (Func<string, object>) (rawText => "world")
};

string result1 = compiledTemplate.Render(dictionary);

string result2 = compiledTemplate.Render(classObject);

string result3 = compiledTemplate.Render(dynamicObject);
```
`result1`, `result2` and `result3` have the same value
```
Hello, world!
```
`LambdaObject` is defined as
```csharp
public class InterpolationTestLambdaObject
{
    public string Lambda(string rawText)
    {
        return "world";
    }
}
```