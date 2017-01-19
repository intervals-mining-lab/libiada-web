function LocalCalculationResultController() {
    "use strict";

    function localCalculationResult($scope, $http) {
        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];
        $scope.loading = true;
        $http({
            url: "/api/TaskManagerWebApi/" + $scope.taskId,
            method: "GET"
        }).success(function (data) {
            MapModelFromJson($scope, JSON.parse(data));

            $scope.fillLegend();

            $scope.firstCharacteristic = $scope.characteristicsList[0];
            $scope.secondCharacteristic = $scope.characteristicsList.length > 1 ? $scope.characteristicsList[1] : $scope.characteristicsList[0];

            $scope.legendHeight = $scope.legend.length * 20;
            $scope.hight = 800 + $scope.legendHeight;

            $scope.loading = false;
        }).error(function (data) {
            alert("Failed loading local characteristics data");
        });

        function fillLegend() {
            $scope.legend = [];

            for (var k = 0; k < $scope.characteristics.length; k++) {
                $scope.legend.push({ name: $scope.characteristics[k].matterName, visible: true });
            }

        }

        // initializes data for chart
        function fillPoints() {
            $scope.points = [];
            var first = +$scope.firstCharacteristic.Value;
            var second = +$scope.secondCharacteristic.Value;

            for (var i = 0; i < $scope.characteristics.length; i++) {
                var characteristic = $scope.characteristics[i];
                for (var j = 0; j < characteristic.fragmentsData.length; j++) {
                    var fragmentData = characteristic.fragmentsData[j];
                    $scope.points.push({
                        id: j,
                        characteristicId: i,
                        name: fragmentData.Name,
                        x: fragmentData.Characteristics[first],
                        y: fragmentData.Characteristics[second],
                        cluster: characteristic.matterName
                    });
                }

            }
        }


        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];
            tooltipContent.push(d.cluster);
            tooltipContent.push("Name: " + d.name);
            tooltipContent.push("Fragment №: " + d.id);
            var pointSharacteristics = [];
            var characteristics = $scope.characteristics[d.characteristicId].fragmentsData[d.id].Characteristics;
            for (var i = 0; i < characteristics.length; i++) {
                pointSharacteristics.push($scope.characteristicsList[i].Text + ": " + characteristics[i]);
            }

            tooltipContent.push(pointSharacteristics.join("<br/>"));

            return tooltipContent.join("</br>");
        }

        function showTooltip(d, tooltip, newSelectedDot, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            var tooltipHtml = [];

            tooltip.selectedDots = svg.selectAll(".dot")
                .filter(function (dot) {
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

            tooltip.style("background", "#000")
                .style("color", "#fff")
                .style("border-radius", "5px")
                .style("font-family", "monospace")
                .style("padding", "5px")
                .style("left", (d3.event.pageX + 10) + "px")
                .style("top", (d3.event.pageY - 8) + "px");

            tooltip.hideTooltip = false;
        }

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

        function draw() {
            $scope.fillPoints();

            // removing previous chart and tooltip if any
            d3.select(".tooltip").remove();
            d3.select(".chart-svg").remove();

            // chart size and margin settings
            var margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            var width = $scope.width - margin.left - margin.right;
            var height = $scope.hight - margin.top - margin.bottom;

            // setup x
            var xValue = function (d) { return $scope.lineChart ? d.id : d.x; }; // data -> value
            var xScale = d3.scale.linear().range([0, width]); // value -> display
            var xMap = function (d) { return xScale(xValue(d)); }; // data -> display
            var xAxis = d3.svg.axis().scale(xScale).orient("bottom");
            xAxis.innerTickSize(-height).outerTickSize(0).tickPadding(10);

            // setup y
            var yValue = function (d) { return $scope.lineChart ? d.x : d.y; }; // data -> value
            var yScale = d3.scale.linear().range([height, 0]); // value -> display
            var yMap = function (d) { return yScale(yValue(d)); }; // data -> display
            var yAxis = d3.svg.axis().scale(yScale).orient("left");
            yAxis.innerTickSize(-width).outerTickSize(0).tickPadding(10);

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

            // setup fill color
            var cValue = function (d) { return d.cluster; };
            var color = d3.scale.category20();
            var elementColor = function(d) { return color(cValue(d)); };

            // add the graph canvas to the body of the webpage
            var svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.hight)
                .attr("class", "chart-svg")
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add the tooltip area to the webpage
            var tooltip = d3.select("#chart").append("div")
                .attr("class", "tooltip")
                .style("opacity", 0);

            // preventing tooltip hiding if dot clicked
            tooltip.on("click", function () { tooltip.hideTooltip = false; });

            // hiding tooltip
            d3.select("#chart").on("click", function () { $scope.clearTooltip(tooltip); });

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
                .text($scope.lineChart ? "Fragment №" : $scope.firstCharacteristic.Text)
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
                .text($scope.lineChart ? $scope.firstCharacteristic.Text : $scope.secondCharacteristic.Text)
                .style("font-size", "12pt");

            if ($scope.lineChart) {
                var line = d3.svg.line()
                    .x(xMap)
                    .y(yMap);

                // Nest the entries by symbol
                var dataNest = d3.nest()
                    .key(function(d) { return d.cluster })
                    .entries($scope.points);

                // Loop through each symbol / key
                dataNest.forEach(function(d) {
                    svg.append("path")
                        .datum(d.values)
                        .attr("class", "line")
                        .attr("d", line)
                        .attr('stroke', function (d) { return color(cValue(d[0])); })
                        .attr('stroke-width', 1)
                        .attr('fill', 'none');
                });

            }
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
                    .style("opacity", $scope.lineChart ? 0 : 1)
                    .style("fill", elementColor)
                    .style("stroke", elementColor)
                    .on("click", function(d) { return $scope.showTooltip(d, tooltip, d3.select(this), svg); });

            // draw legend
            var legend = svg.selectAll(".legend")
                .data($scope.legend)
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

                    svg.selectAll(".dot")
                        .filter(function (dot) { return dot.cluster === d.name; })
                        .attr("visibility", function (dot) {
                            return d.visible ? "visible" : "hidden";
                        });

                    svg.selectAll(".line")
                        .filter(function (line) { return line[0].cluster === d.name; })
                        .attr("visibility", function (line) {
                            return d.visible ? "visible" : "hidden";
                        });
                });;

            // draw legend colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", function (d) { return color(d.name); })
                .style("stroke", function (d) { return color(d.name); })
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

        $scope.draw = draw;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.fillLegend = fillLegend;

        $scope.width = 800;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 2;
    }

    angular.module("LocalCalculationResult", []).controller("LocalCalculationResultCtrl", ["$scope", "$http", localCalculationResult]);
}