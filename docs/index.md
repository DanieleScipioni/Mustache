# Mustache.NETStandard & Mustache.PCL - Logic-less templates for .NET 

These libraries are implementations of Mustache logic-less templates specifications.

Mustache.NETStandard is a .NETStandard Class Library for .NETStandard 2.0.

Mustache.PCL is a .NET Portable Class Library for 
- .NET Framework 4.6
- ASP.NET Core 1.0
- Windows Universal 10.0

Mustache documentation and specifications can be found at [http://mustache.github.com](http://mustache.github.com).

# Compliance to Mustache specifications
This implementation is compliant to version 1.1.3 (currently this is the last version) of Mustache specifications except for the optional module *lambdas*. Specifications are available at [https://github.com/mustache/spec](https://github.com/mustache/spec).

# NuGet
Mustache.NETStandard is availabe using NuGet at [https://www.nuget.org/packages/Mustache.NETStandard](https://www.nuget.org/packages/Mustache.NETStandard).

Mustache.PCL is availabe using NuGet at [https://www.nuget.org/packages/Mustache.PCL](https://www.nuget.org/packages/Mustache.PCL).

# How to use the library
Here is a code sample to explain how to use this library
    
    {% raw %}
    const string templateString = @"Hello {{Name}}
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

The value of result is
    
    Hello Chris
    You have just won 10000 dollars!
    Well, 6000 dollars, after taxes.
    
It is possible to use Dictionaries too:

    var data = new Dictionary<string, object>
    {
        {"Name", "Chris"},
        {"Value", "10000"},
        {"TaxedValue", 6000},
        {"Currency", "dollars"},
        {"InCa", true}
    };
    
If you need partials use method Template.Render(object, Dictionary<string, string>), example:

    var data = new
    {
        Name = "Chris",
        Value = 10000,
        TaxedValue = 6000,
        Currency = "dollars",
        InCa = true
    };

    {% raw %}var partials = new Dictionary<string, string> { { "partial", "Well, {{TaxedValue}} {{Currency}}, after taxes.\r\n" } };

    const string templateString = @"Hello {{Name}}
    You have just won {{Value}} {{Currency}}!
    {{#InCa}}
    {{>partial}}
    {{/InCa}}
    ";{% endraw %}

     string result = Template.Compile(templateString).Render(data, partials);
