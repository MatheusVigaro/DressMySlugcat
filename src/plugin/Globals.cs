global using BepInEx;
global using BepInEx.Logging;
global using Menu;
global using Menu.Remix;
global using Menu.Remix.MixedUI;
global using Mono.Cecil.Cil;
global using MonoMod.Cil;
global using MoreSlugcats;
global using RWCustom;
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Runtime.CompilerServices;
global using System.Runtime.Serialization.Formatters.Binary;
global using UnityEngine;
global using Watcher;
global using static DressMySlugcat.Plugin;
global using static TriangleMesh;
global using Color = UnityEngine.Color;
global using Vector2 = UnityEngine.Vector2;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]