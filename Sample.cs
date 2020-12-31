using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NWN.Core;
using NWN.Core.Native;
using ObjectType = NWN.Core.Native.NWNXLib.API.Constants.ObjectType.TYPE;

namespace NWN
{
  public static class Internal
  {
    private static CServerExoApp exoServer;

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
      exoServer = NWNNative.AppManager.MPServerExoApp;

      Console.WriteLine($"Server Version is: {NWNNative.BuildNumber.ToMString()}.{NWNNative.BuildRevision.ToMString()}");
      Console.WriteLine($"Module Name is: {exoServer.GetModuleName().ToMString()}");

      List<uint> areas = GetAreas();
      List<uint> objs = GetAllObjects(areas);

      // while (!Debugger.IsAttached)
      // {
      //   continue;
      // }

      PrintAreas(areas);
      PrintObjects(objs);
    }

    private static void PrintAreas(List<uint> areas)
    {
      for (int i = 0; i < areas.Count; i++)
      {
        uint areaId = areas[i];
        string nextArea = i < areas.Count - 1 ? $"Next area is {NWScript.GetName(areas[i + 1])}, ID {areas[i + 1]}" : "";

        Console.WriteLine($"::::Resolving Area {NWScript.GetName(areaId)}, ID {areaId}:::: {nextArea}");
        CNWSArea area = exoServer.GetGameObject(areaId).AsNWSArea();

        Console.WriteLine($"Area ID - Managed: {areaId} Native: {area.MIdSelf}");
        Console.WriteLine($"Area tag: {area.MSTag.ToMString()}");
        Console.WriteLine($"Area tileset ResRef: {area.MRefTileSet.GetResRefName()}");
        Console.WriteLine($"Area comment: {area.MSComments.ToMString()}");

        Console.WriteLine();
      }
    }

    private static void PrintObjects(List<uint> objs)
    {
      for (int i = 0; i < objs.Count; i++)
      {
        uint objId = objs[i];
        string nextObj = i < objs.Count - 1 ? $"Next object is {NWScript.GetName(objs[i + 1])}, ID {objs[i + 1]}" : "";

        Console.WriteLine($"::::Resolving Object {NWScript.GetName(objId)}, ID {objId}:::: {nextObj}");
        CGameObject gameObject = (CGameObject)exoServer.GetGameObject(objId);

        ObjectType type = (ObjectType)gameObject.MNObjectType;
        Console.WriteLine($"- Object ID - Managed: {objId} Native: {gameObject.MIdSelf}");
        Console.WriteLine($"- Object type: {type}");
        Console.WriteLine($"- .NET type: {gameObject.GetType().FullName}");

        Console.WriteLine();
      }
    }

    private static List<uint> GetAreas()
    {
      List<uint> areas = new List<uint>();
      for (uint areaId = NWScript.GetFirstArea(); areaId != NWScript.OBJECT_INVALID; areaId = NWScript.GetNextArea())
      {
        areas.Add(areaId);
      }

      return areas;
    }

    private static List<uint> GetAllObjects(List<uint> areas)
    {
      List<uint> objs = new List<uint>();
      foreach (uint areaId in areas)
      {
        for (uint objId = NWScript.GetFirstObjectInArea(areaId); objId != NWScript.OBJECT_INVALID; objId = NWScript.GetNextObjectInArea(areaId))
        {
          objs.Add(objId);
        }
      }

      return objs;
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
