# Sprig [![dotnet-build](https://github.com/SomeProgInThere/Rubics/actions/workflows/dotnet.yml/badge.svg)](https://github.com/SomeProgInThere/Rubics/actions/workflows/dotnet.yml)

### A Compiler for my own language Sprig made with .NET framework
## Status
Currently a work in progress interpreter:
  
  ### Implemented Features:
  * Variable declatations (mutable and unmutable)
  * Basic Casting
  * Basic Types like int, bool and string
  * Expressions and statements (if-else, while, for, etc.) with scopes
  * Builtin functions (print, input, rand)
  * Proper Diagnostics and Errors with line and column numbers
  
  ### REPL Features:

  * Pretty printing with rendered colors in REPL
  * Fully editable document-like REPL environment
  * Viewable History in REPL
  * Viewable Parse and binding trees of each statement

## Structure
The Solution has three projects in it:
  * Sprig (Main)
  * Sprig.CLI (Command-line Interface)
  * Sprig.Test (Tests for compiler)

## Goals
Learning the fundamentals of making a interpreter (and a *actual* compiler later on)

## Build

To build the project, make sure to have .NET SDK installed (project uses net 9.0). 

The project can be opened in Visual Studio or build via dotnet CLI.

Build in release mode:

      dotnet build --configuration Release

Run tests:

      dotnet test

Following the amazing series by [*terrajobst*](https://youtube.com/playlist?list=PLRAdsfhKI4OWNOSfS7EUu5GRAVmze1t2y&si=Uh0fbgiPPq36D50x)
