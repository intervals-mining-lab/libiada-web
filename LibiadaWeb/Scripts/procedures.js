function AppendLabel(elem, text) {
    elem.append($("<div/>").attr("class", "editor-label").text(text));
}

function AppendBr(elem) {
    elem.append($("<br/>"));
}

function DeleteElementByName(elemName) {
    $("#" + elemName).remove();
    return false;
}

function CreateOption(value, text) {
    return $("<option/>").attr("value", value).text(text);
}

function CreateSelectList(options, name) {
    var selectList = $("<select/>").attr("name", name);
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

function SwitchVisibilityByName(name) {
    var element = $("#" + name)[0];
    SwitchVisibility(element);
}

function CreateCharacteristicsBlock(divNumber, paramsList) {
    var characteristicsDiv = $("<div/>").attr("name", "characteristc" + divNumber).attr("id", "characteristic" + divNumber);

    for(var name in paramsList) {
        var selectList = CreateSelectList(paramsList[name], name);
        characteristicsDiv.append(selectList); 
    }

    //функция, возвращающая функцию для call back"а
    var deleteFunction = function (element) {
        return function () {
            DeleteElementByName(element.elemName);
        };
    };
    var callBackDeleteFunction = deleteFunction({ elemName: "characteristic" + divNumber });
    characteristicsDiv.append($("<input/>").attr("type", "button").attr("value", "Удалить").click(callBackDeleteFunction));
    AppendBr(characteristicsDiv);

    return characteristicsDiv;
}

function AddDataTables() {
    window.formTable = $("table").dataTable({
        aLengthMenu: [[25, 50, 100, -1], [25, 50, 100, "Все"]],
        bJQueryUI: true,
        sPaginationType: "full_numbers"
    });
}

function AddDataTablesWithSubmit(name) {
    AddDataTables();
    name = name || "form";
    $(name).submit(function () {
        $(formTable.fnGetHiddenNodes()).find("input:checked").attr("hidden", true).appendTo(this);
    });
}

function FilterOptionsByNature($scope, filterFilter, arrayName) {
    if (angular.isDefined($scope[arrayName])) {
        $scope[arrayName + "Filtered"] = filterFilter($scope[arrayName], { Nature: $scope.natureId });
    }
}

function MapModelFromJson($scope, data) {
    for (var param in data) {
        $scope[param] = data[param];
    }
}