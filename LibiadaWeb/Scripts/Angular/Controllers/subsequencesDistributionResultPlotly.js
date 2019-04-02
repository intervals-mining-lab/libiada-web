function SubsequencesDistributionResultPlotlyController() {
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
                $scope.matters.push({ id: sequenceData.MatterId, name: sequenceData.MatterName, visible: true });
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
                        matterVisible: true,
                        filtersVisible: []
                    };
                    $scope.points[i].push(point);
                    $scope.visiblePoints[i].push(point);
                    id++;
                }
            }
        }

        // filters dots by subsequences feature @@@
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
            return dot.featureVisible && filterVisible;
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

            $scope.tooltipElements.push(fillPointTooltip(selectedPoint, matterName, $scope.pointsSimilarity.same));

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
                            var matterName = $scope.matters[i].name;
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
                hovermode: "closest",
                xaxis: {
                    title: {
                        text: $scope.sequenceCharacteristicName,
                        font: {
                            family: 'Courier New, monospace',
                            size: 12,
                        }
                    },
                },
                yaxis: {
                    title: {
                        text: $scope.subsequenceCharacteristic.Text,
                        font: {
                            family: 'Courier New, monospace',
                            size: 12,
                        }
                    }
                }
            };

            $scope.plot = document.getElementById("chart");

            while ($scope.plot.firstChild) $scope.plot.removeChild($scope.plot.firstChild);

            $scope.redrawGenesMap();



            //$scope.loading = true;
            //$scope.loadingScreenHeader = "Drawing...";
            //// removing previous chart and tooltip if any
            //d3.select(".tooltip").remove();
            //d3.select(".genes-map-svg").remove();

            //// sorting points by selected characteristic
            //$scope.points.sort(function (first, second) {
            //	return $scope.yValue(second) - $scope.yValue(first);
            //});
            //$scope.visiblePoints.sort(function (first, second) {
            //	return $scope.yValue(second) - $scope.yValue(first);
            //});

            //// all organisms are visible after redrawing
            //$scope.points.forEach(function (point) {
            //	point.matterVisible = true;
            //	point.featureVisible = $scope.features[point.featureId].Selected;
            //});

            //// chart size and margin settings
            //var margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            //var width = $scope.width - margin.left - margin.right;
            //var height = $scope.height - margin.top - margin.bottom;

            //// setup x
            //// calculating margins for dots
            //var xMin = d3.min($scope.points, $scope.xValue);
            //var xMax = d3.max($scope.points, $scope.xValue);
            //var xMargin = (xMax - xMin) * 0.05;

            //var xScale = d3.scaleLinear()
            //	.domain([xMin - xMargin, xMax + xMargin])
            //	.range([0, width]);
            //var xAxis = d3.axisBottom(xScale)
            //	.tickSizeInner(-height)
            //	.tickSizeOuter(0)
            //	.tickPadding(10);

            //$scope.xMap = function (d) { return xScale($scope.xValue(d)); };

            //// setup y
            //// calculating margins for dots
            //var yMax = d3.max($scope.points, $scope.yValue);
            //var yMin = d3.min($scope.points, $scope.yValue);
            //var yMargin = (yMax - yMin) * 0.05;

            //var yScale = d3.scaleLinear()
            //	.domain([yMin - yMargin, yMax + yMargin])
            //	.range([height, 0]);
            //var yAxis = d3.axisLeft(yScale)
            //	.tickSizeInner(-width)
            //	.tickSizeOuter(0)
            //	.tickPadding(10);

            //$scope.yMap = function (d) { return yScale($scope.yValue(d)); };

            //// setup fill color
            //$scope.cValue = function (d) { return d.matterId; };
            //$scope.colorMap = d3.scaleOrdinal(d3.schemeCategory20);

            //// add the graph canvas to the body of the webpage
            //var svg = d3.select("#chart").append("svg")
            //	.attr("width", $scope.width)
            //	.attr("height", $scope.height)
            //	.attr("class", "genes-map-svg")
            //	.append("g")
            //	.attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            //// add the tooltip area to the webpage
            //var tooltip = d3.select("#chart").append("div")
            //	.attr("class", "tooltip")
            //	.style("opacity", 0);

            //// x-axis
            //svg.append("g")
            //	.attr("class", "x axis")
            //	.attr("transform", "translate(0," + height + ")")
            //	.call(xAxis);

            //svg.append("text")
            //	.attr("class", "label")
            //	.attr("transform", "translate(" + (width / 2) + " ," + (height + margin.top - $scope.legendHeight) + ")")
            //	.style("text-anchor", "middle")
            //	.text($scope.sequenceCharacteristicName)
            //	.style("font-size", "12pt");

            //// y-axis
            //svg.append("g")
            //	.attr("class", "y axis")
            //	.call(yAxis);

            //svg.append("text")
            //	.attr("class", "label")
            //	.attr("transform", "rotate(-90)")
            //	.attr("y", 0 - margin.left)
            //	.attr("x", 0 - (height / 2))
            //	.attr("dy", ".71em")
            //	.style("text-anchor", "middle")
            //	.text($scope.subsequenceCharacteristic.Text)
            //	.style("font-size", "12pt");

            //// draw dots
            //svg.selectAll(".dot")
            //	.data($scope.points)
            //	.enter()
            //	.append("ellipse")
            //	.attr("class", "dot")
            //	.attr("rx", $scope.dotRadius)
            //	.attr("ry", $scope.dotRadius)
            //	.attr("cx", $scope.xMap)
            //	.attr("cy", $scope.yMap)
            //	.style("fill-opacity", 0.6)
            //             .style("fill", function (d) { return $scope.colorMap($scope.cValue(d)); })
            //             .style("stroke", function (d) { return $scope.colorMap($scope.cValue(d)); })
            //	.attr("visibility", function (dot) {
            //		return $scope.dotVisible(dot) ? "visible" : "hidden";
            //	});

            //// draw legend
            //var legend = svg.selectAll(".legend")
            //	.data($scope.matters)
            //	.enter().append("g")
            //	.attr("class", "legend")
            //	.attr("transform", function (d, i) { return "translate(0," + i * 20 + ")"; })
            //	.on("click", function (d) {
            //		d.visible = !d.visible;
            //		var legendEntry = d3.select(this);
            //		legendEntry.select("text")
            //			.style("opacity", function () { return d.visible ? 1 : 0.5; });
            //		legendEntry.select("rect")
            //			.style("fill-opacity", function () { return d.visible ? 1 : 0; });

            //		svg.selectAll(".dot")
            //			.filter(function (dot) { return dot.matterId === d.id; })
            //			.attr("visibility", function (dot) {
            //				dot.matterVisible = d.visible;
            //				return $scope.dotVisible(dot) ? "visible" : "hidden";
            //			});
            //	});

            //// draw legend colored rectangles
            //legend.append("rect")
            //	.attr("width", 15)
            //	.attr("height", 15)
            //             .style("fill", function (d) { return $scope.colorMap(d.id); })
            //             .style("stroke", function (d) { return $scope.colorMap(d.id); })
            //	.style("stroke-width", 4)
            //	.attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            //// draw legend text
            //legend.append("text")
            //	.attr("x", 24)
            //	.attr("y", 9)
            //	.attr("dy", ".35em")
            //	.attr("transform", "translate(0, -" + $scope.legendHeight + ")")
            //	.text(function (d) { return d.name; })
            //	.style("font-size", "9pt");

            //// tooltip event bind
            //d3.select("body").on("click", function () {
            //	var selectedPoints = svg.selectAll(".dot").filter(function () {
            //		return this === d3.event.target;
            //	}).data();

            //	if (selectedPoints.length === 0) {
            //		$scope.clearTooltip(tooltip);
            //	} else {
            //		$scope.showTooltip(selectedPoints, tooltip, svg);
            //	}
            //});

            //// tooltip show on key up or key down
            //d3.select("body")
            //	.on("keydown", function () {
            //		var keyCode = d3.event.keyCode;
            //		if (tooltip.selectedPoints && $scope.isKeyUpOrDown(keyCode)) {
            //			var nextPoint;
            //			var selectedPoint = tooltip.selectedPoints[0];
            //			var indexOfPoint = $scope.visiblePoints.indexOf(selectedPoint);
            //			$scope.clearTooltip(tooltip);

            //			switch (keyCode) {
            //				case 40: // down
            //					for (var i = indexOfPoint + 1; i < $scope.visiblePoints.length; i++) {
            //						if ($scope.visiblePoints[i].matterId === selectedPoint.matterId
            //							&& $scope.yValue($scope.visiblePoints[i]) !== $scope.yValue(selectedPoint)) {
            //							nextPoint = $scope.visiblePoints[i];
            //							break;
            //						}
            //					}
            //					break;
            //				case 38: // up
            //					for (var j = indexOfPoint - 1; j >= 0; j--) {
            //						if ($scope.visiblePoints[j].matterId === selectedPoint.matterId
            //							&& $scope.yValue($scope.visiblePoints[j]) !== $scope.yValue(selectedPoint)) {
            //							nextPoint = $scope.visiblePoints[j];
            //							break;
            //						}
            //					}
            //					break;
            //			}

            //			if (nextPoint) {
            //				var selectedPoints = svg.selectAll(".dot").filter(function (d) {
            //					return nextPoint.matterId === d.matterId && $scope.yValue(nextPoint) === $scope.yValue(d);
            //				}).data();

            //				$scope.showTooltip(selectedPoints, tooltip, svg);
            //			}
            //		}
            //	});

            //// preventing scroll in key up and key down
            //window.addEventListener("keydown", function (e) {
            //	if ($scope.isKeyUpOrDown(e.keyCode)) {
            //		e.preventDefault();
            //	}
            //}, false);

            //$scope.loading = false;
        }

        function redrawGenesMap() {

            $scope.fillVisiblePoints();

            var data = $scope.visiblePoints.map(function (points, index) {
                return {
                    hoverinfo: 'text+x+y',
                    type: 'scattergl',
                    mode: 'markers',
                    x: points.map(p => $scope.numericXAxis ? p.numericX : p.x),
                    y: points.map(p => p.subsequenceCharacteristics[$scope.subsequenceCharacteristic.Value]),
                    text: cText(points, index),
                    mode: "markers",
                    marker: { opacity: 0.5 },
                    name: $scope.matters[index].name
                }
            });

            Plotly.newPlot($scope.plot, data, $scope.layout, { responsive: true });

            $scope.plot.on("plotly_click", data => {
                var selectedPoint = $scope.visiblePoints[data.points[0].curveNumber][data.points[0].pointNumber];
                $scope.showTooltip(selectedPoint);
            });
        }


        $scope.setCheckBoxesState = SetCheckBoxesState;

        $scope.drawGenesMap = drawGenesMap;
        $scope.redrawGenesMap = redrawGenesMap;
        $scope.dotVisible = dotVisible;
        $scope.dotsSimilar = dotsSimilar;
        $scope.fillVisiblePoints = fillVisiblePoints;
        $scope.filterByFeature = filterByFeature;
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

        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 3;
        $scope.points = [];
        $scope.visiblePoints = [];
        $scope.matters = [];
        $scope.characteristicComparers = [];
        $scope.filters = [];
        $scope.productFilter = "";
        $scope.tooltipVisible = false;
        $scope.tooltipElements = [];
        $scope.pointsSimilarity = Object.freeze({ "same": 0, "similar": 1, "different": 2 });

        $scope.i = 0;
        $scope.dragging = false;

        $('[data-toggle="tooltip"]').tooltip();



        // dragbar
        function dragbarMouseDown() {
            var main = document.getElementById('main');
            var right = document.getElementById('sidebar');
            var bar = document.getElementById('dragbar');

            const drag = (e) => {
                document.selection ? document.selection.empty() : window.getSelection().removeAllRanges();
                var chart_width = main.style.width = (e.pageX - bar.offsetWidth / 2) + 'px';
                console.log(chart_width)

                Plotly.relayout('chart', {
                    autosize: true
                })
            }

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