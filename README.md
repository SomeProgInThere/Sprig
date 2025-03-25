# Sprig [![dotnet-build](https://github.com/SomeProgInThere/Sprig/actions/workflows/dotnet.yml/badge.svg)](https://github.com/SomeProgInThere/Sprig/actions/workflows/dotnet.yml)

> [!NOTE]
> This project only supports windows based platform.

### Sprig: Overview
An Ahead-Of-Time compiler for the Sprig programming language, built with .NET framework.

## Features
- Syntax and grammer inspired by go and python
- Multiple files are supported
- Static types: int, decimal, string, bool and any
- [Roslyn](https://github.com/dotnet/roslyn) based backend with IL emission
- Twig: CLI for building with output support for IR-trees, parse-trees and symbols
- Control flow vizualization for functions using [Graphviz](https://graphviz.org/)

## Code Example
```go
func factorial(num: int): int {
    var fact = 1

    for i in 1..num {
        fact = fact * i
    }

    return fact
}

func ncr(n: int, r: int): int {
    if r > n {
        print("Invalid input!")
        return -1
    }
    
    return factorial(n) / (factorial(r) * factorial(n - r))
}

func main() {
    print("Enter n and r: ")
    let n = int(input())
    let r = int(input())
    print("nCr: " + ncr(n , r))
}
```

## Build Guide

> [!IMPORTANT]
> Solution uses .NET 9.0 as of writing 

* Solution can be opened with Visual Studio 2022 (with .NET desktop development)
* Build using dotnet cli:

```bash
# Clone or download the repository
git clone https://github.com/SomeProgInThere/Sprig.git

# Build the project (in release)
dotnet build --configuration Release

# Run twig with given example
cd .\Twig
dotnet run build ..\samples\main.sg

# (optional) use clean script to clean the generated outputs
cd ..
.\clean.ps1
```

## Project Structure
```
Sprig
├── Sprig           # Sprig Compiler
├── Twig            # CLI App for Sprig
└── Samples         # Examples
```

## Status 
- Adding support for lists and associated functions
- Adding Switch statements

## Credits
Special thanks to [terrajobst](https://youtube.com/playlist?list=PLRAdsfhKI4OWNOSfS7EUu5GRAVmze1t2y) for making an amazing compiler series
