namespace MoodyBudgeter.Utility.Clients.EnvironmentRequester
{
    public class EnvironmentRequester : IEnvironmentRequester
    {
        public string GetVariable(string variableName)
        {
            return System.Environment.GetEnvironmentVariable(variableName);
        }
    }
}
