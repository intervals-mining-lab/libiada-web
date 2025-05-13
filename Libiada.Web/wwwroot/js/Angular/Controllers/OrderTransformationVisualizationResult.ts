/// <reference types="angular" />
/// <reference types="d3" />
/// <reference types="functions" />
/// <reference path="./Interfaces/commonInterfaces.d.ts" />

type IScope = ng.IScope;
interface IPoint extends IBasePoint {
    value: number; // Value (order identifier)
    transformationVisibility: ITransformationVisibility[];
}

// Interface for the line between points
interface ILine {
    id: number;
    value: string;
    arrowType: number;
    iterator: number;
    x1: number;
    y1: number;
    x2: number;
    y2: number;
    startOrderId: number;
    colorId: number;
}

// Interface for the legend item
interface ILegendItem {
    id: number;
    name: string;
    visible: boolean;
}

//Interface for the visibility of the transformation
interface ITransformationVisibility {
    id: number;
    name: string;
    visible: boolean;
}

// Interface for the order ID
interface IOrderId {
    id: number;
}

// Interface for the transformation type
interface ITransformationType {
    Text: string;
    Value?: number;
}

// Interface for the transformation data
interface ITransformationData {
    ResultTransformation: IResultTransformation[];
}

// Interface for the transformation result
interface IResultTransformation {
    Transformation: string;
    OrderId: number;
}

// Interface for controller data
interface IOrderTransformationVisualizationData {
    orders: string[];
    transformationsData: { [key: number]: ITransformationData };
    transformationsList: ITransformationType[];
    //[key: string]: any;
}

// Interface for controller scope
interface IOrderTransformationVisualizationScope extends ng.IScope {
    // Data from server
    orders: string[];
    transformationsData: { [key: number]: ITransformationData };
    transformationsList: ITransformationType[];

    // Chart settings
    width: number;
    height: number;
    dotRadius: number;
    selectedDotRadius: number;
    legendHeight: number;

    // Chart elements
    points: IPoint[];
    lines: ILine[];
    legend: ILegendItem[];
    ordersIds: IOrderId[];

    // Current settings
    initialOrder: IOrderId;
    transformationType: ITransformationType;
    counterIteration: number;

    // Properties for loading data
    taskId: string;
    loadingScreenHeader: string;
    loading: boolean;

    // Chart functions
    xMap: (d: IPoint) => number;
    yMap: (d: IPoint) => number;

    // Methods
    draw: () => void;
    fillPointsAndLines: () => void;
    fillLegend: () => void;
    fillPointTooltip: (d: IPoint) => string;
    showTooltip: (event: MouseEvent, d: IPoint, tooltip: any, svg: any) => void;
    clearTooltip: (tooltip: any) => void;
    xValue: (d: IPoint) => number;
    yValue: (d: IPoint) => number;
}

// Interface definition for tooltip with additional properties
interface ID3Tooltip extends d3.Selection<HTMLDivElement, unknown, HTMLElement, any> {
    hideTooltip?: boolean;
    selectedDots?: any;
}

// Methods in IOrderTransformationVisualizationScope interface

/**
* Controller for visualizing order transformation results
*/
class OrderTransformationVisualizationResultHandler {
    constructor() {
        this.initializeController();
    }

    private initializeController(): void {
        "use strict";

        const orderTransformationVisualizationResult = ($scope: IOrderTransformationVisualizationScope, $http: ng.IHttpService): void => {

            /**
            * Initializes the data for the legend
            */
            function fillLegend(): void {
                $scope.legend = [];
                for (let i = 1; i < $scope.transformationsList.length; i++) {
                    $scope.legend.push({ id: i - 1, name: $scope.transformationsList[i].Text, visible: true });
                }
            }

            /**
            * Initializes the data for the chart points
            */
            function fillPoints(): void {
                let initialOrder = $scope.initialOrder.id;
                let checkedOrders = [initialOrder];
                let ordersForChecking = [initialOrder];
                let transformationVisibility: ITransformationVisibility[] = [];

                for (let l = 0; l < $scope.legend.length; l++) {
                    transformationVisibility.push({
                        id: l,
                        name: $scope.legend[l].name,
                        visible: $scope.legend[l].visible
                    });
                }

                $scope.points.push({
                    id: 0,
                    value: initialOrder,
                    x: 0,
                    y: initialOrder,
                    transformationVisibility: transformationVisibility,
                    colorId: 0
                });

                let counterIdPoints = 1;
                $scope.counterIteration = 1;

                while (ordersForChecking.length > 0) {
                    let newOrdersForChecking: number[] = [];

                    for (let i = 0; i < ordersForChecking.length; i++) {
                        let resultTransformations = $scope.transformationsData[ordersForChecking[i]].ResultTransformation;

                        for (let j = 0; j < resultTransformations.length; j++) {
                            let resultTransformation = resultTransformations[j];

                            if ($scope.transformationType.Text === "All" || resultTransformation.Transformation === $scope.transformationType.Text) {
                                let pointExist = false;
                                let orderId = resultTransformation.OrderId;

                                for (let k = 0; k < $scope.points.length; k++) {
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
                                        transformationVisibility: transformationVisibility,
                                        colorId: 0
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

            /** 
            * Initializes data for graph lines 
            */
            function fillLines(): void {
                let counterIdLines = 0;

                for (let i = 0; i < $scope.points.length; i++) {
                    let resultTransformation = $scope.transformationsData[$scope.points[i].value].ResultTransformation;

                    for (let j = 0; j < resultTransformation.length; j++) {
                        let transformationType = resultTransformation[j].Transformation;

                        if ($scope.transformationType.Text === "All" || transformationType === $scope.transformationType.Text) {
                            let colorId = $scope.legend.find(d => d.name === transformationType).id;
                            let childOrder = resultTransformation[j].OrderId;

                            let line = {
                                x1: $scope.points[i].x,
                                y1: $scope.points[i].y,
                                x2: $scope.points[i].x + 1,
                                y2: childOrder,
                                value: transformationType,
                                startOrderId: $scope.points[i].value,
                                colorId: colorId
                            };

                            let orderExist = false;

                            for (let k = 0; k < $scope.points.length; k++) {
                                if ($scope.points[k].x === $scope.points[i].x + 1 &&
                                    $scope.points[k].y === childOrder) {
                                    orderExist = true;
                                    break;
                                }
                            }

                            if (orderExist) {
                                let lineExist = false;
                                let lineIterator = 0;

                                for (let m = 0; m < $scope.lines.length; m++) {
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
                                    let cyline = (line.y1 + line.y2) / 2.0;
                                    let cxline = (line.x1 + line.x2) / 2.0;
                                    let yAmplitude = line.y2 - line.y1;
                                    let shifty = 0.2 * lineIterator;
                                    let shiftx = 0.05 * lineIterator * Math.abs(yAmplitude) / $scope.ordersIds.length;

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

            /**
            * Initializes data for graph points and lines
            */
            function fillPointsAndLines(): void {
                $scope.points = [];
                $scope.lines = [];
                fillPoints();
                fillLines();
            }

            /**
            * Generates tooltip text
            * @param d Point for which tooltip is generated
            */
            function fillPointTooltip(d: IPoint): string {
                let tooltipContent: string[] = [];
                tooltipContent.push(`Order ID: ${d.value}`);
                tooltipContent.push(`Order: ${$scope.orders[d.value]}`);
                return tooltipContent.join("</br>");
            }

            /**
            * Shows a tooltip when a point is clicked
            * @param event Mouse event
            * @param d Data point
            * @param tooltip Tooltip element
            * @param svg SVG element
            */
            function showTooltip(event: MouseEvent, d: IPoint, tooltip: ID3Tooltip, svg: any): void {
                $scope.clearTooltip(tooltip);
                let color = d3.scaleOrdinal(d3.schemeCategory10);

                tooltip.style("opacity", 0.9);

                let tooltipHtml: string[] = [];

                tooltip.selectedDots = svg.selectAll(".dot")
                    .filter((dot: IPoint) => {
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
                    .attr("transform", (vt: ITransformationVisibility, i: number) => `translate(0,${i * 20})`)
                    .append("rect")
                    .attr("width", 15)
                    .attr("height", 15)
                    .style("fill", (vt: ITransformationVisibility) => color(vt.name))
                    .style("stroke", (vt: ITransformationVisibility) => color(vt.name))
                    .style("stroke-width", 4)
                    .style("fill-opacity", (vt: ITransformationVisibility) => vt.visible ? 1 : 0)
                    .on("click", function (event: MouseEvent, vt: ITransformationVisibility) {
                        vt.visible = !vt.visible;
                        d3.select(event.currentTarget as Element).style("fill-opacity", () => vt.visible ? 1 : 0);
                        svg.selectAll(".transform-line")
                            .filter((line: ILine) => line.startOrderId === d.value && line.value === vt.name)
                            .attr("visibility", () => vt.visible ? "visible" : "hidden");
                    });

                tooltip.style("left", `${event.pageX + 10}px`)
                    .style("top", `${event.pageY - 8}px`);

                tooltip.hideTooltip = false;
            }

            /**
            * Clears the tooltip
            * @param tooltip Tooltip element
            */
            function clearTooltip(tooltip: ID3Tooltip): void {
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
            * Returns the X value of the point
            * @param d Data point
            */
            function xValue(d: IPoint): number {
                return d.x;
            }

            /**
            * Returns the Y value of the point
            * @param d Data point
            */
            function yValue(d: IPoint): number {
                return d.y;
            }

            /**
            * Draws the chart
            */
            function draw(): void {
                $scope.fillPointsAndLines();

                // Remove the previous chart and tooltip, if any
                d3.select(".chart-tooltip").remove();
                d3.select(".chart-svg").remove();

                // Chart size and margin settings
                let margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
                let width = $scope.width - margin.left - margin.right;
                let height = $scope.height - margin.top - margin.bottom;

                // Set up the X axis 
                // Calculate indents for points 
                let xMin = d3.min($scope.points, $scope.xValue) || 0;
                let xMax = d3.max($scope.points, $scope.xValue) || 0;
                let xMargin = (xMax - xMin) * 0.05;

                let xScale = d3.scaleLinear()
                    .domain([xMin - xMargin, xMax + xMargin])
                    .range([0, width]);

                let xAxis = d3.axisBottom(xScale)
                    .ticks($scope.counterIteration)
                    .tickSizeInner(-height)
                    .tickSizeOuter(0)
                    .tickPadding(10);

                $scope.xMap = (d: IPoint) => xScale($scope.xValue(d));

                // Setting up the Y axis 
                // Calculate indents for points 
                let yMax = d3.max($scope.points, $scope.yValue) || 0;
                let yMin = d3.min($scope.points, $scope.yValue) || 0;
                let yMargin = (yMax - yMin) * 0.05;

                let yAmplitude = yMax - yMin;

                let yScale = d3.scaleLinear()
                    .domain([yMin - yMargin, yMax + yMargin])
                    .range([height, 0]);

                let yAxis = d3.axisLeft(yScale)
                    .ticks(yAmplitude > 20 ? yAmplitude / 10 : yAmplitude)
                    .tickSizeInner(-width)
                    .tickSizeOuter(0)
                    .tickPadding(10);

                $scope.yMap = (d: IPoint) => yScale($scope.yValue(d));

                // Set the fill color
                let color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);

                // Add the chart canvas to the page
                let svg = d3.select("#chart").append("svg")
                    .attr("width", $scope.width)
                    .attr("height", $scope.height)
                    .attr("class", "chart-svg");

                let g = svg.append("g")
                    .attr("transform", `translate(${margin.left},${margin.top})`);

                // Add definitions for line ends
                let defs = svg.append("defs");
                for (let i = 0; i < $scope.legend.length; i++) {
                    defs.append("marker")
                        .attr("id", `arrow${i}`)
                        .attr("viewBox", "0 -5 10 10").attr("refX", 6)
                        .attr("refY", 0)
                        .attr("markerWidth", 6)
                        .attr("markerHeight", 6)
                        .attr("orient", "auto")
                        .append("path")
                        .attr("d", "M0,-5L10,0L0,5")
                        .attr("stroke", color($scope.legend[i].id))
                        .attr("fill", color($scope.legend[i].id));
                }

                // Add a tooltip area to the page 
                let tooltip = d3.select("#chart").append("div")
                    .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
                    .style("opacity", 0);

                // Prevent tooltip from hiding when clicked
                tooltip.on("click", () => { (tooltip as any).hideTooltip = false; });

                // Hide tooltip when clicked outside elements
                d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });

                // X-axis
                g.append("g")
                    .attr("class", "x axis")
                    .attr("transform", `translate(0,${height})`)
                    .call(xAxis);

                g.append("text")
                    .attr("class", "label")
                    .attr("transform", `translate(${width / 2} ,${height + margin.top - $scope.legendHeight})`)
                    .style("text-anchor", "middle")
                    .text("Iteration")
                    .style("font-size", "12pt");

                // Y axis 
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

                // Draw lines 
                g.selectAll(".transform-line")
                    .data($scope.lines)
                    .enter()
                    .append("line")
                    .attr("class", "transform-line")
                    .attr("x1", (d: ILine) => xScale(d.x1))
                    .attr("y1", (d: ILine) => yScale(d.y1))
                    .attr("x2", (d: ILine) => xScale(d.x2))
                    .attr("y2", (d: ILine) => yScale(d.y2))
                    .attr("marker-end", (d: ILine) => `url(#arrow${d.arrowType})`)
                    .style("stroke", (d: ILine) => color(d.colorId))
                    .style("stroke-width", "2")
                    .attr("visibility", "visible");

                // Draw the legend 
                let legend = g.selectAll(".legend")
                    .data($scope.legend)
                    .enter()
                    .append("g")
                    .attr("class", "legend")
                    .attr("transform", (d: ILegendItem, i: number) => `translate(0,${i * 20})`)
                    .on("click", function (event: MouseEvent, d: ILegendItem) {
                        d.visible = !d.visible;
                        let legendEntry = d3.select(event.currentTarget as Element);

                        legendEntry.select("text")
                            .style("opacity", () => d.visible ? 1 : 0.5);

                        legendEntry.select("rect")
                            .style("fill-opacity", () => d.visible ? 1 : 0);

                        svg.selectAll(".transform-line")
                            .filter((line: ILine) => line.value === d.name)
                            .attr("visibility", () => d.visible ? "visible" : "hidden");

                        for (let k = 0; k < $scope.points.length; k++) {
                            for (let j = 0; j < $scope.legend.length; j++) {
                                if ($scope.points[k].transformationVisibility[j].name === d.name) {
                                    $scope.points[k].transformationVisibility[j].visible = d.visible;
                                }
                            }
                        }
                    });

                // Drawing points 
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
                    .on("click", (event: MouseEvent, d: IPoint) => $scope.showTooltip(event, d, tooltip, g));

                // Drawing colored rectangles of the legend
                legend.append("rect")
                    .attr("width", 15)
                    .attr("height", 15)
                    .style("fill", (d: ILegendItem) => color(d.id))
                    .style("stroke", (d: ILegendItem) => color(d.id))
                    .style("stroke-width", 4)
                    .attr("transform", `translate(0, -${$scope.legendHeight})`);

                // Drawing the legend text
                legend.append("text")
                    .attr("x", 24)
                    .attr("y", 9)
                    .attr("dy", ".35em")
                    .attr("transform", `translate(0, -${$scope.legendHeight})`)
                    .text((d: ILegendItem) => d.name)
                    .style("font-size", "9pt");
            }

            // Assigning methods in $scope
            $scope.draw = draw;
            $scope.fillPointsAndLines = fillPointsAndLines;
            $scope.fillPointTooltip = fillPointTooltip;
            $scope.showTooltip = showTooltip;
            $scope.clearTooltip = clearTooltip;
            $scope.yValue = yValue;
            $scope.xValue = xValue;

            // Initialize $scope properties 
            $scope.width = 800;
            $scope.dotRadius = 4;
            $scope.selectedDotRadius = $scope.dotRadius * 2;

            $scope.fillLegend = fillLegend;

            $scope.loadingScreenHeader = "Loading data";

            // Get task ID from URL 
            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            $scope.loading = true;

            // Loading data from the server 
            $http.get<any>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                    MapModelFromJson($scope, data.data);

                    $scope.ordersIds = [];
                    for (let i = 0; i < $scope.orders.length; i++) {
                        $scope.ordersIds.push({ id: i });
                    }

                    $scope.fillLegend();
                    $scope.legendHeight = $scope.legend.length * 20;
                    $scope.height = 800 + $scope.legendHeight;
                    $scope.initialOrder = $scope.ordersIds[0];
                    $scope.transformationType = $scope.transformationsList[0];

                    $scope.loading = false;
                })
                .catch(function () {
                    alert("Failed loading characteristic data");
                    $scope.loading = false;
                });
        };

        // Register the controller in the Angular module
        angular.module("libiada").controller("OrderTransformationVisualizationResultCtrl", ["$scope", "$http", orderTransformationVisualizationResult]);
    }
}

/**
* Wrapper for backward compatibility
*/
function OrderTransformationVisualizationResultController(): OrderTransformationVisualizationResultHandler {
    return new OrderTransformationVisualizationResultHandler();
}