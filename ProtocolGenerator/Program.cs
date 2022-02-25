// See https://aka.ms/new-console-template for more information
using ProtocolGenerator;

Console.WriteLine("Hello, World!");

var protocol = new MiraiProtocol();
var module = new MiraiModule(protocol);

foreach (var classDef in module.Classes)
{
    Console.WriteLine(classDef.ToString());
}