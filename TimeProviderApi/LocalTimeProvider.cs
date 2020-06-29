using System;

namespace TimeProviderApi
{
	public sealed class LocalTimeProvider : ITimeProvider
	{
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }

        }
        
	}
}