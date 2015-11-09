﻿/*------------------------------------------------------------------------------
Wintellect.Analyzers - .NET Compiler Platform ("Roslyn") Analyzers and CodeFixes
Copyright (c) Wintellect. All rights reserved
Licensed under the Apache License, Version 2.0
See License.txt in the project root for license information
------------------------------------------------------------------------------*/
using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CodingStandardCodeAnalyzers {
    public static class SymbolExtensions
    {

        /// <summary>
        /// Determines if the symbol is part of generated or non-user code.
        /// </summary>
        /// <param name="symbol">
        /// The <see cref="ISymbol"/> derived type to check.
        /// </param>
        /// <param name="checkAssembly">
        /// Set to true to check the assembly for the attributes. False to stop at the type.
        /// </param>
        /// <returns>
        /// Returns true if the item, type, or assembly has the GeneratedCode attribute 
        /// applied.
        /// </returns>
        public static Boolean IsGeneratedOrNonUserCode(this ISymbol symbol, Boolean checkAssembly = true)
        {
            // The goal here is to see if this ISymbol is part of auto generated code.
            // To do that, I'll walk up the hierarchy of item, type, to module/assembly
            // looking to see if the GeneratedCodeAttribute is set on any of them.
            var attributes = symbol.GetAttributes();
            if (!HasIgnorableAttributes(attributes))
            {
                if (symbol.Kind != SymbolKind.NamedType && HasIgnorableAttributes(symbol.ContainingType.GetAttributes()))
                {
                    return true;
                }

                if (checkAssembly)
                {
                    attributes = symbol.ContainingAssembly.GetAttributes();
                    if (HasIgnorableAttributes(attributes))
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Given a list of methods, finds the exact matching method based on parameter types.
        /// </summary>
        /// <param name="methods">
        /// The IEnumerable list of methods to look through.
        /// </param>
        /// <param name="parameters">
        /// The array of parameter types to look for.
        /// </param>
        /// <returns>
        /// Not null is the exact method match, null if there is no match.
        /// </returns>
        static public IMethodSymbol WithParameters(this IEnumerable<IMethodSymbol> methods, Type[] parameters)
        {
            foreach (var currMethod in methods)
            {
                if (currMethod.DoParamatersMatch(parameters))
                {
                    return currMethod;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns true if this method is an override of a specific method.
        /// </summary>
        /// <param name="method">
        /// The Roslyn method to check.
        /// </param>
        /// <param name="methodInfo">
        /// The reflection-based method you want to see is being overridden.
        /// </param>
        /// <returns>
        /// True, the Roslyn method is an override of <paramref name="methodInfo"/>.
        /// </returns>
        static public Boolean IsOverideOf(this IMethodSymbol method, MethodInfo methodInfo)
        {
            String methAssemblyName = methodInfo.DeclaringType.AssemblyQualifiedName;
            if (!method.GetTypesQualifiedAssemblyName().Equals(methAssemblyName))
            {
                if (!method.IsOverride)
                {
                    return false;
                }
                return method.OverriddenMethod.IsOverideOf(methodInfo);
            }

            if (!method.Name.Equals(methodInfo.Name))
            {
                return false;
            }

            return method.DoParamatersMatch(methodInfo.GetParameters().Select(pi => pi.ParameterType).ToArray());
        }

        /// <summary>
        /// Returns the specific method of a class/structure with the types as parameters.
        /// </summary>
        /// <remarks>
        /// Unfortunately, Roslyn only supports the INamedTypeSymbol.GetMethods that looks up hard coded strings. This 
        /// extension method does a smarter look up by requiring you to also specify the parameters.
        /// </remarks>
        /// <param name="namedTypeSymbol">
        /// The type being extended.
        /// </param>
        /// <param name="methodName">
        /// The name of the method you want.
        /// </param>
        /// <param name="parameters">
        /// The parameter types as an array. To look up a parameter-less method, pass an empty array.
        /// </param>
        /// <returns>
        /// The matching IMethodSymbol or null if that exact method is not found.
        /// </returns>
        static public IMethodSymbol GetSpecificMethod(this INamedTypeSymbol namedTypeSymbol, String methodName, Type[] parameters)
        {
            IMethodSymbol method = namedTypeSymbol.GetMembers(methodName).OfType<IMethodSymbol>().WithParameters(parameters);
            return method;
        }

        /// <summary>
        /// Finds the first method either on the class/structure or it's based classes.
        /// </summary>
        /// <param name="namedTypeSymbol">
        /// The type being extended.
        /// </param>
        /// <param name="methodName">
        /// The name of the method you want.
        /// </param>
        /// <param name="parameters">
        /// The parameter types as an array. To look up a parameter-less method, pass an empty array.
        /// </param>
        /// <returns>
        /// The matching IMethodSymbol or null if that exact method is not found.
        /// </returns>
        static public IMethodSymbol FirstMethodOfSelfOrBaseType(this INamedTypeSymbol namedTypeSymbol, String methodName, Type[] parameters)
        {
            IMethodSymbol method = namedTypeSymbol.GetSpecificMethod(methodName, parameters);
            if (method != null)
            {
                return method;
            }

            namedTypeSymbol = namedTypeSymbol.BaseType;
            if (namedTypeSymbol == null)
            {
                return null;
            }
            return namedTypeSymbol.FirstMethodOfSelfOrBaseType(methodName, parameters);
        }

        /// <summary>
        /// Returns true if the methods parameters match those of the type array in order.
        /// </summary>
        /// <param name="method">
        /// The method to check.
        /// </param>
        /// <param name="parameters">
        /// The parameter types to match.
        /// </param>
        /// <returns>
        /// True if the parameters match the types, false otherwise.
        /// </returns>
        public static Boolean DoParamatersMatch(this IMethodSymbol method, Type[] parameters)
        {
            Int32 methodParamCount = method.Parameters.Count();
            if (methodParamCount != parameters.Length)
            {
                return false;
            }

            for (Int32 i = 0; i < methodParamCount; i++)
            {
                if (!method.Parameters[i].IsType(parameters[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines if the INamedTypeSymbol is derived from the specific reflection interface.
        /// </summary>
        /// <param name="namedType">
        /// The type being extended.
        /// </param>
        /// <param name="type">
        /// The reflection type to check.
        /// </param>
        /// <returns>
        /// True if <paramref name="type"/> is in the inheritance chain.
        /// </returns>
        /// <remarks>
        /// This method does no checking if <paramref name="type"/> is actually an interface.
        /// </remarks>
        public static Boolean IsDerivedFromInterface(this INamedTypeSymbol namedType, Type type)
        {
            foreach (var iFace in namedType.AllInterfaces)
            {
                if (iFace.IsType(type))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the IParameterSymbol matches the specified reflection type.
        /// </summary>
        /// <param name="parameter">
        /// The type being extended.
        /// </param>
        /// <param name="type">
        /// The reflection type.
        /// </param>
        /// <returns>
        /// True if they match, false otherwise.
        /// </returns>
        public static Boolean IsType(this IParameterSymbol parameter, Type type) => parameter.GetTypesQualifiedAssemblyName().Equals(type.AssemblyQualifiedName, StringComparison.Ordinal);

        /// <summary>
        /// Returns true if the INamedTypeSymbols matches the specified reflection type.
        /// </summary>
        /// <param name="namedType"></param>
        /// The INamedTypeSymbols in question
        /// <param name="type">
        /// The reflection type.
        /// </param>
        /// <returns>
        /// True if they match, false otherwise.
        /// </returns>
        public static Boolean IsType(this INamedTypeSymbol namedType, Type type) => namedType.GetTypesQualifiedAssemblyName().Equals(type.AssemblyQualifiedName, StringComparison.Ordinal);

        /// <summary>
        /// For the INamedTypeSymbol returns the assembly qualified name from Roslyn.
        /// </summary>
        /// <param name="namedType">
        /// The item being extended.
        /// </param>
        /// <returns>
        /// The qualified assembly name this type comes from.
        /// </returns>
        public static String GetTypesQualifiedAssemblyName(this INamedTypeSymbol namedType) => BuildQualifiedAssemblyName(null, namedType.ToDisplayString(), namedType.ContainingAssembly);

        /// <summary>
        /// For the IParameterSymbol returns the assembly qualified name from Roslyn.
        /// </summary>
        /// <param name="parameter">
        /// The item being extended.
        /// </param>
        /// <returns>
        /// The qualified assembly name this type comes from.
        /// </returns>
        public static String GetTypesQualifiedAssemblyName(this IParameterSymbol parameter) => BuildQualifiedAssemblyName(parameter.Type.ContainingNamespace.Name,
                                                                                                                          parameter.Type.Name,
                                                                                                                          parameter.Type.ContainingAssembly);

        /// <summary>
        /// For the IMethodSymbol returns the assembly qualified name from Roslyn.
        /// </summary>
        /// <param name="parameter">
        /// The item being extended.
        /// </param>
        /// <returns>
        /// The qualified assembly name this type comes from.
        /// </returns>
        public static String GetTypesQualifiedAssemblyName(this IMethodSymbol method) => BuildQualifiedAssemblyName(method.ContainingType.ContainingNamespace.Name,
                                                                                                                    method.ContainingType.Name,
                                                                                                                    method.ContainingType.ContainingAssembly);

        private static String BuildQualifiedAssemblyName(String nameSpace, String typeName, IAssemblySymbol assemblySymbol)
        {
            String symbolType;
            if (String.IsNullOrEmpty(nameSpace))
            {
                symbolType = typeName;
            }
            else
            {
                symbolType = String.Format("{0}.{1}", nameSpace, typeName);
            }
            String symbolAssemblyQualiedName = symbolType + ", " + new AssemblyName(assemblySymbol.Identity.GetDisplayName(true));
            return symbolAssemblyQualiedName;
        }

        private static Boolean HasIgnorableAttributes(ImmutableArray<AttributeData> attributes)
        {
            for (Int32 i = 0; i < attributes.Count(); i++)
            {
                AttributeData attr = attributes[i];
                if ((attr.AttributeClass.IsType(typeof(GeneratedCodeAttribute)) ||
                    (attr.AttributeClass.IsType(typeof(DebuggerNonUserCodeAttribute)))))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
