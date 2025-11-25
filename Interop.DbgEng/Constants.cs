using System.Reflection;
using System.Runtime.InteropServices.Marshalling;

namespace Interop.DbgEng;

/// <summary>
/// Constants appear in <c>#define</c> or <c>const</c> in the <c>dbgeng.h</c> file.
/// </summary>
public static partial class Constants
{
    /// <summary>
    /// Gets the interface id of the COM interface.
    /// </summary>
    public static Guid GetIid<IComInterface>()
    {
        var type = typeof(IComInterface);

        if (type.IsInterface && type.GetCustomAttribute(typeof(IUnknownDerivedAttribute<,>)) is IIUnknownDerivedDetails details)
        {
            return details.Iid;
        }

        throw new ArgumentException("Expect a source generated COM interface.", nameof(IComInterface));
    }
}
