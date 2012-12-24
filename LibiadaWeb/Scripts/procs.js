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