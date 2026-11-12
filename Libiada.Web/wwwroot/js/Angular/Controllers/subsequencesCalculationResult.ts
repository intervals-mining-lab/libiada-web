//import d3 from "d3";
import type * as plotly from "plotly.js";
import { getArrayMinMax, initScopeFromServer, throwHelper } from "functions";
import {
    Characteristic,
    ParallelCoordinatesPlotData,
    ResultsLoadingScreenData,
    PointsSimilarity,
    LegendItem,
    CharacteristicComparer,
    ColorClass,
    Rank,
    SubsequenceTooltip,
    SubsequenceData,
    SequenceData,
    AttributeValue,
    FeaturesDictionary
} from "viewDataTypes";

interface Point {
    id: number;
    legendIndex: number;
    researchObjectName: string;
    name: string;
    researchObjectId: number;
    sequenceRemoteId: string | undefined;
    subsequencesData: SubsequencePoint[];
}

interface SubsequencePoint {
    id: number;
    legendIndex: number;
    name: string;
    attributes: number[];
    partial: boolean;
    featureId: number;
    positions: number[];
    lengths: number[];
    subsequenceRemoteId: string | undefined;
    rank: number;
    characteristics: number[];
    featureVisible: boolean;
    legendVisible: boolean;
    filtersVisible: boolean[];
}

// Interface for the data object fetched from the server
interface SubsequencesCalculationResultData {
    attributeValues: AttributeValue[];
    attributes: { [key: number]: string };
    characteristicNames: string[];
    characteristicsList: Characteristic[];
    features: FeaturesDictionary;
    sequencesData: SequenceData[];
}

/**
 * Interface for the angular controller's scope
 */
interface SubsequencesCalculationResultScope extends
    ng.IScope, SubsequencesCalculationResultData, ResultsLoadingScreenData {
    points: Point[];
    legend: LegendItem[];
    visiblePoints: SubsequencePoint[];
    chartCharacteristics: Characteristic[];
    chartsCharacteristicsCount: number;
    tooltipElements: SubsequenceTooltip[];
    characteristicComparers: CharacteristicComparer[];
    chartElement: plotly.PlotlyHTMLElement;
    colorScale: d3.ScaleSequential<string>;
    selectedPointIndex: number;
    selectedResearchObjectIndex: number;
    lineChart?: boolean;
    chartData: Partial<ParallelCoordinatesPlotData>[];
    layout: Partial<plotly.Layout>;
    pointSize: number;
    pointsSimilarity: { same: 0; similar: 1; different: 2 };
    characteristicsTableRendering: { rendered: boolean };

    addCharacteristic: () => void;
    deleteCharacteristic: (characteristic: Characteristic) => void;
    fillPoints: () => void;
    showTooltip: (selectedTrace: Point) => void;
    fillPointTooltip: (trace: Point, point: SubsequencePoint, similarity: PointsSimilarity) => SubsequenceTooltip;
    fillLinePlotData: () => void;
    fillScatterPlotData: () => void;
    fill3dScatterPlotData: () => void;
    fillParallelCoordinatesPlotData: () => void;
    draw: () => void;
    fillLegend: () => void;
    legendClick: (legendItem: LegendItem) => void;
    legendSetVisibilityForAll: (visibility: boolean) => void;
    dragBarMouseDown: () => void;
    getAttributesText: (attributes: number[]) => string[];
    getAttributeIdByName: (dot: SubsequencePoint, attributeName: string) => number | undefined;
    isAttributeEqual: (dot: SubsequencePoint, attributeName: string, expectedValue: string) => boolean;
    addFilter: (newFilter: string) => void;
    deleteFilter: (filter: string, filterIndex: number) => void;
    dotsSimilar: (d: SubsequencePoint, dot: SubsequencePoint) => boolean;
    dotVisible: (d: SubsequencePoint) => boolean;
    fillVisiblePoints: () => void;
}

/**
 * Controller for displaying subsequences calculation results
 */
class SubsequencesCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private async ngOnInit(): Promise<void> {
        const subsequencesCalculationResult = async (
            $scope: SubsequencesCalculationResultScope,
            $http: ng.IHttpService): Promise<void> => {
            function fillLegend(): void {
                $scope.legend = [];

                $scope.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.sequencesData.length]);
                for (let k = 0; k < $scope.sequencesData.length; k++) {
                    const color: string = $scope.colorScale(k + 1);
                    $scope.legend.push({
                        id: k + 1,
                        name: $scope.sequencesData[k].ResearchObjectName,
                        visible: true,
                        color: color
                    });

                    // hack for the legend's dot color
                    document.styleSheets[0].insertRule(`.legend${k + 1}:after { background:${color} }`);
                }
            }

            $scope.addCharacteristic = () => {
                // searching for the characteristic that is not already in the chartCharacteristics array
                const newSelectedCharacteristic: Characteristic = $scope.characteristicsList.find(
                    (cl: Characteristic) => $scope.chartCharacteristics.every((cc: Characteristic) => cc !== cl))!
                $scope.chartCharacteristics.push(newSelectedCharacteristic);
            };

            $scope.deleteCharacteristic = (characteristic: Characteristic): void => {
                $scope.chartCharacteristics.splice($scope.chartCharacteristics.indexOf(characteristic), 1);
            };

            // initializes data for chart
            function fillPoints(): void {
                $scope.points = [];
                for (let i = 0; i < $scope.sequencesData.length; i++) {
                    let sequenceData: SequenceData = $scope.sequencesData[i];
                    $scope.points.push({
                        legendIndex: i,
                        researchObjectName: sequenceData.ResearchObjectName,
                        name: sequenceData.ResearchObjectName,
                        researchObjectId: sequenceData.ResearchObjectId,
                        sequenceRemoteId: sequenceData.RemoteId,
                        subsequencesData: [],
                        id: i
                    });
                    for (let j = 0; j < sequenceData.SubsequencesData.length; j++) {
                        let subsequenceData: SubsequenceData = sequenceData.SubsequencesData[j];
                        let point: SubsequencePoint = {
                            id: subsequenceData.Id,
                            legendIndex: i,
                            name: sequenceData.ResearchObjectName,
                            attributes: subsequenceData.Attributes,
                            partial: subsequenceData.Partial,
                            featureId: subsequenceData.FeatureId,
                            positions: subsequenceData.Starts,
                            lengths: subsequenceData.Lengths,
                            subsequenceRemoteId: subsequenceData.RemoteId,
                            rank: j + 1,
                            characteristics: subsequenceData.CharacteristicsValues,
                            featureVisible: true,
                            legendVisible: true,
                            filtersVisible: []
                        };
                        $scope.points[i].subsequencesData.push(point);
                    }
                }
            }

            // shows tooltip for dot or group of dots
            function showTooltip(selectedTrace: Point): void {
                $("button[data-bs-target='#tooltip-tab-pane']").tab("show");
                let selectedPoint: SubsequencePoint = selectedTrace.subsequencesData[$scope.selectedPointIndex];
                $scope.tooltipElements.length = 0;
                $scope.tooltipElements.push(fillPointTooltip(selectedTrace, selectedPoint, $scope.pointsSimilarity.same));
                let similarPoints: SubsequencePoint[] = [];

                for (let i = 0; i < $scope.points.length; i++) {
                    for (let j = 0; j < $scope.points[i].subsequencesData.length; j++) {
                        if (selectedPoint !== $scope.points[i].subsequencesData[j]) {
                            let similar: boolean = $scope.chartCharacteristics.every(filter => {
                                let characteristicIndex: number = $scope.characteristicsList.indexOf(filter);
                                let selectedPointValue: number = selectedPoint.characteristics[characteristicIndex];
                                let anotherPointValue: number = $scope.points[i].subsequencesData[j].characteristics[characteristicIndex];
                                return selectedPointValue == anotherPointValue;
                            });

                            if (similar) {
                                let point: SubsequencePoint = $scope.points[i].subsequencesData[j];
                                similarPoints.push(point);
                                $scope.tooltipElements.push(fillPointTooltip($scope.points[i], point, $scope.pointsSimilarity.similar));
                            }
                        }
                    }
                }

                let update: plotly.Data = {};
                switch ($scope.chartCharacteristics.length) {
                    case 1:
                    case 2:
                        update = {
                            "marker.symbol": $scope.points.map(t => t.subsequencesData.map(p => p === selectedPoint || similarPoints.includes(p) ? "diamond-wide" : "circle")),
                            "marker.size": $scope.points.map(t => t.subsequencesData.map(p => p === selectedPoint || similarPoints.includes(p) ? 10 : $scope.pointSize * 1.5))
                        };
                        break;
                    case 3:
                        break;
                    default:
                }

                Plotly.restyle($scope.chartElement, update);

                $scope.$apply();
            }

            // constructs string representing tooltip text (inner html)
            function fillPointTooltip(trace: Point, point: SubsequencePoint, similarity: PointsSimilarity): SubsequenceTooltip {
                let color: ColorClass = similarity === $scope.pointsSimilarity.same ? "default"
                        : similarity === $scope.pointsSimilarity.similar ? "success"
                        : similarity === $scope.pointsSimilarity.different ? "danger" : "danger";

                let tooltipElement: SubsequenceTooltip = {
                    id: point.id,
                    name: trace.researchObjectName,
                    sequenceRemoteId: trace.sequenceRemoteId,
                    feature: $scope.features[point.featureId].Text,
                    attributes: $scope.getAttributesText(point.attributes),
                    partial: point.partial,
                    color: color,
                    characteristics: point.characteristics,
                    similarity: similarity,
                    selectedForAlignment: false,
                    position: "(",
                    length: 0,
                    positions: point.positions,
                    lengths: point.lengths
                };

                if (point.subsequenceRemoteId) {
                    tooltipElement.remoteId = point.subsequenceRemoteId;
                }

                for (let i = 0; i < point.positions.length; i++) {
                    tooltipElement.position += point.positions[i] + 1;
                    tooltipElement.position += "..";
                    tooltipElement.position += point.positions[i] + point.lengths[i];
                    tooltipElement.length += point.lengths[i];
                    if (i !== point.positions.length - 1) {
                        tooltipElement.position += ", ";
                    }
                }

                tooltipElement.position += ")";

                return tooltipElement;
            }

            function fillLinePlotData(): void {
                let characteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0]);
                let characteristicsValues: number[] = $scope.points.map((p => p.subsequencesData.map(fd => fd.characteristics[characteristicIndex]))).flat();
                let { min, max }: { min: number; max: number } = getArrayMinMax(characteristicsValues);
                let range: number = Math.abs(max - min);
                // adding margins
                min -= Math.abs(range * 0.05);
                max += Math.abs(range * 0.05);

                let ranks: Rank[] = [];
                if ($scope.lineChart) {
                    for (let i = 0; i < $scope.points.length; i++) {
                        let y: number[] = $scope.points[i].subsequencesData.map(sd => sd.characteristics[characteristicIndex]);
                        y.sort((first, second) => second - first);
                        ranks.push({
                            //x is range from 1 to subsequencesData length
                            x: Array.from({ length: y.length }, (_, i) => i + 1),
                            y: y
                        });
                    }
                } else {
                    for (let i = 0; i < $scope.points.length; i++) {
                        ranks.push({
                            x: $scope.points[i].subsequencesData.map(sd => sd.rank),
                            y: $scope.points[i].subsequencesData.map(sd => sd.characteristics[characteristicIndex])
                        });
                    }
                }

                $scope.layout = {
                    margin: {
                        l: 50,
                        r: 20,
                        t: 10,
                        b: 20
                    },
                    showlegend: false,
                    xaxis: { categoryorder: "total ascending" },
                    yaxis: {
                        range: [min, max],
                        title: {
                            text: $scope.characteristicNames[characteristicIndex],
                        }
                    }
                };

                $scope.pointSize = 3;
                $scope.chartData = $scope.points.map<Partial<plotly.PlotData>>((p, i) => ({
                    hoverinfo: "x+y+text",
                    x: ranks[i].x,
                    y: ranks[i].y,
                    text: p.researchObjectName,
                    marker: {
                        color: $scope.legend[p.legendIndex].color,
                        size: $scope.pointSize,
                        opacity: 0.8
                    },
                    type: "scatter",
                    mode: "markers",
                    customdata: [p.legendIndex],
                    name: p.researchObjectName,
                    visible: $scope.legend[p.legendIndex].visible ? true : "legendonly"
                }));
            }

            function fillScatterPlotData(): void {
                let firstCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0]);
                let secondCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1]);

                $scope.layout = {
                    margin: {
                        l: 50,
                        r: 20,
                        t: 30,
                        b: 40
                    },
                    showlegend: false,
                    hovermode: "closest",
                    xaxis: {
                        //type: $scope.plotTypeX ? "log" : "",
                        title: {
                            text: $scope.characteristicNames[firstCharacteristicIndex]
                        }
                    },
                    yaxis: {
                        //type: $scope.plotTypeY ? "log" : "",
                        title: {
                            text: $scope.characteristicNames[secondCharacteristicIndex]
                        }
                    }
                };

                $scope.pointSize = 5;
                $scope.chartData = $scope.points.map<Partial<plotly.PlotData>>(p => ({
                    hoverinfo: "x+y+text",
                    type: "scattergl",
                    x: p.subsequencesData.map(sd => sd.characteristics[firstCharacteristicIndex]),
                    y: p.subsequencesData.map(sd => sd.characteristics[secondCharacteristicIndex]),
                    text: p.name,
                    mode: "markers",
                    marker: {
                        opacity: 0.8,
                        color: $scope.legend[p.legendIndex].color,
                        size: $scope.pointSize
                    },
                    name: p.name,
                    customdata: [p.legendIndex],
                    visible: $scope.legend[p.legendIndex].visible
                }));
            }

            function fill3dScatterPlotData(): void {
                let firstCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0]);
                let secondCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1]);
                let thirdCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[2]);

                $scope.pointSize = 5;
                $scope.chartData = $scope.points.map<Partial<plotly.PlotData>>(p => ({
                    hoverinfo: "x+y+z+text",
                    x: p.subsequencesData.map(sd => sd.characteristics[firstCharacteristicIndex]),
                    y: p.subsequencesData.map(sd => sd.characteristics[secondCharacteristicIndex]),
                    z: p.subsequencesData.map(sd => sd.characteristics[thirdCharacteristicIndex]),
                    text: p.name,
                    mode: "markers",
                    marker: {
                        opacity: 0.8,
                        color: $scope.legend[p.legendIndex].color,
                        size: $scope.pointSize
                    },
                    name: p.name,
                    type: "scatter3d",
                    customdata: [p.legendIndex],
                    visible: $scope.legend[p.legendIndex].visible
                }));

                $scope.layout = {
                    margin: {
                        l: 0,
                        r: 0,
                        b: 0,
                        t: 0
                    },
                    showlegend: false,
                    scene: {
                        xaxis: {
                            //type: $scope.plotTypeX ? "log" : "",
                            title: {
                                text: $scope.characteristicNames[firstCharacteristicIndex],
                                font: {
                                    size: 10
                                }
                            },
                        },
                        yaxis: {
                            //type: $scope.plotTypeY ? "log" : "",
                            title: {
                                text: $scope.characteristicNames[secondCharacteristicIndex],
                                font: {
                                    size: 10
                                }
                            }
                        },
                        zaxis: {
                            //type: $scope.plotTypeY ? "log" : "",
                            title: {
                                text: $scope.characteristicNames[thirdCharacteristicIndex],
                                font: {
                                    size: 10
                                }
                            }
                        }
                    }
                };
            }

            function fillParallelCoordinatesPlotData(): void {
                let characteristicsIndices: number[] = $scope.chartCharacteristics.map(c => $scope.characteristicsList.indexOf(c));

                $scope.chartData = [{
                    type: "parcoords",
                    line: {
                        color: $scope.points.map(p => p.subsequencesData.map(sd => sd.legendIndex)).flat(),
                        //colorscale: "Turbo"
                    },

                    dimensions: characteristicsIndices.map(ci => ({
                        label: $scope.characteristicNames[ci],
                        values: $scope.points.map(p => p.subsequencesData.map(sd => sd.characteristics[ci])).flat()
                    }))
                }];

                $scope.layout = {
                    margin: {
                        l: 50,
                        r: 50,
                        b: 20,
                        t: 70
                    },
                    showlegend: false
                };
            }

            function draw(): void {
                $scope.fillPoints();

                switch ($scope.chartCharacteristics.length) {
                    case 1:
                        $scope.fillLinePlotData();
                        break;
                    case 2:
                        $scope.fillScatterPlotData();
                        break;
                    case 3:
                        $scope.fill3dScatterPlotData();
                        break;
                    default:
                        $scope.fillParallelCoordinatesPlotData();
                }

                Plotly.newPlot($scope.chartElement, $scope.chartData, $scope.layout, { responsive: true });

                $scope.chartElement.on("plotly_click", (data: plotly.PlotMouseEvent) => {
                    $scope.selectedPointIndex = data.points[0].pointNumber;
                    $scope.selectedResearchObjectIndex = data.points[0].curveNumber;
                    let selectedTrace = $scope.points[data.points[0].curveNumber];
                    $scope.showTooltip(selectedTrace);
                });
            }

            function legendClick(legendItem: LegendItem): void {
                if ($scope.chartData?.[0]?.customdata && $scope.chartData[0].customdata.length > 0) {
                    let index: number[] = [];
                    let update: plotly.Data = { visible: legendItem.visible ? "legendonly" : true };
                    const legendIndex: number = $scope.legend.findIndex((l: LegendItem) => l.id === legendItem.id);
                    for (let i = 0; i < $scope.chartData.length; i++) {
                        if ($scope.chartData[i].customdata![0] === legendIndex) {
                            index.push(i);
                        }
                    }

                    Plotly.restyle($scope.chartElement, update, index);
                }
            }

            function legendSetVisibilityForAll(visibility: boolean): void {
                if ($scope.chartData && $scope.chartData[0].customdata) {
                    let update: plotly.Data = { visible: visibility ? true : "legendonly" };
                    $scope.legend.forEach(l => l.visible = visibility);
                    Plotly.restyle($scope.chartElement, update);
                }
            }

            function dragBarMouseDown(): void {
                let right: HTMLElement | null = document.getElementById("sidebar");
                let bar: HTMLElement | null = document.getElementById("drag-bar")!;

                const drag = (e: MouseEvent): void => {
                    const selection: Selection | null = window.getSelection();
                    if (selection) {
                        selection.empty();
                        selection.removeAllRanges();
                    }

                    $scope.chartElement.style.width = `${e.pageX - bar.offsetWidth / 2}px`;
                    Plotly.relayout($scope.chartElement, { autosize: true });

                };

                bar.addEventListener("mousedown", () => {
                    document.addEventListener("mousemove", drag);
                });

                window.addEventListener("mouseup", () => {
                    document.removeEventListener("mousemove", drag);
                });
            }

            // gets attributes text for given subsequence
            function getAttributesText(attributes: number[]): string[] {
                let attributesText: string[] = [];
                for (let i = 0; i < attributes.length; i++) {
                    let attributeValue: AttributeValue = $scope.attributeValues[attributes[i]];
                    attributesText.push($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : ` = ${attributeValue.value}`));
                }

                return attributesText;
            }

            // returns attribute index by its name if any
            function getAttributeIdByName(dot: SubsequencePoint, attributeName: string): number | undefined {
                return dot.attributes.find(a => $scope.attributes[$scope.attributeValues[a].attribute] === attributeName);
            }

            // returns true if dot has given attribute and its value equal to the given value
            function isAttributeEqual(dot: SubsequencePoint, attributeName: string, expectedValue: string): boolean {
                let attributeId: number | undefined = $scope.getAttributeIdByName(dot, attributeName);
                if (attributeId !== undefined) {
                    let product: string = $scope.attributeValues[attributeId].value.toUpperCase();
                    return product.indexOf(expectedValue) !== -1;
                }

                return false;
            }

            // applies new filter
            function addFilter(newFilter: string): void {
                d3.selectAll<SVGGElement, SubsequencePoint>(".dot")
                    .attr("visibility", (d: SubsequencePoint) => {
                        let filterValue = newFilter.toUpperCase();
                        let visible = $scope.isAttributeEqual(d, "product", filterValue);
                        visible = visible || $scope.isAttributeEqual(d, "locus_tag", filterValue);
                        d.filtersVisible.push(visible);
                        return $scope.dotVisible(d) ? "visible" : "hidden";
                    });

                $scope.fillVisiblePoints();
            }

            // removes given filter
            function deleteFilter(_filter: string, filterIndex: number): void {
                d3.selectAll<SVGGElement, SubsequencePoint>(".dot")
                    .attr("visibility", (d: SubsequencePoint) => {
                        if (d.filtersVisible) {
                            d.filtersVisible.splice(filterIndex, 1);
                        }
                        return $scope.dotVisible(d) ? "visible" : "hidden";
                    });

                $scope.fillVisiblePoints();
            }

            // determines if dots are similar by product
            function dotsSimilar(d: SubsequencePoint, dot: SubsequencePoint): boolean {
                if (d.featureId !== dot.featureId) {
                    return false;
                }

                switch (d.featureId) {
                    case 1: // CDS
                    case 2: // RRNA
                    case 3: // TRNA
                        const firstProductId: number | undefined = $scope.getAttributeIdByName(d, "product");
                        const secondProductId: number | undefined = $scope.getAttributeIdByName(dot, "product");
                        if (firstProductId !== undefined && secondProductId !== undefined) {
                            const firstAttributeValue = $scope.attributeValues[firstProductId].value.toUpperCase();
                            const secondAttributeValue = $scope.attributeValues[secondProductId].value.toUpperCase();
                            if (firstAttributeValue !== secondAttributeValue) {
                                return false;
                            }
                        }
                        break;
                }

                return true;
            }

            function dotVisible(_d: SubsequencePoint): boolean {
                // TODO: implement this function
                throw new Error("Not implemented.");
            }

            function fillVisiblePoints(): void {
                // TODO: implement this function
                throw new Error("Not implemented.");
            }

            $scope.fillLinePlotData = fillLinePlotData;
            $scope.fillScatterPlotData = fillScatterPlotData;
            $scope.fill3dScatterPlotData = fill3dScatterPlotData;
            $scope.fillParallelCoordinatesPlotData = fillParallelCoordinatesPlotData;
            $scope.draw = draw;
            $scope.fillPoints = fillPoints;
            $scope.fillLegend = fillLegend;
            $scope.legendClick = legendClick;
            $scope.legendSetVisibilityForAll = legendSetVisibilityForAll;
            $scope.dragBarMouseDown = dragBarMouseDown;
            $scope.showTooltip = showTooltip;
            $scope.dotVisible = dotVisible;
            $scope.fillVisiblePoints = fillVisiblePoints;

            $scope.dotsSimilar = dotsSimilar;
            $scope.getAttributesText = getAttributesText;
            $scope.addFilter = addFilter;
            $scope.deleteFilter = deleteFilter;
            $scope.getAttributeIdByName = getAttributeIdByName;
            $scope.isAttributeEqual = isAttributeEqual;
            $scope.fillPointTooltip = fillPointTooltip;

            $scope.chartElement = document.getElementById("chart") as plotly.PlotlyHTMLElement ?? throwHelper("Chart div element not found");
            $scope.characteristicsTableRendering = { rendered: false };
            $scope.chartsCharacteristicsCount = 1;

            $scope.tooltipElements = [];
            $scope.visiblePoints = [];
            $scope.characteristicComparers = [];
            $scope.pointsSimilarity = Object.freeze({ "same": 0, "similar": 1, "different": 2 });

            await initScopeFromServer<SubsequencesCalculationResultData>(
                $http, $scope, "Loading subsequences characteristics", "Failed loading subsequences characteristics");
            $scope.fillLegend();

            $scope.chartCharacteristics = [$scope.characteristicsList[0]];
            $scope.$apply();
        };

        // Register controller in Angular module
        angular.module("libiada").controller("SubsequencesCalculationResultCtrl",
            ["$scope", "$http", "$sce", subsequencesCalculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of subsequence calculation result handler
 */
export default function SubsequencesCalculationResultController(): SubsequencesCalculationResultHandler {
    return new SubsequencesCalculationResultHandler();
}
