namespace AigioLTemplate.UnitTest;

sealed partial class Program
{
    /// <summary>
    /// 伪入口点，在测试项目中实现类似控制台程序的入口点行为，非静态函数避免被识别为入口点冲突
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    internal partial int Main(string[] args);
}