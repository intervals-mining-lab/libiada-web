function SubsequencesComparerResultController() {
    "use strict";

    function subsequencesComparerResult($scope, $http) {

        // shows modal window with progressbar and given text
        function showModalLoadingWindow(headerText) {
            $scope.loadingScreenHeader = headerText;
            $scope.loadingModalWindow.modal("show");
            $scope.loading = true;
        }

        // hides modal window
        function hideModalLoadingWindow() {
            $scope.loading = false;
            $scope.loadingModalWindow.modal("hide");
        }

        // adds and applies new filter
        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });

                for (var i = 0; i < $scope.equalElements.length; i++) {
                    for (var j = 0; j < $scope.equalElements[i].length; j++) {
                        if ($scope.equalElements[i][j]) {
                            $scope.applyFilters($scope.equalElements[i][j]);
                        }
                    }
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
            var filterIndex = $scope.filters.indexOf(filter);
            $scope.filters.splice(filterIndex, 1);
            for (var i = 0; i < $scope.equalElements.length; i++) {
                for (var j = 0; j < $scope.equalElements[i].length; j++) {
                    if ($scope.equalElements[i][j]) {
                        for (var k = 0; k < $scope.equalElements[i][j].length; k++) {
                            $scope.equalElements[i][j][k].filtersVisible.splice(filterIndex, 1);
                        }
                    }
                }
            }
        }
        // applies filters
        function applyFilters(elements) {
            for (var i = 0; i < elements.length; i++) {
                elements[i].filtersVisible = [];
                for (var j = 0; j < $scope.filters.length; j++) {
                    var filterValue = $scope.filters[j].value.toUpperCase();

                    var firstProductId = $scope.getFirstProductAttributeId(elements[i]);
                    var firstVisible = firstProductId && $scope.attributeValues[firstProductId].value.toUpperCase().indexOf(filterValue) !== -1;

                    var secondProductId = $scope.getSecondProductAttributeId(elements[i]);
                    var secondVisible = secondProductId && $scope.attributeValues[secondProductId].value.toUpperCase().indexOf(filterValue) !== -1;

                    elements[i].filtersVisible.push(firstVisible || secondVisible);
                }
            }
        }

        // returns first product attribute index if any
        function getFirstProductAttributeId(equalElement) {
            return $scope.characteristics[$scope.firstMatterIndex][equalElement.FirstSubsequenceId].Attributes.find(function (a) {
                return $scope.attributes[$scope.attributeValues[a].attribute] === "product";
            });
        }

        // returns second product attribute index if any
        function getSecondProductAttributeId(equalElement) {
            return $scope.characteristics[$scope.secondMatterIndex][equalElement.SecondSubsequenceId].Attributes.find(function (a) {
                return $scope.attributes[$scope.attributeValues[a].attribute] === "product";
            });
        }

        // shows list of equal elements only for given pair of matters
        function showEqualPairs(firstIndex, secondIndex) {
            $scope.showModalLoadingWindow("Loading equal subsequences list...");

            $scope.firstMatterIndex = firstIndex;
            $scope.secondMatterIndex = secondIndex;

            if ($scope.equalElements[firstIndex][secondIndex]) {
                $scope.equalElementsToShow = $scope.equalElements[firstIndex][secondIndex];
                $scope.hideModalLoadingWindow();
            } else {
                $http({
                    url: "/api/TaskManagerWebApi?taskId=" + $scope.taskId
                    + "&firstIndex=" + firstIndex
                    + "&secondIndex=" + secondIndex,
                    method: "GET"
                }).success(function (equalElements) {
                    $scope.equalElements[firstIndex][secondIndex] = JSON.parse(equalElements);

                    $scope.applyFilters($scope.equalElements[firstIndex][secondIndex]);

                    $scope.equalElementsToShow = $scope.equalElements[firstIndex][secondIndex];

                    $scope.hideModalLoadingWindow();
                }).error(function (data) {
                    alert("Failed loading subsequences data");

                    $scope.hideModalLoadingWindow();
                });
            }
        }

        // calculates cell hihglight color using d3.js color scale
        function getHighlightColor(value) {
            var color = d3.scaleLinear()
                .domain([0, 0.1, 0.5, 1])
                .range(["lightcoral", "gold", "yellow", "limegreen"]);
            return { 'background-color': color(value) };
        }

        // calculates and displays local characteristics for given subsequences
        function calculateLocalCharacteristics(firstSubsequenceId, secondSubsequenceId) {

        }

        $scope.getHighlightColor = getHighlightColor;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.getFirstProductAttributeId = getFirstProductAttributeId;
        $scope.getSecondProductAttributeId = getSecondProductAttributeId;
        $scope.elementVisible = elementVisible;
        $scope.showEqualPairs = showEqualPairs;
        $scope.showModalLoadingWindow = showModalLoadingWindow;
        $scope.hideModalLoadingWindow = hideModalLoadingWindow;
        $scope.applyFilters = applyFilters;
        $scope.calculateLocalCharacteristics = calculateLocalCharacteristics;

        $scope.isLinkable = IsLinkable;
        $scope.selectLink = SelectLink;

        $scope.loadingModalWindow = $("#loadingDialog");

        $scope.showModalLoadingWindow("Loading data");

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];
        $scope.loading = true;
        $http({
            url: "/api/TaskManagerWebApi/" + $scope.taskId,
            method: "GET"
        }).success(function (data) {
            MapModelFromJson($scope, JSON.parse(data));

            $scope.equalElements = new Array($scope.mattersNames.length);

            for (var i = 0; i < $scope.mattersNames.length; i++) {
                $scope.equalElements[i] = new Array($scope.mattersNames.length);
            }

            $scope.characteristic = {
                characteristicType: $scope.characteristicTypes[0],
                link: $scope.characteristicTypes[0].CharacteristicLinks[0]
            };

            $scope.hideModalLoadingWindow();
        }).error(function (data) {
            alert("Failed loading characteristic data");
        });

        $scope.windowSize = 50;
        $scope.step = 1;
        $scope.equalElementsToShow = [];
        $scope.filters = [];
    }

    function makePositive() {
        return function (num) { return Math.abs(num); }
    }

    angular.module("SubsequencesComparerResult", [])
        .filter('makePositive', makePositive)
        .controller("SubsequencesComparerResultCtrl", ["$scope", "$http", subsequencesComparerResult]);
}
