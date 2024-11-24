using Dolly.Exmaple;

Console.WriteLine("Test");
var test = new SimpleClass()
{
    First = "First",
    Second = 2
};

var clone = test.Clone();

var test2 = new ComplexClass()
{
    SimpleClass = test
};

Console.WriteLine();