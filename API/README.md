# GrexStore API

This folder contains the API project and its test project.

## Solution Structure

The solution file `GrexStore.sln` is located in this folder and includes:

- **API** - Main ASP.NET Core Web API project
- **API.Tests** - Unit test project using xUnit and Moq

## How to Run

### Build the Solution

```bash
cd API
dotnet build GrexStore.sln
```

### Run the API

```bash
cd API
dotnet run --project API.csproj
```

Or in Visual Studio/VS Code, just press F5 with the API project set as startup project.

### Run Tests

```bash
cd API
dotnet test GrexStore.sln
```

Or run tests for a specific project:

```bash
dotnet test API.Tests/API.Tests.csproj
```

### Run Tests with Verbose Output

```bash
dotnet test GrexStore.sln --verbosity normal
```

### Run Specific Test

```bash
dotnet test GrexStore.sln --filter "FullyQualifiedName~GetProduct_WithValidId"
```

## Project Structure

```
API/
├── GrexStore.sln              # Solution file (contains both projects)
├── API.csproj                 # Main API project
├── API.Tests/
│   ├── API.Tests.csproj      # Test project
│   └── Controllers/          # Test files
└── [Other API folders...]
```

## Notes

- The solution is located inside the `API` folder for better organization
- Both API and API.Tests projects are included in the solution
- You can build and test everything from the solution level
