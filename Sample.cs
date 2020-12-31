using System;
using System.Runtime.InteropServices;
using NWN.Core;
using NWN.Core.Native;
using ObjectType = NWN.Core.Native.NWNXLib.API.Constants.ObjectType.TYPE;

namespace NWN
{
  public static class Internal
  {
    public static int Bootstrap(IntPtr ptr, int nativeHandlesLength)
    {
      CoreGameManager manager = new CoreGameManager();
      manager.OnSignal += OnSignal;
      return NWNCore.Init(ptr, nativeHandlesLength, manager);
    }

    private static void OnSignal(string signal)
    {
      if (signal == "ON_MODULE_LOAD_FINISH")
      {
        NativeTest();
      }
    }

    private static void NativeTest()
    {
      CServerExoApp exoServer = NWNNative.AppManager.MPServerExoApp;

      Console.WriteLine($"Server Version is: {NWNNative.BuildNumber.ToMString()}.{NWNNative.BuildRevision.ToMString()}");
      Console.WriteLine($"Module Name is: {exoServer.GetModuleName().ToMString()}");

      for (uint areaId = NWScript.GetFirstArea(); areaId != NWScript.OBJECT_INVALID; areaId = NWScript.GetNextArea())
      {
        Console.WriteLine($"::::Resolving Area {areaId}: {NWScript.GetName(areaId)}::::");
        CNWSArea area = exoServer.GetGameObject(areaId).AsNWSArea();

        Console.WriteLine($"Area ID - Managed: {areaId} Native: {area.MIdSelf}");
        Console.WriteLine($"Area tag: {area.MSTag.ToMString()}");
        Console.WriteLine($"Area tileset ResRef: {area.MRefTileSet.GetResRefName()}");
        Console.WriteLine($"Area comment: {area.MSComments.ToMString()}");

        Console.WriteLine();

        for (uint objId = NWScript.GetFirstObjectInArea(areaId); objId != NWScript.OBJECT_INVALID; objId = NWScript.GetNextObjectInArea())
        {
          Console.WriteLine($"::::Resolving GameObject {objId}: {NWScript.GetName(objId)}::::");
          ICGameObject gameObject = exoServer.GetGameObject(objId);

          Console.WriteLine($"- Object ID - Managed: {objId} Native: {gameObject.MIdSelf}");
          Console.WriteLine($"- Object type: {(ObjectType)gameObject.MNObjectType}");
          Console.WriteLine($"- .NET type: {gameObject.GetType().FullName}");
          Console.WriteLine();
        }
      }
    }
  }

  public static class CExoStringExtensions
  {
    public static unsafe string ToMString(this CExoString exoString)
    {
      return Marshal.PtrToStringAuto(new IntPtr(exoString.CStr()));
    }

    public static unsafe string GetResRefName(this CResRef resRef)
    {
      return Marshal.PtrToStringAuto(new IntPtr(resRef.GetResRefStr()));
    }
  }
}
