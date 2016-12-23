function SubsequencesDistributionResultController(data) {
    "use strict";

    function subsequencesDistributionResult($scope, $http) {

        // shows modal window with progressbar and given text
        function showModalLoadingWindow(headerText) {
            $scope.loadingScreenHeader = headerText;
            $scope.loadingModalWindow.modal("show");
            $scope.loading = true;
        }

        function hideModalLoadingWindow() {
            $scope.loading = false;
            $scope.loadingModalWindow.modal("hide");
        }

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
                if ($scope.dotVisible($scope.points[i])) {
                    $scope.visiblePoints.push($scope.points[i]);
                }
            }
        }

        // returns product attribute index if any
        function getProductAttributeId(dot) {
            return dot.attributes.find(function (a) {
                return $scope.attributes[$scope.attributeValues[a].attribute] === "product";
            });
        }

        // adds and applies new filter
        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });

                d3.selectAll(".dot")
                    .attr("visibility",
                        function (d) {
                            var productId = getProductAttributeId(d);
                            d.filtersVisible.push(productId && $scope.attributeValues[productId].value.toUpperCase().indexOf($scope.newFilter.toUpperCase()) !== -1);
                            return $scope.dotVisible(d) ? "visible" : "hidden";
                        });

                $scope.fillVisiblePoints();
                $scope.newFilter = "";
            }
            // todo: add error message if filter is empty
        }

        // deletes given filter
        function deleteFilter(filter) {
            d3.selectAll(".dot")
                    .attr("visibility",
                        function (d) {
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

                for (var j = 0; j < sequenceData.SubsequencesData.length; j++) {
                    var subsequenceData = sequenceData.SubsequencesData[j];
                    var point = {
                        id: id,
                        matterId: sequenceData.MatterId,
                        sequenceRemoteId: sequenceData.RemoteId,
                        attributes: subsequenceData.Attributes,
                        partial: subsequenceData.partial,
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
                    $scope.points.push(point);
                    $scope.visiblePoints.push(point);
                    id++;
                }
            }
        }

        // filters dots by subsequences feature
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
            var filterVisible = dot.filtersVisible.length === 0 || dot.filtersVisible.some(function(element) {
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
                case 4: // CDS
                case 5: // RRNA
                case 6: // TRNA
                    var firstProductId = getProductAttributeId(d);
                    var secondProductId = getProductAttributeId(dot);
                    if ($scope.attributeValues[firstProductId].value.toUpperCase() !== $scope.attributeValues[secondProductId].value.toUpperCase()) {
                        return false;
                    }
                    break;
            }

            return true;
        }

        // shows tooltip for dot or group of dots
        function showTooltip(selectedDots, tooltip, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            var tooltipHtml = [];

            tooltip.clickedDots = selectedDots;
            var point = selectedDots.data()[0];
            tooltip.selectedDots = svg.selectAll(".dot")
                .filter(function (dot) {
                    if ($scope.dotVisible(dot)) {
                        if (dot.matterId === point.matterId && yValue(dot) === yValue(point)) { // if dots are in the same position
                            tooltipHtml.push($scope.fillPointTooltip(dot));
                            return true;
                        } else if ($scope.highlight) { // if similar dot are highlighted
                            for (var i = 0; i < $scope.characteristicComparers.length; i++) {
                                var dotValue = dot.subsequenceCharacteristics[$scope.characteristicComparers[i].characteristic.Value];
                                var dValue = point.subsequenceCharacteristics[$scope.characteristicComparers[i].characteristic.Value];
                                if (Math.abs(dotValue - dValue) > $scope.characteristicComparers[i].precision) { // if dValue is out of range for any comparer
                                    return false;
                                }
                            }

                            var tooltipColor = $scope.dotsSimilar(point, dot) ? "text-success" : "text-danger";
                            tooltipHtml.push("<span class='" + tooltipColor + "'>" + $scope.fillPointTooltip(dot) + "</span>");

                            return true;
                        }
                    }

                    return false;
                })
                .attr("rx", $scope.selectedDotRadius);

            tooltip.html(tooltipHtml.join("</br></br>"));

            var matrix = selectedDots[0].parentNode.getScreenCTM()
                .translate($scope.xMap(point), $scope.yMap(point));

            tooltip.style("background", "#000")
                .style("color", "#fff")
                .style("border-radius", "5px")
                .style("font-family", "monospace")
                .style("padding", "5px")
                .style("left", (window.pageXOffset + matrix.e + 18) + "px")
                .style("top", (window.pageYOffset + matrix.f + 18) + "px");
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];
            var genBankLink = "<a target='_blank' rel='noopener' href='https://www.ncbi.nlm.nih.gov/nuccore/";
            var name = $scope.matters.find(function(m) { return m.id === d.matterId; }).name;
            var header = d.sequenceRemoteId ? genBankLink + d.sequenceRemoteId + "'>" + name + "</a>" : name;
            tooltipContent.push(header);

            if (d.subsequenceRemoteId) {
                var peptideGenbankLink = genBankLink + d.subsequenceRemoteId + "'>Peptide ncbi page</a>";
                tooltipContent.push(peptideGenbankLink);
            }

            tooltipContent.push($scope.features[d.featureId].Text);

            var attributes = [];
            for (var i = 0; i < d.attributes.length; i++) {
                var attributeValue = $scope.attributeValues[d.attributes[i]];
                attributes.push($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : " = " + attributeValue.value));
            }

            tooltipContent.push(attributes.join("<br/>"));

            if (d.partial) {
                tooltipContent.push("partial");
            }

            var start = d.positions[0] + 1;
            var end = d.positions[0] + d.lengths[0];
            var positionGenbankLink = d.sequenceRemoteId ?
                                      genBankLink + d.sequenceRemoteId + "?from=" + start + "&to=" + end + "'>" + d.positions.join(", ") + "</a>" :
                                      d.positions.join(", ");
            tooltipContent.push("Position: " + positionGenbankLink);
            tooltipContent.push("Length: " + d.lengths.join(", "));
            tooltipContent.push("(" + d.x + ", " + yValue(d) + ")");

            return tooltipContent.join("</br>");
        }

        function clearTooltip(tooltip) {
            if (tooltip) {
                tooltip.html("").style("opacity", 0);

                if (tooltip.selectedDots) {
                    tooltip.selectedDots.attr("rx", $scope.dotRadius);
                }
            }
        }

        function isKeyUpOrDown(keyCode) {
            return keyCode === 40 || keyCode === 38;
        }

        function xValue(d) {
            return $scope.numericXAxis ? d.numericX : d.x;
        }

        function yValue(d) {
            return d.subsequenceCharacteristics[$scope.subsequenceCharacteristic.Value];
        }

        // main drawing method
        function drawGenesMap() {
            $scope.showModalLoadingWindow("Drawing...");
            // removing previous chart and tooltip if any
            d3.select(".tooltip").remove();
            d3.select(".genes-map-svg").remove();

            // sorting points by selected characteristic
            $scope.points.sort(function (first, second) {
                return $scope.yValue(second) - $scope.yValue(first);
            });
            $scope.visiblePoints.sort(function (first, second) {
                return $scope.yValue(second) - $scope.yValue(first);
            });

            // all organisms are visible after redrawing
            $scope.points.forEach(function (point) {
                point.matterVisible = true;
                for (var i = 0; i < $scope.features.length; i++) {
                    if ($scope.features[i].Value === point.featureId) {
                        point.featureVisible = $scope.features[i].Selected;
                        break;
                    }
                }
            });

            // chart size and margin settings
            var margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            var width = $scope.width - margin.left - margin.right;
            var height = $scope.hight - margin.top - margin.bottom;

            // setup x 
            $scope.xScale = d3.scale.linear().range([0, width]); // value -> display
            $scope.xMap = function (d) { return $scope.xScale($scope.xValue(d)); }; // data -> display
            var xAxis = d3.svg.axis().scale($scope.xScale).orient("bottom");
            xAxis.innerTickSize(-height).outerTickSize(0).tickPadding(10);

            // setup y
            $scope.yScale = d3.scale.linear().range([height, 0]); // value -> display
            $scope.yMap = function (d) { return $scope.yScale($scope.yValue(d)); }; // data -> display
            var yAxis = d3.svg.axis().scale($scope.yScale).orient("left");
            yAxis.innerTickSize(-width).outerTickSize(0).tickPadding(10);

            // setup fill color
            var cValue = function (d) { return d.matterId; };
            var color = d3.scale.category20();

            // add the graph canvas to the body of the webpage
            var svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.hight)
                .attr("class", "genes-map-svg")
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add the tooltip area to the webpage
            var tooltip = d3.select("#chart").append("div")
                .attr("class", "tooltip")
                .style("opacity", 0);


            // calculating margins for dots
            var xMin = d3.min($scope.points, $scope.xValue);
            var xMax = d3.max($scope.points, $scope.xValue);
            var xMargin = (xMax - xMin) * 0.05;
            var yMax = d3.max($scope.points, $scope.yValue);
            var yMin = d3.min($scope.points, $scope.yValue);
            var yMargin = (yMax - yMin) * 0.05;

            // don't want dots overlapping axis, so adding buffer to data domain
            $scope.xScale.domain([xMin - xMargin, xMax + xMargin]);
            $scope.yScale.domain([yMin - yMargin, yMax + yMargin]);

            // x-axis
            svg.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(0," + height + ")")
                .call(xAxis)
                .append("text")
                .attr("class", "label")
                .attr("x", width)
                .attr("y", -6)
                .style("text-anchor", "end")
                .text($scope.sequenceCharacteristicName)
                .style("font-size", "12pt");

            // y-axis
            svg.append("g")
                .attr("class", "y axis")
                .call(yAxis)
                .append("text")
                .attr("class", "label")
                .attr("transform", "rotate(-90)")
                .attr("y", 6)
                .attr("dy", ".71em")
                .style("text-anchor", "end")
                .text($scope.subsequenceCharacteristic.Text)
                .style("font-size", "12pt");

            // draw dots
            svg.selectAll(".dot")
                .data($scope.points)
                .enter()
                .append("ellipse")
                .attr("class", "dot")
                .attr("rx", $scope.dotRadius)
                .attr("ry", $scope.dotRadius)
                .attr("cx", $scope.xMap)
                .attr("cy", $scope.yMap)
                .style("fill-opacity", 0.6)
                .style("fill", function (d) { return color(cValue(d)); })
                .style("stroke", function (d) { return color(cValue(d)); })
                .attr("visibility", function (dot) {
                    return $scope.dotVisible(dot) ? "visible" : "hidden";
                });

            // draw legend
            var legend = svg.selectAll(".legend")
                .data($scope.matters)
                .enter().append("g")
                .attr("class", "legend")
                .attr("transform", function (d, i) { return "translate(0," + i * 20 + ")"; })
                .on("click", function (d) {
                    d.visible = !d.visible;
                    var legendEntry = d3.select(this);
                    legendEntry.select("text")
                        .style("opacity", function () { return d.visible ? 1 : 0.5; });
                    legendEntry.select("rect")
                        .style("fill-opacity", function () { return d.visible ? 1 : 0; });

                    svg.selectAll(".dot")
                        .filter(function (dot) { return dot.matterId === d.id; })
                        .attr("visibility", function (dot) {
                            dot.matterVisible = d.visible;
                            return $scope.dotVisible(dot) ? "visible" : "hidden";
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

            // tooltip event bind
            d3.select("body").on("click", function () {
                var clickedDots = svg.selectAll(".dot").filter(function () {
                    return this === d3.event.target;
                });

                if (clickedDots.empty()) {
                    $scope.clearTooltip(tooltip);
                } else {
                    $scope.showTooltip(clickedDots, tooltip, svg);
                }
            });

            // tooltip show on key up or key down
            d3.select("body")
                .on("keydown", function () {
                    if (tooltip.clickedDots) {
                        var clickedpoint = tooltip.clickedDots.data()[0];
                        var keyCode = d3.event.keyCode;
                        if (isKeyUpOrDown(keyCode)) {
                            var nextPoint;
                            var indexOfPoint = $scope.visiblePoints.indexOf(clickedpoint);
                            $scope.clearTooltip(tooltip);

                            switch (keyCode) {
                                case 40: // down
                                    for (var i = indexOfPoint + 1; i < $scope.visiblePoints.length; i++) {
                                        if ($scope.visiblePoints[i].matterId === clickedpoint.matterId 
                                           && yValue($scope.visiblePoints[i]) !== yValue(clickedpoint)) {
                                            nextPoint = $scope.visiblePoints[i];
                                            break;
                                        }
                                    }
                                    break;
                                case 38: // up
                                    for (var j = indexOfPoint - 1; j >= 0; j--) {
                                        if ($scope.visiblePoints[j].matterId === clickedpoint.matterId
                                            && yValue($scope.visiblePoints[j]) !== yValue(clickedpoint)) {
                                            nextPoint = $scope.visiblePoints[j];
                                            break;
                                        }
                                    }
                                    break;
                            }

                            if (nextPoint) {
                                var clickedDots = svg.selectAll(".dot").filter(function (d) {
                                    return nextPoint.matterId === d.matterId && yValue(nextPoint) === yValue(d);
                                });
                                return $scope.showTooltip(clickedDots, tooltip, svg);
                            }
                        }
                    }
                });

            // preventing scroll in key up and key down
            window.addEventListener("keydown", function (e) {
                if (isKeyUpOrDown(e.keyCode)) {
                    e.preventDefault();
                }
            }, false);
            $scope.hideModalLoadingWindow();
        }

        $scope.setCheckBoxesState = SetCheckBoxesState;

        $scope.drawGenesMap = drawGenesMap;
        $scope.dotVisible = dotVisible;
        $scope.dotsSimilar = dotsSimilar;
        $scope.fillVisiblePoints = fillVisiblePoints;
        $scope.filterByFeature = filterByFeature;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.isKeyUpOrDown = isKeyUpOrDown;
        $scope.xValue = xValue;
        $scope.yValue = yValue;
        $scope.addCharacteristicComparer = addCharacteristicComparer;
        $scope.deleteCharacteristicComparer = deleteCharacteristicComparer;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.getProductAttributeId = getProductAttributeId;
        $scope.showModalLoadingWindow = showModalLoadingWindow;
        $scope.hideModalLoadingWindow = hideModalLoadingWindow;

        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 3;
        $scope.points = [];
        $scope.visiblePoints = [];
        $scope.matters = [];
        $scope.characteristicComparers = [];
        $scope.filters = [];
        $scope.productFilter = "";
        $scope.loadingModalWindow = $("#loadingDialog");

        $scope.showModalLoadingWindow("Loading genes map data");

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http({
            url: "/api/TaskManagerWebApi/" + $scope.taskId,
            method: "GET"
        }).success(function (data) {
            MapModelFromJson($scope, JSON.parse(data));

            $scope.legendHeight = $scope.result.length * 20;
            $scope.hight = 800 + $scope.legendHeight;
            $scope.width = 800;
            $scope.subsequenceCharacteristic = $scope.subsequencesCharacteristicsList[0];

            $scope.fillPoints();
            $scope.addCharacteristicComparer();
            $scope.hideModalLoadingWindow();
        }).error(function (data) {
            alert("Failed loading genes map data");
        });
    }

    angular.module("SubsequencesDistributionResult", []).controller("SubsequencesDistributionResultCtrl", ["$scope", "$http", subsequencesDistributionResult]);
}