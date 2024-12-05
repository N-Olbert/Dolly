<p align="center">
  <a href="https://github.com/AnderssonPeter/Dolly">
    <img src="icon_white.svg" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">Dolly</h3>

  <p align="center">
    Clone .net objects using source generation
    <br />
    <br />
    ·
    <a href="https://github.com/AnderssonPeter/Dolly/issues">Report Bug</a>
    ·
    <a href="https://github.com/AnderssonPeter/Dolly/issues">Request Feature</a>
    ·
  </p>
</p>
<br />

[![NuGet version](https://badge.fury.io/nu/Dolly.svg)](https://www.nuget.org/packages/Dolly)
[![Nuget](https://img.shields.io/nuget/dt/Dolly)](https://www.nuget.org/packages/Dolly)
[![GitHub license](https://img.shields.io/badge/license-Apache%202-blue.svg)](https://raw.githubusercontent.com/AnderssonPeter/Dolly/main/LICENSE)

[![unit tests](https://img.shields.io/github/actions/workflow/status/AnderssonPeter/Dolly/test.yml?branch=main&style=flat-square&label=unit%20tests)](https://github.com/AnderssonPeter/Dolly/actions/workflows/test.yml)
[![Testspace tests](https://img.shields.io/testspace/tests/AnderssonPeter/AnderssonPeter:Dolly/main?style=flat-square)](https://anderssonpeter.testspace.com/spaces/293120/result_sets)
[![Coverage Status](https://img.shields.io/coveralls/github/AnderssonPeter/Dolly?style=flat-square)](https://coveralls.io/github/AnderssonPeter/Dolly)

## Table of Contents
* [About the Project](#about-the-project)
* [Getting Started](#getting-started)
* [Example](#example)

## About The Project
Generate c# code to clone objects on the fly.

## Getting Started
* Add the `Dolly` nuget and add `[Clonable]` attribute to a class and ensure that the class is marked as `partial`.
* Add `[CloneIgnore]` to any property or field that you don't want to include in the clone.
* Call `DeepClone()` or `ShallowClone()` on the object.

### Example
```C#
[Clonable]
public partial class SimpleClass
{
    public string First { get; set; }
    public int Second { get; set; }
    [CloneIgnore]
    public float DontClone { get; set; }
}
```
Should generate
```C#
partial class SimpleClass : IClonable<SimpleClass>
{
    
    object ICloneable.Clone() => this.DeepClone();

    public SimpleClass DeepClone() =>
        new SimpleClass()
        {
            First = First,
            Second = Second
        };

    public SimpleClass ShallowClone() =>
        new SimpleClass()
        {
            First = First,
            Second = Second
        };
}
```
