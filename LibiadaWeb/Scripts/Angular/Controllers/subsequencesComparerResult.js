﻿function SubsequencesComparerResultController() {
    "use strict";

    function subsequencesComparerResult($scope, $http) {

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
            return element.filtersVisible.length === 0 || element.filtersVisible.some(e => e);
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

                    var firstSubsequenceIndex = elements[i].firstSubsequenceIndex;
                    var firstVisible = $scope.isAttributeEqual($scope.firstMatterIndex, firstSubsequenceIndex, "product", filterValue);
                    firstVisible = firstVisible || $scope.isAttributeEqual($scope.firstMatterIndex, firstSubsequenceIndex, "locus_tag", filterValue);

                    var secondSubsequenceIndex = elements[i].secondSubsequenceIndex;
                    var secondVisible = $scope.isAttributeEqual($scope.secondMatterIndex, secondSubsequenceIndex, "product", filterValue);
                    secondVisible = secondVisible || $scope.isAttributeEqual($scope.secondMatterIndex, secondSubsequenceIndex, "locus_tag", filterValue);

                    elements[i].filtersVisible.push(firstVisible || secondVisible);
                }
            }
        }

        // returns attribute index by its name if any
        function getAttributeIdByName(matterIndex, subsequenceIndex, attributeName) {
            return $scope.characteristics[matterIndex][subsequenceIndex].Attributes.find(a =>
                $scope.attributes[$scope.attributeValues[a].attribute] === attributeName
            );
        }

        // returns true if dot has given attribute and its value equal to the given value
        function isAttributeEqual(matterIndex, subsequenceIndex, attributeName, expectedValue) {
            var attributeId = $scope.getAttributeIdByName(matterIndex, subsequenceIndex, attributeName);
            if (attributeId) {
                var product = $scope.attributeValues[attributeId].value.toUpperCase();
                return product.indexOf(expectedValue) !== -1;
            }

            return false;
        }

        // shows list of equal elements only for given pair of matters
        function showEqualPairs(firstIndex, secondIndex, similarityValue) {
            $scope.loading = true;
            $scope.loadingScreenHeader = "Loading equal subsequences list...";

            $scope.firstMatterIndex = firstIndex;
            $scope.secondMatterIndex = secondIndex;

            $scope.similarityValue = similarityValue;
            $scope.similarityValueSelected = true;

            if ($scope.equalElements[firstIndex][secondIndex]) {
                $scope.equalElementsToShow = $scope.equalElements[firstIndex][secondIndex];
                $scope.loading = false;
            } else {
                $http.get("/api/TaskManagerWebApi/GetSubsequencesComparerDataElement", {
                    params: {
                        taskId: $scope.taskId,
                        firstIndex: firstIndex,
                        secondIndex: secondIndex
                    }
                })
                    .then(response => JSON.parse(response.data))
                    .then(equalElements => $scope.equalElements[firstIndex][secondIndex] = equalElements)
                    .then(_ => $http.get("/api/TaskManagerWebApi/GetTaskData", {
                        params: {
                            id: $scope.taskId,
                            key: "characteristics"
                        }
                    }))
                    .then(response => JSON.parse(response.data))
                    .then(characteristics => $scope.characteristics = characteristics)
                    .then(_ => $http.get("/api/TaskManagerWebApi/GetTaskData", {
                        params: {
                            id: $scope.taskId,
                            key: "attributeValues"
                        }
                    }))
                    .then(response => JSON.parse(response.data))
                    .then(attributeValues => $scope.attributeValues = attributeValues)
                    .then(() => {
                        $scope.applyFilters($scope.equalElements[firstIndex][secondIndex]);
                        $scope.equalElementsToShow = $scope.equalElements[firstIndex][secondIndex];
                        $scope.loading = false;
                    })
                    .catch(error => {
                        alert("Failed to load subsequences data");
                        console.error(error);
                        $scope.loading = false;
                    });
            }
        }

        // calculates cell highlight color using d3.js color scale
        function getHighlightColor(value) {
            var color = d3.scaleLinear()
                .domain([0, 0.1, 0.5, 1])
                .range(["lightcoral", "gold", "yellow", "limegreen"]);
            return { 'background-color': color(value) };
        }

        // calculates and displays local characteristics for given subsequences
        function calculateLocalCharacteristics(firstSubsequenceId, secondSubsequenceId, index) {
            $scope.loading = true;
            $scope.loadingScreenHeader = "Loading local characteristics...";

            var characteristicType = $scope.characteristic.characteristicType.Value;
            var link = $scope.characteristic.link.Value;
            var arrangementType = $scope.characteristic.arrangementType.Value;
            var characteristicId = $scope.characteristicsDictionary["(" + characteristicType + ", " + link + ", " + arrangementType + ")"];

            $http.get("/api/LocalCalculationWebApi/GetSubsequenceCharacteristic", {
                params: {
                    subsequenceId: firstSubsequenceId,
                    characteristicLinkId: characteristicId,
                    windowSize: $scope.slidingWindowParams.windowSize,
                    step: $scope.slidingWindowParams.step
                }
            }).then(firstCharacteristics => {
                $scope.firstSubsequenceLocalCharacteristics = JSON.parse(firstCharacteristics.data);

                $http.get("/api/LocalCalculationWebApi/GetSubsequenceCharacteristic", {
                    params: {
                        subsequenceId: secondSubsequenceId,
                        characteristicLinkId: characteristicId,
                        windowSize: $scope.slidingWindowParams.windowSize,
                        step: $scope.slidingWindowParams.step
                    }
                }).then(secondCharacteristics => {
                    $scope.secondSubsequenceLocalCharacteristics = JSON.parse(secondCharacteristics.data);
                    $scope.drawLocalCharacteristics(firstSubsequenceId, secondSubsequenceId, index);

                    $scope.loading = false;
                }, () => {
                    alert("Failed loading characteristics data");

                    $scope.loading = false;
                });
            }, () => {
                alert("Failed loading local characteristics data");

                $scope.loading = false;
            });
        }

        // draws local characteristics line-chart
        function drawLocalCharacteristics(firstSubsequenceId, secondSubsequenceId, index) {
            var legendData = [
                { id: firstSubsequenceId, name: "First", visible: true, points: [] },
                { id: secondSubsequenceId, name: "Second", visible: true, points: [] }];

            for (var j = 0; j < $scope.firstSubsequenceLocalCharacteristics.length; j++) {
                legendData[0].points.push({
                    id: firstSubsequenceId,
                    x: j,
                    value: +$scope.firstSubsequenceLocalCharacteristics[j]
                });
            }

            for (var k = 0; k < $scope.secondSubsequenceLocalCharacteristics.length; k++) {
                legendData[1].points.push({
                    id: secondSubsequenceId,
                    x: k,
                    value: +$scope.secondSubsequenceLocalCharacteristics[k]
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

            legendData.forEach(data => {
                xMinArray.push(d3.min(data.points, d => d.x));
                xMaxArray.push(d3.max(data.points, d => d.x));
                yMinArray.push(d3.min(data.points, d => d.value));
                yMaxArray.push(d3.max(data.points, d => d.value));
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


            var xMap = d => xScale(d.x);

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

            var yMap = d => yScale(d.value);

            // setup fill color
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

            legendData.forEach(data => {
                // Nest the entries by symbol
                var dataGroups = d3.group(data.points, d => d.id);

                // Loop through each symbol / key
                dataGroups.forEach(value => {
                    svg.append("path")
                        .datum(value)
                        .attr("class", "line")
                        .attr("d", line)
                        .attr('stroke', d => color(d[0].id))
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
                .attr("transform", (_d, i) => "translate(0," + i * 20 + ")")
                .on("click", function (_event, d) {
                    d.visible = !d.visible;
                    var legendEntry = d3.select(this);
                    legendEntry.select("text")
                        .style("opacity", () => d.visible ? 1 : 0.5);
                    legendEntry.select("rect")
                        .style("fill-opacity", () => d.visible ? 1 : 0);

                    svg.selectAll(".line")
                        .filter((line) => line[0].id === d.id)
                        .attr("visibility", () => d.visible ? "visible" : "hidden");
                });

            // draw legend's colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", d => color(d.id))
                .style("stroke", d => color(d.id))
                .style("stroke-width", 4)
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            // draw legend's text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
                .text(d => d.name)
                .style("font-size", "9pt");
        }

        $scope.getHighlightColor = getHighlightColor;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.getAttributeIdByName = getAttributeIdByName;
        $scope.isAttributeEqual = isAttributeEqual;
        $scope.elementVisible = elementVisible;
        $scope.showEqualPairs = showEqualPairs;
        $scope.applyFilters = applyFilters;
        $scope.calculateLocalCharacteristics = calculateLocalCharacteristics;
        $scope.drawLocalCharacteristics = drawLocalCharacteristics;

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
        $scope.loadingScreenHeader = "Loading data";
        $scope.characteristic = {};

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];
        $scope.loading = true;
        $http.get(`/api/TaskManagerWebApi/${$scope.taskId}`)
            .then(data => {
                MapModelFromJson($scope, JSON.parse(data.data));

                $scope.equalElements = new Array($scope.mattersNames.length);

                for (var i = 0; i < $scope.mattersNames.length; i++) {
                    $scope.equalElements[i] = new Array($scope.mattersNames.length);
                }

                $scope.loading = false;
            }, () => {
                alert("Failed loading characteristic data");

                $scope.loading = false;
            });
    }

    function makePositive() {
        return num => Math.abs(num);
    }

    angular.module("libiada")
        .filter('makePositive', makePositive)
        .controller("SubsequencesComparerResultCtrl", ["$scope", "$http", subsequencesComparerResult]);
}
