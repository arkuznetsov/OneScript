﻿Перем юТест;

Перем ВнешняяКомпонента;

Перем ЗаписьXML, ЗаписьZIP, КаталогСкрипта, КаталогПакета, СуффиксВерсии;

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт
	
	юТест = ЮнитТестирование;
	
	ВсеТесты = Новый Массив;
	
	СистемнаяИнформация = Новый СистемнаяИнформация;
	ЭтоWindows = Найти(НРег(СистемнаяИнформация.ВерсияОС), "windows") > 0;
	Если ЭтоWindows Тогда
		ВсеТесты.Добавить("ТестДолжен_ПроверитьПодключениеВнешнейКомпонентыZIP");
		ВсеТесты.Добавить("ТестДолжен_ПроверитьПодключениеВнешнейКомпонентыDLL");
		ВсеТесты.Добавить("ТестДолжен_ПроверитьСвойстваВнешнейКомпоненты");
		ВсеТесты.Добавить("ТестДолжен_ПроверитьМетодыВнешнейКомпоненты");
	КонецЕсли;
	
	Возврат ВсеТесты;
	
КонецФункции

Процедура ТестДолжен_ПроверитьПодключениеВнешнейКомпонентыZIP() Экспорт

	ИмяФайлаПакета = юТест.ИмяВременногоФайла();
	ЗаписьZIP = Новый ЗаписьZipФайла(ИмяФайлаПакета); 
	Каталог = ТекущийСценарий().Каталог + "\native-api\";
	ЗаписьZIP.Добавить(Каталог + "MANIFEST.XML", РежимСохраненияПутейZIP.НеСохранятьПути);

	МасивФайлов = Новый Массив;
	МасивФайлов.Добавить("bin\AddInNativeWin32.dll");
	МасивФайлов.Добавить("bin64\AddInNativeWin64.dll");
	МасивФайлов.Добавить("build32\libAddInNativeLin32.so");
	МасивФайлов.Добавить("build64\libAddInNativeLin64.so");
	
	Для каждого ИмяФайла из МасивФайлов Цикл
		ПутьФайла = Каталог + ИмяФайла;
		Файл = Новый Файл(ПутьФайла);
		Если Файл.Существует() Тогда
			ЗаписьZIP.Добавить(ПутьФайла, РежимСохраненияПутейZIP.НеСохранятьПути);
		КонецЕсли;
	КонецЦикла;

	ЗаписьZIP.Записать();

	Идентификатор = "AddinNativeZip";
	Успешно = ПодключитьВнешнююКомпоненту(ИмяФайлаПакета, Идентификатор, ТипВнешнейКомпоненты.Native);
	юТест.ПроверитьРавенство(Успешно, Истина);

	ИмяВнешнейКомпоненты = "AddIn." + Идентификатор + ".CAddInNative";
	ВнешняяКомпонента = Новый(ИмяВнешнейКомпоненты);

	юТест.ПроверитьРавенство(Строка(ТипЗнч(ВнешняяКомпонента)), Строка(Тип(ИмяВнешнейКомпоненты)));

КонецПроцедуры	

Процедура ТестДолжен_ПроверитьПодключениеВнешнейКомпонентыDLL() Экспорт

	Идентификатор = "AddinNativeDLL";
	ПутьБиблиотеки = ТекущийСценарий().Каталог + "\native-api\";
	Это32бит = ПодключитьВнешнююКомпоненту(ПутьБиблиотеки + "bin\AddInNativeWin32.dll", Идентификатор, ТипВнешнейКомпоненты.Native);
	Это64бит = ПодключитьВнешнююКомпоненту(ПутьБиблиотеки + "bin64\AddInNativeWin64.dll", Идентификатор, ТипВнешнейКомпоненты.Native);

	юТест.ПроверитьРавенство(Это32бит И Это64бит, Ложь);
	юТест.ПроверитьРавенство(Это32бит ИЛИ Это64бит, Истина);

	ИмяВнешнейКомпоненты = "AddIn." + Идентификатор + ".CAddInNative";
	ВнешняяКомпонента = Новый(ИмяВнешнейКомпоненты);

	ВнешняяКомпонента.Включить();
	юТест.ПроверитьРавенство(ВнешняяКомпонента.Включен, Истина);

	ВнешняяКомпонента.Выключить();
	юТест.ПроверитьРавенство(ВнешняяКомпонента.Включен, Ложь);

КонецПроцедуры

Процедура ТестДолжен_ПроверитьСвойстваВнешнейКомпоненты() Экспорт

	ВнешняяКомпонента.Включен = Истина;
	юТест.ПроверитьРавенство(ВнешняяКомпонента.Включен, Истина);
	юТест.ПроверитьРавенство(ВнешняяКомпонента.IsEnabled, Истина);

	ВнешняяКомпонента.Включен = Ложь;
	юТест.ПроверитьРавенство(ВнешняяКомпонента.Включен, Ложь);
	юТест.ПроверитьРавенство(ВнешняяКомпонента.IsEnabled, Ложь);

	СодержимоеСтроки = "Тест строки только на запись";
	ВнешняяКомпонента.СтрокаТолькоЗапись = СодержимоеСтроки;
	юТест.ПроверитьРавенство(ВнешняяКомпонента.СтрокаЧтениеЗапись, СодержимоеСтроки);
	юТест.ПроверитьРавенство(ВнешняяКомпонента.СтрокаТолькоЧтение, СодержимоеСтроки);

	СодержимоеСтроки = "Тест строки на чтение и запись";
	ВнешняяКомпонента.СтрокаЧтениеЗапись = СодержимоеСтроки;
	юТест.ПроверитьРавенство(ВнешняяКомпонента.СтрокаЧтениеЗапись, СодержимоеСтроки);
	юТест.ПроверитьРавенство(ВнешняяКомпонента.СтрокаТолькоЧтение, СодержимоеСтроки);

	Попытка
		ВнешняяКомпонента.СтрокаТолькоЧтение = СодержимоеСтроки;
		юТест.ТестПровален("Удалось изменить недоступное для записи свойство");
	Исключение
		юТест.ТестПройден();
	КонецПопытки;

	Попытка
		СодержимоеСтроки = ВнешняяКомпонента.СтрокаТолькоЗапись;
		юТест.ТестПровален("Удалось прочитать недоступное для чтения свойство");
	Исключение
		юТест.ТестПройден();
	КонецПопытки;

	Попытка
		ВнешняяКомпонента.СвойствоОтсутствует = СодержимоеСтроки;
		юТест.ТестПровален("Удалось обратиться к отсутствующему свойству");
	Исключение
		юТест.ТестПройден();
	КонецПопытки;

КонецПроцедуры	

Процедура ТестДолжен_ПроверитьМетодыВнешнейКомпоненты() Экспорт

	ВнешняяКомпонента.Включить();
	юТест.ПроверитьРавенство(ВнешняяКомпонента.Включен, Истина);

	ВнешняяКомпонента.Выключить();
	юТест.ПроверитьРавенство(ВнешняяКомпонента.Включен, Ложь);

	Попытка
		ВнешняяКомпонента.Выключить(Истина);
		юТест.ТестПровален("Удалось передать параметр в метод без параметров");
	Исключение
		юТест.ТестПройден();
	КонецПопытки;

	юТест.ПроверитьРавенство(ВнешняяКомпонента.ПараметрПоУмолчанию(), Истина);
	юТест.ПроверитьРавенство(ВнешняяКомпонента.ПараметрПоУмолчанию(Ложь), Ложь);

	Попытка
		Значение = ВнешняяКомпонента.ПараметрПоУмолчанию(Истина, Ложь);
		юТест.ТестПровален("Удалось передать лишний параметр в метод с одним параметром");
	Исключение
		юТест.ТестПройден();
	КонецПопытки;

	СодержимоеФайла = "Тест двоичных данных";
	ВременныйФайл = юТест.ИмяВременногоФайла();
	ЗаписьТекста = Новый ЗаписьТекста(ВременныйФайл, КодировкаТекста.UTF8);
	ЗаписьТекста.Записать(СодержимоеФайла);
	ЗаписьТекста.Закрыть();

	ДвоичныеДанные = Новый ДвоичныеДанные(ВременныйФайл);
	ДвоичныеДанные = ВнешняяКомпонента.Петля(ДвоичныеДанные);
	Поток = ДвоичныеДанные.ОткрытьПотокДляЧтения();

	ЧтениеТекста = Новый ЧтениеТекста(Поток, КодировкаТекста.UTF8);
	юТест.ПроверитьРавенство(ЧтениеТекста.Прочитать(), СодержимоеФайла);

КонецПроцедуры	

