﻿# OneScript.Web.Server #

## Проект является интерфейсом для использования веб-сервера Kestrel из состава ASP.NET Core для разработки веб-приложений ##

Проект добавляет с систему типов OneScript новый класс 

```bsl
ВебСервер
```

#### Инициализация объекта сервера и запуск:

С портом по умолчанию (8080):

```bsl
ВебСервер = Новый ВебСервер();
ВебСервер.Запустить();
```

На указанном порту:

```bsl
Порт = 7000;
ВебСервер = Новый ВебСервер(Порт);
ВебСервер.Запустить();
```

#### Обработка запросов осуществляется посредством внедрения в конвейер обработки запросов собственных middleware:

Middleware принимает в качестве параметра объект HTTPКонтекст, предоставляющий информацию о соединении, входящем и исходящем запросе и 2-м параметров делегат на вызов следующего middleware в контейнере. 
От порядка регистрации обработчиков в конвейере зависит порядок обработки запросах и то, делегат на какой обработчик придет в параметре "СледующийОбработчик" 

```bsl
Процедура МаппингСтраниц(Контекст, СледующийОбработчик) Экспорт
	
	Путь = ПолучитьПутьСтраницы(Контекст);

	Если Новый Файл(Путь).Существует() Тогда
		ЗаписатьФайлВОтвет(Контекст.Ответ, Путь);
	Иначе
		СледующийОбработчик.Вызвать(Контекст);
	КонецЕсли;

КонецПроцедуры

Порт = 7000;
ВебСервер = Новый ВебСервер(Порт);

ВебСервер.ДобавитьОбработчикЗапросов(ЭтотОбъект, "МаппингСтраниц");

ВебСервер.Запустить();
```

#### Сервер позволяет обрабатывать входящие вебсокет подключения, так-же через внедрение в конвейер middleware и вызова специального метода, включающего обработку протокола ws:

```bsl
Процедура ОбработчикВебСокетПодключений(Контекст, СледующийОбработчик) Экспорт

	Если Контекст.ВебСокеты.ЭтоВебСокетЗапрос Тогда

		Сокет = Контекст.ВебСокеты.ПодключитьВебСокет();

		Пока Сокет.Состояние = СостояниеВебСокета.Открыт Цикл

			ПринятаяСтрока = Сокет.ПолучитьСтроку();
			ПринятыеДанные = ДесериализоватьJson(ПринятаяСтрока);

			Сообщение = Новый Структура("user, message", "Server", "You sent: " + ПринятыеДанные.message);

			Сокет.ОтправитьСтроку(СериализоватьJson(Сообщение));

		КонецЦикла;

	Иначе
		СледующийОбработчик.Вызвать(Контекст);
	КонецЕсли;

КонецПроцедуры

Порт = 7000;
ВебСервер = Новый ВебСервер(Порт);

ВебСервер.ИспользоватьВебСокеты();
ВебСервер.ДобавитьОбработчикЗапросов(ЭтотОбъект, "ОбработчикВебСокетПодключений");

ВебСервер.Запустить();
```