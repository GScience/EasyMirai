using ProtocolGenerator;
using SourceGenerator.CSharp.Generator;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerator.CSharp
{
    public class MiraiSource
    {
        public Dictionary<string, string> SourceCodeDict
            = new Dictionary<string, string>();

        public MiraiSource(MiraiModule module, string namespaceDef)
        {
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryObject] = new ObjectGenerator();
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryMessage] = new MessageGenerator();
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryApi] = new ApiGenerator();
            GeneratorBase.SourceGeneratorTable[MiraiModule.CategoryIMessage] = new IMessageGenerator();

            foreach (var classDef in module.Classes)
                if (GeneratorBase.SourceGeneratorTable.TryGetValue(classDef.Category, out ISourceGenerator generator))
                    generator.PreProcessing(classDef);

            foreach (var classDef in module.Classes)
                if (GeneratorBase.SourceGeneratorTable.TryGetValue(classDef.Category, out ISourceGenerator generator))
                {
                    var source = generator.GenerateFrom(classDef, namespaceDef);
                    var classsPath = generator.GetClassDir(classDef);
                    var fileName = string.IsNullOrEmpty(classsPath) ?
                        $"{classDef.Name}.g.cs" :
                        $"{classsPath}.{classDef.Name}.g.cs";
                    SourceCodeDict[fileName] = source;
                }
        }
    }
}
