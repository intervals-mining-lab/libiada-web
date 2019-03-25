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

        // fills array of currently visible points ***
        function fillVisiblePoints() {
            $scope.visiblePoints = [];
            for (var i = 0; i < $scope.points.length; i++) {
                if ($scope.dotVisible($scope.points[i])) {
                    $scope.visiblePoints.push($scope.points[i]);
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

        // adds and applies new filter @@@
        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });

                d3.selectAll(".dot")
                    .attr("visibility", function (d) {
                        var filterValue = $scope.newFilter.toUpperCase();
                        var visible = $scope.isAttributeEqual(d, "product", filterValue);
                        visible = visible || $scope.isAttributeEqual(d, "locus_tag", filterValue);
                        d.filtersVisible.push(visible);
                        return $scope.dotVisible(d) ? "visible" : "hidden";
                    });

                $scope.fillVisiblePoints();
                $scope.newFilter = "";
            }
            // todo: add error message if filter is empty
        }

        // deletes given filter @@@
        function deleteFilter(filter) {
            d3.selectAll(".dot")
                .attr("visibility", function (d) {
                    d.filtersVisible.splice($scope.filters.indexOf(filter), 1);
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
            $scope.filters.splice($scope.filters.indexOf(filter), 1);
            $scope.fillVisiblePoints();
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
            d3.selectAll(".dot")
                .filter(function (dot) {
                    return dot.featureId === parseInt(feature.Value);
                })
                .attr("visibility", function (d) {
                    d.featureVisible = feature.Selected;
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });

            for (var i = 0; i < $scope.points.length; i++) {
                if ($scope.points[i].featureId === parseInt(feature.Value)) {
                    $scope.points[i].featureVisible = feature.Selected;
                }
            }

            // optimize this method calls
            $scope.fillVisiblePoints();
        }

        // checks if dot is visible
        function dotVisible(dot) {
            var filterVisible = dot.filtersVisible.length === 0 || dot.filtersVisible.some(function (element) {
                return element;
            });

            return dot.featureVisible && dot.matterVisible && filterVisible;
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
                        return false;
                    }
                    break;
            }

            return true;
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

        // shows tooltip for dot or group of dots @@@
        function showTooltip(data) {
         //   $scope.clearTooltip(tooltip);
            $scope.tooltipVisible = true;
            $scope.tooltipElements.length = 0;

            var tooltipHtml = [];

            //tooltip.selectedPoints = selectedPoints;

            var points = data.points;

            for (var i = 0; i < points.length; i++) {
                $scope.tooltipElements.push(fillPointTooltip(points[i]));
            }

            //var point = selectedPoints[0];
            //tooltip.selectedDots = svg.selectAll(".dot")
            //    .filter(function (dot) {
            //        if ($scope.dotVisible(dot)) {
            //            if (dot.matterId === point.matterId && $scope.yValue(dot) === $scope.yValue(point)) { // if dots are in the same position
            //                tooltipHtml.push($scope.fillPointTooltip(dot));
            //                return true;
            //            } else if ($scope.highlight) { // if similar dot are highlighted
            //                for (var i = 0; i < $scope.characteristicComparers.length; i++) {
            //                    var dotValue = dot.subsequenceCharacteristics[$scope.characteristicComparers[i].characteristic.Value];
            //                    var dValue = point.subsequenceCharacteristics[$scope.characteristicComparers[i].characteristic.Value];
            //                    if (Math.abs(dotValue - dValue) > $scope.characteristicComparers[i].precision) { // if dValue is out of range for any comparer
            //                        return false;
            //                    }
            //                }

            //                var tooltipColor = $scope.dotsSimilar(point, dot) ? "text-success" : "text-danger";
            //                tooltipHtml.push("<span class='" + tooltipColor + "'>" + $scope.fillPointTooltip(dot) + "</span>");
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
            //tooltip.html(tooltipHtml.join("</br></br>"));

            //var matrix = tooltip.selectedDots.nodes()[0].parentNode.getScreenCTM()
            //    .translate($scope.xMap(point), $scope.yMap(point));

            //tooltip.style("background", "#eee")
            //    .style("color", "#000")
            //    .style("border-radius", "5px")
            //    .style("font-family", "monospace")
            //    .style("padding", "5px")
            //    .style("left", (window.pageXOffset + matrix.e + 15) + "px")
            //    .style("top", (window.pageYOffset + matrix.f + 15) + "px");

            $scope.$apply();
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var point = $scope.points[d.curveNumber][d.pointNumber];
            var tooltipElement = {
                name: $scope.matters[d.curveNumber].name,
                sequenceRemoteId: point.sequenceRemoteId,
                feature: $scope.features[point.featureId].Text,
                attributes: $scope.getAttributesText(point.attributes),
                partial: point.partial
            };



            if (point.subsequenceRemoteId) {
                tooltipElement.remoteId = point.subsequenceRemoteId;
            }
            tooltipElement.position = "(";
            tooltipElement.length = 0;
            tooltipElement.positions = point.positions;
            tooltipElement.lengths = point.lengths;


            for (var i = 0; i < point.positions.length; i++) {
                tooltipElement.position += point.positions[i]+1;
                tooltipElement.position += "..";
                tooltipElement.position += point.positions[i] + point.lengths[i];
                tooltipElement.length += point.lengths[i];
                if (i !== point.positions.length - 1) {
                    tooltipElement.position += ", ";
                }
            }

            tooltipElement.position += ")";
            
            //var positionGenbankLink = d.sequenceRemoteId ?
            //    genBankLink + d.sequenceRemoteId + "?from=" + start + "&to=" + end + "'>" + d.positions.join(", ") + "</a>" :
            //    d.positions.join(", ");
            //tooltipContent.push("Position: " + positionGenbankLink);
            //tooltipContent.push("Length: " + d.lengths.join(", "));
            //tooltipContent.push("(" + d.x + ", " + $scope.yValue(d) + ")");

            return tooltipElement;
        }

        // clears tooltip and unselects dots @@@
        function clearTooltip(tooltip) {
            if (tooltip) {
                tooltip.html("").style("opacity", 0);

                if (tooltip.selectedDots) {
                    tooltip.selectedDots.attr("rx", $scope.dotRadius);
                }
                if (tooltip.lines) {
                    tooltip.lines.remove();
                }
            }
        }

        function isKeyUpOrDown(keyCode) {
            return keyCode === 40 || keyCode === 38;
        }

        function xValues(points) {
            return points.map(function (d) { return $scope.numericXAxis ? d.numericX : d.x; });
        }

        function yValues(points) {
            return points.map(function (d) { return d.subsequenceCharacteristics[$scope.subsequenceCharacteristic.Value] });
        }

        function cText(points, index) {
            return points.map(function (d) { return $scope.matters[index].name; });
        }

        function cColor(v) {
            $scope.colorMap = d3.scaleOrdinal(d3.schemeCategory20);
            return $scope.colorMap(v.Id);
        }


        // main drawing method
        function drawGenesMap() {
            var chartParams = { responsive: true };
            var layout = {
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
            var myPlot = document.getElementById('chart');

            while (myPlot.firstChild) myPlot.removeChild(myPlot.firstChild);

            var data = $scope.points.map(function (points, index) {
                return {
                    hoverinfo: 'text+x+y',
                    type: 'scattergl',
                    mode: 'markers',
                    x: xValues(points),
                    y: yValues(points),
                    text: cText(points, index),
                    mode: "markers",
                    marker: {
                        opacity: 0.5,
                    },
                    name: $scope.matters[index].name
                }
            });

            Plotly.plot('chart', data, layout, chartParams);

            myPlot.on('plotly_click', $scope.showTooltip
            //    function (data) {
            //    var pts = '';
            //    for (var i = 0; i < data.points.length; i++) {
            //        pts = 'x = ' + data.points[i].x + '\ny = ' +
            //            data.points[i].y.toPrecision(4) + '\n\n' +
            //            "curve number = " + data.points[i].curveNumber + '\n\n' +
            //            "point number = " + data.points[i].pointNumber + '\n\n' +
            //            "point index = " + data.points[i].pointIndex + '\n\n' +
            //            "text = " + data.points[i].text + '\n\n';
            //    }
            //    alert('Closest point clicked:\n\n' + pts);
                //}
            );

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

        //function dragbarMouseDown(e) {
        //    e.preventDefault();

        //    $scope.dragging = true;
        //    var main = $('#main');
        //    var ghostbar = $('<div>',
        //        {
        //            id: 'ghostbar',
        //            css: {
        //                height: main.outerHeight(),
        //                top: main.offset().top,
        //                left: main.offset().left
        //            }
        //        }).appendTo('body');

        //    $(document).mousemove(function (e) {
        //        ghostbar.css("left", e.pageX + 2);
        //    });
        //};

        $scope.setCheckBoxesState = SetCheckBoxesState;

        $scope.drawGenesMap = drawGenesMap;
        $scope.dotVisible = dotVisible;
        $scope.dotsSimilar = dotsSimilar;
        $scope.fillVisiblePoints = fillVisiblePoints;
        $scope.filterByFeature = filterByFeature;
        $scope.fillPoints = fillPoints;
        $scope.getAttributesText = getAttributesText;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.isKeyUpOrDown = isKeyUpOrDown;
        $scope.yValue = yValues;
        $scope.xValue = xValues;
        $scope.addCharacteristicComparer = addCharacteristicComparer;
        $scope.deleteCharacteristicComparer = deleteCharacteristicComparer;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.getAttributeIdByName = getAttributeIdByName;
        $scope.isAttributeEqual = isAttributeEqual;
       // $scope.dragbarMouseDown = dragbarMouseDown;

        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 3;
        $scope.points = [];
        $scope.visiblePoints = [];
        $scope.matters = [];
        $scope.characteristicComparers = [];
        $scope.filters = [];
        $scope.productFilter = "";
        $scope.tab = "None";
        $scope.tooltipVisible = false;
        $scope.tooltipElements = [];

        $scope.i = 0;
        $scope.dragging = false;


        //$(document).mouseup(function (e) {
        //    if ($scope.dragging) {
        //        $('#sidebar').css("width", e.pageX + 2);
        //        $('#main').css("left", e.pageX + 2);
        //        $('#ghostbar').remove();
        //        $(document).unbind('mousemove');
        //        $scope.dragging = false;
        //    }
        //});


        $scope.loadingScreenHeader = "Loading genes map data";

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;


        $http.get("/api/TaskManagerWebApi/" + $scope.taskId)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));

                $scope.legendHeight = $scope.result.length * 20;
                $scope.height = 800 + $scope.legendHeight;
                $scope.width = 800;
                $scope.subsequenceCharacteristic = $scope.subsequencesCharacteristicsList[0];

                $scope.fillPoints();
                $scope.addCharacteristicComparer();
                $scope.loading = false;
                drawGenesMap();
            }, function () {
                alert("Failed loading genes map data");

                $scope.loading = false;
            });
    }

    angular.module("libiada", []).controller("SubsequencesDistributionResultPlotlyCtrl", ["$scope", "$http", subsequencesDistributionResultPlotly]);

}