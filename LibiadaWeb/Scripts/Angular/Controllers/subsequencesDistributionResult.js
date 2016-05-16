function SubsequencesDistributionResultController(data) {
    "use strict";

    function subsequencesDistributionResult($scope) {
        MapModelFromJson($scope, data);

        // initializes data for genes map 
        function fillPoints() {
            var id = 0;
            for (var i = 0; i < $scope.result.length; i++) {
                var sequenceData = $scope.result[i];
                $scope.matters.push({ id: sequenceData.MatterId, name: sequenceData.MatterName, visible: true });

                for (var j = 0; j < sequenceData.SubsequencesData.length; j++) {
                    var subsequenceData = sequenceData.SubsequencesData[j];
                    $scope.points.push({
                        id: id,
                        matterId: sequenceData.MatterId,
                        name: sequenceData.MatterName,
                        sequenceRemoteId: sequenceData.RemoteId,
                        attributes: subsequenceData.Attributes,
                        partial: subsequenceData.partial,
                        featureId: subsequenceData.FeatureId,
                        positions: subsequenceData.Starts,
                        lengths: subsequenceData.Lengths,
                        subsequenceRemoteId: subsequenceData.RemoteId,
                        numericX: i + 1,
                        x: sequenceData.Characteristic,
                        y: subsequenceData.CharacteristicsValues[0],
                        gcRatio: subsequenceData.CharacteristicsValues[subsequenceData.CharacteristicsValues.length - 1],
                        featureVisible: true,
                        matterVisible: true
                    });

                    id++;
                }
            }
        }

        // filters dots by subsequences feature
        function filterByFeature(feature) {
            d3.selectAll(".dot")
                .filter(function (dot) { return dot.featureId === feature.Value; })
                .attr("visibility", function (d) {
                    d.featureVisible = feature.Selected;
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
        }

        // checks if dot is visible
        function dotVisible(dot) {
            return dot.featureVisible && dot.matterVisible;
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];
            var genBankLink = "<a target='_blank' href='http://www.ncbi.nlm.nih.gov/nuccore/";

            var header = d.sequenceRemoteId ? genBankLink + d.sequenceRemoteId + "'>" + d.name + "</a>" : d.name;
            tooltipContent.push(header);

            if (d.subsequenceRemoteId) {
                var peptideGenbankLink = genBankLink + d.subsequenceRemoteId + "'>Peptide ncbi page</a>";
                tooltipContent.push(peptideGenbankLink);
            }

            tooltipContent.push($scope.featuresNames[d.featureId]);

            if (d.attributes.length > 0) {
                tooltipContent.push(d.attributes.join("<br/>"));
            }

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
            tooltipContent.push("GC ratio: " + d.gcRatio);
            tooltipContent.push("(" + d.x + ", " + d.y + ")");

            return tooltipContent.join("</br>");
        }

        function showTooltip(d, tooltip, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            var tooltipHtml = [];

            tooltip.selectedPoint = d;
            tooltip.selectedDots = svg.selectAll(".dot")
                    .filter(function (dot) {
                        if (dot.matterId === d.matterId && dot.y === d.y) {
                            tooltipHtml.push($scope.fillPointTooltip(dot));
                            return true;
                        } else {
                            return false;
                        }
                    })
                    .attr("rx", $scope.selectedDotRadius);

            if ($scope.highlight) {
                tooltip.similarDots = svg.selectAll(".dot")
                    .filter(function (dot) {
                        if (dot.matterId !== d.matterId
                            && Math.abs(dot.y - d.y) <= $scope.precision
                            && Math.abs(dot.gcRatio - d.gcRatio) <= $scope.gcPrecision) {
                            tooltipHtml.push($scope.fillPointTooltip(dot));
                            return true;
                        } else {
                            return false;
                        }
                    })
                    .attr("rx", $scope.selectedDotRadius);
            }

            tooltip.html(tooltipHtml.join("</br></br>"));

            tooltip.style("background", "#000")
                .style("color", "#fff")
                .style("border-radius", "5px")
                .style("font-family", "monospace")
                .style("padding", "5px")
                .style("left", (d3.event.pageX + 18) + "px")
                .style("top", (d3.event.pageY + 18) + "px");

            tooltip.hideTooltip = false;
        }

        function clearTooltip(tooltip) {
            if (tooltip) {
                if (tooltip.hideTooltip) {
                    tooltip.html("").style("opacity", 0);

                    if (tooltip.selectedDots) {
                        tooltip.selectedDots.attr("rx",  $scope.dotRadius);
                    }

                    if (tooltip.similarDots) {
                        tooltip.similarDots.attr("rx", $scope.dotRadius);
                    }
                }

                tooltip.hideTooltip = true;
            }
        }

        function drawGenesMap() {
            // removing previous chart and tooltip if any
            d3.select(".tooltip").remove();
            d3.select("svg").remove();

            // all organisms are visible after redrawing
            $scope.points.forEach(function(point) {
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
            var xValue = function(d) { return $scope.numericXAxis ? d.numericX : d.x; }; // data -> value
            var xScale = d3.scale.linear().range([0, width]); // value -> display
            var xMap = function(d) { return xScale(xValue(d)); }; // data -> display
            var xAxis = d3.svg.axis().scale(xScale).orient("bottom");
            xAxis.innerTickSize(-height).outerTickSize(0).tickPadding(10);

            // setup y
            var yValue = function(d) { return d.y; }; // data -> value
            var yScale = d3.scale.linear().range([height, 0]); // value -> display
            var yMap = function(d) { return yScale(yValue(d)); }; // data -> display
            var yAxis = d3.svg.axis().scale(yScale).orient("left");
            yAxis.innerTickSize(-width).outerTickSize(0).tickPadding(10);

            // setup fill color
            var cValue = function(d) { return d.matterId; };
            var color = d3.scale.category20();

            // add the graph canvas to the body of the webpage
            var svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.hight)
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add the tooltip area to the webpage
            var tooltip = d3.select("#chart").append("div")
                .attr("class", "tooltip")
                .style("opacity", 0);

            // preventing tooltip hiding if dot clicked
            tooltip.on("click", function() { tooltip.hideTooltip = false; });

            // hiding tooltip
            d3.select("#chart").on("click", function() { $scope.clearTooltip(tooltip); });

            // calculating margins for dots
            var xMin = d3.min($scope.points, xValue);
            var xMax = d3.max($scope.points, xValue);
            var xMargin = (xMax - xMin) * 0.05;
            var yMax = d3.max($scope.points, yValue);
            var yMin = d3.min($scope.points, yValue);
            var yMargin = (yMax - yMin) * 0.05;

            // don't want dots overlapping axis, so add in buffer to data domain
            xScale.domain([xMin - xMargin, xMax + xMargin]);
            yScale.domain([yMin - yMargin, yMax + yMargin]);

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
                .text($scope.subsequencesCharacteristicName)
                .style("font-size", "12pt");

            // draw dots
            svg.selectAll(".dot")
                .data($scope.points)
                .enter()
                .append("ellipse")
                .attr("class", "dot")
                .attr("rx", $scope.dotRadius)
                .attr("ry", $scope.dotRadius)
                .attr("cx", xMap)
                .attr("cy", yMap)
                .style("fill-opacity", 0.6)
                .style("fill", function(d) { return color(cValue(d)); })
                .style("stroke", function(d) { return color(cValue(d)); })
                .attr("visibility", function(dot) {
                    return $scope.dotVisible(dot) ? "visible" : "hidden";
                })
                .on("click", function(d) { return $scope.showTooltip(d, tooltip, svg); });

            $scope.mattersDots = [];
            for (var i = 0; i < $scope.matters; i++) {
                var matterId = $scope.matters[i].id;
                $scope.mattersDots[matterId] = d3.selectAll(".dot")
                    .filter(function(dot) { return dot.matterId === matterId; });
            }

            $scope.featuresDots = [];
            for (var j = 0; j < $scope.features; j++) {
                var featureId = $scope.features[j].Value;
                $scope.mattersDots[featureId] = d3.selectAll(".dot")
                    .filter(function(dot) { return dot.featureId === featureId; });
            }

            // draw legend
            var legend = svg.selectAll(".legend")
                .data($scope.matters)
                .enter().append("g")
                .attr("class", "legend")
                .attr("transform", function(d, i) { return "translate(0," + i * 20 + ")"; })
                .on("click", function(d) {
                    d.visible = !d.visible;
                    var legendEntry = d3.select(this);
                    legendEntry.select("text")
                        .style("opacity", function() { return d.visible ? 1 : 0.5; });
                    legendEntry.select("rect")
                        .style("fill-opacity", function() { return d.visible ? 1 : 0; });

                    svg.selectAll(".dot")
                        .filter(function(dot) { return dot.matterId === d.id; })
                        .attr("visibility", function(dot) {
                            dot.matterVisible = d.visible;
                            return $scope.dotVisible(dot) ? "visible" : "hidden";
                        });
                });

            // draw legend colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", function(d) { return color(d.id); })
                .style("stroke", function(d) { return color(d.id); })
                .style("stroke-width", 4)
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            // draw legend text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
                .text(function(d) { return d.name; })
                .style("font-size", "9pt");

            d3.select("body")
                .on("keydown", function() {
                    var nextPoint;
                    var indexOfPoint = $scope.points.indexOf(tooltip.selectedPoint);
                    switch (d3.event.keyCode) {
                    case 40:
                        $scope.clearTooltip(tooltip);
                        for (var i = indexOfPoint + 1; i < $scope.points.length; i++) {
                            if ($scope.points[i].matterId === tooltip.selectedPoint.matterId) {
                                nextPoint = $scope.points[i];
                                break;
                            }
                        }
                        if (nextPoint) {
                            return $scope.showTooltip(nextPoint, tooltip, svg);
                        }
                        break;
                    case 38:
                        $scope.clearTooltip(tooltip);
                        for (var j = indexOfPoint - 1; j >= 0; j++) {
                            if ($scope.points[j].matterId === tooltip.selectedPoint.matterId) {
                                nextPoint = $scope.points[j];
                                break;
                            }
                        }

                        if (nextPoint) {
                            return $scope.showTooltip(nextPoint, tooltip, svg);
                        }
                        break;
                    }
                });
        }

        $scope.setCheckBoxesState = SetCheckBoxesState;

        $scope.drawGenesMap = drawGenesMap;
        $scope.dotVisible = dotVisible;
        $scope.filterByFeature = filterByFeature;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;

        $scope.legendHeight = $scope.result.length * 20;
        $scope.hight = 800 + $scope.legendHeight;
        $scope.width = 800;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 3;
        $scope.precision = 0;
        $scope.gcPrecision = 10;
        $scope.points = [];
        $scope.matters = [];
        $scope.fillPoints();
    }

    angular.module("SubsequencesDistributionResult", []).controller("SubsequencesDistributionResultCtrl", ["$scope", subsequencesDistributionResult]);
}