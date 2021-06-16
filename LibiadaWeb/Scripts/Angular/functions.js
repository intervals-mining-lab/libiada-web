function MapModelFromJson($scope, data) {
    "use strict";

    var param;
    for (param in data) {
        if (data.hasOwnProperty(param)) {
            $scope[param] = data[param];
        }
    }
}

function SelectLink(characteristic) {
    "use strict";

    characteristic.link = characteristic.characteristicType.Links[0];
    characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
}

function SetCheckBoxesState(checkboxes, state, filter) {
    "use strict";

    angular.forEach(checkboxes, function (item) {
        item.Selected = state;
        if (filter) {
            filter(item);
        }
    });
}

function FakeFilterByFeature() {
    return false;
}

function FakeDisableSubmit() {
    return false;
}