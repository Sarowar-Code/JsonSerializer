# JsonSerializer

This is a small C# project that implements a JSON serializer and deserializer from scratch.
It is mainly for learning how JSON works internally.

## How it works

When reading JSON, the data moves through a few simple steps:
JSON text -> lexer -> tokens -> parser -> JSON value -> C# object

When writing JSON, the direction is reversed:
C# object -> serializer -> JSON text

The lexer reads the input character by character. The parser turns the tokens into objects, arrays, strings, numbers, booleans, and null values. The serializer and deserializer handle the conversion between these values and basic C# objects.

## Run the project

From the repository folder, run:

-> dotnet build
-> dotnet run --project tests/JsonSerializer.Tests

The test project is currently a small console application. When everything works, it prints:
All tests passed.

## Current support

- Objects and arrays
- Strings and basic escaping
- Integers and floating-point numbers
- Boolean values
- Null values
- Basic C# classes
- Simple lists and arrays
