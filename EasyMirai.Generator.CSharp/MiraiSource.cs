using EasyMirai.Generator;
using EasyMirai.Generator.CSharp.Generator;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp
{
    public class MiraiSource
    {
        public static string RootNamespace { get; private set; }

        public Dictionary<string, string> SourceCodeDict
            = new Dictionary<string, string>();

        public MiraiSource(MiraiModule module, string namespaceDef)
        {
            RootNamespace = namespaceDef;

            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryObject] = new ObjectGenerator();
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryMessage] = new MessageGenerator();
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryApi] = new ApiGenerator();
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryIMessage] = new IMessageGenerator();
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryAdapterWs] = new WsAdapterGenerator();
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryAdapterHttp] = new HttpAdapterGenerator();
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryEvent] = new EventGenerator();
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategorySerializer] = new SerializeGenerator();

            foreach (var generator in GeneratorBase.SourceGeneratorTable.Values)
                generator.Init();

            foreach (var classDef in module.Classes)
                if (GeneratorBase.SourceGeneratorTable.TryGetValue(classDef.Category, out ISourceGenerator generator))
                    generator.PreProcessing(classDef);

            foreach (var classDef in module.Classes)
                if (GeneratorBase.SourceGeneratorTable.TryGetValue(classDef.Category, out ISourceGenerator generator))
                {
                    var source = generator.GenerateFrom(classDef, namespaceDef);
                    var classsPath = generator.GetClassDir(classDef);
                    var fileName = GetOutputFileName(classDef.Name, classsPath);
                    SourceCodeDict[fileName] = source;
                }

            foreach (var generator in GeneratorBase.SourceGeneratorTable.Values)
                generator.PostProcessing(SourceCodeDict);

            GeneratorBase.SourceGeneratorTable.Clear();
        }

        public static string GetOutputFileName(string className, string classPath = "")
        {
            return string.IsNullOrEmpty(classPath) ?
                $"{className}.g.cs" :
                $"{classPath}.{className}.g.cs";
        }
    }
}
