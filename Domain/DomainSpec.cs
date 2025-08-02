using System.Linq;
using System.Reflection;
using TypeGen.Core.SpecGeneration;

namespace Domain
{
    public class DomainSpec : GenerationSpec
    {
        public DomainSpec()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var modelTypes = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsPublic && t.Namespace != null &&
                            t.Namespace.StartsWith("Domain.Models.TMDb"));

            foreach (var type in modelTypes)
            {
                AddClass(type);
            }
        }
    }
}
