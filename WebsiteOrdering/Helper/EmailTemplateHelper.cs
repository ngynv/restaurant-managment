namespace WebsiteOrdering.Helper
{
    public class EmailTemplateHelper
    {

        public static string PopulateTemplate(string filePath, Dictionary<string, string> placeholders)
        {
            var template = File.ReadAllText(filePath);
            foreach (var pair in placeholders)
            {
                template = template.Replace($"{{{{{pair.Key}}}}}", pair.Value);
            }
            return template;
        }
    }
}
