using System;
namespace Orleans.Providers.CouchDB.Configuration
{
	public class CouchDbOptionsValidator<T> : IConfigurationValidator where T: CouchDbOptions
    {
        private readonly CouchDbOptions options;
        private readonly string name;

        public CouchDbOptionsValidator(T options, string name)
        {
            this.options = options;
            this.name = name;
        }

        public void ValidateConfiguration()
        {
            options.Validate();
        }
    }
}

