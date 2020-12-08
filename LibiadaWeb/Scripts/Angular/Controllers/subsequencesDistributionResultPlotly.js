﻿function SubsequencesDistributionResultPlotlyController() {
    "use strict";

    function subsequencesDistributionResultPlotly($scope, $http) {

        // adds new characteristics value based filter
        function addCharacteristicComparer() {
            $scope.characteristicComparers.push({ characteristic: $scope.subsequencesCharacteristicsList[0], precision: 0 });
        }

        // deletes given characteristics filter
        function deleteCharacteristicComparer(characteristicComparer) {
            $scope.characteristicComparers.splice($scope.characteristicComparers.indexOf(characteristicComparer), 1);
        }

        // fills array of currently visible points
        function fillVisiblePoints() {
            $scope.visiblePoints = [];
            for (var i = 0; i < $scope.points.length; i++) {
                $scope.visiblePoints[i] = [];
                for (var j = 0; j < $scope.points[i].length; j++) {
                    if ($scope.dotVisible($scope.points[i][j])) {
                        $scope.visiblePoints[i].push($scope.points[i][j]);
                    }
                }
            }
        }

        // returns attribute index by its name if any
        function getAttributeIdByName(dot, attributeName) {
            return dot.attributes.find(function (a) {
                return $scope.attributes[$scope.attributeValues[a].attribute] === attributeName;
            });
        }

        // returns true if dot has given attribute and its value equal to the given value
        function isAttributeEqual(dot, attributeName, expectedValue) {
            var attributeId = $scope.getAttributeIdByName(dot, attributeName);
            if (attributeId) {
                var product = $scope.attributeValues[attributeId].value.toUpperCase();
                return product.indexOf(expectedValue) !== -1;
            }

            return false;
        }

        // adds and applies new filter
        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });

                var filterValue = $scope.filters[$scope.filters.length - 1].value.toUpperCase();

                for (var i = 0; i < $scope.points.length; i++) {
                    for (var j = 0; j < $scope.points[i].length; j++) {
                        var point = $scope.points[i][j];
                        var visible = $scope.isAttributeEqual(point, "product", filterValue);
                        visible = visible || $scope.isAttributeEqual(point, "locus_tag", filterValue);
                        point.filtersVisible.push(visible);
                        point.visible = $scope.dotVisible(point);
                    }
                }

                $scope.redrawGenesMap();

                $scope.newFilter = "";
            }
            // todo: add error message if filter is empty
        }

        // deletes given filter
        function deleteFilter(filter) {
            for (var i = 0; i < $scope.points.length; i++) {
                for (var j = 0; j < $scope.points[i].length; j++) {
                    var point = $scope.points[i][j];

                    point.filtersVisible.splice($scope.filters.indexOf(filter), 1);
                    point.visible = $scope.dotVisible(point);
                }
            }

            $scope.filters.splice($scope.filters.indexOf(filter), 1);
            $scope.redrawGenesMap();
        }

        // initializes data for genes map
        function fillPoints() {
            var id = 0;
            for (var i = 0; i < $scope.result.length; i++) {
                var sequenceData = $scope.result[i];
                $scope.matters.push({ id: sequenceData.MatterId, name: sequenceData.MatterName, visible: true, index: i, color: $scope.colorScale(i) });
                // hack for legend dot color
                document.styleSheets[0].insertRule(".legend" + sequenceData.MatterId + ":after { background:" + $scope.colorScale(i)+"}");
                $scope.points.push([]);
                $scope.visiblePoints.push([]);
                for (var j = 0; j < sequenceData.SubsequencesData.length; j++) {
                    var subsequenceData = sequenceData.SubsequencesData[j];
                    var point = {
                        id: id,
                        matterId: sequenceData.MatterId,
                        sequenceRemoteId: sequenceData.RemoteId,
                        attributes: subsequenceData.Attributes,
                        partial: subsequenceData.Partial,
                        featureId: subsequenceData.FeatureId,
                        positions: subsequenceData.Starts,
                        lengths: subsequenceData.Lengths,
                        subsequenceRemoteId: subsequenceData.RemoteId,
                        numericX: i + 1,
                        x: sequenceData.Characteristic,
                        subsequenceCharacteristics: subsequenceData.CharacteristicsValues,
                        featureVisible: true,
                        legendVisible: true,
                        filtersVisible: []
                    };
                    $scope.points[i].push(point);
                    $scope.visiblePoints[i].push(point);
                    id++;
                }
            }
        }

        // filters dots by subsequences feature
        function filterByFeature(feature) {
            for (var i = 0; i < $scope.points.length; i++) {
                for (var j = 0; j < $scope.points[i].length; j++) {
                    var point = $scope.points[i][j];

                    if (point.featureId === parseInt(feature.Value)) {
                        point.featureVisible = feature.Selected;
                        point.visible = $scope.dotVisible(point);
                    }
                }
            }

            $scope.redrawGenesMap();
        }

        // checks if dot is visible
        function dotVisible(dot) {
            var filterVisible = dot.filtersVisible.length === 0 || dot.filtersVisible.some(fv => fv);
            return dot.legendVisible && dot.featureVisible && filterVisible;
        }

        // determines if dots are similar by product
        function dotsSimilar(d, dot) {
            if (d.featureId !== dot.featureId) {
                return false;
            }

            switch (d.featureId) {
                case 1: // CDS
                case 2: // RRNA
                case 3: // TRNA
                    var firstProductId = $scope.getAttributeIdByName(d, "product");
                    var secondProductId = $scope.getAttributeIdByName(dot, "product");
                    if ($scope.attributeValues[firstProductId].value.toUpperCase() !== $scope.attributeValues[secondProductId].value.toUpperCase()) {
                        return $scope.pointsSimilarity.different;
                    }
                    break;
            }

            return $scope.pointsSimilarity.similar;
        }

        // gets attributes text for given subsequence
        function getAttributesText(attributes) {
            var attributesText = [];
            for (var i = 0; i < attributes.length; i++) {
                var attributeValue = $scope.attributeValues[attributes[i]];
                attributesText.push($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : " = " + attributeValue.value));
            }

            return attributesText;
        }

        // shows tooltip for dot or group of dots
        function showTooltip(selectedPoint) {
            $("a[href='#tooltip']").tab("show");

            $scope.tooltipVisible = true;
            $scope.tooltipElements.length = 0;
            var matterName = $scope.matters.find(value => value.id === selectedPoint.matterId).name;
            $scope.tooltipElements.push(fillPointTooltip(selectedPoint, matterName, $scope.pointsSimilarity.same));
            var similarPoints = [];

            for (var i = 0; i < $scope.visiblePoints.length; i++) {
                for (var j = 0; j < $scope.visiblePoints[i].length; j++) {
                    if (selectedPoint !== $scope.visiblePoints[i][j] && $scope.highlight) {
                        var similar = $scope.characteristicComparers.every(function (filter) {
                            var selectedPointValue = selectedPoint.subsequenceCharacteristics[filter.characteristic.Value];
                            var anotherPointValue = $scope.visiblePoints[i][j].subsequenceCharacteristics[filter.characteristic.Value];
                            return Math.abs(selectedPointValue - anotherPointValue) <= filter.precision;
                        });

                        if (similar) {
                            var point = $scope.visiblePoints[i][j];
                            similarPoints.push(point);
                            matterName = $scope.matters[i].name;
                            $scope.tooltipElements.push(fillPointTooltip(point, matterName, $scope.dotsSimilar(point, selectedPoint)));
                        }
                    }
                }
            }


            //                svg.append("line")
            //                    .attr("class", "similar-line")
            //                    .attr("x1", $scope.xMap(point))
            //                    .attr("y1", $scope.yMap(point))
            //                    .attr("x2", $scope.xMap(dot))
            //                    .attr("y2", $scope.yMap(dot))
            //                    .attr("stroke-width", 1)
            //                    .attr("opacity", 0.4)
            //                    .attr("stroke", $scope.colorMap($scope.cValue(point)));
            //                return true;
            //            }
            //        }

            //        return false;
            //    })
            //    .attr("rx", $scope.selectedDotRadius);
            //tooltip.lines = svg.selectAll(".similar-line");

            var update = {
                "marker.symbol": $scope.visiblePoints.map(points => points.map(point => point === selectedPoint || similarPoints.includes(point) ? "diamond-wide" : "circle-open")),
                "marker.size": $scope.visiblePoints.map(points => points.map(point => point === selectedPoint || similarPoints.includes(point) ? 15 : 6))
            };

            Plotly.restyle($scope.plot, update);

            $scope.$apply();
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(point, matterName, similarity) {
            var color = similarity === $scope.pointsSimilarity.same ? ""
                : similarity === $scope.pointsSimilarity.similar ? "bg-success"
                    : similarity === $scope.pointsSimilarity.different ? "bg-danger" : "bg-danger";

            var tooltipElement = {
                name: matterName,
                sequenceRemoteId: point.sequenceRemoteId,
                feature: $scope.features[point.featureId].Text,
                attributes: $scope.getAttributesText(point.attributes),
                partial: point.partial,
                color: color
            };

            if (point.subsequenceRemoteId) {
                tooltipElement.remoteId = point.subsequenceRemoteId;
            }

            tooltipElement.position = "(";
            tooltipElement.length = 0;
            tooltipElement.positions = point.positions;
            tooltipElement.lengths = point.lengths;

            for (var i = 0; i < point.positions.length; i++) {
                tooltipElement.position += point.positions[i] + 1;
                tooltipElement.position += "..";
                tooltipElement.position += point.positions[i] + point.lengths[i];
                tooltipElement.length += point.lengths[i];
                if (i !== point.positions.length - 1) {
                    tooltipElement.position += ", ";
                }
            }

            tooltipElement.position += ")";

            //tooltipContent.push("(" + d.x + ", " + $scope.yValue(d) + ")");

            return tooltipElement;
        }

        function isKeyUpOrDown(keyCode) {
            return keyCode === 40 || keyCode === 38;
        }

        function cText(points, index) {
            return points.map(function (d) { return $scope.matters[index].name; });
        }

        // main drawing method
        function drawGenesMap() {

            $scope.layout = {
                showlegend: false,
                hovermode: "closest",
                xaxis: {
                    title: {
                        text: $scope.sequenceCharacteristicName,
                        font: {
                            family: 'Courier New, monospace',
                            size: 12
                        }
                    }
                },
                yaxis: {
                    title: {
                        text: $scope.subsequenceCharacteristic.Text,
                        font: {
                            family: 'Courier New, monospace',
                            size: 12
                        }
                    }
                }
            };

            $scope.plot = document.getElementById("chart");

        //    while ($scope.plot.firstChild) $scope.plot.removeChild($scope.plot.firstChild);

            $scope.redrawGenesMap();
        }

        function keyUpDownPress(keyCode) {
            var nextPointIndex = -1;
            var visibleMattersPoints = $scope.visiblePoints[$scope.selectedMatterIndex];

            if ($scope.selectedPointIndex >= 0) {
                var characteristic;
                var firstPointCharacteristic;
                var secondPointCharacteristic;
                switch (keyCode) {
                    case 38: // up
                        for (var i = $scope.selectedPointIndex + 1; i < visibleMattersPoints.length; i++) {
                            characteristic = $scope.subsequenceCharacteristic.Value;
                            firstPointCharacteristic = visibleMattersPoints[$scope.selectedPointIndex].subsequenceCharacteristics[characteristic];
                            secondPointCharacteristic = visibleMattersPoints[i].subsequenceCharacteristics[characteristic];

                            if (firstPointCharacteristic !== secondPointCharacteristic) {
                                nextPointIndex = i;
                                break;
                            }
                        }
                        break;
                    case 40: // down
                        for (var j = $scope.selectedPointIndex - 1; j >= 0; j--) {
                            characteristic = $scope.subsequenceCharacteristic.Value;
                            firstPointCharacteristic = visibleMattersPoints[$scope.selectedPointIndex].subsequenceCharacteristics[characteristic];
                            secondPointCharacteristic = visibleMattersPoints[j].subsequenceCharacteristics[characteristic];

                            if (firstPointCharacteristic !== secondPointCharacteristic) {
                                nextPointIndex = j;
                                break;
                            }
                        }
                        break;
                }
            }

            if (nextPointIndex >= 0) {
                $scope.showTooltip(visibleMattersPoints[nextPointIndex]);
                $scope.selectedPointIndex = nextPointIndex;
            }
        }

        function redrawGenesMap() {

            $scope.fillVisiblePoints();
            $scope.selectedPointIndex = -1;

            var data = $scope.visiblePoints.map(function (points, index) {
                return {
                    hoverinfo: 'text+x+y',
                    type: 'scattergl',
                    x: points.map(p => $scope.numericXAxis ? p.numericX : p.x),
                    y: points.map(p => p.subsequenceCharacteristics[$scope.subsequenceCharacteristic.Value]),
                    text: cText(points, index),
                    mode: "markers",
                    marker: { opacity: 0.5, symbol: "circle-open", color: $scope.colorScale(index) },
                    name: $scope.matters[index].name,

                };
            });

            Plotly.newPlot($scope.plot, data, $scope.layout, { responsive: true });

            $scope.plot.on("plotly_click", data => {
                $scope.selectedPointIndex = data.points[0].pointNumber;
                $scope.selectedMatterIndex = data.points[0].curveNumber;
                var selectedPoint = $scope.visiblePoints[data.points[0].curveNumber][data.points[0].pointNumber];
                $scope.showTooltip(selectedPoint);
            });
        }

        function legendClick(legendItem) {
                for (var j = 0; j < $scope.points[legendItem.index].length; j++) {
                    var point = $scope.points[legendItem.index][j];
                    point.legendVisible = !point.legendVisible;
                }

            $scope.redrawGenesMap();
        }


        $scope.setCheckBoxesState = SetCheckBoxesState;

        $scope.drawGenesMap = drawGenesMap;
        $scope.redrawGenesMap = redrawGenesMap;
        $scope.dotVisible = dotVisible;
        $scope.dotsSimilar = dotsSimilar;
        $scope.fillVisiblePoints = fillVisiblePoints;
        $scope.filterByFeature = filterByFeature;
        $scope.legendClick = legendClick;
        $scope.fillPoints = fillPoints;
        $scope.getAttributesText = getAttributesText;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.isKeyUpOrDown = isKeyUpOrDown;
        $scope.addCharacteristicComparer = addCharacteristicComparer;
        $scope.deleteCharacteristicComparer = deleteCharacteristicComparer;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.getAttributeIdByName = getAttributeIdByName;
        $scope.isAttributeEqual = isAttributeEqual;
        $scope.dragbarMouseDown = dragbarMouseDown;
        $scope.keyUpDownPress = keyUpDownPress;
        $scope.colorScale = d3.scaleOrdinal(d3.schemeCategory20);

        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 3;
        $scope.points = [];
        $scope.visiblePoints = [];
        $scope.matters = [];
        $scope.legend = [];
        $scope.characteristicComparers = [];
        $scope.filters = [];
        $scope.productFilter = "";
        $scope.tooltipVisible = false;
        $scope.tooltipElements = [];
        $scope.pointsSimilarity = Object.freeze({ "same": 0, "similar": 1, "different": 2 });

        $scope.i = 0;
        $scope.dragging = false;

        $('[data-toggle="tooltip"]').tooltip();

        // preventing scroll in key up and key down
        window.addEventListener("keydown", function (e) {
            if ($scope.isKeyUpOrDown(e.keyCode)) {
                e.preventDefault();
                $scope.keyUpDownPress(e.keyCode);
            }
        }, false);



        // dragbar
        function dragbarMouseDown() {
            var main = document.getElementById('main');
            var right = document.getElementById('sidebar');
            var bar = document.getElementById('dragbar');

            const drag = (e) => {
                document.selection ? document.selection.empty() : window.getSelection().removeAllRanges();
                var chart_width = main.style.width = (e.pageX - bar.offsetWidth / 2) + 'px';

                Plotly.relayout('chart', { autosize: true });
            };

            bar.addEventListener('mousedown', () => {
                document.addEventListener('mousemove', drag);
            });

            bar.addEventListener('mouseup', () => {
                document.removeEventListener('mousemove', drag);
            });

        }

        $scope.loadingScreenHeader = "Loading genes map data";

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;


        $http.get("/api/TaskManagerWebApi/" + $scope.taskId)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));

                $scope.subsequenceCharacteristic = $scope.subsequencesCharacteristicsList[0];

                $scope.fillPoints();

                var comparer = (first, second) =>
                    first.subsequenceCharacteristics[$scope.subsequenceCharacteristic.Value] - second.subsequenceCharacteristics[$scope.subsequenceCharacteristic.Value];

                $scope.points = $scope.points.map(points => points.sort(comparer));
                $scope.visiblePoints = $scope.visiblePoints.map(points => points.sort(comparer));

                $scope.addCharacteristicComparer();
                drawGenesMap();
                $scope.loading = false;
            }, function () {
                alert("Failed loading genes map data");

                $scope.loading = false;
            });
    }

    angular.module("libiada", []).controller("SubsequencesDistributionResultPlotlyCtrl", ["$scope", "$http", subsequencesDistributionResultPlotly]);

}