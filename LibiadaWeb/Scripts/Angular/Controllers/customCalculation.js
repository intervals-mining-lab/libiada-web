function CustomCalculationController(data) {
    "use strict";

    function customCalculation($scope) {
        MapModelFromJson($scope, data);

        function addCharacteristic() {
            $scope.characteristics.push({
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0]
            });
        }

        function deleteCharacteristic(characteristic) {
            $scope.characteristics.splice($scope.characteristics.indexOf(characteristic), 1);
        }

        function addSequence() {
            $scope.customSequences.push({});
        }

        function deleteSequence(customSequence) {
            $scope.customSequences.splice($scope.customSequences.indexOf(customSequence), 1);
        }

        function addTransformation() {
            $scope.transformations.push({
                link: $scope.transformationLinks[0],
                operation: $scope.operations[0]
            });
        }

        function deleteTransformation(transformation) {
            $scope.transformations.splice($scope.transformations.indexOf(transformation), 1);
        }

        function addImageTransformation() {
            $scope.selectedImageTransformators.push({ value: $scope.imageTransformators[0].Value });
        }

        function deleteImageTransformation(transformation) {
            $scope.selectedImageTransformators.splice($scope.transformations.indexOf(transformation), 1);
        }


        $scope.addCharacteristic = addCharacteristic;
        $scope.deleteCharacteristic = deleteCharacteristic;
        $scope.addSequence = addSequence;
        $scope.deleteSequence = deleteSequence;
        $scope.addTransformation = addTransformation;
        $scope.deleteTransformation = deleteTransformation;
        $scope.addImageTransformation = addImageTransformation;
        $scope.deleteImageTransformation = deleteImageTransformation;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;
        $scope.disableSubmit = FakeDisableSubmit;

        $scope.transformations = [];
        $scope.characteristics = [];
        $scope.customSequences = [];
        $scope.selectedImageTransformators = [];
    }

    angular.module("libiada").controller("CustomCalculationCtrl", ["$scope", customCalculation]);
}
