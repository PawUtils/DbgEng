using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Interop.DbgEng;

public partial interface IDebugClient
{
    /// <summary>
    /// Creates a new client object and returns an interface pointer to it.
    /// </summary>
    /// <exception cref="COMException"></exception>
    static IDebugClient Create()
    {
        var hr = DbgEngLib.DebugCreate(Constants.GetIid<IDebugClient>(), out var pDebugClient);

        if (hr != 0)
        {
            throw new COMException(nameof(DbgEngLib.DebugCreate), hr);
        }

        var cw = new StrategyBasedComWrappers();

        return (IDebugClient)cw.GetOrCreateObjectForComInstance(pDebugClient, CreateObjectFlags.UniqueInstance);
    }
}
