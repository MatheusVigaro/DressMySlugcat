global using Mono.Cecil.Cil;
global using MonoMod.Cil;
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using UnityEngine;
global using Debug = UnityEngine.Debug; //This should be changed to BepInEx Logger
global using RWCustom;
global using System.Runtime.Serialization.Formatters.Binary;
global using BepInEx;
global using BepInEx.Logging;
global using Menu;
global using Menu.Remix.MixedUI;
global using Menu.Remix;
global using MoreSlugcats;
global using System.Runtime.CompilerServices;
global using Watcher;
global using static TriangleMesh;
global using Color = UnityEngine.Color;
global using Vector2 = UnityEngine.Vector2;
using System.Security.Permissions;
using System.Security;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]