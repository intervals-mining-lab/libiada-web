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
            return $scope.characteristics[$scope.firstMatterIndex][equalElement.FirstSubsequenceIndex].Attributes.find(function (a) {
                return $scope.attributes[$scope.attributeValues[a].attribute] === "product";
            });
        }

        // returns second product attribute index if any
        function getSecondProductAttributeId(equalElement) {
            return $scope.characteristics[$scope.secondMatterIndex][equalElement.SecondSubsequenceIndex].Attributes.find(function (a) {
                return $scope.attributes[$scope.attributeValues[a].attribute] === "product";
            });
        }

        // shows list of equal elements only for given pair of matters
        function showEqualPairs(firstIndex, secondIndex, similarityValue) {
            $scope.showModalLoadingWindow("Loading equal subsequences list...");

            $scope.firstMatterIndex = firstIndex;
            $scope.secondMatterIndex = secondIndex;

            $scope.similarityValue = similarityValue;
            $scope.similarityValueSelected = true;

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
        function calculateLocalCharacteristics(firstSubsequenceId, secondSubsequenceId, index) {
            $scope.showModalLoadingWindow("Loading local characteristics...");

            $http({
                url: "/api/LocalCalculationWebApi?subsequenceId=" + firstSubsequenceId +
                    "&characteristicLinkId=" + $scope.characteristic.link.CharacteristicLinkId +
                    "&windowSize=" + $scope.slidingWindowParams.windowSize +
                    "&step=" + $scope.slidingWindowParams.step,
                method: "GET"
            }).success(function (firstCharacteristics) {
                $scope.firstSubsequenceLocalCharactristics = JSON.parse(firstCharacteristics);

                $http({
                    url: "/api/LocalCalculationWebApi?subsequenceId=" + secondSubsequenceId +
                        "&characteristicLinkId=" + $scope.characteristic.link.CharacteristicLinkId +
                        "&windowSize=" + $scope.slidingWindowParams.windowSize +
                        "&step=" + $scope.slidingWindowParams.step,
                    method: "GET"
                }).success(function (secondCharacteristics) {
                    $scope.secondSubsequenceLocalCharactristics = JSON.parse(secondCharacteristics);
                    $scope.drawLocalCharacteristics(firstSubsequenceId, secondSubsequenceId, index);

                    $scope.hideModalLoadingWindow();
                }).error(function (data) {
                    alert("Failed loading characteristics data");

                    $scope.hideModalLoadingWindow();
                });
            }).error(function (data) {
                alert("Failed loading local characteristics data");

                $scope.hideModalLoadingWindow();
            });
        }

        function drawLocalCharacteristics(firstSubsequenceId, secondSubsequenceId, index) {
            var legendData = [
                { id: firstSubsequenceId, name: "First", visible: true, points: [] },
                { id: secondSubsequenceId, name: "Second", visible: true, points: [] }];

            for (var j = 0; j < $scope.firstSubsequenceLocalCharactristics.length; j++) {
                legendData[0].points.push({
                    id: firstSubsequenceId,
                    x: j,
                    value: +$scope.firstSubsequenceLocalCharactristics[j]
                });
            }

            for (var k = 0; k < $scope.secondSubsequenceLocalCharactristics.length; k++) {
                legendData[1].points.push({
                    id: secondSubsequenceId,
                    x: k,
                    value: +$scope.secondSubsequenceLocalCharactristics[k]
                });
            }

            // removing previous chart if any
            d3.select(".chart" + index).remove();

            // chart size and margin settings
            var margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            var width = $scope.width - margin.left - margin.right;
            var height = $scope.height - margin.top - margin.bottom;

            // calculating margins for dots
            var xMinArray = [];
            var xMaxArray = [];
            var yMaxArray = [];
            var yMinArray = [];

            legendData.forEach(function (data) {
                xMinArray.push(d3.min(data.points, function (d) { return d.x }));
                xMaxArray.push(d3.max(data.points, function (d) { return d.x }));
                yMinArray.push(d3.min(data.points, function (d) { return d.value }));
                yMaxArray.push(d3.max(data.points, function (d) { return d.value }));
            });

            // setup x
            // calculating margins for dots
            var xMin = d3.min(xMinArray);
            var xMax = d3.max(xMaxArray);
            var xMargin = (xMax - xMin) * 0.05;

            var xScale = d3.scaleLinear()
                .domain([xMin - xMargin, xMax + xMargin])
                .range([0, width]);
            var xAxis = d3.axisBottom(xScale)
                .tickSizeInner(-height)
                .tickSizeOuter(0)
                .tickPadding(10);


            var xMap = function (d) { return xScale(d.x); };

            // setup y
            var yMin = d3.min(yMinArray);
            var yMax = d3.max(yMaxArray);
            var yMargin = (yMax - yMin) * 0.05;

            var yScale = d3.scaleLinear()
                .domain([yMin - yMargin, yMax + yMargin])
                .range([height, 0]);
            var yAxis = d3.axisLeft(yScale)
                .tickSizeInner(-width)
                .tickSizeOuter(0)
                .tickPadding(10);

            var yMap = function (d) { return yScale(d.value); };

            // setup fill color
            var cValue = function (d) { return d.id; };
            var color = d3.scaleOrdinal(["red", "blue"]);

            // add the graph canvas to the body of the webpage
            var svg = d3.select("#chart" + index).append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height)
                .attr("class", "chart" + index)
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // x-axis
            svg.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(0," + height + ")")
                .call(xAxis);

            svg.append("text")
                .attr("class", "label")
                .attr("transform", "translate(" + (width / 2) + " ," + (height + margin.top - $scope.legendHeight) + ")")
                .style("text-anchor", "middle")
                .text("Fragment №")
                .style("font-size", "12pt");

            // y-axis
            svg.append("g")
                .attr("class", "y axis")
                .call(yAxis);

            svg.append("text")
                .attr("class", "label")
                .attr("transform", "rotate(-90)")
                .attr("y", 0 - margin.left)
                .attr("x", 0 - (height / 2))
                .attr("dy", ".71em")
                .style("text-anchor", "middle")
                .text($scope.characteristic.characteristicType.Text)
                .style("font-size", "12pt");

            var line = d3.line()
                .x(xMap)
                .y(yMap);

            legendData.forEach(function (data) {
                // Nest the entries by symbol
                var dataNest = d3.nest()
                    .key(function (d) { return d.id })
                    .entries(data.points);

                // Loop through each symbol / key
                dataNest.forEach(function (d) {
                    svg.append("path")
                        .datum(d.values)
                        .attr("class", "line")
                        .attr("d", line)
                        .attr('stroke', function (d) { return color(cValue(d[0])); })
                        .attr('stroke-width', 1)
                        .attr('fill', 'none')
                        .attr("opacity", 0.6);
                });
            });

            // draw legend
            var legend = svg.selectAll(".legend")
                .data(legendData)
                .enter()
                .append("g")
                .attr("class", "legend")
                .attr("transform", function (d, i) { return "translate(0," + i * 20 + ")"; })
                .on("click", function (d) {
                    d.visible = !d.visible;
                    var legendEntry = d3.select(this);
                    legendEntry.select("text")
                        .style("opacity", function () { return d.visible ? 1 : 0.5; });
                    legendEntry.select("rect")
                        .style("fill-opacity", function () { return d.visible ? 1 : 0; });

                    svg.selectAll(".line")
                        .filter(function(line) {
                             return line[0].id === d.id;
                        })
                        .attr("visibility", function (line) {
                            return d.visible ? "visible" : "hidden";
                        });
                });

            // draw legend colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", function (d) { return color(d.id); })
                .style("stroke", function (d) { return color(d.id); })
                .style("stroke-width", 4)
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            // draw legend text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
                .text(function (d) { return d.name; })
                .style("font-size", "9pt");
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
        $scope.drawLocalCharacteristics = drawLocalCharacteristics;

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

        $scope.slidingWindowParams = {
            windowSize: 50,
            step: 1
        };

        $scope.equalElementsToShow = [];
        $scope.filters = [];
        $scope.legendHeight = 40;
        $scope.width = 1050;
        $scope.height = 800;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 2;
        $scope.similarityValue = {};
        $scope.similarityValueSelected = false;
    }

    function makePositive() {
        return function (num) { return Math.abs(num); }
    }

    angular.module("SubsequencesComparerResult", [])
        .filter('makePositive', makePositive)
        .controller("SubsequencesComparerResultCtrl", ["$scope", "$http", subsequencesComparerResult]);
}
