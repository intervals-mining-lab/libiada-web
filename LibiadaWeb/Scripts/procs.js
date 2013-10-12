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

function CreateOption(value, text) {
    return $('<option/>').attr('value', value).text(text);
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

function CreateCharacteristicsBlock(divNumber, paramsList) {
    var characteristicsDiv = $("<div/>").attr('name', 'characteristc' + divNumber).attr('id', 'characteristic' + divNumber);

    for(var name in paramsList) {
        var selectList = CreateSelectList(paramsList[name], name);
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