﻿#native

Процедура ПриСозданииОбъекта()
	
КонецПроцедуры

Функция МетодСтроки(Текст1 = "Т1 ", Текст2 = "Т2 ") Экспорт
	Возврат Текст1 + Текст2;
КонецФункции

Функция МетодБулево(Булево = Истина) Экспорт
	
	Если Булево = Истина Или Булево Тогда
		Возврат 1;
	Иначе 
		Возврат 0;
	КонецЕсли;

КонецФункции

Функция МетодНеопределено(Параметр = Неопределено) Экспорт
	
	Если Параметр = Неопределено Тогда
		Возврат 1;
	Иначе 
		Возврат 0;
	КонецЕсли;

КонецФункции

Функция МетодЧисло(Параметр1 = -1, Параметр2 ) Экспорт
	
	Если Параметр1 = -1 Тогда
		Возврат 1;
	Иначе 
		Возврат 0 * Параметр2;
	КонецЕсли;

КонецФункции