public interface ITurnAction
{
    void ExecuteAction();
    bool IsActionComplete();
    void ResetActionComplete();
}
