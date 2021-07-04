function IntervalsCharacteristicsDistributionResultController() {
    "use strict";

    function intervalsCharacteristicsDistributionResult($scope, $http) {

        // initializes data for chart
        function fillPoints() {
            $scope.points = [];
            var index = 0;
            var chT = $scope.characteristic.Text.split("  ");
            var ch = +$scope.characteristic.Value;
            for (var i = 0; i < $scope.result.length; i++) {
                if ($scope.result[i].link === chT[chT.length - 1]) {
                    for (var j = 0; j < $scope.result[i].accordance.length; j++) {
                        var distributionIntervals = $scope.result[i].accordance[j].distributionIntervals;
                        var orders = $scope.result[i].accordance[j].orders;
                        for (var k = 0; k < orders.length; k++) {
                            $scope.points.push({
                                id: index++,
                                distributionIntervals: distributionIntervals,
                                x: j + 1,
                                y: orders[k].characteristics.Characteristics[ch],
                                order: orders[k].order
                            });
                        }
                    }
                }
            }
        }


        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];


            var pointsOrder = [];
            pointsOrder.push(d.order);

            tooltipContent.push(pointsOrder.join("<br/>"));

            return tooltipContent.join("</br>");
        }

        // shows tooltip for dot or group of dots
        function showTooltip(event, d, tooltip, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            var tooltipHtml = [];
            tooltipHtml.push("Characterisctic value: ");
            tooltipHtml.push(d.y);
            tooltipHtml.push("Distribution intervals: ");
            var intervals = [];
            //var pointsIntervals = [];
            //for (var i = 0; i < d.distributionIntervals.length; i++) {
            //    pointsIntervals.push(d.distributionIntervals[i].interval)
            //}
            //tooltipContent.push(pointsIntervals.join("|"));
            //var pointsCounts = [];
            //for (var i = 0; i < d.distributionIntervals.length; i++) {
            //    pointsCounts.push(d.distributionIntervals[i].count)
            //}
            //tooltipContent.push(pointsCounts.join("|"));
            for (var i = 0; i < d.distributionIntervals.length; i++) {
                intervals.push([d.distributionIntervals[i].interval, d.distributionIntervals[i].count]);
                tooltipHtml.push(intervals[i].join("|"));
            }
            tooltipHtml.push("Orders: ");


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
            $scope.fillPoints();

            // removing previous chart and tooltip if any
            d3.select(".tooltip").remove();
            d3.select(".chart-svg").remove();

            // chart size and margin settings
            var margin = { top: 30, right: 30, bottom: 30, left: 60 };
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
            var xAxis = $scope.points.length > 10 ?
                d3.axisBottom(xScale)
                    .tickSizeInner(-height)
                    .tickSizeOuter(0)
                    .tickPadding(10) :
                d3.axisBottom(xScale)
                    .ticks($scope.points.length)
                    .tickSizeInner(-height)
                    .tickSizeOuter(0)
                    .tickPadding(10);

            $scope.xMap = d => xScale($scope.xValue(d));

            // setup y
            // calculating margins for dots
            var yMax = d3.max($scope.points, $scope.yValue);
            var yMin = d3.min($scope.points, $scope.yValue);
            var yMargin = (yMax - yMin) * 0.05;

            var yScale = yMax - yMin < 100 ?
                d3.scaleLinear()
                    .domain([yMin - yMargin, yMax + yMargin])
                    .range([height, 0]) :
                d3.scaleLog()
                    .base(10)
                    .domain([1, Math.pow(10, Math.ceil(Math.log10(yMax)))])
                    .range([height, 0]);
            var yAxis = yMax - yMin < 100 ?
                d3.axisLeft(yScale)
                    .tickSizeInner(-width)
                    .tickSizeOuter(0)
                    .tickPadding(10) :
                d3.axisLeft(yScale)
                    .tickFormat(d3.format(""))
                    .tickSizeInner(-width)
                    .tickSizeOuter(0)
                    .tickPadding(10);

            $scope.yMap = d => yScale($scope.yValue(d));

            // setup fill color
            var color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.points.length]);

            // add the graph canvas to the body of the webpage
            var svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height)
                .attr("class", "chart-svg")
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add the tooltip area to the webpage
            var tooltip = d3.select("#chart").append("div")
                .attr("class", "tooltip")
                .style("opacity", 0);

            // preventing tooltip hiding if dot clicked
            tooltip.on("click", () => { tooltip.hideTooltip = false; });

            // hiding tooltip
            d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });

            // x-axis
            svg.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(0," + height + ")")
                .call(xAxis);

            var chTX = $scope.characteristic.Text.split("  ");

            svg.append("text")
                .attr("class", "label")
                .attr("transform", "translate(" + (width / 2) + " ," + (height + margin.top) + ")")
                .style("text-anchor", "middle")
                .text("Intervals distributon link " + chTX[chTX.length - 1])
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
                .text($scope.characteristic.Text)
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
                .style("fill", d => color(d.id))
                .style("stroke", d => color(d.id))
                .on("click", (event, d) => $scope.showTooltip(event, d, tooltip, svg));
        }

        $scope.draw = draw;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.yValue = yValue;
        $scope.xValue = xValue;

        $scope.width = 800;
        $scope.height = 600;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 2;

        $scope.loadingScreenHeader = "Loading Data";
        $scope.loading = true;

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http.get(`/api/TaskManagerWebApi/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));
                $scope.loading = false;

            }, function () {
                alert("Failed loading sequences order distribution data");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("IntervalsCharacteristicsDistributionResultCtrl", ["$scope", "$http", intervalsCharacteristicsDistributionResult]);

}
