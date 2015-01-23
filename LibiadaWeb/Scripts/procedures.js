function FilterOptionsByNature($scope, filterFilter, arrayName) {
    "use strict";

    if (angular.isDefined($scope[arrayName])) {
        $scope[arrayName + "Filtered"] = filterFilter($scope[arrayName], { Nature: $scope.natureId });
    }
}

function MapModelFromJson($scope, data) {
    "use strict";

    var param;
    for (param in data) {
        if (data.hasOwnProperty(param)) {
            $scope[param] = data[param];
        }
    }
}