using System.Reflection;
using TypeGen.Core.SpecGeneration;

namespace Domain
{
    public class TypeGen : GenerationSpec
    {
        public TypeGen()
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
