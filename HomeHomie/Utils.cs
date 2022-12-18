namespace HomeHomie
{
    internal static class Utils
    {
        public static string GetVariable(string variableName) => Environment.GetEnvironmentVariable(variableName) ?? throw new Exception("Environment variable is not defined: " + variableName);
    }
}
