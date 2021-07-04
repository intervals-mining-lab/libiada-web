function OrderTransformationVisualizationResultController() {
    "use strict";

    function orderTransformationVisualizationResult($scope, $http) {

        // initializes data for chart
        function fillLegend() {
            $scope.legend = [];
            for (var i = 1; i < $scope.transformationsList.length; i++) {
                $scope.legend.push({ id: i - 1, name: $scope.transformationsList[i].Text, visible: true });
            }
        }

        function fillPoints() {
            var initialOrder = $scope.initialOrder.id;
            var checkedOrders = [initialOrder];
            var ordersForChecking = [initialOrder];
            var transformationVisibility = [];
            for (var l = 0; l < $scope.legend.length; l++) {
                transformationVisibility.push({ id: l, name: $scope.legend[l].name, visible: $scope.legend[l].visible });
            }
            $scope.points.push({
                id: 0,
                value: initialOrder,
                x: 0,
                y: initialOrder,
                transformationVisibility: transformationVisibility
            });
            var counterIdPoints = 1;
            $scope.counterIteration = 1;
            while (ordersForChecking.length > 0) {
                var newOrdersForChecking = [];
                for (var i = 0; i < ordersForChecking.length; i++) {
                    var resultTransformations = $scope.transformationsData[ordersForChecking[i]].ResultTransformation;
                    for (var j = 0; j < resultTransformations.length; j++) {
                        var resultTransformation = resultTransformations[j];
                        if ($scope.transformationType.Text === "All" || resultTransformation.Transformation === $scope.transformationType.Text) {
                            var pointExist = false;
                            var orderId = resultTransformation.OrderId;
                            for (var k = 0; k < $scope.points.length; k++) {
                                if ($scope.points[k].x === $scope.counterIteration &&
                                    $scope.points[k].y === orderId) {
                                    pointExist = true;
                                    break;
                                }
                            }
                            if (!pointExist) {
                                $scope.points.push({
                                    id: counterIdPoints++,
                                    value: orderId,
                                    x: $scope.counterIteration,
                                    y: orderId,
                                    transformationVisibility: transformationVisibility
                                });
                            }
                            if (checkedOrders.indexOf(orderId) === -1) {
                                checkedOrders.push(orderId);
                                newOrdersForChecking.push(orderId);
                            }
                        }
                    }
                }
                ordersForChecking = newOrdersForChecking;
                $scope.counterIteration++;
            }
        }

        function fillLines() {
            var counterIdLines = 0;
            for (var i = 0; i < $scope.points.length; i++) {
                var resultTransformation = $scope.transformationsData[$scope.points[i].value].ResultTransformation;
                for (var j = 0; j < resultTransformation.length; j++) {
                    var transformationType = resultTransformation[j].Transformation;
                    if ($scope.transformationType.Text === "All" || transformationType === $scope.transformationType.Text) {
                        var colorId = $scope.legend.find(d => d.name === transformationType).id;
                        var childOrder = resultTransformation[j].OrderId;
                        var line = {
                            x1: $scope.points[i].x,
                            y1: $scope.points[i].y,
                            x2: $scope.points[i].x + 1,
                            y2: childOrder,
                            value: transformationType,
                            startOrderId: $scope.points[i].value,
                            colorId: colorId
                        };
                        var orderExist = false;
                        for (var k = 0; k < $scope.points.length; k++) {
                            if ($scope.points[k].x === $scope.points[i].x + 1 &&
                                $scope.points[k].y === childOrder) {
                                orderExist = true;
                                break;
                            }
                        }
                        if (orderExist) {
                            var lineExist = false;
                            var lineIterator = 0;
                            for (var m = 0; m < $scope.lines.length; m++) {
                                if ($scope.lines[m].x1 === line.x1 &&
                                    $scope.lines[m].y1 === line.y1 &&
                                    $scope.lines[m].x2 === line.x2 &&
                                    $scope.lines[m].y2 === line.y2) {
                                    lineExist = true;
                                    lineIterator = ++$scope.lines[m].iterator;
                                    break;
                                }
                            }
                            if (lineExist) {
                                var cyline = (line.y1 + line.y2) / 2.0;
                                var cxline = (line.x1 + line.x2) / 2.0;
                                var yAmplitude = line.y2 - line.y1;
                                var shifty = 0.2 * lineIterator;
                                var shiftx = 0.05 * lineIterator * Math.abs(yAmplitude) / $scope.ordersIds.length;
                                $scope.lines.push({
                                    id: counterIdLines++,
                                    value: line.value,
                                    arrowType: -1,
                                    iterator: 0,
                                    x1: line.x1,
                                    y1: line.y1,
                                    x2: cxline + shiftx,
                                    y2: cyline + shifty,
                                    startOrderId: line.startOrderId,
                                    colorId: line.colorId
                                });
                                $scope.lines.push({
                                    id: counterIdLines++,
                                    value: line.value,
                                    arrowType: j,
                                    iterator: 0,
                                    x1: cxline + shiftx,
                                    y1: cyline + shifty,
                                    x2: line.x2,
                                    y2: line.y2,
                                    startOrderId: line.startOrderId,
                                    colorId: line.colorId
                                });
                            } else {
                                $scope.lines.push({
                                    id: counterIdLines++,
                                    value: line.value,
                                    arrowType: j,
                                    iterator: 0,
                                    x1: line.x1,
                                    y1: line.y1,
                                    x2: line.x2,
                                    y2: line.y2,
                                    startOrderId: line.startOrderId,
                                    colorId: line.colorId
                                });
                            }
                        }
                    }
                }
            }
        }

        // initializes data for chart
        function fillPointsAndLines() {
            $scope.points = [];
            $scope.lines = [];
            fillPoints();
            fillLines();
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];
            tooltipContent.push("Order ID: " + d.value);
            tooltipContent.push("Order: " + $scope.orders[d.value]);
            return tooltipContent.join("</br>");
        }

        // shows tooltip for dot or group of dots
        function showTooltip(event, d, tooltip, svg) {
            $scope.clearTooltip(tooltip);
            var color = d3.scaleOrdinal(d3.schemeCategory10);

            tooltip.style("opacity", 0.9);

            var tooltipHtml = [];

            tooltip.selectedDots = svg.selectAll(".dot")
                .filter(dot => {
                        if (dot.x === d.x && dot.y === d.y) {
                            tooltipHtml.push($scope.fillPointTooltip(dot));
                            return true;
                        } else {
                            return false;
                        }
                    })
                .attr("rx", $scope.selectedDotRadius)
                .attr("ry", $scope.selectedDotRadius);

            tooltip.html(tooltipHtml.join("</br></br>"));

            tooltip.append("br");
            tooltip.append("div")
                .append("svg")
                .attr("height", $scope.legend.length * 20)
                .attr("width", 20)
                .selectAll(".dotlegend")
                .data(d.transformationVisibility)
                .enter()
                .append("g")
                .attr("class", "dotlegend")
                .attr("transform", (vt, i) => "translate(0," + i * 20 + ")")
                .append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", vt => color(vt.name))
                .style("stroke", vt => color(vt.name))
                .style("stroke-width", 4)
                .style("fill-opacity", vt => vt.visible ? 1 : 0)
                .on("click", function (_event, vt) {
                    vt.visible = !vt.visible;
                    d3.select(this).style("fill-opacity", () => vt.visible ? 1 : 0);
                    svg.selectAll(".transform-line")
                        .filter(line => line.startOrderId === d.value && line.value === vt.name)
                        .attr("visibility", () => vt.visible ? "visible" : "hidden");
                });


            tooltip.style("background", "#eee")
                .style("color", "#000")
                .style("border-radius", "5px")
                .style("font-family", "monospace")
                .style("padding", "5px")
                .style("left", (event.pageX + 10) + "px")
                .style("top", (event.pageY - 8) + "px");

            tooltip.hideTooltip = false;
        }

        // clears tooltip and unselects dots
        function clearTooltip(tooltip) {
            if (tooltip) {
                if (tooltip.hideTooltip) {
                    tooltip.html("").style("opacity", 0);

                    if (tooltip.selectedDots) {
                        tooltip.selectedDots.attr("rx", $scope.dotRadius)
                            .attr("ry", $scope.dotRadius);
                    }
                }

                tooltip.hideTooltip = true;
            }
        }

        function xValue(d) {
            return d.x;
        }

        function yValue(d) {
            return d.y;
        }

        function draw() {
            $scope.fillPointsAndLines();

            // removing previous chart and tooltip if any
            d3.select(".tooltip").remove();
            d3.select(".chart-svg").remove();

            // chart size and margin settings
            var margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            var width = $scope.width - margin.left - margin.right;
            var height = $scope.height - margin.top - margin.bottom;

            // setup x
            // calculating margins for dots
            var xMin = d3.min($scope.points, $scope.xValue);
            var xMax = d3.max($scope.points, $scope.xValue);
            var xMargin = (xMax - xMin) * 0.05;

            var xScale = d3.scaleLinear()
                .domain([xMin - xMargin, xMax + xMargin])
                .range([0, width]);
            var xAxis = d3.axisBottom(xScale)
                .ticks($scope.counterIteration)
                .tickSizeInner(-height)
                .tickSizeOuter(0)
                .tickPadding(10);

            $scope.xMap = d => xScale($scope.xValue(d));

            // setup y
            // calculating margins for dots
            var yMax = d3.max($scope.points, $scope.yValue);
            var yMin = d3.min($scope.points, $scope.yValue);
            var yMargin = (yMax - yMin) * 0.05;

            var yAmplitude = yMax - yMin;

            var yScale = d3.scaleLinear()
                .domain([yMin - yMargin, yMax + yMargin])
                .range([height, 0]);
            var yAxis = d3.axisLeft(yScale)
                .ticks(yAmplitude > 20 ? yAmplitude / 10 : yAmplitude)
                .tickSizeInner(-width)
                .tickSizeOuter(0)
                .tickPadding(10);

            $scope.yMap = d => yScale($scope.yValue(d));

            // setup fill color
            var color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);

            // add the graph canvas to the body of the webpage
            var svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height)
                .attr("class", "chart-svg");

            var g = svg.append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add defs for lines ends
            var defs = svg.append("defs");
            for (var i = 0; i < $scope.legend.length; i++) {
                defs.append("marker")
                    .attr("id", "arrow" + i)
                    .attr("viewBox", "0 -5 10 10")
                    .attr("refX", 6)
                    .attr("refY", 0)
                    .attr("markerWidth", 6)
                    .attr("markerHeight", 6)
                    .attr("orient", "auto")
                    .append("path")
                    .attr("d", "M0,-5L10,0L0,5")
                    .attr("stroke", color($scope.legend[i].id))
                    .attr("fill", color($scope.legend[i].id));
            }

            // add the tooltip area to the webpage
            var tooltip = d3.select("#chart").append("div")
                .attr("class", "tooltip")
                .style("opacity", 0);

            // preventing tooltip hiding if dot clicked
            tooltip.on("click", () => { tooltip.hideTooltip = false; });

            // hiding tooltip
            d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });

            // x-axis
            g.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(0," + height + ")")
                .call(xAxis);

            g.append("text")
                .attr("class", "label")
                .attr("transform", "translate(" + (width / 2) + " ," + (height + margin.top - $scope.legendHeight) + ")")
                .style("text-anchor", "middle")
                .text("Iteration")
                .style("font-size", "12pt");

            // y-axis
            g.append("g")
                .attr("class", "y axis")
                .call(yAxis);

            g.append("text")
                .attr("class", "label")
                .attr("transform", "rotate(-90)")
                .attr("y", 0 - margin.left)
                .attr("x", 0 - (height / 2))
                .attr("dy", ".71em")
                .style("text-anchor", "middle")
                .text("Order Id")
                .style("font-size", "12pt");

            // draw lines
            g.selectAll(".transform-line")
                .data($scope.lines)
                .enter()
                .append("line")
                .attr("class", "transform-line")
                .attr("x1", d => xScale(d.x1))
                .attr("y1", d => yScale(d.y1))
                .attr("x2", d => xScale(d.x2))
                .attr("y2", d => yScale(d.y2))
                .attr("marker-end", d => "url(#arrow" + d.arrowType + ")")
                .style("stroke", d => color(d.colorId))
                .style("stroke-width", "2")
                .attr("visibility", "visible");

            // draw legend
            var legend = g.selectAll(".legend")
                .data($scope.legend)
                .enter()
                .append("g")
                .attr("class", "legend")
                .attr("transform", (d, i) => "translate(0," + i * 20 + ")")
                .on("click", function (_event, d) {
                    d.visible = !d.visible;
                    var legendEntry = d3.select(this);

                    legendEntry.select("text")
                        .style("opacity", () => d.visible ? 1 : 0.5);

                    legendEntry.select("rect")
                        .style("fill-opacity", () => d.visible ? 1 : 0);

                    svg.selectAll(".transform-line")
                        .filter(line => line.value === d.name)
                        .attr("visibility", () => d.visible ? "visible" : "hidden");

                    for (var k = 0; k < $scope.points.length; k++) {
                        for (var j = 0; j < $scope.legend.length; j++) {
                            if ($scope.points[k].transformationVisibility[j].name === d.name) {
                                $scope.points[k].transformationVisibility[j].visible = d.visible;
                            }
                        }
                    }
                });

            // draw dots
            g.selectAll(".dot")
                .data($scope.points)
                .enter()
                .append("ellipse")
                .attr("class", "dot")
                .attr("rx", $scope.dotRadius)
                .attr("ry", $scope.dotRadius)
                .attr("cx", $scope.xMap)
                .attr("cy", $scope.yMap)
                .style("fill-opacity", 0.6)
                .style("fill", "black")
                .style("stroke", "black")
                .on("click", (event, d) => $scope.showTooltip(event, d, tooltip, g));

            // draw legend colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", d => color(d.id))
                .style("stroke", d => color(d.id))
                .style("stroke-width", 4)
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            // draw legend text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
                .text(d => d.name)
                .style("font-size", "9pt");
        }

        $scope.draw = draw;
        $scope.fillPointsAndLines = fillPointsAndLines;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.yValue = yValue;
        $scope.xValue = xValue;

        $scope.width = 800;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 2;

        $scope.fillLegend = fillLegend;

        $scope.loadingScreenHeader = "Loading data";

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;

        $http.get(`/api/TaskManagerWebApi/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));

                $scope.ordersIds = [];
                for (var i = 0; i < $scope.orders.length; i++) {
                    $scope.ordersIds.push({ id: i });
                }

                $scope.fillLegend();
                $scope.legendHeight = $scope.legend.length * 20;
                $scope.height = 800 + $scope.legendHeight;
                $scope.initialOrder = $scope.ordersIds[0];
                $scope.transformationType = $scope.transformationsList[0];

                $scope.loading = false;
            }, function () {
                alert("Failed loading characteristic data");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("OrderTransformationVisualizationResultCtrl", ["$scope", "$http", orderTransformationVisualizationResult]);
}
