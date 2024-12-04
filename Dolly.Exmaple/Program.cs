using Dolly.Exmaple;

Console.WriteLine("Test");
var test = new SimpleClass()
{
    First = "First",
    Second = 2,
    DontClone = 3.14f
};

var clone = test.DeepClone();

var test2 = new ComplexClass()
{
    SimpleClass = test
};

Console.WriteLine();