function SubsequencesComparerResultController() {
    "use strict";

    function subsequencesComparerResult($scope, $http) {
        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];
        $scope.loading = true;
        $http({
            url: "/api/TaskManagerWebApi/" + $scope.taskId,
            method: "GET"
        }).success(function (data) {
            MapModelFromJson($scope, JSON.parse(data));

            for (var i = 0; i < $scope.equalElements.length; i++) {
                $scope.equalElements[i].filtersVisible = [];
            }

            $scope.loading = false;
        }).error(function (data) {
            alert("Failed loading characteristic data");
        });

        // adds and applies new filter
        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });

                for(var i = 0; i < $scope.equalElements.length; i++) {
                    var firstProductId = $scope.getFirstProductAttributeId($scope.equalElements[i]);

                    var firstVisible = firstProductId && $scope.attributeValues[firstProductId].value.toUpperCase().indexOf($scope.newFilter.toUpperCase()) !== -1;

                    var secondProductId = $scope.getSecondProductAttributeId($scope.equalElements[i]);

                    var secondVisible = secondProductId && $scope.attributeValues[secondProductId].value.toUpperCase().indexOf($scope.newFilter.toUpperCase()) !== -1;

                    $scope.equalElements[i].filtersVisible.push(firstVisible || secondVisible);
                }

                $scope.newFilter = "";
            }
            // todo: add error message if filter is empty
        }

        // checks if element is visible
        function elementVisible(element) {
            return element.filtersVisible.length === 0 || element.filtersVisible.some(function (element) {
                return element;
            });
        }

        // deletes given filter
        function deleteFilter(filter) {
            for (var i = 0; i < $scope.equalElements.length; i++) {
                $scope.equalElements[i].filtersVisible.splice($scope.filters.indexOf(filter), 1);
            }

            $scope.filters.splice($scope.filters.indexOf(filter), 1);
        }

        // returns first product attribute index if any
        function getFirstProductAttributeId(equalElement) {
            return $scope.characteristics[equalElement.FirstMatterId][equalElement.FirstSubsequenceId].Attributes.find(function (a) {
                return $scope.attributes[$scope.attributeValues[a].attribute] === "product";
            });
        }

        // returns second product attribute index if any
        function getSecondProductAttributeId(equalElement) {
            return $scope.characteristics[equalElement.SecondMatterId][equalElement.SecondSubsequenceId].Attributes.find(function (a) {
                return $scope.attributes[$scope.attributeValues[a].attribute] === "product";
            });
        }

        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.getFirstProductAttributeId = getFirstProductAttributeId;
        $scope.getSecondProductAttributeId = getSecondProductAttributeId;
        $scope.elementVisible = elementVisible;

        $scope.filters = [];
    }
    
    angular.module("SubsequencesComparerResult", []).controller("SubsequencesComparerResultCtrl", ["$scope", "$http", subsequencesComparerResult]);
}
