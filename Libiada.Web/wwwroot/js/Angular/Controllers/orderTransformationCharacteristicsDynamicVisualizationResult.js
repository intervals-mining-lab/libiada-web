/// <reference types="angular" />
/// <reference types="d3" />
/// <reference path="./Interfaces/commonInterfaces.d.ts" />
/**
 * Controller for order transformation characteristics dynamic visualization
 */
class OrderTransformationCharacteristicsDynamicVisualizationResultHandler {
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
        const orderTransformationCharacteristicsDynamicVisualizationResult = ($scope, $http) => {
            /**
             * Initializes legend data
             */
            function fillLegend() {
                $scope.legend = [];
                for (let k = 0; k < $scope.characteristics.length; k++) {
                    $scope.legend.push({ id: k, name: $scope.characteristics[k].researchObjectName, visible: true });
                }
            }
            /**
             * Initializes data for chart
             */
            function fillPoints() {
                $scope.points = [];
                for (let i = 0; i < $scope.characteristics.length; i++) {
                    let characteristic = $scope.characteristics[i].characteristics;
                    for (let j = 0; j < characteristic.length; j++) {
                        $scope.points.push({
                            id: i,
                            name: $scope.characteristics[i].researchObjectName,
                            x: j,
                            y: characteristic[j]
                        });
                    }
                }
            }
            /**
             * Constructs string representing tooltip text (inner html)
             * @param d The point data to display in tooltip
             */
            function fillPointTooltip(d) {
                let tooltipContent = [];
                tooltipContent.push(`Name: ${d.name}`);
                tooltipContent.push(`${$scope.characteristicName}: ${$scope.characteristics[d.id].characteristics[d.x]}`);
                tooltipContent.push($scope.transformationsList[d.x % $scope.transformationsList.length]);
                return tooltipContent.join("</br>");
            }
            /**
             * Shows tooltip for dot or group of dots
             * @param event Mouse event that triggered the tooltip
             * @param d Point data to show in tooltip
             * @param tooltip The tooltip element
             * @param svg The SVG container element
             */
            function showTooltip(event, d, tooltip, svg) {
                $scope.clearTooltip(tooltip);
                tooltip.style("opacity", 0.9);
                let tooltipHtml = [];
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
             * @param tooltip The tooltip element to clear
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
             * Returns x value for a data point
             */
            function xValue(d) {
                return d.x;
            }
            /**
             * Returns y value for a data point
             */
            function yValue(d) {
                return d.y;
            }
            /**
             * Creates and draws the chart visualization
             */
            function draw() {
                $scope.fillPoints();
                // Remove previous chart and tooltip if present
                d3.select(".chart-tooltip").remove();
                d3.select(".chart-svg").remove();
                // Chart size and margin settings
                let margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
                let width = $scope.width - margin.left - margin.right;
                let height = $scope.height - margin.top - margin.bottom;
                // Setup x-axis
                // Calculate margins for points
                let xMin = d3.min($scope.points, $scope.xValue) || 0;
                let xMax = d3.max($scope.points, $scope.xValue) || 0;
                let xMargin = (xMax - xMin) * 0.05;
                let xScale = d3.scaleLinear()
                    .domain([xMin - xMargin, xMax + xMargin])
                    .range([0, width]);
                let xAxis = d3.axisBottom(xScale)
                    .tickSizeInner(-height)
                    .tickSizeOuter(0)
                    .tickPadding(10);
                $scope.xMap = (d) => xScale($scope.xValue(d));
                // Setup y-axis
                // Calculate margins for points
                let yMax = d3.max($scope.points, $scope.yValue) || 0;
                let yMin = d3.min($scope.points, $scope.yValue) || 0;
                let yMargin = (yMax - yMin) * 0.05;
                let yScale = d3.scaleLinear()
                    .domain([yMin - yMargin, yMax + yMargin])
                    .range([height, 0]);
                let yAxis = d3.axisLeft(yScale)
                    .tickSizeInner(-width)
                    .tickSizeOuter(0)
                    .tickPadding(10);
                $scope.yMap = (d) => yScale($scope.yValue(d));
                // Setup fill color
                let color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);
                // Add the chart canvas to the page
                let svg = d3.select("#chart").append("svg")
                    .attr("width", $scope.width)
                    .attr("height", $scope.height)
                    .attr("class", "chart-svg")
                    .append("g")
                    .attr("transform", `translate(${margin.left},${margin.top})`);
                // Add the tooltip area to the page
                let tooltip = d3.select("#chart").append("div")
                    .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
                    .style("opacity", 0);
                // Prevent tooltip from hiding when clicked
                tooltip.on("click", () => { tooltip.hideTooltip = false; });
                // Hide tooltip when clicking elsewhere
                d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });
                // Draw x-axis
                svg.append("g")
                    .attr("class", "x axis")
                    .attr("transform", `translate(0,${height})`)
                    .call(xAxis);
                svg.append("text")
                    .attr("class", "label")
                    .attr("transform", `translate(${width / 2} ,${height + margin.top - $scope.legendHeight})`)
                    .style("text-anchor", "middle")
                    .text("Transformation number")
                    .style("font-size", "12pt");
                // Draw y-axis
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
                    .text($scope.characteristicName)
                    .style("font-size", "12pt");
                // Draw data points
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
                // Draw legend
                let legend = svg.selectAll(".legend")
                    .data($scope.legend)
                    .enter()
                    .append("g")
                    .attr("class", "legend")
                    .attr("transform", (_d, i) => `translate(0,${i * 20})`)
                    .on("click", function (event, d) {
                    d.visible = !d.visible;
                    let legendEntry = d3.select(event.currentTarget);
                    legendEntry.select("text")
                        .style("opacity", () => d.visible ? 1 : 0.5);
                    legendEntry.select("rect")
                        .style("fill-opacity", () => d.visible ? 1 : 0);
                    svg.selectAll(".dot")
                        .filter((dot) => dot.name === d.name)
                        .attr("visibility", () => d.visible ? "visible" : "hidden");
                });
                // Draw legend colored rectangles
                legend.append("rect")
                    .attr("width", 15)
                    .attr("height", 15)
                    .style("fill", (d) => color(d.id))
                    .style("stroke", (d) => color(d.id))
                    .style("stroke-width", 4)
                    .attr("transform", `translate(0, -${$scope.legendHeight})`);
                // Draw legend text
                legend.append("text")
                    .attr("x", 24)
                    .attr("y", 9)
                    .attr("dy", ".35em")
                    .attr("transform", `translate(0, -${$scope.legendHeight})`)
                    .text((d) => d.name)
                    .style("font-size", "9pt");
            }
            // Register functions in $scope
            $scope.draw = draw;
            $scope.fillPoints = fillPoints;
            $scope.fillPointTooltip = fillPointTooltip;
            $scope.showTooltip = showTooltip;
            $scope.clearTooltip = clearTooltip;
            $scope.fillLegend = fillLegend;
            $scope.yValue = yValue;
            $scope.xValue = xValue;
            // Initialize $scope properties
            $scope.width = 800;
            $scope.dotRadius = 4;
            $scope.selectedDotRadius = $scope.dotRadius * 2;
            $scope.loadingScreenHeader = "Loading data";
            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            $scope.loading = true;
            // Load data from server
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.fillLegend();
                $scope.legendHeight = $scope.legend.length * 20;
                $scope.height = 800 + $scope.legendHeight;
                $scope.loading = false;
            })
                .catch(function () {
                alert("Failed loading characteristic data");
                $scope.loading = false;
            });
        };
        // Register controller in Angular module
        angular.module("libiada").controller("OrderTransformationCharacteristicsDynamicVisualizationResultCtrl", ["$scope", "$http", orderTransformationCharacteristicsDynamicVisualizationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns OrderTransformationCharacteristicsDynamicVisualizationResultHandler instance
 */
function OrderTransformationCharacteristicsDynamicVisualizationResultController() {
    return new OrderTransformationCharacteristicsDynamicVisualizationResultHandler();
}
//# sourceMappingURL=orderTransformationCharacteristicsDynamicVisualizationResult.js.map