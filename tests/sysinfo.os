﻿///////////////////////////////////////////////////////////////////////
//
// Тест класса СистемнаяИнформация
// 
//
///////////////////////////////////////////////////////////////////////

Перем юТест;

////////////////////////////////////////////////////////////////////
// Программный интерфейс

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт
	
	юТест = ЮнитТестирование;
	
	ВсеТесты = Новый Массив;
	
	ВсеТесты.Добавить("ТестДолжен_ПолучитьЗначенияОкружения");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПеременнуюPATH");
	ВсеТесты.Добавить("ТестДолжен_УстановитьПеременную");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьВерсиюOneScript");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПользователяОС");
	
	// Значения зависят от машины запуска
	ВсеТесты.Добавить("ТестДолжен_ПолучитьЭто64БитнаяОперационнаяСистема");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьТипПлатформы");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьКоличествоПроцессоров");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьРазмерСистемнойСтраницы");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьВремяРаботыСМоментаЗагрузки");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьИменаЛогическихДисков");
	// Пути к папкам
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_РепозиторийДокументов");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_ДанныеПриложений");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_ЛокальныйКаталогДанныхПриложений");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_РабочийСтол");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_КаталогРабочийСтол");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_МояМузыка");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_МоиРисунки");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_Шаблоны");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_МоиВидеозаписи");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_ОбщиеШаблоны");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_ПрофильПользователя");
	ВсеТесты.Добавить("ТестДолжен_ПолучитьПолучитьПутьПапки_ОбщийКаталогДанныхПриложения");
	
	Возврат ВсеТесты;
КонецФункции

Процедура ТестДолжен_ПолучитьЗначенияОкружения() Экспорт
	
	Переменные = ПеременныеСреды();
	
	юТест.ПроверитьЛожь(Переменные.Количество() = 0);
	
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПеременнуюPATH() Экспорт
	
	юТест.ПроверитьЛожь(ПустаяСтрока(ПолучитьПеременнуюСреды("PATH")));
	
КонецПроцедуры

Процедура ТестДолжен_УстановитьПеременную() Экспорт
	
	СтароеЗначение = ПолучитьПеременнуюСреды("OS");
	НовоеЗначение = "NewTestValue";
	
	УстановитьПеременнуюСреды("OS", НовоеЗначение);
	
	юТест.ПроверитьРавенство(НовоеЗначение, ПолучитьПеременнуюСреды("OS"));
	
КонецПроцедуры

Процедура ТестДолжен_ПолучитьВерсиюOneScript() Экспорт
	
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьЛожь(ПустаяСтрока(Си.Версия));
	
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПользователяОС() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьЛожь(ПустаяСтрока(Си.ПользовательОС));
КонецПроцедуры

Процедура ТестДолжен_ПолучитьЭто64БитнаяОперационнаяСистема() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьИстину(Си.Это64БитнаяОперационнаяСистема);
КонецПроцедуры

Процедура ТестДолжен_ПолучитьТипПлатформы() Экспорт
	СИ = Новый СистемнаяИнформация;
	Если Найти(СИ.ВерсияОС, "Windows") > 0 Тогда
		Если Си.Это64БитнаяОперационнаяСистема Тогда
			юТест.ПроверитьРавенство(СИ.ТипПлатформы, ТипПлатформы.Windows_x86_64);
		Иначе
			юТест.ПроверитьРавенство(СИ.ТипПлатформы, ТипПлатформы.Windows_x86);
		КонецЕсли;
	Иначе
		Если Си.Это64БитнаяОперационнаяСистема Тогда
			юТест.ПроверитьРавенство(СИ.ТипПлатформы, ТипПлатформы.Linux_x86_64);
		Иначе
			юТест.ПроверитьРавенство(СИ.ТипПлатформы, ТипПлатформы.Linux_x86);
		КонецЕсли;
	КонецЕсли;
КонецПроцедуры

Процедура ТестДолжен_ПолучитьКоличествоПроцессоров() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьБольше(Си.КоличествоПроцессоров,0);
КонецПроцедуры

Процедура ТестДолжен_ПолучитьРазмерСистемнойСтраницы() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьБольше(Си.РазмерСистемнойСтраницы,0);
КонецПроцедуры

Процедура ТестДолжен_ПолучитьВремяРаботыСМоментаЗагрузки() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьБольше(Си.ВремяРаботыСМоментаЗагрузки,0);
КонецПроцедуры

Процедура ТестДолжен_ПолучитьИменаЛогическихДисков() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьБольше(Си.ИменаЛогическихДисков.Количество(),0);
КонецПроцедуры

///////////////////////////////////////////////////////////////////////
//
// Проверка спец.папок
//
///////////////////////////////////////////////////////////////////////

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_РепозиторийДокументов() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.МоиДокументы),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_ДанныеПриложений() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.ДанныеПриложений),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_ЛокальныйКаталогДанныхПриложений() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.ЛокальныйКаталогДанныхПриложений),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_РабочийСтол() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.РабочийСтол),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_КаталогРабочийСтол() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.КаталогРабочийСтол),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_МояМузыка() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.МояМузыка),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_МоиРисунки() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.МоиРисунки),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_Шаблоны() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.Шаблоны),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_МоиВидеозаписи() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.МоиВидеозаписи),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_ОбщиеШаблоны() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.ОбщиеШаблоны),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_ПрофильПользователя() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.ПрофильПользователя),"");
КонецПроцедуры

Процедура ТестДолжен_ПолучитьПолучитьПутьПапки_ОбщийКаталогДанныхПриложения() Экспорт
	Си = Новый СистемнаяИнформация();
	юТест.ПроверитьНеРавенство(СИ.ПолучитьПутьПапки(СпециальнаяПапка.ОбщийКаталогДанныхПриложения),"");
КонецПроцедуры