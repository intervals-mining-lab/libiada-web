import { MapModelFromJson } from "functions";
/**
 * Angular controller class for multisequence creation page
 */
class MultisequenceEditHandler {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const multisequenceEdit = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
            // Initialize properties with default values
            $scope.nature = $scope.natures[0].Value;
            $scope.name = "";
            $scope.displayMultisequenceNumber = true;
        };
        // Register controller in Angular module
        angular.module("libiada").controller("MultisequenceEditCtrl", ["$scope", "filterFilter", multisequenceEdit]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of MultisequenceEditHandler
 */
export default function MultisequenceEditController(data) {
    return new MultisequenceEditHandler(data);
}
//# sourceMappingURL=multisequenceEdit.js.map