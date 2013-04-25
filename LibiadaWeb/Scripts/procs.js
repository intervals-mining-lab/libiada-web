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