using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Digify.DependecyInjection
{
    public interface IAssemblyProvider
    {
        IEnumerable<Assembly> GetAssemblies(string path);
    }
}
