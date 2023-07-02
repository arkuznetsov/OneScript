﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using OneScript.Types;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class ConstructionTests
    {
        [Fact]
        public void CanCreate_With_ContextInjection()
        {
            var f = new TypeFactory(typeof(TestContextClass));
            var ctx = new TypeActivationContext
            {
                TypeName = "SomeType"
            };
            
            var args = new IValue[2]
            {
                default,
                default
            };
            
            var instance = (TestContextClass)f.Activate(ctx, args);
            instance.CreatedViaMethod.Should().Be("Constructor2-SomeType");
        }
    }
}