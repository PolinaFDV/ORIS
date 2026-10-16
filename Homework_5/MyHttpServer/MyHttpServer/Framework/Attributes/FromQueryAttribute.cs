using System;

namespace MyHttpServer.Framework.Core;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromQueryAttribute : Attribute
{
}