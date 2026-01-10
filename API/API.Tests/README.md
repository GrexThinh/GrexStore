# Unit Testing with Moq - Guide

## What is Moq?

**Moq** is a mocking library for .NET that allows you to create fake (mock) objects for testing. Instead of using real dependencies (like databases, external services, or complex objects), you create mock objects that simulate their behavior.

### Why Use Moq?

1. **Isolation**: Test your code in isolation without real dependencies
2. **Speed**: Mocks are faster than real database calls or API requests
3. **Control**: You can control what the mock returns or how it behaves
4. **Predictability**: Tests don't depend on external factors like database state

## How Moq Works

### 1. Creating a Mock Object

```csharp
// Create a mock of an interface
var mockRepository = new Mock<IProductRepository>();

// Access the actual mocked object using .Object
var mockedRepository = mockRepository.Object;
```

### 2. Setting Up Behavior (Setup)

**Setup** tells the mock what to do when a method is called:

```csharp
// Setup: When GetProductByIdAsync is called with id=1, return a specific product
mockRepository
    .Setup(r => r.GetProductByIdAsync(1))
    .ReturnsAsync(new Product { Id = 1, Name = "Test Product" });

// Setup: When called with id=999, return null
mockRepository
    .Setup(r => r.GetProductByIdAsync(999))
    .ReturnsAsync((Product?)null);
```

### 3. Setting Up Properties

```csharp
var mockUnitOfWork = new Mock<IUnitOfWork>();

// Setup a property to return another mock
mockUnitOfWork.Setup(u => u.Products).Returns(mockRepository.Object);
```

### 4. Verifying Calls (Verify)

**Verify** checks if a method was called and how many times:

```csharp
// Verify a method was called once
mockRepository.Verify(r => r.GetProductByIdAsync(1), Times.Once);

// Verify it was never called
mockRepository.Verify(r => r.DeleteProduct(1), Times.Never);

// Verify it was called at least once
mockRepository.Verify(r => r.GetProductByIdAsync(1), Times.AtLeastOnce);

// Verify it was called exactly 3 times
mockRepository.Verify(r => r.GetProductByIdAsync(1), Times.Exactly(3));
```

### 5. Common Moq Methods

| Method | Purpose | Example |
|--------|---------|---------|
| `Setup()` | Define what happens when method is called | `mock.Setup(x => x.Get()).Returns(value)` |
| `Returns()` | Return a value synchronously | `mock.Setup(x => x.Get()).Returns(5)` |
| `ReturnsAsync()` | Return a value asynchronously | `mock.Setup(x => x.GetAsync()).ReturnsAsync(5)` |
| `Throws()` | Throw an exception | `mock.Setup(x => x.Get()).Throws<Exception>()` |
| `Verify()` | Check if method was called | `mock.Verify(x => x.Get(), Times.Once)` |
| `.Object` | Get the actual mocked object | `var obj = mock.Object` |

## Example: Testing a Controller

### Step 1: Create Mocks

```csharp
var mockUnitOfWork = new Mock<IUnitOfWork>();
var mockProductRepository = new Mock<IProductRepository>();

// Configure UnitOfWork to return the mocked repository
mockUnitOfWork.Setup(u => u.Products).Returns(mockProductRepository.Object);
```

### Step 2: Setup Expected Behavior

```csharp
var expectedProduct = new Product 
{ 
    Id = 1, 
    Name = "Test Product",
    Price = 100 
};

// When GetProductByIdAsync(1) is called, return expectedProduct
mockProductRepository
    .Setup(r => r.GetProductByIdAsync(1))
    .ReturnsAsync(expectedProduct);
```

### Step 3: Create Controller with Mock

```csharp
var controller = new ProductsController(mockUnitOfWork.Object);
```

### Step 4: Execute and Assert

```csharp
// Act: Call the controller method
var result = await controller.GetProduct(1);

// Assert: Check the result
var actionResult = Assert.IsType<ActionResult<Product>>(result);
var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
var product = Assert.IsType<Product>(okResult.Value);

Assert.Equal(1, product.Id);
Assert.Equal("Test Product", product.Name);

// Optional: Verify the repository was called
mockProductRepository.Verify(r => r.GetProductByIdAsync(1), Times.Once);
```

## Complete Example Breakdown

```csharp
[Fact]
public async Task GetProduct_WithValidId_ReturnsProduct()
{
    // ARRANGE - Set up test data and mocks
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockProductRepository = new Mock<IProductRepository>();
    
    mockUnitOfWork.Setup(u => u.Products).Returns(mockProductRepository.Object);
    
    var expectedProduct = new Product 
    { 
        Id = 1, 
        Name = "Test Product" 
    };
    
    // Tell mock: "When GetProductByIdAsync(1) is called, return expectedProduct"
    mockProductRepository
        .Setup(r => r.GetProductByIdAsync(1))
        .ReturnsAsync(expectedProduct);
    
    var controller = new ProductsController(mockUnitOfWork.Object);
    
    // ACT - Execute the code being tested
    var result = await controller.GetProduct(1);
    
    // ASSERT - Verify the result
    var actionResult = Assert.IsType<ActionResult<Product>>(result);
    var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
    var product = Assert.IsType<Product>(okResult.Value);
    
    Assert.Equal(1, product.Id);
    Assert.Equal("Test Product", product.Name);
    
    // VERIFY - Check that the mock was called as expected
    mockProductRepository.Verify(r => r.GetProductByIdAsync(1), Times.Once);
}
```

## Advanced Moq Features

### 1. Setting Up with Parameters (It.IsAny)

```csharp
// Return the same value regardless of the parameter
mockRepository
    .Setup(r => r.GetProductByIdAsync(It.IsAny<int>()))
    .ReturnsAsync(new Product { Id = 1, Name = "Any Product" });

// Return different values based on parameter
mockRepository
    .Setup(r => r.GetProductByIdAsync(1))
    .ReturnsAsync(new Product { Id = 1 });
    
mockRepository
    .Setup(r => r.GetProductByIdAsync(2))
    .ReturnsAsync(new Product { Id = 2 });
```

### 2. Setting Up with Conditions (It.Is)

```csharp
// Return value only if parameter meets condition
mockRepository
    .Setup(r => r.GetProductByIdAsync(It.Is<int>(id => id > 0)))
    .ReturnsAsync(new Product { Id = 1 });
```

### 3. Throwing Exceptions

```csharp
// Make the mock throw an exception
mockRepository
    .Setup(r => r.GetProductByIdAsync(1))
    .ThrowsAsync(new InvalidOperationException("Product not found"));
```

### 4. Callbacks

```csharp
int callCount = 0;

mockRepository
    .Setup(r => r.GetProductByIdAsync(It.IsAny<int>()))
    .ReturnsAsync(new Product())
    .Callback<int>(id => callCount++); // Increment counter each time it's called
```

## How to Run Tests

### Option 1: Using .NET CLI (Command Line)

```bash
# Navigate to the solution/project root
cd C:\Users\thinhnv\study\GrexStore\API

# Run all tests in the test project
dotnet test API.Tests/API.Tests.csproj

# Run tests with verbose output
dotnet test API.Tests/API.Tests.csproj --verbosity normal

# Run a specific test by name
dotnet test API.Tests/API.Tests.csproj --filter "FullyQualifiedName~GetProduct_WithValidId_ReturnsProduct"

# Run tests and show code coverage
dotnet test API.Tests/API.Tests.csproj /p:CollectCoverage=true
```

### Option 2: Using Visual Studio

1. Open **Test Explorer**: `Test` → `Test Explorer` (or `Ctrl+E, T`)
2. Build the solution: `Build` → `Build Solution` (or `Ctrl+Shift+B`)
3. Run all tests: Click "Run All" in Test Explorer
4. Run a specific test: Right-click on the test → `Run Selected Tests`

### Option 3: Using Visual Studio Code

1. Install the **.NET Core Test Explorer** extension
2. Open the Test Explorer panel
3. Click the play button next to tests to run them

### Option 4: Using Rider/VS Code with Test Explorer

1. Tests will appear in the Test Explorer automatically
2. Click the green play button to run tests
3. See results and coverage inline

## Common Test Patterns

### Pattern 1: Testing Success Cases

```csharp
[Fact]
public async Task GetProduct_WithValidId_ReturnsProduct()
{
    // Arrange - setup mocks and expected data
    // Act - call the method
    // Assert - verify the result is correct
}
```

### Pattern 2: Testing Failure Cases

```csharp
[Fact]
public async Task GetProduct_WithInvalidId_ReturnsNotFound()
{
    // Arrange - setup mock to return null
    // Act - call the method
    // Assert - verify NotFound is returned
}
```

### Pattern 3: Testing with Multiple Setup Calls

```csharp
[Fact]
public async Task GetProducts_CallsRepository_ReturnsPagedList()
{
    // Arrange - setup multiple methods
    mockRepository.Setup(r => r.GetProductsQueryAsync(It.IsAny<ProductParams>()))
                  .ReturnsAsync(products.AsQueryable());
    
    // Act
    var result = await controller.GetProducts(params);
    
    // Assert - verify the result
    // Verify - check methods were called correctly
    mockRepository.Verify(r => r.GetProductsQueryAsync(It.IsAny<ProductParams>()), Times.Once);
}
```

## Tips and Best Practices

1. **One Test = One Behavior**: Each test should verify one specific behavior
2. **AAA Pattern**: Always follow Arrange-Act-Assert structure
3. **Descriptive Names**: Test names should clearly describe what they test
   - Format: `MethodName_Scenario_ExpectedResult`
4. **Clean Setup**: Use constructor or setup methods for common mock configurations
5. **Verify Important Calls**: Use `Verify()` to ensure mocks were called as expected
6. **Test Both Success and Failure**: Test both happy paths and error scenarios

## Troubleshooting

### Issue: "The type or namespace name 'Xunit' could not be found"

**Solution**: The `xunit` package is installed and should be globally imported. Make sure:
- The project file has `<Using Include="Xunit" />`
- Run `dotnet restore` to restore packages

### Issue: Tests not appearing in Test Explorer

**Solution**:
1. Rebuild the solution
2. Close and reopen Test Explorer
3. Check that test methods are marked with `[Fact]` or `[Theory]` attributes

### Issue: Mock not working as expected

**Solution**:
- Make sure you're using `.Object` to get the actual mocked instance
- Check that you've called `Setup()` before using the mock
- Verify the method signature matches exactly

## Next Steps

- Add more test cases for edge cases
- Add integration tests that use real database (separate test project)
- Set up code coverage reporting
- Add test categories and run specific test suites

