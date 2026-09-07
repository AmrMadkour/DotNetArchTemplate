namespace Application.Services;

public interface ILogTestService
{
    Task RunAsync(bool fail);
}
