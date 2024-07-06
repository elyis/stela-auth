namespace STELA_AUTH.App.Service
{
    public static class CodeGeneratorService
    {
        public static string Generate()
        {
            return new Random().Next(100_0, 100_000_0).ToString();
        }
    }
}