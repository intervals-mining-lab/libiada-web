/// <reference types="angular" />
/// <reference types="d3" />
/// <reference path="./Interfaces/commonInterfaces.d.ts" />
/**
 * Controller for sequences order distribution results
 */
class SequencesOrderDistributionResultHandler {
    /**
     * Creates a new instance of the controller
     */
    constructor() {
        this.initializeController();
    }
    /**
     * Initializes the Angular controller
     */
    initializeController() {
        "use strict";
        const sequencesOrderDistributionResult = ($scope, $http) => {
            /**
             * Initializes data for chart
             */
            function fillPoints() {
                $scope.points = [];
                $scope.maxLevel = 1;
                for (let i = 0; i < $scope.result.length; i++) {
                    const order = $scope.result[i].order;
                    const sequences = $scope.result[i].sequences;
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
            /**
             * Initializes data for accordance levels
             */
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
            /**
             * Constructs string representing tooltip text (inner html)
             * @param d Point data to include in tooltip
             */
            function fillPointTooltip(d) {
                const tooltipContent = [];
                tooltipContent.push(`Order: ${d.order}`);
                tooltipContent.push(`Count of sequences: ${d.sequences.length}`);
                tooltipContent.push("Sequences: ");
                const pointsSequences = [];
                for (let i = 0; i < d.sequences.length && i < 20; i++) {
                    pointsSequences.push(d.sequences[i]);
                }
                tooltipContent.push(pointsSequences.join("<br/>"));
                return tooltipContent.join("</br>");
            }
            /**
             * Shows tooltip for dot or group of dots
             * @param event Mouse event
             * @param d Point data
             * @param tooltip Tooltip element
             * @param svg SVG element
             */
            function showTooltip(event, d, tooltip, svg) {
                $scope.clearTooltip(tooltip);
                tooltip.style("opacity", 0.9);
                const tooltipHtml = [];
                tooltip.selectedDots = svg.selectAll(".dot")
                    .filter((dot) => {
                    if (dot.x === d.x && dot.y === d.y) {
                        tooltipHtml.push($scope.fillPointTooltip(dot));
                        return true;
                    }
                    else {
                        return false;
                    }
                })
                    .attr("rx", $scope.selectedDotRadius)
                    .attr("ry", $scope.selectedDotRadius);
                tooltip.html(tooltipHtml.join("</br></br>"));
                tooltip.style("left", `${event.pageX + 10}px`)
                    .style("top", `${event.pageY - 8}px`);
                tooltip.hideTooltip = false;
            }
            /**
             * Clears tooltip and unselects dots
             * @param tooltip Tooltip element
             */
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
            /**
             * Returns x value for a point
             * @param d Point data
             */
            function xValue(d) {
                return d.x;
            }
            /**
             * Returns y value for a point
             * @param d Point data
             */
            function yValue(d) {
                return d.y;
            }
            /**
             * Draws the chart
             */
            function draw() {
                $scope.fillPoints();
                $scope.fillAccordanceLevels();
                // removing previous chart and tooltip if any
                d3.select(".chart-tooltip").remove();
                d3.select(".chart-svg").remove();
                // chart size and margin settings
                const margin = { top: 30, right: 30, bottom: 30, left: 60 };
                const width = $scope.width - margin.left - margin.right;
                const height = $scope.height - margin.top - margin.bottom;
                // setup x
                // calculating margins for dots
                const xMin = d3.min($scope.points, $scope.xValue) || 0;
                const xMax = d3.max($scope.points, $scope.xValue) || 0;
                const xMargin = (xMax - xMin) * 0.05;
                const xScale = d3.scaleLinear()
                    .domain([xMin - xMargin, xMax + xMargin])
                    .range([0, width]);
                const xAxis = $scope.points.length > 10 ?
                    d3.axisBottom(xScale)
                        .tickSizeInner(-height)
                        .tickSizeOuter(0)
                        .tickPadding(10) :
                    d3.axisBottom(xScale)
                        .ticks($scope.points.length)
                        .tickSizeInner(-height)
                        .tickSizeOuter(0)
                        .tickPadding(10);
                $scope.xMap = (d) => xScale($scope.xValue(d));
                // setup y
                // calculating margins for dots
                const yMax = d3.max($scope.points, $scope.yValue) || 0;
                const yMin = d3.min($scope.points, $scope.yValue) || 0;
                const yMargin = (yMax - yMin) * 0.05;
                const yScale = yMax - yMin < 100 ?
                    d3.scaleLinear()
                        .domain([yMin - yMargin, yMax + yMargin])
                        .range([height, 0]) :
                    d3.scaleLog()
                        .base(10)
                        .domain([1, Math.pow(10, Math.ceil(Math.log10(yMax)))])
                        .range([height, 0]);
                const yAxis = yMax - yMin < 100 ?
                    d3.axisLeft(yScale)
                        .tickSizeInner(-width)
                        .tickSizeOuter(0)
                        .tickPadding(10) :
                    d3.axisLeft(yScale)
                        .tickFormat(d3.format(""))
                        .tickSizeInner(-width)
                        .tickSizeOuter(0)
                        .tickPadding(10);
                $scope.yMap = (d) => yScale($scope.yValue(d));
                // setup fill color
                const color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.points.length]);
                // add the graph canvas to the body of the webpage
                const svg = d3.select("#chart").append("svg")
                    .attr("width", $scope.width)
                    .attr("height", $scope.height)
                    .attr("class", "chart-svg")
                    .append("g")
                    .attr("transform", `translate(${margin.left},${margin.top})`);
                // add the tooltip area to the webpage
                const tooltip = d3.select("#chart").append("div")
                    .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
                    .style("opacity", 0);
                // preventing tooltip hiding if dot clicked
                tooltip.on("click", () => { tooltip.hideTooltip = false; });
                // hiding tooltip
                d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });
                // x-axis
                svg.append("g")
                    .attr("class", "x axis")
                    .attr("transform", `translate(0,${height})`)
                    .call(xAxis);
                svg.append("text")
                    .attr("class", "label")
                    .attr("transform", `translate(${width / 2} ,${height + margin.top})`)
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
                    .style("fill", (d) => color(d.id))
                    .style("stroke", (d) => color(d.id))
                    .on("click", (event, d) => $scope.showTooltip(event, d, tooltip, svg));
            }
            // Assign methods to $scope
            $scope.draw = draw;
            $scope.fillPoints = fillPoints;
            $scope.fillPointTooltip = fillPointTooltip;
            $scope.showTooltip = showTooltip;
            $scope.clearTooltip = clearTooltip;
            $scope.yValue = yValue;
            $scope.xValue = xValue;
            $scope.fillAccordanceLevels = fillAccordanceLevels;
            // Initialize chart settings
            $scope.width = 800;
            $scope.height = 600;
            $scope.dotRadius = 4;
            $scope.selectedDotRadius = $scope.dotRadius * 2;
            // Initialize accordance levels
            $scope.accordanceLevels = [];
            $scope.maxLevel = 1;
            // Initialize loading screen
            $scope.loadingScreenHeader = "Loading Data";
            $scope.loading = true;
            // Get task ID from URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            // Fetch data from API
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.loading = false;
            })
                .catch(function () {
                alert("Failed loading sequences order distribution data");
                $scope.loading = false;
            });
        };
        // Register the controller in Angular
        angular.module("libiada").controller("SequencesOrderDistributionResultCtrl", ["$scope", "$http", sequencesOrderDistributionResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns SequencesOrderDistributionResultHandler instance
 */
function SequencesOrderDistributionResultController() {
    return new SequencesOrderDistributionResultHandler();
}
//# sourceMappingURL=sequencesOrderDistributionResult.js.map