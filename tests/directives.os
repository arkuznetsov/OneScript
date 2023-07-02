Перем юТест;
Перем мОшибкаКомпиляции;

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт

	юТест = ЮнитТестирование;
	ВсеТесты = Новый Массив;

	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоОбластиКомпилируются");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоИспользоватьНеКомпилируетсяЕслиНачалсяКод");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоМногострочныеДирективыНеКомпилируются");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоИсключенныеСтрокиПропускаются");

	Возврат ВсеТесты;

КонецФункции

Функция ТестДолжен_ПроверитьЧтоОбластиКомпилируются() Экспорт

	ПодключитьСценарий("preprocessor/regions.os", "Области");
	ПроверкаОбластей = Новый ("Области");

	юТест.ПроверитьРавенство(ПроверкаОбластей.Проверка1(), "Region");
	юТест.ПроверитьРавенство(ПроверкаОбластей.Проверка2(), "Вне области");
	юТест.ПроверитьРавенство(ПроверкаОбластей.Проверка3(), "Область");

КонецФункции

Функция ФайлКомпилируется(Знач ПутьКФайлу, Знач ИмяПодключения = "НеПодключится")

	Попытка
		ПодключитьСценарий(ПутьКФайлу, ИмяПодключения);
	Исключение
		мОшибкаКомпиляции = ПодробноеПредставлениеОшибки(ИнформацияОбОшибке());
		Возврат Ложь;
	КонецПопытки;

	Возврат Истина;

КонецФункции

Функция ТестДолжен_ПроверитьЧтоИспользоватьНеКомпилируетсяЕслиНачалсяКод() Экспорт

	юТест.ПроверитьРавенство(ФайлКомпилируется("preprocessor/use-fail.os"), Ложь,
		"Файл use-fail.os компилируется, хотя не должен!"
	);

КонецФункции

Функция ТестДолжен_ПроверитьЧтоМногострочныеДирективыНеКомпилируются() Экспорт

	юТест.ПроверитьРавенство(ФайлКомпилируется("preprocessor/multiline-fail.os"), Ложь,
		"Файл multiline-fail.os компилируется, хотя не должен!"
	);

КонецФункции

Функция ТестДолжен_ПроверитьЧтоИсключенныеСтрокиПропускаются() Экспорт

	юТест.ПроверитьРавенство(ФайлКомпилируется("preprocessor/excluded.os"), Истина,
		"Файл excluded.os не компилируется, хотя должен!
		|" + мОшибкаКомпиляции
	);

КонецФункции

