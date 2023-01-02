function SequencesOrderDistributionResultController() {
    "use strict";

    function sequencesOrderDistributionResult($scope, $http) {

        // initializes data for chart
        function fillPoints() {
            $scope.points = [];
            $scope.maxLevel = 1;

            for (let i = 0; i < $scope.result.length; i++) {
                let order = $scope.result[i].order;
                let sequences = $scope.result[i].sequences;
                if (sequences.length > $scope.maxLevel) {
                    $scope.maxLevel = sequences.length;
                }
                $scope.points.push({
                    id: i,
                    order: order,
                    x: i + 1,
                    y: sequences.length,
                    sequences: sequences
                });
            }
        }

        function fillAccordanceLevels() {
            $scope.accordanceLevels = [];
            for (let i = 0; i <= $scope.maxLevel; i++) {
                let count = 0;
                for (let j = 0; j < $scope.points.length; j++) {
                    if ($scope.points[j].y === i) {
                        count++;
                    }
                }
                if (count !== 0) {
                    $scope.accordanceLevels.push({
                        level: i,
                        distributionsCount: count
                    });
                }
            }
        }


        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            let tooltipContent = [];
            tooltipContent.push("Order: " + d.order);
            tooltipContent.push("Count of sequences: " + d.sequences.length);
            tooltipContent.push("Sequences: ");

            let pointsSequences = [];
            for (let i = 0; i < d.sequences.length && i < 20; i++) {
                pointsSequences.push(d.sequences[i]);
            }

            tooltipContent.push(pointsSequences.join("<br/>"));

            return tooltipContent.join("</br>");
        }

        // shows tooltip for dot or group of dots
        function showTooltip(event, d, tooltip, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            let tooltipHtml = [];

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

            tooltip.style("left", (event.pageX + 10) + "px")
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
            $scope.fillAccordanceLevels();

            // removing previous chart and tooltip if any
            d3.select(".chart-tooltip").remove();
            d3.select(".chart-svg").remove();

            // chart size and margin settings
            let margin = { top: 30, right: 30, bottom: 30, left: 60 };
            let width = $scope.width - margin.left - margin.right;
            let height = $scope.height - margin.top - margin.bottom;

            // setup x
            // calculating margins for dots
            let xMin = d3.min($scope.points, $scope.xValue);
            let xMax = d3.max($scope.points, $scope.xValue);
            let xMargin = (xMax - xMin) * 0.05;

            let xScale = d3.scaleLinear()
                .domain([xMin - xMargin, xMax + xMargin])
                .range([0, width]);
            let xAxis = $scope.points.length > 10 ?
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
            let yMax = d3.max($scope.points, $scope.yValue);
            let yMin = d3.min($scope.points, $scope.yValue);
            let yMargin = (yMax - yMin) * 0.05;

            let yScale = yMax - yMin < 100 ?
                d3.scaleLinear()
                    .domain([yMin - yMargin, yMax + yMargin])
                    .range([height, 0]) :
                d3.scaleLog()
                    .base(10)
                    .domain([1, Math.pow(10, Math.ceil(Math.log10(yMax)))])
                    .range([height, 0]);
            let yAxis = yMax - yMin < 100 ?
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
            let color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.points.length]);

            // add the graph canvas to the body of the webpage
            let svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height)
                .attr("class", "chart-svg")
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add the tooltip area to the webpage
            let tooltip = d3.select("#chart").append("div")
                .attr("class", "chart-tooltip position-absolute text-bg-light font-monospace small lh-sm p-1 rounded")
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

            svg.append("text")
                .attr("class", "label")
                .attr("transform", "translate(" + (width / 2) + " ," + (height + margin.top) + ")")
                .style("text-anchor", "middle")
                .text("Orders")
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
                .text("Count of sequences")
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

        $scope.fillAccordanceLevels = fillAccordanceLevels;
        $scope.accordanceLevels = [];
        $scope.maxLevel = 1;

        $scope.loadingScreenHeader = "Loading Data";
        $scope.loading = true;

        let location = window.location.href.split("/");
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

    angular.module("libiada").controller("SequencesOrderDistributionResultCtrl", ["$scope", "$http", sequencesOrderDistributionResult]);
}
