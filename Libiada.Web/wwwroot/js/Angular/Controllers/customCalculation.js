﻿function CustomCalculationController(data) {
    "use strict";

    function customCalculation($scope) {
        MapModelFromJson($scope, data);

        function addSequence() {
            $scope.customSequences.push({});
        }

        function deleteSequence(customSequence) {
            $scope.customSequences.splice($scope.customSequences.indexOf(customSequence), 1);
        }

        function addImageTransformation() {
            $scope.selectedImageTransformers.push($scope.imageTransformers[0]);
        }

        function deleteImageTransformation(transformation) {
            $scope.selectedImageTransformers.splice($scope.selectedImageTransformers.indexOf(transformation), 1);
        }

        $scope.addSequence = addSequence;
        $scope.deleteSequence = deleteSequence;
        $scope.addImageTransformation = addImageTransformation;
        $scope.deleteImageTransformation = deleteImageTransformation;

        $scope.disableSubmit = FakeDisableSubmit;

        $scope.fileType = "literature";
        $scope.customSequences = [];
        $scope.selectedImageTransformers = [];
    }

    angular.module("libiada").controller("CustomCalculationCtrl", ["$scope", customCalculation]);
}
