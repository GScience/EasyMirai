## EasyMirai
自动从 xml 协议定义来生成对应的 C# mirai 库。

### 项目结构

#### ProtocolGenerator
生成抽象的 mirai 协议模块并输出伪代码，所有协议定义均在该模块内的 Protocol 目录下的xml文件当中。

#### SourceGenerator.CSharp
从协议模块生成 C# 的代码，支持源生成器功能。

#### EasyMirai
简单的 C# mirai 库。