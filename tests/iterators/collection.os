﻿#Обходимое

Функция ПолучитьИтератор()
    Возврат Новый ИтераторКоллекции(ЭтотОбъект);
КонецФункции

Функция Количество() Экспорт
    Возврат 10;
КонецФункции

Функция Получить(Индекс) Экспорт
    Если Индекс < 10 тогда
        Возврат Индекс;
    КонецЕсли;
    
    ВызватьИсключение "Индекс выходит за границы коллекции";
КонецФункции