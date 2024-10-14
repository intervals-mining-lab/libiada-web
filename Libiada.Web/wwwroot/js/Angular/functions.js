function MapModelFromJson($scope, data) {
    "use strict";

    let param;
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
