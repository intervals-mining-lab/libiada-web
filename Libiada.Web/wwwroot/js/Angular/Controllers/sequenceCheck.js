import { MapModelFromJson } from "functions";
/**
 * Controller for sequence check
 */
class SequenceCheckOperator {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    ngOnInit(data) {
        const sequenceCheck = ($scope) => {
            MapModelFromJson($scope, data);
            /**
             * Handles file selection change event
             * @param filePath The input element with file path
             */
            function fileChanged(filePath) {
                if (filePath.value) {
                    $scope.fileSelected.value = true;
                }
                else {
                    $scope.fileSelected.value = false;
                }
                $scope.$apply();
            }
            // Initialize file selected status
            $scope.fileSelected = { value: false };
            $scope.fileChanged = fileChanged;
        };
        // Register controller in Angular module
        angular.module("libiada").controller("SequenceCheckCtrl", ["$scope", sequenceCheck]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of sequence check operator
 */
export default function SequenceCheckController(data) {
    return new SequenceCheckOperator(data);
}
//# sourceMappingURL=sequenceCheck.js.map