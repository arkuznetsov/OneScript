# Developer Docs — архитектура и навигация по проекту OneScript (для новых контрибьюторов)

Этот документ помогает быстро разобраться, что лежит в репозитории, зачем это нужно, как устроено внутри и как использовать.

## 1 Картина целиком: из чего состоит OneScript

OneScript — реализация языка, совместимого с синтаксисом 1С/BSL. В основе - стековая виртуальная машина. Проект включает реализацию компилятора, исполняющей среды, системы типов и стандартной библиотеки BSL. Поверх этого — инструменты (CLI, раннер), отладка (DAP), веб-обёртки и API для нативных расширений.

Слои (сверху вниз):
- Приложения и инструменты: oscript (CLI), StandaloneRunner, VSCode.DebugAdapter, OneScriptDocumenter, примеры (TestApp, Component).
- Хостинг и сервисы: ScriptEngine.HostedScript, DebugServices, Web.Server.
- Рантайм: ScriptEngine, OneScript.Native (компиляция/исполнение, встроенные функции).
- Ядро/язык/библиотека: OneScript.Core (типы/контексты), OneScript.Language (лексер/парсер/AST), OneScript.StandardLibrary.
- Интеграции: ScriptEngine.NativeApi (C++ расширения).
- Инструмент генерации автодокументации OneScriptDocumenter

## 2 Быстрый старт для контрибьютора
- Где собирать: решение src/1Script.sln.
- Входной CLI: src/oscript (консольное приложение для запуска .os‑скриптов).
- Запуск тестов: проекты src/Tests/* (C#) и папка tests/* (скрипты .os). 
- Если добавляете новый контекст/тип — обычно правки в OneScript.Core/ScriptEngine/StandardLibrary, плюс модульные тесты.

## 3 Обзор проектов (назначение, ключевые узлы)

Ниже по каждому проекту — зачем он нужен, где искать основную логику и какие классы отвечают за ключевые задачи.

### 3.1 OneScript.Language — лексер, парсер, AST

Назначение: преобразует исходный BSL‑код в токены и синтаксическое дерево, обрабатывает директивы препроцессора.
Проект сделан максимально независимым и отчуждаемым, и должен иметь возможность использоваться в других решениях, не связанных с 1Script, как просто парсер BSL.

- Лексер: LexicalAnalysis/
  - DefaultLexer.cs — основной лексер.
  - Token.cs, Lexem.cs, LexemType.cs — токены/лексемы.
  - LexerBuilder.cs, LexerDetectorBuilder.cs — сборка/конфигурация лексера.
  - Разные состояния: StringLexerState.cs, NumberLexerState.cs, CommentLexerState.cs, PreprocessorDirectiveLexerState.cs и др.
- Парсер и AST: SyntaxAnalysis/
  - DefaultBslParser.cs — основной парсер.
  - Узлы AST: AstNodes/* (ModuleNode.cs, MethodNode.cs, CallNode.cs, BinaryOperationNode.cs, VariableDefinitionNode.cs, TryExceptNode.cs, If/While/For‑узлы и др.).
  - ParserContext.cs, NodeBuilder.cs — контекст и построение дерева.
- Диагностика: SyntaxErrorException.cs, CodeError.cs, ErrorPositionInfo.cs, LocalizedErrors.cs.

На выходе вы получаете ModuleNode/AST, пригодный для компиляции рантаймом/бэкендом или обработки статическим анализатором.

### 3.2 OneScript.Core — система типов, значения, отражение контекстов
Назначение: общий объектный каркас значений BSL, контекстов (объекты/методы/свойства), аннотаций и исключений.
- Модель значений: Values/
  - IValue, BslValue (база), конкретные типы: BslStringValue.cs, BslNumericValue.cs, BslBooleanValue.cs, BslDateValue.cs, BslNullValue.cs, BslUndefinedValue.cs, BslTypeValue.cs, BslObjectValue.cs.
  - Ссылки на значения: IValueReference, ValueReference.cs, PropertyValueReference.cs, IndexedValueReference.cs.
- Контексты и отражение: Contexts/
  - ContextClassAttribute.cs, ContextMethodAttribute.cs, ContextPropertyAttribute.cs — аннотации контекстов/членов.
  - BslMethodInfo.cs, BslPropertyInfo.cs, BslFieldInfo.cs — метаданные членов.
  - ReflectedClassType.cs, ClassBuilder.cs — построение и отражение классов контекстов.
  - PredefinedInterfaceRegistration.cs, PredefinedInterfaceResolver.cs — предопределённые интерфейсы.
- Символы/связывание (для компиляции): Compilation/Binding/* (ISymbol, SymbolTable, SymbolBinding и др.).
- Исключения: Exceptions/* (RuntimeException.cs, TypeConversionException.cs, PropertyAccessException.cs и др.).

### 3.3 ScriptEngine — движок выполнения (стек‑машина, окружение)

Основная среда исполнения на базе стековой виртуальной машины.

Назначение: организует выполнение скриптов, стек вызовов, области видимости, глобальные функции и интеграцию с отладкой.
- Исполнитель: Machine/
  - StackMachineExecutor.cs — стек‑машина.
  - ExecutionContext.cs, ExecutionFrame.cs — контекст и кадры исполнения.
  - ValueFactory.cs, TypeFactory.cs, GlobalInstancesManager.cs — фабрики и реестр глобалей.
  - BuiltinFunctions.cs (Machine) — глобальные функции рантайма.
- Компиляция/бэкенды: Compiler/
  - CompilerBackendSelector.cs, DefaultCompilerBackend.cs, NativeCompilerBackend.cs — выбор и адаптация бэкенда.
- Хостинг/DI: Hosting/
  - DefaultEngineBuilder.cs, ServiceRegistrationExtensions.cs — построение движка/регистрация зависимостей.
- Входные точки API: ScriptEngine.cs, ScriptingEngine.cs — фасады для запуска.

### 3.4 OneScript.Native — компилятор/исполнитель, встроенные функции

Альтернативная среда исполнения (не основная). Предоставляет компиляцию BSL в ExpressionTrees фреймворка .NET.
Данная среда исполнения имеет ряд ограничений и в целом является экспериментом. В какой именно среде будет исполнен скрипт решает директива в начале скрипта: 

* `#native` - скрипт будет скомпилировать в Native Runtime
* `#stack` или отсутствие директивы - скрипт будет использован основной средой исполнения.

Назначение: преобразует AST+символы в исполняемую форму и предоставляет нативный бэкенд выполнения, включая встроенные функции.
- Компиляция: Compiler/
  - ModuleCompiler.cs, MethodCompiler.cs — генерация модулей/методов.
  - ExpressionTreeGeneratorBase.cs, BinaryOperationCompiler.cs — выражения/операции.
  - BuiltInFunctionsCache.cs, ContextMethodsCache.cs — кеширование метаданных/встроенных функций.
- Исполнение: Runtime/
  - NativeExecutorProvider.cs — провайдер исполнителя.
  - BuiltInFunctions.cs — реализации встроенных функций.
  - DynamicOperations.cs — динамические операции.


### 3.5 ScriptEngine.HostedScript — хостинг и загрузка библиотек
Назначение: встраивание движка в приложения, управление конфигурациями и библиотеками.
- HostedScriptEngine.cs, Process.cs — процесс выполнения/хостинг.
- LibraryLoader.cs, ComponentLoadingContext.cs — загрузка библиотек/компонент.
- EngineConfigProvider.cs, SystemConfigAccessor.cs — конфигурации.

### 3.6 OneScript.StandardLibrary — стандартная библиотека
Назначение: коллекции, файлы/потоки, текст и кодировки, сеть/HTTP, JSON/XML, ZIP, процессы, таймзоны и др.
- Коллекции: Collections/
  - ArrayImpl.cs, MapImpl.cs, StructureImpl.cs, ValueListImpl.cs.
  - Таблицы/деревья значений: ValueTable.cs (+ ValueTableColumn/Row), ValueTree.cs.
- Файлы/потоки/текст: FileOperations.cs, FileContext.cs, Text/* (TextReadImpl.cs, TextWriteImpl.cs), CustomLineFeedStreamReader.cs.
- Сеть/HTTP: Http/* (HttpRequestContext.cs, HttpResponseContext.cs, HttpRequestBody*, InternetProxyContext.cs).
- JSON: Json/* (JSONReader.cs, JSONWriter.cs, JSONDataExtractor.cs, JSONWriterSettings.cs).
- XML/XSLT: Xml/* (XmlReaderImpl.cs, XmlWriterImpl.cs, XSLTransform.cs).
- ZIP: Zip/* (ZipReader.cs, ZipWriter.cs и перечисления параметров).
- Процессы: Processes/* (ProcessContext.cs, GlobalProcessesFunctions.cs).
- Разное: RandomNumberGenerator.cs, StringOperations.cs, Timezones/*.

### 3.7 OneScript.Web.Server — веб-обёртки для BSL (ASP.NET Core)
Назначение: адаптеры поверх ASP.NET Core API, чтобы работать с HTTP/WebSocket из BSL.
- WebServer.cs — базовая обвязка.
- Http*Wrapper.cs — HttpContext/Request/Response/Cookies.
- WebSockets/* — WebSocketWrapper, WebSocketsManagerWrapper.

### 3.8 Отладка: OneScript.DebugProtocol, OneScript.DebugServices, VSCode.DebugAdapter
- Протокол (OneScript.DebugProtocol):
  - TcpServer/* (DefaultMessageServer.cs, JsonDtoChannel.cs, DispatchingServer.cs) — транспорт и сериализация.
  - Модель отладки: Breakpoint.cs, StackFrame.cs, Variable.cs, DebuggerSettings.cs.
- Сервисы (OneScript.DebugServices):
  - DefaultDebugService.cs, DefaultDebugController.cs — серверные сервисы отладки.
  - ThreadManager.cs, TcpDebugServer.cs — управление потоками и TCP‑сервер.
- Адаптер для VS Code (VSCode.DebugAdapter):
  - DebugSession.cs, OscriptDebugSession.cs — основная сессия и проксирование событий.
  - ServerProcess.cs, DebugeeProcess.cs — управление процессом отлаживаемого приложения.

### 3.9 Приложения/утилиты
- oscript (CLI): src/oscript
  - Program.cs — входная точка.
  - ConsoleHostBuilder.cs — сборка консольного хоста.
  - Поведения (режимы): ExecuteScriptBehavior.cs, CheckSyntaxBehavior.cs, ShowVersionBehavior.cs, DebugBehavior.cs и др.
- StandaloneRunner: сборка самодостаточных пакетов/запуск
  - Program.cs, StandaloneRunner.cs, ProcessLoader.cs.
- OneScriptDocumenter: генерация документации по сборкам
  - На вход получает список DLL (в каталоге с ними рядом должны лежать файлы xml-docs от этих dll) и файл оглавления (в репозитории уже лежит готовый файл оглавления OneScriptDocumenter\default_toc.json)
  - На выходе формирует файл с документацией формата Json и/или каталог с документацией в формате markdown.
- Примеры/демо:
  - Component — простая .NET‑библиотека/компонент и примеры использования.
  - TestApp — WPF‑приложение‑песочница (подсветка синтаксиса, запуск модулей).

### 3.10 ScriptEngine.NativeApi — C++ API для нативных расширений
- Основные файлы: NativeApiProxy.cpp, NativeInterface.cpp, include/* (AddInDefBase.h, ComponentBase.h и др.).
- Задача: писать нативные аддины, видимые в BSL как объекты/контексты.

## 4 Как компоненты связаны между собой (словесная диаграмма)
- Language → даёт AST и ошибки компиляции.
- Core → базовые типы/контексты; используется Native, ScriptEngine, StandardLibrary.
- Native → компилирует и исполняет, опирается на Language/Core.
- ScriptEngine → организует выполнение (стек‑машина), использует Native и Core.
- HostedScript → высокий уровень хостинга поверх ScriptEngine.
- StandardLibrary → реализована на Core/ScriptEngine.
- DebugProtocol/DebugServices ↔ ScriptEngine — обмен данными отладки; VSCode.DebugAdapter ↔ DebugServices.
- Web.Server ↔ ASP.NET Core API — обёртки для работы из BSL.
- oscript/StandaloneRunner/Documenter/TestApp/Component — надстройки поверх ядра/рантайма.
- NativeApi ↔ ScriptEngine/Core — нативные расширения.

## 5 Типичные сценарии доработок (куда лезть)
- Добавить новый тип/значение BSL: OneScript.Core/Values/*, регистрации в фабриках (ScriptEngine/Machine/ValueFactory.cs и/или TypeFactory.cs).
- Добавить объект/контекст с методами: OneScript.Core/Contexts/* (аннотации Context*Attribute), затем регистрация в ScriptEngine/Hosting (DefaultEngineBuilder/ServiceRegistrationExtensions) или через HostedScript.
- Добавить функцию в стандартную библиотеку: соответствующий раздел OneScript.StandardLibrary (например, Json/ или Collections/), плюс экспорт в общий контекст (StandardGlobalContext.cs или SymbolsContext.cs, если нужно).
- Встроенная функция языка/операция: OneScript.Native/Runtime/BuiltInFunctions.cs и/или Compiler/*, при необходимости — поддержка в ScriptEngine/Machine.
- Хостинг/конфигурации: ScriptEngine.HostedScript (HostedScriptEngine, Process, EngineConfigProvider).
- Отладка: DebugServices/DebugProtocol — добавление/изменение событий или представления переменных; VSCode.DebugAdapter — проксирование.

## 6 Навигация по тестам
- C#‑тесты: src/Tests/* (например, OneScript.Core.Tests, OneScript.Language.Tests и др.) — проверка корректности ядра, парсинга, компиляции, стандартной библиотеки.
- Скриптовые тесты: папка tests/* — проверка сценариев языка и библиотек.
- Где искать примеры по областям:
  - Язык/лексер/парсер: src/Tests/OneScript.Language.Tests/*.
  - Типы/контексты/ядро: src/Tests/OneScript.Core.Tests/*.
  - Динамика/встроенные/компиляция: src/Tests/OneScript.Dynamic.Tests/*.
  - Стандартная библиотека: src/Tests/OneScript.StandardLibrary.Tests/* и tests/*.os.

## 7 Локальная сборка и запуск (кратко)
- Сборка: открыть src/1Script.sln и собрать все проекты (или использовать make/скрипты из корня — по желанию).
- CLI‑запуск: после сборки из src/oscript запускать .os‑скрипты (см. tests/* для примеров).
- Отладка: поднять DebugServices, запустить VSCode.DebugAdapter и подключиться из VS Code (см. проекты OneScript.DebugServices и VSCode.DebugAdapter).

## 8 Советы по чтению кода
- Язык/AST: начните с OneScript.Language/DefaultBslParser.cs и AstNodes/ModuleNode.cs.
- Типы/контексты: OneScript.Core/Values/*, Contexts/* и Bsl*Info‑классы.
- Исполнение: ScriptEngine/Machine/StackMachineExecutor.cs и ExecutionContext.cs, затем OneScript.Native/Compiler/*.
- Библиотека: OneScript.StandardLibrary — по подкаталогам доменов (Collections, Json, Xml, Http, Zip и др.).
- Отладка: OneScript.DebugServices/DefaultDebugService.cs и VSCode.DebugAdapter/OscriptDebugSession.cs.