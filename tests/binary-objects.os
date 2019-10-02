﻿
Перем юТест;

////////////////////////////////////////////////////////////////////
// Программный интерфейс

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт
	
	юТест = ЮнитТестирование;
	
	ВсеТесты = Новый Массив;
	
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоСоздаетсяБуферДвоичныхДанных");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоМожноЗаписатьБайты");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоМожноПрочитатьБайты");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое16");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое16BigEndian");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое32");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое32BigEndian");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое64");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое64BigEndian");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоСоздаетсяФайловыйПоток");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоСоздаетсяПотокВПамяти");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоВозможнаЗаписьВФайл");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоСчитываетсяТекстДвоичнымЧтением");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоЧтениеДанныхЧитаетВсеДанныеМетодомПрочитатьСимволы");
	
	Возврат ВсеТесты;
	
КонецФункции

Процедура ПослеЗапускаТеста() Экспорт
	
	юТест.УдалитьВременныеФайлы();
	
КонецПроцедуры

Функция ПолучитьБуферСДанными()

	Буфер = Новый БуферДвоичныхДанных(10);
	Для Сч = 0 По 9 Цикл
		Буфер[Сч] = Сч+1;
	КонецЦикла;

	Возврат Буфер;
	
КонецФункции

Процедура ТестДолжен_ПроверитьЧтоСоздаетсяБуферДвоичныхДанных() Экспорт
	
	Б = ПолучитьБуферСДанными();
	юТест.ПроверитьРавенство(10, Б.Размер);
	Для Сч = 0 По Б.Размер-1 Цикл
		юТест.ПроверитьРавенство(Сч+1, Б[Сч], "Проверка значения в индексе " + Сч);
	КонецЦикла;
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоМожноЗаписатьБайты() Экспорт
	
	Б = ПолучитьБуферСДанными();
	Чистый = Новый БуферДвоичныхДанных(Б.Размер);
	
	Чистый.Записать(0, Б);
	юТест.ПроверитьРавенство(Б.Размер, Чистый.Размер);
	Для Сч = 0 По Чистый.Размер-1 Цикл
		юТест.ПроверитьРавенство(Б[Сч], Чистый[Сч], "Сверка в индексе " + Сч);
		юТест.ПроверитьРавенство(Сч+1, Чистый[Сч], "Сверка значения в индексе " + Сч);
	КонецЦикла;
	
	Чистый = Новый БуферДвоичныхДанных(5);
	Чистый.Записать(0, Б, 5);
	Для Сч = 0 По Чистый.Размер-1 Цикл
		юТест.ПроверитьРавенство(Б[Сч], Чистый[Сч], "Сверка частичной записи в индексе " + Сч);
	КонецЦикла;
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоМожноПрочитатьБайты() Экспорт
	
	Б = ПолучитьБуферСДанными();
	Прочитанный = Б.Прочитать(2,2);
	
	юТест.ПроверитьРавенство(2, Прочитанный.Размер);
	юТест.ПроверитьРавенство(3, Прочитанный[0]);
	юТест.ПроверитьРавенство(4, Прочитанный[1]);
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое16() Экспорт
	
	Б = Новый БуферДвоичныхДанных(4);
	Б.ЗаписатьЦелое16(0, 255);
	юТест.ПроверитьРавенство(255, Б.ПрочитатьЦелое16(0));
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое16BigEndian() Экспорт
	
	Б = Новый БуферДвоичныхДанных(4);
	Б.ЗаписатьЦелое16(0, 255);
	юТест.ПроверитьРавенство(65280, Б.ПрочитатьЦелое16(0, ПорядокБайтов.BigEndian));
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое32() Экспорт
	
	юТест.ПодробныеОписанияОшибок(Истина);
	
	Б = Новый БуферДвоичныхДанных(4);
	Б.ЗаписатьЦелое32(0, 4278190080);
	юТест.ПроверитьРавенство(4278190080, Б.ПрочитатьЦелое32(0));
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое32BigEndian() Экспорт
	
	Б = Новый БуферДвоичныхДанных(4);
	Б.ЗаписатьЦелое32(0, 255);
	юТест.ПроверитьРавенство(4278190080, Б.ПрочитатьЦелое32(0, ПорядокБайтов.BigEndian));
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое64() Экспорт
	
	Б = Новый БуферДвоичныхДанных(8);
	Б.ЗаписатьЦелое32(0, 255);
	юТест.ПроверитьРавенство(255, Б.ПрочитатьЦелое32(0));
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоМожноЗаписатьПрочитатьЦелое64BigEndian() Экспорт
	
	Б = Новый БуферДвоичныхДанных(8);
	Б.ЗаписатьЦелое64(0, 255);
	юТест.ПроверитьРавенство(18374686479671623680, Б.ПрочитатьЦелое64(0, ПорядокБайтов.BigEndian));
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоСоздаетсяПотокВПамяти() Экспорт

	ПотокСЕмкостью = Новый ПотокВПамяти(100);
	ПростоПоток = Новый ПотокВПамяти;
	Буфер = Новый БуферДвоичныхДанных(10);
	ПотокПоБуферу = Новый ПотокВПамяти(Буфер);
	юТест.ПроверитьРавенство(10, ПотокПоБуферу.Размер());

	ПотокПоБуферу.КопироватьВ(ПотокСЕмкостью);
	юТест.ПроверитьРавенство(10, ПотокСЕмкостью.Размер());
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоСоздаетсяФайловыйПоток() Экспорт
	
	Поток = ФайловыеПотоки.Открыть(ТекущийСценарий().Источник, РежимОткрытияФайла.Открыть);
	Поток.Закрыть();
	
	Поток = Новый ФайловыйПоток(ТекущийСценарий().Источник, РежимОткрытияФайла.Открыть);
	Поток.Закрыть();
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоВозможнаЗаписьВФайл() Экспорт
	Буфер = ПолучитьБуферСДанными();
	Файл = юТест.ИмяВременногоФайла();
	Поток = ФайловыеПотоки.Создать(Файл);
	Поток.Записать(Буфер, 0, Буфер.Размер);
	Поток.Закрыть();
	
	ФайлОбъект = Новый Файл(Файл);
	юТест.ПроверитьИстину(ФайлОбъект.Существует());
	юТест.ПроверитьРавенство(10, ФайлОбъект.Размер());
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоСчитываетсяТекстДвоичнымЧтением() Экспорт
	ДД = Новый ДвоичныеДанные(ОбъединитьПути(ТекущийСценарий().Каталог, "templateslib/TemplateData.txt"));
	ЧтениеДанных = Новый ЧтениеДанных(ДД);
	Строка = ЧтениеДанных.ПрочитатьСтроку();
	
	юТест.ПроверитьРавенство("ПРИВЕТ, Я МАКЕТ", Строка);
КонецПроцедуры

Процедура ТестДолжен_ПроверитьЧтоЧтениеДанныхЧитаетВсеДанныеМетодомПрочитатьСимволы() Экспорт

	ДД = Новый ДвоичныеДанные(ОбъединитьПути(ТекущийСценарий().Каталог, "templateslib/TemplateData.txt"));
	ЧтениеДанных = Новый ЧтениеДанных(ДД);
	ЧтениеДанных.Пропустить(3);
	Строка = ЧтениеДанных.ПрочитатьСимволы();

	юТест.ПроверитьРавенство("ПРИВЕТ, Я МАКЕТ", Строка, "Неопределено");
	ЧтениеДанных.Закрыть();

	ЧтениеДанных = Новый ЧтениеДанных(ДД);
	ЧтениеДанных.Пропустить(3);
	Строка = ЧтениеДанных.ПрочитатьСимволы(0);
	юТест.ПроверитьРавенство("ПРИВЕТ, Я МАКЕТ", Строка, "Ноль символов");
	ЧтениеДанных.Закрыть();

КонецПроцедуры
