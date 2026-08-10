import { MapModelFromJson } from "functions";
/**
 * Angular controller class for multisequence creation page
 */
class MultisequenceCreateHandler {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const multisequenceCreate = ($scope) => {
            MapModelFromJson($scope, data);
            // Initialize properties with default values
            $scope.nature = $scope.natures[0].Value;
            $scope.name = "";
            $scope.displayMultisequenceNumber = true;
        };
        // Register controller in Angular module
        angular.module("libiada").controller("MultisequenceCreateCtrl", ["$scope", multisequenceCreate]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of MultisequenceCreateHandler
 */
export default function MultisequenceCreateController(data) {
    return new MultisequenceCreateHandler(data);
}
//# sourceMappingURL=multisequenceCreate.js.map