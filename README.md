# Rubics [![dotnet-build](https://github.com/SomeProgInThere/Rubics/actions/workflows/dotnet.yml/badge.svg)](https://github.com/SomeProgInThere/Rubics/actions/workflows/dotnet.yml)

### A Compiler for my own language Rubics made with .NET framework
## Status
Currently a work in progress interpreter:
  
  * Expression Evaluation
  * Variable declatations (mutable and unmutable)
  * Expressions and statements with scopes
  * Diagnostics and Errors with line and column numbers
  * Pretty printing with colors in CLI
  * Multiline Repl with commands
  * Viewable Parse trees of each statement

## Structure
The Solution has three projects in it:
  * Rubics (Main)
  * Rubics.CLI (Command-line Interface)
  * Rubics.Test (Tests for compiler)

## Goals
Learning the fundamentals of making a interpreter (and a *actual* compiler later on)

Following the amazing series by [*terrajobst*](https://youtube.com/playlist?list=PLRAdsfhKI4OWNOSfS7EUu5GRAVmze1t2y&si=Uh0fbgiPPq36D50x)
