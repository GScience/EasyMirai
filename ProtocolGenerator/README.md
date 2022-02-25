## ProtocolGenerator
加载 XML 格式的 mirai-http 协议定义，所有协议定义相关文件在Protocol目录下，Common为各个版本之间的通用定义，若新版本对某项内容有更改，请创建对应版本号然后加入更改即可。生成器在加载协议的时候会自动从低版本加载并覆盖到高版本。

目前仅支持 mirai-http 2.5.0 以及更高版本。