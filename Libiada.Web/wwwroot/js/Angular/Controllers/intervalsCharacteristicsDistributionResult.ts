/// <reference types="angular" />
/// <reference types="d3" />
/// <reference path="./Interfaces/commonInterfaces.d.ts" />

// Interface for distribution interval data
interface IDistributionInterval {
    interval: number;
    count: number;
}

// Interface for order data
interface IOrder {
    order: string;
    characteristics: {
        Characteristics: number[];
    };
}

// Interface for accordance data
interface IAccordance {
    distributionIntervals: IDistributionInterval[];
    orders: IOrder[];
}

// Interface for link result data
interface ILinkResult {
    link: string;
    accordance: IAccordance[];
}

// Interface for point data to display on chart

interface IDistributionPoint extends IBasePoint  {
 
    distributionIntervals?: IDistributionInterval[];
    order?: string;
    
}

// Interface for characteristic data
interface ICharacteristic {
    Text: string;
    Value: number;
}

// Interface for d3-tooltip with additional properties
interface ID3Tooltip extends d3.Selection<HTMLDivElement, unknown, HTMLElement, any> {
    hideTooltip?: boolean;
    selectedDots?: any;
}

// Interface for the controller scope
interface IIntervalsCharacteristicsDistributionResultScope extends ng.IScope {
    // Chart data and settings
    result: ILinkResult[];
    points: IDistributionPoint[];
    characteristic: ICharacteristic;
    characteristicsList: ICharacteristic[];
    width: number;
    height: number;
    dotRadius: number;
    selectedDotRadius: number;

    // Data loading properties
    taskId: string;
    loadingScreenHeader: string;
    loading: boolean;

    // Chart functions
    draw: () => void;
    fillPoints: () => void;
    fillPointTooltip: (d: IDistributionPoint) => string;
    showTooltip: (event: MouseEvent, d: IDistributionPoint, tooltip: ID3Tooltip, svg: any) => void;
    clearTooltip: (tooltip: ID3Tooltip) => void;
    xValue: (d: IDistributionPoint) => number;
    yValue: (d: IDistributionPoint) => number;
    xMap?: (d: IDistributionPoint) => number;
    yMap?: (d: IDistributionPoint) => number;
}

/**
 * Controller for displaying interval characteristics distribution results
 */
class IntervalsCharacteristicsDistributionResultHandler {
    constructor() {
        this.initializeController();
    }

    private initializeController(): void {
        const intervalsCharacteristicsDistributionResult = ($scope: IIntervalsCharacteristicsDistributionResultScope, $http: ng.IHttpService): void => {
            "use strict";

            /**
             * Initializes data for chart visualization
             */
            function fillPoints(): void {
                $scope.points = [];
                let index: number = 0;
                const chT: string[] = $scope.characteristic.Text.split("  ");
                const ch: number = +$scope.characteristic.Value;

                for (let i = 0; i < $scope.result.length; i++) {
                    if ($scope.result[i].link === chT[chT.length - 1]) {
                        for (let j = 0; j < $scope.result[i].accordance.length; j++) {
                            const distributionIntervals = $scope.result[i].accordance[j].distributionIntervals;
                            const orders = $scope.result[i].accordance[j].orders;

                            for (let k = 0; k < orders.length; k++) {
                                $scope.points.push({
                                    id: index++,
                                    distributionIntervals: distributionIntervals,
                                    order: orders[k].order,
                                    x: j + 1,
                                    y: orders[k].characteristics.Characteristics[ch],
                                    colorId: i,    
                                    featureVisible: true,
          
                                });
                            }
                        }
                    }
                }
            }

            /**
             * Constructs tooltip content text as HTML
             * @param d The point data to display in tooltip
             */
            function fillPointTooltip(d: IDistributionPoint): string {
                const tooltipContent: string[] = [];

                const pointsOrder: string[] = [];
                pointsOrder.push(d.order);

                tooltipContent.push(pointsOrder.join("<br/>"));

                return tooltipContent.join("</br>");
            }

            /**
             * Shows tooltip for a point or group of points
             * @param event Mouse event that triggered the tooltip
             * @param d Point data to show in tooltip
             * @param tooltip The tooltip element
             * @param svg The SVG container element
             */
            function showTooltip(event: MouseEvent, d: IDistributionPoint, tooltip: ID3Tooltip, svg: any): void {
                $scope.clearTooltip(tooltip);

                tooltip.style("opacity", 0.9);

                const tooltipHtml: string[] = [];
                tooltipHtml.push("Characteristic value: ");
                tooltipHtml.push(d.y.toString());
                tooltipHtml.push("Distribution intervals: ");

                const intervals: Array<[number, number]> = [];
                for (let i = 0; i < d.distributionIntervals.length; i++) {
                    intervals.push([d.distributionIntervals[i].interval, d.distributionIntervals[i].count]);
                    tooltipHtml.push(intervals[i].join("|"));
                }
                tooltipHtml.push("Orders: ");

                tooltip.selectedDots = svg.selectAll(".dot")
                    .filter((dot: IDistributionPoint) => {
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

                tooltip.style("left", `${event.pageX + 10}px`)
                    .style("top", `${event.pageY - 8}px`);

                tooltip.hideTooltip = false;
            }

            /**
             * Clears tooltip and unselects dots
             * @param tooltip The tooltip element to clear
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
             * Returns x value for a data point
             */
            function xValue(d: IDistributionPoint): number {
                return d.x;
            }

            /**
             * Returns y value for a data point
             */
            function yValue(d: IDistributionPoint): number {
                return d.y;
            }

            /**
             * Creates and draws the chart visualization
             */
            function draw(): void {
                $scope.fillPoints();

                // Remove previous chart and tooltip if present
                d3.select(".chart-tooltip").remove();
                d3.select(".chart-svg").remove();

                // Chart size and margin settings
                const margin = { top: 30, right: 30, bottom: 30, left: 60 };
                const width = $scope.width - margin.left - margin.right;
                const height = $scope.height - margin.top - margin.bottom;

                // Setup x-axis
                // Calculate margins for points
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

                $scope.xMap = (d: IDistributionPoint) => xScale($scope.xValue(d));

                // Setup y-axis
                // Calculate margins for points
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

                $scope.yMap = (d: IDistributionPoint) => yScale($scope.yValue(d));

                // Setup fill color
                const color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.points.length]);

                // Add the chart canvas to the page
                const svg = d3.select("#chart").append("svg")
                    .attr("width", $scope.width)
                    .attr("height", $scope.height)
                    .attr("class", "chart-svg")
                    .append("g")
                    .attr("transform", `translate(${margin.left},${margin.top})`);

                // Add the tooltip area to the page
                const tooltip = d3.select("#chart").append("div")
                    .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
                    .style("opacity", 0) as ID3Tooltip;

                // Prevent tooltip from hiding when clicked
                tooltip.on("click", () => { tooltip.hideTooltip = false; });

                // Hide tooltip when clicking elsewhere
                d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });

                // Draw x-axis
                svg.append("g")
                    .attr("class", "x axis")
                    .attr("transform", `translate(0,${height})`)
                    .call(xAxis);

                const chTX = $scope.characteristic.Text.split("  ");

                svg.append("text")
                    .attr("class", "label")
                    .attr("transform", `translate(${width / 2} ,${height + margin.top})`)
                    .style("text-anchor", "middle")
                    .text(`Intervals distribution link ${chTX[chTX.length - 1]}`)
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
                    .text($scope.characteristic.Text)
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
                    .style("fill", (d: IDistributionPoint) => color(d.id))
                    .style("stroke", (d: IDistributionPoint) => color(d.id))
                    .on("click", (event: MouseEvent, d: IDistributionPoint) => $scope.showTooltip(event, d, tooltip, svg));
            }

            // Register functions in $scope
            $scope.draw = draw;
            $scope.fillPoints = fillPoints;
            $scope.fillPointTooltip = fillPointTooltip;
            $scope.showTooltip = showTooltip;
            $scope.clearTooltip = clearTooltip;
            $scope.yValue = yValue;
            $scope.xValue = xValue;

            // Initialize $scope properties
            $scope.width = 800;
            $scope.height = 600;
            $scope.dotRadius = 4;
            $scope.selectedDotRadius = $scope.dotRadius * 2;

            $scope.loadingScreenHeader = "Loading Data";
            $scope.loading = true;

            // Get task ID from URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            // Load data from server
            $http.get<any>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                    MapModelFromJson($scope, data.data);
                    $scope.characteristic = $scope.characteristicsList[0];
                    $scope.loading = false;
                })
                .catch(function () {
                    alert("Failed loading sequences order distribution data");
                    $scope.loading = false;
                });
        };

        // Register controller in Angular module
        angular.module("libiada").controller("IntervalsCharacteristicsDistributionResultCtrl", ["$scope", "$http", intervalsCharacteristicsDistributionResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
function IntervalsCharacteristicsDistributionResultController(): IntervalsCharacteristicsDistributionResultHandler {
    return new IntervalsCharacteristicsDistributionResultHandler();
}
