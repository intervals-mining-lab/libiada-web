function CalculationResultController(data) {
    "use strict";

    function calculationResult($scope) {
        MapModelFromJson($scope, data);

        // initializes data for genes map 
        function fillPoints() {
            var first = +$scope.firstCharacteristic.Value;
            var second = +$scope.secondCharacteristic.Value;

            for (var i = 0; i < $scope.characteristics.length; i++) {
                var characteristic = $scope.characteristics[i];
                $scope.matters.push({ name: characteristic.matterName });
                $scope.points.push({
                    id: i,
                    name: characteristic.matterName,
                    x: characteristic.characteristics[first],
                    y: characteristic.characteristics[second]
                });
            }
        }


        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];
            tooltipContent.push("Name: " + d.name);

            var pointSharacteristics = [];
            for (var i = 0; i < $scope.characteristics[d.id].characteristics.length; i++) {
                pointSharacteristics.push($scope.characteristicsList.Text + ": " + $scope.characteristics[d.id].characteristrics[i])
            }

            tooltipContent.push(pointSharacteristics.join("<br/>"));

            return tooltipContent.join("</br>");
        }

        function showTooltip(d, tooltip, newSelectedDot, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            var tooltipHtml = [];

            tooltip.selectedDots = svg.selectAll(".dot")
                .filter(function(dot) {
                    if (dot.x === d.x && dot.y === d) {
                        tooltipHtml.push($scope.fillPointTooltip(dot));
                        return true;
                    } else {
                        return false;
                    }
                })
                .attr("rx", $scope.selectedDotRadius);

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
                        tooltip.selectedDots.attr("rx", $scope.dotRadius);
                    }
                }

                tooltip.hideTooltip = true;
            }
        }

        function draw() {
            $scope.fillPoints();

            // removing previous chart and tooltip if any
            d3.select(".tooltip").remove();
            d3.select("svg").remove();

            // chart size and margin settings
            var margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            var width = $scope.width - margin.left - margin.right;
            var height = $scope.hight - margin.top - margin.bottom;

            // setup x 
            var xValue = function(d) { return d.x; }; // data -> value
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
            var cValue = function(d) { return d.name; };
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
                .text($scope.firstCharacteristic.Text)
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
                .text($scope.secondCharacteristic.Text)
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
                .on("click", function(d) { return $scope.showTooltip(d, tooltip, d3.select(this), svg); });

            // draw legend
            var legend = svg.selectAll(".legend")
                .data($scope.matters)
                .enter()
                .append("g")
                .attr("class", "legend")
                .attr("transform", function(d, i) { return "translate(0," + i * 20 + ")"; });

            // draw legend colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", function(d) { return color(d.name); })
                .style("stroke", function(d) { return color(d.name); })
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
        }

        $scope.draw = draw;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;

        $scope.legendHeight = $scope.characteristics.length * 20;
        $scope.hight = 800 + $scope.legendHeight;
        $scope.width = 800;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 3;
        $scope.points = [];
        $scope.matters = [];
    }

    angular.module("CalculationResult", []).controller("CalculationResultCtrl", ["$scope", calculationResult]);
}