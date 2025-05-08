/// <reference types="angular" />
/// <reference types="d3" />
/// <reference types="functions" />
/// <reference path="./Interfaces/commonInterfaces.d.ts" />

// Interface for data points on the chart
// Unified interface for data points on the chart
interface ISubsequencesCalculationResultPoint extends IBasePoint {

    distributionIntervals?: IDistributionInterval[];
    order?: string;
    researchObjectId?: number;
    researchObjectName?: string;
    sequenceRemoteId?: string;
    attributes?: number[];
    partial?: boolean;
    featureId?: number;
    positions?: number[];
    lengths?: number[];
    subsequenceRemoteId?: string;
    rank?: number;
    characteristicsValues?: number[];
    FeatureVisible?: boolean;
    legendVisible?: boolean;
    filtersVisible?: boolean[];
    remoteId?: string;
}

// Interface for research object
interface IResearchObjectResult {
    id: number;
    name: string;
    visible: boolean;
    colorId: number;
    Nature: string;
    Group: number;
    SequenceType: number;
    Multisequence: any;
    Matter: number;
    MatterIds: number[];
    RemoteId: string | null;
    Notation: number;
    NotationValue: number;
    LanguageId: number | null;
    TranslatorId: number | null;
    Characteristics: number[];
    Description: string;
}

// Interface for attribute value
interface IAttributeValue {
    attribute: number;
    value: string;
}

// Interface for feature
interface IFeature {
    Value: string;
    Text?: string;
    Selected: boolean;
}

// Interface for characteristic
interface ICharacteristic {
    Value: number;
    Text: string;
}

// Interface for sequence data
interface ISequenceData {
    ResearchObjectId: number;
    ResearchObjectName: string;
    RemoteId: string;
    SubsequencesData: ISubsequenceData[];
}

// Interface for subsequence data
interface ISubsequenceData {
    Id: number;
    FeatureId: number;
    Attributes: number[];
    Partial: boolean;
    Starts: number[];
    Lengths: number[];
    CharacteristicsValues: number[];
    RemoteId: string;
}

// Interface for d3-tooltip with additional properties
interface ID3Tooltip extends d3.Selection<HTMLDivElement, unknown, HTMLElement, any> {
    hideTooltip?: boolean;
    selectedDots?: any;
}

// Interface for controller scope
interface ISubsequencesCalculationResultScope extends ng.IScope {
    // Chart data
    visiblePoints: ISubsequencesCalculationResultPoint[][];
    points: ISubsequencesCalculationResultPoint[][];
    researchObjects: IResearchObjectResult[];
    attributes: string[];
    attributeValues: IAttributeValue[];
    //features: { [key: number]: IFeature };
    features: any;
    lineChart: boolean;
    firstCharacteristic: ICharacteristic;
    secondCharacteristic: ICharacteristic;
    sequencesData: ISequenceData[];
    width: number;
    height: number;
    legendHeight: number;
    dotRadius: number;
    selectedDotRadius: number;
    xMap: (d: ISubsequencesCalculationResultPoint) => number;
    yMap: (d: ISubsequencesCalculationResultPoint) => number;

    // Data loading properties
    taskId: string;
    loadingScreenHeader: string;
    loading: boolean;
    characteristicComparers: any[];
    productFilter: string;
    subsequencesCharacteristicsList: ICharacteristic[];

    // Chart operation methods
    dotVisible: (dot: ISubsequencesCalculationResultPoint) => boolean;
    dotsSimilar: (d: ISubsequencesCalculationResultPoint, dot: ISubsequencesCalculationResultPoint) => boolean;
    fillVisiblePoints: () => void;
    filterByFeature: (feature: IFeature) => void;
    getAttributesText: (attributes: number[]) => string;
    fillPoints: () => void;
    fillPointTooltip: (d: ISubsequencesCalculationResultPoint) => string;
    showTooltip: (event: MouseEvent, d: ISubsequencesCalculationResultPoint, tooltip: ID3Tooltip, svg: any) => void;
    clearTooltip: (tooltip: ID3Tooltip) => void;
    yValue: (d: ISubsequencesCalculationResultPoint) => number;
    xValue: (d: ISubsequencesCalculationResultPoint) => number;
    addFilter: (newFilter: string) => void;
    deleteFilter: (filter: string, filterIndex: number) => void;
    getAttributeIdByName: (dot: ISubsequencesCalculationResultPoint, attributeName: string) => number | undefined;
    isAttributeEqual: (dot: ISubsequencesCalculationResultPoint, attributeName: string, expectedValue: string) => boolean;
    draw: () => void;
}

/**
 * Handler for displaying subsequence calculation results
 */
class SubsequencesCalculationResultHandler {
    constructor() {
        this.initializeController();
    }

    /**
     * Initializes the Angular controller
     */
    private initializeController(): void {
        const subsequencesCalculationResult = ($scope: ISubsequencesCalculationResultScope, $http: ng.IHttpService, $sce: ng.ISCEService): void => {
            "use strict";

            /**
             * Fills the array of visible points
             */
            function fillVisiblePoints(): void {
                $scope.visiblePoints = [];
                for (let i = 0; i < $scope.points.length; i++) {
                    $scope.visiblePoints.push([]);
                    for (let j = 0; j < $scope.points[i].length; j++) {
                        if ($scope.dotVisible($scope.points[i][j])) {
                            $scope.visiblePoints[i].push($scope.points[i][j]);
                        }
                    }
                }
            }

            /**
             * Gets the attribute text for a given subsequence
             * @param attributes Array of attribute IDs
             */
            function getAttributesText(attributes: number[]): string {
                const attributesText: string[] = [];
                for (let i = 0; i < attributes.length; i++) {
                    const attributeValue = $scope.attributeValues[attributes[i]];
                    attributesText.push($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : ` = ${attributeValue.value}`));
                }

                return $sce.trustAsHtml(attributesText.join("<br/>"));
            }

            /**
             * Returns the attribute index by its name, if exists
             * @param dot Data point
             * @param attributeName Attribute name
             */
            function getAttributeIdByName(dot: ISubsequencesCalculationResultPoint, attributeName: string): number | undefined {
                return dot.attributes.find(a => $scope.attributes[$scope.attributeValues[a].attribute] === attributeName);
            }

            /**
             * Returns true if the point has the given attribute and its value equals the expected value
             * @param dot Data point
             * @param attributeName Attribute name
             * @param expectedValue Expected value
             */
            function isAttributeEqual(dot: ISubsequencesCalculationResultPoint, attributeName: string, expectedValue: string): boolean {
                const attributeId = $scope.getAttributeIdByName(dot, attributeName);
                if (attributeId !== undefined) {
                    const product = $scope.attributeValues[attributeId].value.toUpperCase();
                    return product.indexOf(expectedValue) !== -1;
                }

                return false;
            }

            /**
             * Applies a new filter
             * @param newFilter Filter string
             */
            function addFilter(newFilter: string): void {
                d3.selectAll(".dot")
                    .attr("visibility", (d: ISubsequencesCalculationResultPoint) => {
                        const filterValue = newFilter.toUpperCase();
                        let visible = $scope.isAttributeEqual(d, "product", filterValue);
                        visible = visible || $scope.isAttributeEqual(d, "locus_tag", filterValue);
                        d.filtersVisible.push(visible);
                        return $scope.dotVisible(d) ? "visible" : "hidden";
                    });

                $scope.fillVisiblePoints();
            }

            /**
             * Removes the specified filter
             * @param filter Filter string
             * @param filterIndex Filter index
             */
            function deleteFilter(filter: string, filterIndex: number): void {
                d3.selectAll(".dot")
                    .attr("visibility", (d: ISubsequencesCalculationResultPoint) => {
                        d.filtersVisible.splice(filterIndex, 1);
                        return $scope.dotVisible(d) ? "visible" : "hidden";
                    });

                $scope.fillVisiblePoints();
            }

            /**
             * Initializes data for the gene map
             */
            function fillPoints(): void {
                $scope.researchObjects = [];
                $scope.points = [];
                for (let i = 0; i < $scope.sequencesData.length; i++) {
                    const sequenceData = $scope.sequencesData[i];
                    $scope.researchObjects.push({
                        id: sequenceData.ResearchObjectId,
                        name: sequenceData.ResearchObjectName,
                        visible: true,
                        colorId: i,
                        Nature: "",
                        Group: 0,
                        SequenceType: 0,
                        Multisequence: false,
                        Matter: 0,
                        MatterIds: [],
                        RemoteId: sequenceData.RemoteId || null,
                        Notation: 0,
                        NotationValue: 0,
                        LanguageId: null,
                        TranslatorId: null,
                        Characteristics: [],
                        Description: null
                    });

                    $scope.points.push([]);

                    for (let j = 0; j < sequenceData.SubsequencesData.length; j++) {
                        const subsequenceData = sequenceData.SubsequencesData[j];
                        const point: ISubsequencesCalculationResultPoint = {
                            id: subsequenceData.Id,
                            researchObjectId: sequenceData.ResearchObjectId,
                            researchObjectName: sequenceData.ResearchObjectName,
                            sequenceRemoteId: sequenceData.RemoteId,
                            attributes: subsequenceData.Attributes,
                            partial: subsequenceData.Partial,
                            featureId: subsequenceData.FeatureId,
                            positions: subsequenceData.Starts,
                            lengths: subsequenceData.Lengths,
                            subsequenceRemoteId: subsequenceData.RemoteId,
                            rank: j + 1,
                            characteristicsValues: subsequenceData.CharacteristicsValues,
                            colorId: i,
                            featureVisible: true,
                            legendVisible: true,
                            filtersVisible: [],
                            remoteId: subsequenceData.RemoteId // Added for compatibility with JS version
                            
                        };
                        $scope.points[i].push(point);
                    }
                }
            }

            /**
             * Filters points by subsequence feature
             * @param feature Feature for filtering
             */
            function filterByFeature(feature: IFeature): void {
                const featureValue = parseInt(feature.Value);
                d3.selectAll(".dot")
                    .filter((dot: ISubsequencesCalculationResultPoint) => dot.featureId === featureValue)
                    .attr("visibility", (d: ISubsequencesCalculationResultPoint) => {
                        d.featureVisible = feature.Selected;
                        return $scope.dotVisible(d) ? "visible" : "hidden";
                    });

                for (let i = 0; i < $scope.points.length; i++) {
                    for (let j = 0; j < $scope.points[i].length; j++) {
                        if ($scope.points[i][j].featureId === parseInt(feature.Value)) {
                            $scope.points[i][j].featureVisible = feature.Selected;
                        }
                    }
                }

                // TODO: optimize calls to this method
                $scope.fillVisiblePoints();
            }

            /**
             * Checks if a point is visible
             * @param dot Point to check
             */
            function dotVisible(dot: ISubsequencesCalculationResultPoint): boolean {
                const filterVisible = dot.filtersVisible.length === 0 || dot.filtersVisible.some(element => element);
                return dot.featureVisible && dot.legendVisible && filterVisible;
            }

            /**
             * Determines if points are similar by product
             * @param d First point
             * @param dot Second point
             */
            function dotsSimilar(d: ISubsequencesCalculationResultPoint, dot: ISubsequencesCalculationResultPoint): boolean {
                if (d.featureId !== dot.featureId) {
                    return false;
                }

                switch (d.featureId) {
                    case 1: // CDS
                    case 2: // RRNA
                    case 3: // TRNA
                        const firstProductId = $scope.getAttributeIdByName(d, "product");
                        const secondProductId = $scope.getAttributeIdByName(dot, "product");

                        if (firstProductId === undefined || secondProductId === undefined) {
                            return false;
                        }

                        const firstAttributeValue = $scope.attributeValues[firstProductId].value.toUpperCase();
                        const secondAttributeValue = $scope.attributeValues[secondProductId].value.toUpperCase();
                        if (firstAttributeValue !== secondAttributeValue) {
                            return false;
                        }
                        break;
                }

                return true;
            }

            /**
             * Shows tooltip for a point or group of points
             * @param event Mouse event
             * @param d Data point
             * @param tooltip Tooltip element
             * @param svg SVG element
             */
            function showTooltip(event: MouseEvent, d: ISubsequencesCalculationResultPoint, tooltip: ID3Tooltip, svg: any): void {
                $scope.clearTooltip(tooltip);
                const tooltipHtml: string[] = [];
                tooltip.style("opacity", 0.9);

                tooltip.selectedDots = svg.selectAll(".dot")
                    .filter((dot: ISubsequencesCalculationResultPoint) => {
                        if ($scope.xValue(dot) === $scope.xValue(d)
                            && $scope.yValue(dot) === $scope.yValue(d)) {
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
             * Creates a string representing tooltip text
             * @param d Data point
             */
            function fillPointTooltip(d: ISubsequencesCalculationResultPoint): string {
                const tooltipContent: string[] = [];
                const genBankLink = "<a target='_blank' rel='noopener' href='https://www.ncbi.nlm.nih.gov/nuccore/";

                // Using remoteId instead of sequenceRemoteId for compatibility with JS version
                const header = d.remoteId ? `${genBankLink}${d.remoteId}'>${d.researchObjectName}</a>` : d.researchObjectName;
                tooltipContent.push(header);

                if (d.remoteId) {
                    const peptideGenbankLink = `${genBankLink}${d.remoteId}'>Peptide ncbi page</a>`;
                    tooltipContent.push(peptideGenbankLink);
                }

                tooltipContent.push($scope.features[d.featureId]);
                tooltipContent.push($scope.getAttributesText(d.attributes));

                if (d.partial) {
                    tooltipContent.push("partial");
                }

                const start = d.positions[0] + 1;
                const end = d.positions[0] + d.lengths[0];
                const positionGenbankLink = d.remoteId ?
                    `${genBankLink}${d.remoteId}?from=${start}&to=${end}'>${d.positions.join(", ")}</a>` :
                    d.positions.join(", ");
                tooltipContent.push(`Position: ${positionGenbankLink}`);
                tooltipContent.push(`Length: ${d.lengths.join(", ")}`);
                // TODO: show all characteristics
                tooltipContent.push(`(${$scope.xValue(d)}, ${$scope.yValue(d)})`);

                return tooltipContent.join("</br>");
            }

            /**
             * Clears tooltip and deselects points
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
             * Returns X value for a data point
             * @param d Data point
             */
            function xValue(d: ISubsequencesCalculationResultPoint): number {
                return $scope.lineChart ? d.rank : d.characteristicsValues[+$scope.firstCharacteristic.Value];
            }

            /**
             * Returns Y value for a data point
             * @param d Data point
             */
            function yValue(d: ISubsequencesCalculationResultPoint): number {
                return $scope.lineChart ? d.characteristicsValues[+$scope.firstCharacteristic.Value] : d.characteristicsValues[+$scope.secondCharacteristic.Value];
            }

            /**
             * Main chart drawing method
             */
            function draw(): void {
                $scope.loading = true;
                $scope.loadingScreenHeader = "Drawing...";
                $scope.fillPoints();
                // Removing previous chart and tooltip if any
                d3.select(".chart-tooltip").remove();
                d3.select(".chart-svg").remove();

                // Sorting points by selected characteristic
                if ($scope.lineChart) {
                    for (let i = 0; i < $scope.points.length; i++) {
                        $scope.points[i].sort((first, second) => $scope.yValue(second) - $scope.yValue(first));

                        for (let j = 0; j < $scope.points[i].length; j++) {
                            $scope.points[i][j].rank = j + 1;
                        }
                    }
                }

                // All organisms are visible after redrawing
                $scope.researchObjects.forEach(researchObject => { researchObject.visible = true; });

                $scope.points.forEach(points => {
                    points.forEach(point => {
                        point.legendVisible = true;
                        point.FeatureVisible = $scope.features[point.featureId].Selected;
                    });
                });

                // Chart size and margin settings
                const margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
                const width = $scope.width - margin.left - margin.right;
                const height = $scope.height - margin.top - margin.bottom;

                // Calculating bounds for points
                const xMinArray: number[] = [];
                const xMaxArray: number[] = [];
                const yMaxArray: number[] = [];
                const yMinArray: number[] = [];

                $scope.points.forEach(points => {
                    xMinArray.push(d3.min(points, $scope.xValue) as number);
                    xMaxArray.push(d3.max(points, $scope.xValue) as number);
                    yMinArray.push(d3.min(points, $scope.yValue) as number);
                    yMaxArray.push(d3.max(points, $scope.yValue) as number);
                });

                // X-axis setup
                // Calculating bounds for points
                const xMin = d3.min(xMinArray) as number;
                const xMax = d3.max(xMaxArray) as number;
                const xMargin = (xMax - xMin) * 0.05;

                const xScale = d3.scaleLinear()
                    .domain([xMin - xMargin, xMax + xMargin])
                    .range([0, width]);
                const xAxis = d3.axisBottom(xScale)
                    .tickSizeInner(-height)
                    .tickSizeOuter(0)
                    .tickPadding(10);

                $scope.xMap = (d: ISubsequencesCalculationResultPoint) => xScale($scope.xValue(d));

                // Y-axis setup
                const yMin = d3.min(yMinArray) as number;
                const yMax = d3.max(yMaxArray) as number;
                const yMargin = (yMax - yMin) * 0.05;

                const yScale = d3.scaleLinear()
                    .domain([yMin - yMargin, yMax + yMargin])
                    .range([height, 0]);
                const yAxis = d3.axisLeft(yScale)
                    .tickSizeInner(-width)
                    .tickSizeOuter(0)
                    .tickPadding(10);

                $scope.yMap = (d: ISubsequencesCalculationResultPoint) => yScale($scope.yValue(d));

                // Fill color setup
                const color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.researchObjects.length]);

                // Adding chart canvas to the webpage
                const svg = d3.select("#chart").append("svg")
                    .attr("width", $scope.width)
                    .attr("height", $scope.height)
                    .attr("class", "chart-svg")
                    .append("g")
                    .attr("transform", `translate(${margin.left},${margin.top})`);

                // Adding tooltip area to the webpage
                const tooltip = d3.select("#chart").append("div")
                    .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
                    .style("opacity", 0) as ID3Tooltip;

                // Preventing tooltip hiding when clicked
                tooltip.on("click", () => { tooltip.hideTooltip = false; });

                // Hiding tooltip when clicking outside
                d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });

                // X-axis
                svg.append("g")
                    .attr("class", "x axis")
                    .attr("transform", `translate(0,${height})`)
                    .call(xAxis);

                svg.append("text")
                    .attr("class", "label")
                    .attr("transform", `translate(${width / 2} ,${height + margin.top - $scope.legendHeight})`)
                    .style("text-anchor", "middle")
                    .text($scope.lineChart ? "Rank" : $scope.firstCharacteristic.Text)
                    .style("font-size", "12pt");

                // Y-axis
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
                    .text($scope.lineChart ? $scope.firstCharacteristic.Text : $scope.secondCharacteristic.Text)
                    .style("font-size", "12pt");

                const researchObjectsGroups = svg.selectAll(".researchObject")
                    .data($scope.points)
                    .enter()
                    .append("g")
                    .attr("class", "researchObject");

                // Drawing points
                researchObjectsGroups.selectAll(".dot")
                    .data((d: ISubsequencesCalculationResultPoint[]) => d)
                    .enter()
                    .append("ellipse")
                    .attr("class", "dot")
                    .attr("rx", $scope.dotRadius)
                    .attr("ry", $scope.dotRadius)
                    .attr("cx", $scope.xMap)
                    .attr("cy", $scope.yMap)
                    .style("fill-opacity", 0.6)
                    .style("fill", (d: ISubsequencesCalculationResultPoint) => color(d.colorId))
                    .style("stroke", (d: ISubsequencesCalculationResultPoint) => color(d.colorId))
                    .attr("visibility", (d: ISubsequencesCalculationResultPoint) => $scope.dotVisible(d) ? "visible" : "hidden")
                    .on("click", (event: MouseEvent, d: ISubsequencesCalculationResultPoint) => $scope.showTooltip(event, d, tooltip, svg));

                // Drawing legend
                const legend = svg.selectAll(".legend")
                    .data($scope.researchObjects)
                    .enter()
                    .append("g")
                    .attr("class", "legend")
                    .attr("transform", (_d: IResearchObjectResult, i: number) => "translate(0," + i * 20 + ")")
                    .on("click", function (event: MouseEvent, d: IResearchObjectResult) {
                        d.visible = !d.visible;
                        const legendEntry = d3.select(event.currentTarget as HTMLElement);
                        legendEntry.select("text")
                            .style("opacity", () => d.visible ? 1 : 0.5);
                        legendEntry.select("rect")
                            .style("fill-opacity", () => d.visible ? 1 : 0);

                        svg.selectAll(".dot")
                            .filter((dot: ISubsequencesCalculationResultPoint) => dot.researchObjectId === d.id)
                            .attr("visibility", (dot: ISubsequencesCalculationResultPoint) => {
                                dot.legendVisible = d.visible;
                                return $scope.dotVisible(dot) ? "visible" : "hidden";
                            });
                    });

                // Drawing colored legend rectangles
                legend.append("rect")
                    .attr("width", 15)
                    .attr("height", 15)
                    .style("fill", (d: IResearchObjectResult) => color(d.colorId))
                    .style("stroke", (d: IResearchObjectResult) => color(d.colorId))
                    .style("stroke-width", 4)
                    .attr("transform", `translate(0, -${$scope.legendHeight})`);

                // Drawing legend text
                legend.append("text")
                    .attr("x", 24)
                    .attr("y", 9)
                    .attr("dy", ".35em")
                    .attr("transform", `translate(0, -${$scope.legendHeight})`)
                    .text((d: IResearchObjectResult) => d.name)
                    .style("font-size", "9pt");

                $scope.loading = false;
            }

            // Registering functions in $scope
            $scope.draw = draw;
            $scope.dotVisible = dotVisible;
            $scope.dotsSimilar = dotsSimilar;
            $scope.fillVisiblePoints = fillVisiblePoints;
            $scope.filterByFeature = filterByFeature;
            $scope.getAttributesText = getAttributesText;
            $scope.fillPoints = fillPoints;
            $scope.fillPointTooltip = fillPointTooltip;
            $scope.showTooltip = showTooltip;
            $scope.clearTooltip = clearTooltip;
            $scope.yValue = yValue;
            $scope.xValue = xValue;
            $scope.addFilter = addFilter;
            $scope.deleteFilter = deleteFilter;
            $scope.getAttributeIdByName = getAttributeIdByName;
            $scope.isAttributeEqual = isAttributeEqual;

            // Initializing $scope properties
            $scope.dotRadius = 3;
            $scope.selectedDotRadius = $scope.dotRadius * 3;
            $scope.visiblePoints = [];
            $scope.characteristicComparers = [];
            $scope.productFilter = "";

            $scope.loadingScreenHeader = "Loading subsequences characteristics";
            $scope.loading = true;

            // Getting task ID from URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            // Loading data from server
            $http.get<any>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                    MapModelFromJson($scope, data.data);

                    $scope.legendHeight = $scope.sequencesData.length * 20;
                    $scope.height = 800 + $scope.legendHeight;
                    $scope.width = 800;

                    $scope.firstCharacteristic = $scope.subsequencesCharacteristicsList[0];
                    $scope.secondCharacteristic = $scope.subsequencesCharacteristicsList[$scope.subsequencesCharacteristicsList.length - 1];

                    $scope.loading = false;
                })
                .catch(function () {
                    alert("Failed loading subsequences characteristics");
                    $scope.loading = false;
                });
        };

        // Registering controller in Angular module
        angular.module("libiada").controller("SubsequencesCalculationResultCtrl", ["$scope", "$http", "$sce", subsequencesCalculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 */
function SubsequencesCalculationResultController(): SubsequencesCalculationResultHandler {
    return new SubsequencesCalculationResultHandler();
}
