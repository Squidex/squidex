using System;

namespace DeploymentApp
{
    public interface ILogger
    {
        void Start(string process);

        void Failed(Exception ex);

        void Success();

        void Skipped(string reason);
    }
}
