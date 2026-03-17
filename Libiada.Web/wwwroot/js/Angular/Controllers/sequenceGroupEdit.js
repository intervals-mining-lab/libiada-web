import { MapModelFromJson } from "functions";
/**
 * Angular controller class for sequences group edit page
 */
class SequenceGroupEditHandler {
    constructor(data) {
        this.ngOnInit(data);
    }
    ngOnInit(data) {
        const sequenceGroupEdit = ($scope) => {
            MapModelFromJson($scope, data);
            $scope.sequenceGroupTypeIndex ??= 0;
            $scope.sequenceGroupType = $scope.sequenceGroupTypes[$scope.sequenceGroupTypeIndex].Value;
        };
        // Register the controller in Angular
        angular.module("libiada").controller("sequenceGroupEditCtrl", ["$scope", sequenceGroupEdit]);
    }
}
/**
* Wrapper function for backward compatibility
* @param data Data to initialize the controller
* @returns SequenceGroupsEdit Controller instance
*/
export default function SequenceGroupEditController(data) {
    return new SequenceGroupEditHandler(data);
}
//# sourceMappingURL=sequenceGroupEdit.js.map