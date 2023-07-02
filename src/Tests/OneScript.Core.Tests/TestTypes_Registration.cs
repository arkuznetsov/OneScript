﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;
using Xunit;

namespace OneScript.Core.Tests
{
    public class TestTypesRegistration
    {
        [Fact]
        public void All_Classes_Have_Unique_Types()
        {
            var tm = new DefaultTypeManager();
            var discoverer = new ContextDiscoverer(tm, Mock.Of<IGlobalsManager>(), default);
            
            discoverer.DiscoverClasses(typeof(ArrayImpl).Assembly);

            var manualDiscover = typeof(ArrayImpl).Assembly.GetTypes()
                .Where(t => t.IsDefined(typeof(ContextClassAttribute), false))
                .ToArray();

            var alreadySearchedTypes = new HashSet<TypeDescriptor>();

            foreach (var type in manualDiscover)
            {
                TypeDescriptor definition;
                try
                {
                    definition = type.GetTypeFromClassMarkup();
                }
                catch (InvalidOperationException)
                {
                    definition = tm.GetTypeByFrameworkType(type); // если не упало, то зарегистрировалось
                    var typeData = $"name: {definition.Name}\n" +
                                   $"id: {definition.Id}\n" +
                                   $"class: {definition.ImplementingClass.Name}\n" +
                                   $"discoveredClass: {type.Name}";
                    
                    alreadySearchedTypes.Add(definition).Should().BeTrue(typeData + "\nshould not be in hashset");
                    continue; 
                }

                var registeredType = tm.GetTypeByName(definition.Name);
                Assert.Equal(type, registeredType.ImplementingClass);
                Assert.Equal(definition, registeredType);
                
                alreadySearchedTypes.Add(definition).Should().BeTrue(definition.Name + "\nshould not be in hashset");
            }
        }

        [Fact]
        public void TypeEqualityById()
        {
            var guid = Guid.Parse("826429B0-F662-4CCD-BF15-384A10B53611");
            var type1 = new TypeDescriptor(guid, "TheType");
            var type2 = new TypeDescriptor(guid, "TheType");
            
            Assert.Equal(type1, type2);
            Assert.True(type1 == type2); // operator==
            Assert.False(type1 != type2); // operator !=
        }
        
        [Fact]
        public void TypeInequalityForNull()
        {
            var guid = Guid.Parse("826429B0-F662-4CCD-BF15-384A10B53611");
            var type1 = new TypeDescriptor(guid, "TheType");
            TypeDescriptor type2 = null;
            
            Assert.NotEqual(type1, type2);
            Assert.False(type1 == type2); // operator==
            Assert.True(type1 != type2); // operator !=
        }
        
        [Fact]
        public void TypeEqualityForNull()
        {
            TypeDescriptor type1 = null;
            TypeDescriptor type2 = null;
            
            Assert.Equal(type1, type2);
            Assert.True(type1 == type2); // operator==
            Assert.False(type1 != type2); // operator !=
        }
        
    }
}