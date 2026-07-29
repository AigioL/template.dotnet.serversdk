# AigioLTemplate.ServerSdk
Client(Browser Wasm)/Server 应用程序架构中的 ServerSdk  
1. 在客户端与服务端中共享 C# 代码
2. 提供共享常量、模型、枚举类型定义
3. 提供 HTTP WebApi 接口调用函数

## 项目模板重命名
在项目根目录(slnx 文件所在文件夹)下执行以下命令
1. 编译工具 ```dotnet build src\AigioLTemplate.ServerSdk.BuildTools -c Debug```
2. 重命名文件 ```src\artifacts\bin\AigioLTemplate.ServerSdk.BuildTools\debug\b.exe rename --projName 新项目英文名称```