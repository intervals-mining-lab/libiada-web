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

    characteristic.link = characteristic.characteristicType.CharacteristicLinks[0];
}

function IsLinkable(characteristic) {
    "use strict";

    return characteristic.characteristicType.CharacteristicLinks.length > 1;
}

function SetCheckBoxesState(checkboxes, state) {
    "use strict";

    angular.forEach(checkboxes, function (item) {
        item.Selected = state;
    });
}

function FakeDisableMattersSelect() {
    return false;
};

function FakeFilterByFeature() {
    return false;
}

function FakeDisableSubmit() {
    return false;
};