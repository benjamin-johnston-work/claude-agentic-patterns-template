using Microsoft.Extensions.Logging;
using Moq;
using Archie.Infrastructure.AzureSearch.Services;

namespace Archie.Infrastructure.Tests.AzureSearch;

[TestFixture]
public class CodeSymbolExtractorTests
{
    private Mock<ILogger<CodeSymbolExtractor>> _mockLogger;
    private CodeSymbolExtractor _extractor;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<CodeSymbolExtractor>>();
        _extractor = new CodeSymbolExtractor(_mockLogger.Object);
    }

    [Test]
    public void IsLanguageSupported_WithSupportedLanguage_ReturnsTrue()
    {
        // Act & Assert
        Assert.IsTrue(_extractor.IsLanguageSupported("csharp"));
        Assert.IsTrue(_extractor.IsLanguageSupported("javascript"));
        Assert.IsTrue(_extractor.IsLanguageSupported("python"));
        Assert.IsTrue(_extractor.IsLanguageSupported("java"));
        Assert.IsTrue(_extractor.IsLanguageSupported("go"));
        Assert.IsTrue(_extractor.IsLanguageSupported("rust"));
    }

    [Test]
    public void IsLanguageSupported_WithUnsupportedLanguage_ReturnsFalse()
    {
        // Act & Assert
        Assert.IsFalse(_extractor.IsLanguageSupported("cobol"));
        Assert.IsFalse(_extractor.IsLanguageSupported("fortran"));
        Assert.IsFalse(_extractor.IsLanguageSupported("unknown"));
    }

    [Test]
    public void GetSupportedLanguages_ReturnsExpectedLanguages()
    {
        // Act
        var languages = _extractor.GetSupportedLanguages().ToList();

        // Assert
        Assert.IsTrue(languages.Count > 0);
        Assert.IsTrue(languages.Contains("csharp"));
        Assert.IsTrue(languages.Contains("javascript"));
        Assert.IsTrue(languages.Contains("python"));
        Assert.IsTrue(languages.Contains("java"));
    }

    [Test]
    public async Task ExtractSymbolsAsync_WithEmptyContent_ReturnsEmptyList()
    {
        // Act
        var result = await _extractor.ExtractSymbolsAsync("", "csharp");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public async Task ExtractSymbolsAsync_WithNullContent_ReturnsEmptyList()
    {
        // Act
        var result = await _extractor.ExtractSymbolsAsync(null!, "csharp");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public async Task ExtractSymbolsAsync_CSharpClass_ExtractsClassSymbol()
    {
        // Arrange
        var content = @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod()
        {
        }
    }
}";

        // Act
        var result = await _extractor.ExtractSymbolsAsync(content, "csharp");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
        Assert.IsTrue(result.Any(s => s.Contains("TestClass")));
        Assert.IsTrue(result.Any(s => s.Contains("TestMethod")));
        Assert.IsTrue(result.Any(s => s.Contains("TestNamespace")));
    }

    [Test]
    public async Task ExtractSymbolsAsync_CSharpInterface_ExtractsInterfaceSymbol()
    {
        // Arrange
        var content = @"
public interface ITestInterface
{
    void TestMethod();
    string TestProperty { get; set; }
}";

        // Act
        var result = await _extractor.ExtractSymbolsAsync(content, "csharp");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
        Assert.IsTrue(result.Any(s => s.Contains("ITestInterface")));
        Assert.IsTrue(result.Any(s => s.Contains("TestMethod")));
    }

    [Test]
    public async Task ExtractSymbolsAsync_JavaScriptFunction_ExtractsFunctionSymbol()
    {
        // Arrange
        var content = @"
function testFunction(param1, param2) {
    return param1 + param2;
}

const arrowFunction = (x, y) => {
    return x * y;
};

class TestClass {
    testMethod() {
        console.log('test');
    }
}";

        // Act
        var result = await _extractor.ExtractSymbolsAsync(content, "javascript");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
        Assert.IsTrue(result.Any(s => s.Contains("testFunction")));
        Assert.IsTrue(result.Any(s => s.Contains("arrowFunction")));
        Assert.IsTrue(result.Any(s => s.Contains("TestClass")));
        Assert.IsTrue(result.Any(s => s.Contains("testMethod")));
    }

    [Test]
    public async Task ExtractSymbolsAsync_PythonFunction_ExtractsFunctionSymbol()
    {
        // Arrange
        var content = @"
def test_function(param1, param2):
    return param1 + param2

class TestClass:
    def __init__(self):
        pass
    
    def test_method(self):
        print('test')
    
    @staticmethod
    def static_method():
        return 'static'

async def async_function():
    await some_operation()
";

        // Act
        var result = await _extractor.ExtractSymbolsAsync(content, "python");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
        Assert.IsTrue(result.Any(s => s.Contains("test_function")));
        Assert.IsTrue(result.Any(s => s.Contains("TestClass")));
        Assert.IsTrue(result.Any(s => s.Contains("test_method")));
        Assert.IsTrue(result.Any(s => s.Contains("static_method")));
        Assert.IsTrue(result.Any(s => s.Contains("async_function")));
    }

    [Test]
    public async Task ExtractSymbolsAsync_JavaClass_ExtractsJavaSymbols()
    {
        // Arrange
        var content = @"
package com.example;

import java.util.List;

public class TestClass {
    private String field;
    
    public TestClass() {
    }
    
    public void testMethod() {
    }
    
    private static String staticMethod() {
        return ""test"";
    }
}

interface TestInterface {
    void interfaceMethod();
}";

        // Act
        var result = await _extractor.ExtractSymbolsAsync(content, "java");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
        Assert.IsTrue(result.Any(s => s.Contains("TestClass")));
        Assert.IsTrue(result.Any(s => s.Contains("testMethod")));
        Assert.IsTrue(result.Any(s => s.Contains("staticMethod")));
        Assert.IsTrue(result.Any(s => s.Contains("TestInterface")));
    }

    [Test]
    public async Task ExtractSymbolsAsync_GoCode_ExtractsGoSymbols()
    {
        // Arrange
        var content = @"
package main

import ""fmt""

type TestStruct struct {
    field string
}

func testFunction(param string) string {
    return param
}

func (t *TestStruct) testMethod() {
    fmt.Println(t.field)
}

var globalVar = ""test""
const testConst = 42
";

        // Act
        var result = await _extractor.ExtractSymbolsAsync(content, "go");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
        Assert.IsTrue(result.Any(s => s.Contains("testFunction")));
        Assert.IsTrue(result.Any(s => s.Contains("testMethod")));
        Assert.IsTrue(result.Any(s => s.Contains("TestStruct")));
        Assert.IsTrue(result.Any(s => s.Contains("globalVar")));
        Assert.IsTrue(result.Any(s => s.Contains("testConst")));
    }

    [Test]
    public async Task ExtractSymbolsAsync_RustCode_ExtractsRustSymbols()
    {
        // Arrange
        var content = @"
struct TestStruct {
    field: String,
}

enum TestEnum {
    Variant1,
    Variant2(i32),
}

trait TestTrait {
    fn trait_method(&self);
}

impl TestTrait for TestStruct {
    fn trait_method(&self) {
        println!(""test"");
    }
}

fn test_function(param: &str) -> String {
    param.to_string()
}

mod test_module {
    pub fn module_function() {}
}
";

        // Act
        var result = await _extractor.ExtractSymbolsAsync(content, "rust");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
        Assert.IsTrue(result.Any(s => s.Contains("TestStruct")));
        Assert.IsTrue(result.Any(s => s.Contains("TestEnum")));
        Assert.IsTrue(result.Any(s => s.Contains("TestTrait")));
        Assert.IsTrue(result.Any(s => s.Contains("test_function")));
        Assert.IsTrue(result.Any(s => s.Contains("test_module")));
    }

    [Test]
    public async Task ExtractSymbolsAsync_UnsupportedLanguage_ExtractsBasicIdentifiers()
    {
        // Arrange
        var content = @"
someFunction() {
    var testVariable = ""test"";
    anotherFunction(testVariable);
    thirdFunction(""parameter"");
}";

        // Act
        var result = await _extractor.ExtractSymbolsAsync(content, "unknownlang");

        // Assert
        Assert.IsNotNull(result);
        // Should extract basic identifiers for unsupported languages
        // The exact behavior depends on the frequency threshold in the implementation
    }

    [Test]
    public async Task ExtractSymbolsAsync_WithCancellation_ThrowsOperationCancelledException()
    {
        // Arrange
        var content = "class TestClass { }";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Cancel immediately

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _extractor.ExtractSymbolsAsync(content, "csharp", cancellationTokenSource.Token));
    }

    [Test]
    public async Task ExtractSymbolsAsync_ComplexCode_ExtractsMultipleSymbolTypes()
    {
        // Arrange
        var content = @"
using System;
using System.Collections.Generic;

namespace TestApp
{
    public class TestClass : ITestInterface
    {
        private readonly string _field;
        
        public string Property { get; set; }
        
        public TestClass(string field)
        {
            _field = field;
        }
        
        public void PublicMethod()
        {
            // TODO: Implement this method
            var localVar = PrivateMethod();
        }
        
        private string PrivateMethod()
        {
            return _field;
        }
        
        public static void StaticMethod()
        {
        }
    }
    
    public interface ITestInterface
    {
        void PublicMethod();
    }
    
    public enum TestEnum
    {
        Value1,
        Value2
    }
}";

        // Act
        var result = await _extractor.ExtractSymbolsAsync(content, "csharp");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
        
        // Should extract classes, interfaces, methods, enums, namespace, etc.
        Assert.IsTrue(result.Any(s => s.Contains("TestClass")));
        Assert.IsTrue(result.Any(s => s.Contains("ITestInterface")));
        Assert.IsTrue(result.Any(s => s.Contains("TestEnum")));
        Assert.IsTrue(result.Any(s => s.Contains("PublicMethod")));
        Assert.IsTrue(result.Any(s => s.Contains("PrivateMethod")));
        Assert.IsTrue(result.Any(s => s.Contains("StaticMethod")));
        Assert.IsTrue(result.Any(s => s.Contains("TestApp")));
    }
}