function AppendLabel(elem, text) {
    elem.append($('<div/>').attr('class', 'editor-label').text(text));
}

function AppendBr(elem) {
    elem.append($('<br/>'));
}

function DeleteElementByName(elemName) {
    $('#' + elemName).remove();
    return false;
}

function CreateSelectList(options, name) {
    var selectList = $('<select/>').attr('name', name);
    for (var i = 0; i < options.length; i++) {
        selectList.append(options[i]);
    }
    return selectList;
}

function SwitchVisibility(element) {
    if (element.hidden) {
        element.hidden = false;
    } else {
        element.hidden = true;
    }
}

function CreateCharacteristicsBlock(divNumber, characteristicTypes, linkUps, notations, languages) {
    var characteristicsDiv = $("<div/>").attr('name', 'characteristc' + divNumber).attr('id', 'characteristic' + divNumber);


    //TODO: свернять это всё в вызов одной функции (заодно свернуть принимаемые параметры в один объект)
    var selectList = CreateSelectList(characteristicTypes, 'characteristicIds');
    characteristicsDiv.append(selectList);

    selectList = CreateSelectList(linkUps, 'linkUpIds');
    characteristicsDiv.append(selectList);

    if (notations != undefined) {
        selectList = CreateSelectList(notations, 'notationIds');
        characteristicsDiv.append(selectList);
    }

    if (languages != undefined) {
        selectList = CreateSelectList(languages, 'languageIds');
        characteristicsDiv.append(selectList);
    }

    //функция, возвращающая функцию для call back'а
    var deleteFunction = function (element) {
        return function () {
            DeleteElementByName(element.elemName);
        };
    };
    var callBackDeleteFunction = deleteFunction({ elemName: 'characteristic' + divNumber });
    characteristicsDiv.append($('<input/>').attr('type', 'button').attr('value', 'Удалить').click(callBackDeleteFunction));
    AppendBr(characteristicsDiv);

    return characteristicsDiv;
}