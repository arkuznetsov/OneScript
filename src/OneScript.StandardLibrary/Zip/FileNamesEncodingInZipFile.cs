/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.Zip
{
    [EnumerationType("КодировкаИменФайловВZipФайле","FileNamesEncodingInZipFile")]
    public enum FileNamesEncodingInZipFile
    {
        [EnumValue("Авто")]
        Auto,
        
        [EnumValue("UTF8")]
        Utf8,
        
        [EnumValue("КодировкаОСДополнительноUTF8","OSEncodingWithUTF8")]
        OsEncodingWithUtf8
    }
}