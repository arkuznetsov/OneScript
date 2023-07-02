﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml;
using OneScript.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    /// <summary>
    /// Параметры, используемые для формирования XML
    /// </summary>
    [ContextClass("ПараметрыЧтенияXML", "XmlReaderSettings")]
    public class XmlReaderSettingsImpl : AutoContext<XmlReaderSettingsImpl>
    {
        readonly XmlParserContext _context;
        readonly XmlReaderSettings _settings;

        public XmlReaderSettingsImpl(string version, XmlParserContext context, XmlReaderSettings settings,
            bool ignoreXMLDeclaration=true,
            bool ignoreDocumentType=true,
            bool CDataSectionAsText=true, // умолчание отличается от конструктора скрипта
            bool useIgnorableWhitespace=false)
        {
            Version = version;
            _context = context;
            _settings = settings;
            IgnoreXMLDeclaration = ignoreXMLDeclaration;
            IgnoreDocumentType = ignoreDocumentType;
            CDATASectionAsText = CDataSectionAsText;
            UseIgnorableWhitespace = useIgnorableWhitespace;
        }

        public XmlReaderSettings Settings => _settings;
        public XmlParserContext Context => _context;

        /// <summary>
        /// Версия XML
        /// </summary>
        [ContextProperty("Версия", "Version")]
        public string Version { get; }

        /// <summary>
        /// Язык
        /// </summary>
        [ContextProperty("Язык", "Language")]
        public string Language => _context.XmlLang;

        [ContextProperty("ПробельныеСимволы", "Space")]
        public IValue Space => XmlSpaceEnum.Instance.FromNativeValue(_context.XmlSpace);

        [ContextProperty("ТипПроверкиПравильности","ValidationType")]
        public IValue ValidationTypeImpl => XmlValidationTypeEnum.Instance.FromNativeValue(_settings.ValidationType);

        [ContextProperty("ИгнорироватьОбъявлениеXML", "IgnoreXMLDeclaration")]
        public bool IgnoreXMLDeclaration { get; }

        [ContextProperty("ИгнорироватьТипДокумента", "IgnoreDocumentType")]
        public bool IgnoreDocumentType { get; }

        [ContextProperty("ИгнорироватьИнструкцииОбработки", "IgnoreDataProcessorInstructions")]
        public bool IgnoreDataProcessorInstructions => _settings.IgnoreProcessingInstructions;

        [ContextProperty("ИгнорироватьКомментарии", "IgnoreComments")]
        public bool IgnoreComments => _settings.IgnoreComments;

        [ContextProperty("ИгнорироватьПробельныеСимволы", "IgnoreWhitespace")]
        public bool IgnoreWhitespace => _settings.IgnoreWhitespace;

        [ContextProperty("СекцииCDATAКакТекст", "CDATASectionAsText")]
        public bool CDATASectionAsText { get; }

        [ContextProperty("ИспользоватьИгнорируемыеПробельныеСимволы", "UseIgnorableWhitespace")]
        public bool UseIgnorableWhitespace { get; }

        [ScriptConstructor]
        public static XmlReaderSettingsImpl Constructor(IValue version = null, IValue lang = null,
             IValue spaceChars = null, 
             IValue validityCheckType = null,
             IValue ignoreXMLDeclaration = null, IValue ignoreDocumentType = null,
             IValue ignoreDataProcessorInstructions = null, IValue ignoreComments = null,
             IValue ignoreSpaceCharacters = null, IValue CDATASectionAsText = null,
             IValue useIgnorableWhitespace = null)
        {
            var context = new XmlParserContext(null, null,
                lang?.AsString() ?? "",
                ContextValuesMarshaller.ConvertWrappedEnum(spaceChars, XmlSpace.Default))
                {
                    Encoding = System.Text.Encoding.UTF8
                };

            var settings = new XmlReaderSettings
            {
                ValidationType = ContextValuesMarshaller.ConvertWrappedEnum(validityCheckType, ValidationType.None),
                IgnoreComments = ContextValuesMarshaller.ConvertParam(ignoreComments, false),
                IgnoreProcessingInstructions = ContextValuesMarshaller.ConvertParam(ignoreDataProcessorInstructions, false),
                IgnoreWhitespace = ContextValuesMarshaller.ConvertParam(ignoreSpaceCharacters, true),
            };

            return new XmlReaderSettingsImpl(version?.AsString() ?? "1.0", context, settings,
                ContextValuesMarshaller.ConvertParam(ignoreXMLDeclaration, true),
                ContextValuesMarshaller.ConvertParam(ignoreDocumentType, true),
                ContextValuesMarshaller.ConvertParam(CDATASectionAsText, false),
                ContextValuesMarshaller.ConvertParam(useIgnorableWhitespace, false) );
        }

        public static XmlReaderSettingsImpl Create()
        {
            var context = new XmlParserContext(null, null,"",XmlSpace.Default);

            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.None,
                IgnoreComments = true, // отличается от конструктора скрипта
                IgnoreProcessingInstructions = false,
                IgnoreWhitespace =  true,
            };

            return new XmlReaderSettingsImpl("1.0", context, settings);
        }
    }
}