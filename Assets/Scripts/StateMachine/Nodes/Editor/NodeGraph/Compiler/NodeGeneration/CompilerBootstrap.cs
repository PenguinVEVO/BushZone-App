using UnityEditor;
using System;
using System.Linq;
using FrameLabs.AI.Nodes;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    [InitializeOnLoad]
    public static class CompilerBootstrap
    {
        static CompilerBootstrap()
        {
            const string registryPath = "Assets/Cache/CustomCompilerRegistry.asset";
            var registry = AssetDatabase.LoadAssetAtPath<CompilerRegistryAsset>(registryPath);

            if (registry == null || registry.compilerTypeNames == null)
                return;

            foreach (var typeName in registry.compilerTypeNames.Distinct())
            {
                Type compilerType = Type.GetType(typeName);
                if (compilerType == null || !compilerType.IsClass) continue;

                var baseCompilerInterface = compilerType
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INodeProcessor<>));

                if (baseCompilerInterface == null) continue;

                Type nodeType = baseCompilerInterface.GetGenericArguments().FirstOrDefault();
                if (nodeType == null || !typeof(Node).IsAssignableFrom(nodeType)) continue;

                try
                {
                    object instance = Activator.CreateInstance(compilerType);
                    var registerMethod = typeof(NodeCompilerRegistry)
                        .GetMethod("RegisterCustomCompiler")?
                        .MakeGenericMethod(nodeType);

                    registerMethod?.Invoke(null, new object[] { instance, true });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CompilerBootstrap] Failed to register {typeName}: {ex.Message}");
                }
            }
        }
    }
}
