namespace MoodyBudgeter.Utility.Clients.EnvironmentRequester
{
    public interface IEnvironmentRequester
    {
        string GetVariable(string variableName);
    }
}
