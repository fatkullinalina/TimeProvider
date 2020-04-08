using System;

namespace TimeProviderApi
{
    public interface ITimeProvider
    {
        DateTime Now { get; }
    }
}
