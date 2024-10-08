﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using Newtonsoft.Json;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Text;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Json
{
    internal class JsonReaderInternal: JsonTextReader  // из библиотеки Newtonsoft
    {
        public JsonReaderInternal(TextReader reader) : base(reader)
        {
            Finished = false;
        }

        public override bool Read()
        {
            if (!base.Read())
            {
                Finished = true;
                return false;
            }
            if (TokenType != JsonToken.Undefined)
                return true;

            throw JSONReaderException.UnexpectedSymbol();
        }
        public bool Finished { get; private set; }
    }

    /// <summary>
    /// 
    /// Предназначен для последовательного чтения JSON-данных из файла или строки.
    /// </summary>
    [ContextClass("ЧтениеJSON", "JSONReader")]
    public class JSONReader : AutoContext<JSONReader>
    {

        private JsonReaderInternal _reader;

        /// <summary>
        /// 
        /// Возвращает true если для объекта чтения json был задан текст для парсинга.
        /// </summary>
        private bool IsOpen() => _reader != null;

        private void CheckIfOpen()
        {
            if (_reader == null) throw JSONReaderException.NotOpen();
        }

        public JSONReader()
        {
        }

        [ScriptConstructor]
        public static JSONReader Constructor()
        {
            return new JSONReader();
        }

        /// <summary>
        /// 
        /// Указывает на позицию, находящуюся сразу после прочитанного значения.
        /// При ошибке чтение остается на позиции последнего успешно считанного символа.
        /// </summary>
        /// <value>Число (Number), Неопределено (Undefined)</value>
        [ContextProperty("ТекущаяПозиция", "CurrentPosition")]
        public IValue CurrentPosition
        {
            get
            {
                if (IsOpen())
                {
                    return ValueFactory.Create(_reader.LinePosition);
                }

                return ValueFactory.Create(); // Неопределено 
            }
        }

        /// <summary>
        /// 
        /// Указывает на позицию сразу после прочитанного значения.
        /// Например, перед чтением первого элемента - 0, после чтения первого элемента -1 .
        /// </summary>
        /// <value>Число (Number), Неопределено (Undefined)</value>
        [ContextProperty("ТекущаяСтрока", "CurrentLine")]
        public IValue CurrentLine
        {
            get
            {
                if (IsOpen())
                {
                    return ValueFactory.Create(_reader.LineNumber);
                }

                return ValueFactory.Create(); // Неопределено
            }
        }

        /// <summary>
        /// 
        /// Содержит текущее значение:
        /// 
        ///  - Число - если ТипТекущегоЗначения имеет значение Число;
        ///  - Строка - если ТипТекущегоЗначения имеет одно из следующих значений:
        ///    - Комментарий,
        ///    - ИмяСвойства,
        ///    - Строка;
        ///  - Булево - если ТипТекущегоЗначения имеет значение Булево,
        ///  - Неопределено - если ТипТекущегоЗначения имеет значение Null.
        /// Исключение генерируется в случае, если ТипТекущегоЗначения имеет одно из следующих значений:
        ///  - НачалоМассива,
        ///  - КонецМассива,
        ///  - НачалоОбъекта,
        ///  - КонецОбъекта,
        ///  - Ничего.
        /// </summary>
        /// <value>Число (Number), Строка (String), Булево (Boolean), Неопределено (Undefined)</value>
        [ContextProperty("ТекущееЗначение", "CurrentValue")]
        public IValue CurrentValue
        {
            get
            {
                CheckIfOpen();
                if (_reader.Finished)
                    throw JSONReaderException.CannotGetValue();

                switch (_reader.TokenType)
                {
                    case JsonToken.String:
                    case JsonToken.Comment:
                    case JsonToken.PropertyName:
                        return ValueFactory.Create((string)_reader.Value);

                    case JsonToken.Integer:
                    case JsonToken.Float:
                        return ValueFactory.Create(Convert.ToDecimal(_reader.Value));

                    case JsonToken.Boolean:
                        return ValueFactory.Create((bool)_reader.Value);

                    case JsonToken.Date:
                        return ValueFactory.Create((DateTime)_reader.Value);

                    case JsonToken.Null:
                        return ValueFactory.Create();

                    default:
                        throw JSONReaderException.CannotGetValue();
                        ;
                }
            }
        }

        /// <summary>
        /// 
        /// Тип текущего значения в документе JSON во внутреннем формате.
        /// null - если чтение еще не началось или достигнут конец файла.
        /// </summary>
        /// <value>CurrentJsonTokenType</value>
        public JsonToken CurrentJsonTokenType
        {
            get
            {
                CheckIfOpen();
                return _reader.TokenType;
            }
        }

        /// <summary>
        /// 
        /// Тип текущего значения в документе JSON.
        /// Неопределено - если чтение еще не началось или достигнут конец файла.
        /// </summary>
        /// <value>ТипЗначенияJSON (JSONValueType)</value>
        [ContextProperty("ТипТекущегоЗначения", "CurrentValueType")]
        public JSONValueTypeEnum CurrentValueType
        {
            get
            {
                CheckIfOpen();

                if (_reader.Finished)
                    return JSONValueTypeEnum.None;

                return _reader.TokenType switch
                {
                    JsonToken.Null => JSONValueTypeEnum.Null,
                    JsonToken.StartObject => JSONValueTypeEnum.ObjectStart,
                    JsonToken.StartArray => JSONValueTypeEnum.ArrayStart,
                    JsonToken.PropertyName => JSONValueTypeEnum.PropertyName,
                    JsonToken.Comment => JSONValueTypeEnum.Comment,
                    JsonToken.Integer => JSONValueTypeEnum.Number,
                    JsonToken.Float => JSONValueTypeEnum.Number,
                    JsonToken.String => JSONValueTypeEnum.String,
                    JsonToken.Boolean => JSONValueTypeEnum.Boolean,
                    JsonToken.EndObject => JSONValueTypeEnum.ObjectEnd,
                    JsonToken.EndArray => JSONValueTypeEnum.ArrayEnd,
                    _ => JSONValueTypeEnum.None
                };
            }
        }

        /// <summary>
        /// 
        /// Завершает чтение текста JSON из файла или строки.
        /// </summary>
        ///
        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }

        /// <summary>
        /// 
        /// Открывает JSON-файл для чтения данным объектом. Если перед вызовом данного метода уже производилось чтение JSON из другого файла или строки, то чтение прекращается и объект инициализируется для чтения из указанного файла.
        /// </summary>
        ///
        /// <param name="JSONFileName">
        /// Имя файла, содержащего текст JSON. </param>
        /// <param name="encoding">
        /// Позволяет задать кодировку входного файла.</param>
        [ContextMethod("ОткрытьФайл", "OpenFile")]
        public void OpenFile(string JSONFileName, IValue encoding = null)
        {
            if (IsOpen())
                Close();

            StreamReader _fileReader;

            try
            {
                if (encoding != null)
                    _fileReader = FileOpener.OpenReader(JSONFileName, TextEncodingEnum.GetEncoding(encoding));
                else
                    _fileReader = FileOpener.OpenReader(JSONFileName, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                throw new RuntimeException(e.Message, e);
            }

            _reader = new JsonReaderInternal(_fileReader)
            {
                SupportMultipleContent = true
            };
        }

        /// <summary>
        /// Если текущее значение – начало массива или объекта, то пропускает его содержимое и конец.
        /// Для остальных типов значений работает аналогично методу Прочитать().
        /// </summary>
        ///
        [ContextMethod("Пропустить", "Skip")]
        public bool Skip()
        {
            CheckIfOpen();

            if (_reader.TokenType == JsonToken.StartArray || _reader.TokenType == JsonToken.StartObject)
            {
                _reader.Skip();
            }

            return _reader.Read();
        }

        /// <summary>
        /// Выполняет чтение значения JSON.
        /// </summary>
        ///
        [ContextMethod("Прочитать", "Read")]
        public bool Read()
        {
            CheckIfOpen();
            return _reader.Read();

        }

        /// <summary>
        /// 
        /// Устанавливает строку, содержащую текст JSON для чтения данным объектом. Если перед вызовом данного метода уже производилось чтение JSON из другого файла или строки, то чтение прекращается и объект инициализируется для чтения из указанной строки.
        /// </summary>
        ///
        /// <param name="JSONString">
        /// Строка, содержащая текст в формате JSON. </param>
        ///
        ///
        [ContextMethod("УстановитьСтроку", "SetString")]
        public void SetString(string JSONString)
        {
            if (IsOpen())
                Close();

            _reader = new JsonReaderInternal(new StringReader(JSONString))
            {
                SupportMultipleContent = true
            };
        }

    }

    public class JSONReaderException : RuntimeException
    {
        public JSONReaderException(string message) : base(message)
        {
        }

        public static JSONReaderException NotOpen()
        {
            return new JSONReaderException(Locale.NStr
                ("ru='Источник данных JSON не открыт'; en='JSON data source is not opened'"));
        }

        public static JSONReaderException CannotGetValue()
        {
            return new JSONReaderException(Locale.NStr
                ("ru='Текущее значение JSON не может быть получено';en='Cannot get current JSON value'"));
        }

        public static JSONReaderException UnexpectedSymbol()
        {
            return new JSONReaderException(Locale.NStr
                ("ru='Непредвиденный символ при чтении JSON';en='Unexpected symbol during JSON reading'"));
        }
    }
}
