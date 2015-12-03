function SubsequencesDistributionResultController(data) {
    "use strict";

    function subsequencesDistributionResult($scope) {
        MapModelFromJson($scope, data);

        function prepareDataAndDraw() {
            var points = [];
            var id = 0;
            for (var i = 0; i < $scope.result.length; i++) {
                for (var j = 0; j < $scope.result[i].SubsequencesCharacteristics.length; j++) {
                    points.push({
                        id: id,
                        name: $scope.result[i].MatterName,
                        x: $scope.numericXAxis ? i + 1 : $scope.result[i].Characteristic,
                        y: $scope.result[i].SubsequencesCharacteristics[j].Characteristic
                    });
                    id++;
                }
            }

            $scope.drawScatter(points);
        }

        function drawScatter(points) {

            var margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            var width = 800 - margin.left - margin.right;
            var height = $scope.hight - margin.top - margin.bottom;

            // setup x 
            var xValue = function (d) { return d.x; }; // data -> value
            var xScale = d3.scale.linear().range([0, width]); // value -> display
            var xMap = function (d) { return xScale(xValue(d)); }; // data -> display
            var xAxis = d3.svg.axis().scale(xScale).orient("bottom");

            // setup y

            var yValue = function (d) { return d.y; }; // data -> value
            var yScale = d3.scale.linear().range([height, 0]); // value -> display
            var yMap = function (d) { return yScale(yValue(d)); }; // data -> display
            var yAxis = d3.svg.axis().scale(yScale).orient("left");

            // setup fill color
            var cValue = function (d) { return d.name; };
            var color = d3.scale.category10();

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

            var xMin = d3.min(points, xValue);
            var xMax = d3.max(points, xValue);
            var xMargin = (xMax - xMin) * 0.05;
            var yMax = d3.max(points, yValue);
            var yMin = d3.min(points, yValue);
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
                .text($scope.fullCharacteristicName)
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
                .data(points)
              .enter().append("circle")
                .attr("class", "dot")
                .attr("r", 3.5)
                .attr("cx", xMap)
                .attr("cy", yMap)
                .style("fill", function (d) { return color(cValue(d)); })
                .on("mouseover", function (d) {
                    tooltip.transition()
                         .duration(200)
                         .style("opacity", .9);
                    tooltip.html(d["name"] + "<br/> (" + xValue(d) + ", " + yValue(d) + ")")
                         .style("left", (d3.event.pageX + 5) + "px")
                         .style("top", (d3.event.pageY - 28) + "px");
                })
                .on("mouseout", function (d) {
                    tooltip.transition()
                         .duration(500)
                         .style("opacity", 0);
                });

            // draw legend
            var legend = svg.selectAll(".legend")
                .data(color.domain())
                .enter().append("g")
                .attr("class", "legend")
                .attr("transform", function (d, i) { return "translate(0," + i * 20 + ")"; });

            // draw legend colored rectangles
            legend.append("rect")
                .attr("x", width - 18)
                .attr("width", 18)
                .attr("height", 18)
                .style("fill", color)
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            // draw legend text
            legend.append("text")
                .attr("x", width - 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
                .style("text-anchor", "end")
                .text(function (d) { return d; })
                .style("font-size", "9pt");
        }

        $scope.drawScatter = drawScatter;
        $scope.prepareDataAndDraw = prepareDataAndDraw;
        $scope.legendHeight = $scope.result.length * 20;
        $scope.hight = 800 + $scope.legendHeight;
        $scope.width = 800;
    }

    angular.module("SubsequencesDistributionResult", []).controller("SubsequencesDistributionResultCtrl", ["$scope", subsequencesDistributionResult]);
}