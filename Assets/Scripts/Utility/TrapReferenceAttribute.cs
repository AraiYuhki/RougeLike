using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false )]
public class TrapReferenceAttribute : PropertyAttribute
{
}
