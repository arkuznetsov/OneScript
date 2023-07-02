﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine;

namespace OneScript.Values
{
    public interface IValueReference : IEquatable<IValueReference>
    {
        IValue Value { get; set; }

        BslValue BslValue
        {
            get => (BslValue)Value;
            set => Value = value;
        }
    }
}
