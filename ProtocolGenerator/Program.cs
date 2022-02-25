// See https://aka.ms/new-console-template for more information
using ProtocolGenerator;
using System;

static class Program
{

    static void Main(string[] args)
    {
        var protocol = new MiraiProtocol();
        var module = new MiraiModule(protocol);

        foreach (var classDef in module.Classes)
            Console.WriteLine(classDef.ToString());
    }
}