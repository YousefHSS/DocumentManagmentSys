namespace DoucmentManagmentSys.Helpers
{
    public class ConfigurationHelper
    {
        public static string GetString(string key)
        {
            IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            // Retrieve value from appsettings.json
            string logoPath = config[key];
            return logoPath;
        }
    }
}
