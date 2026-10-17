import * as plotly from "plotly.js";
import {
    MapModelFromJson,
    getArrayMinMax,
    InitializeBootstrapTooltips,
    initScopeFromServer,
    throwHelper
} from "functions";
import type {
    Aggregator,
    Aligner,
    Characteristic,
    DistanceCalculator,
    LegendItem
} from "viewDataTypes";

//import d3 from "d3";
//import Plotly from "plotly.js";
/// <reference types="jquery" />

/**
 * Interface for the data object fetched from the server
 */
interface LocalCalculationResultData {
    aggregators: Aggregator[];
    aligners: Aligner[];
    characteristicNames: string[];
    characteristics: LocalCharacteristicsData[];
    characteristicsList: Characteristic[];
    distanceCalculators: DistanceCalculator[];
    lengths: number[][];
    notationName: string;
    partNames: string[][];
    researchObjectIds: string[];
    starts: number[][];
}

/**
 * Interface for the fragments data of each sequence
 */
interface LocalCharacteristicsData {
    AutocorrelationData: number[][];
    DifferenceData: number[][];
    FourierData: number[][];
    FragmentsData: FragmentData[];
    ResearchObjectName: string;
}

/**
 * Interface for the data on each fragment
 */
interface FragmentData {
    Characteristics: number[];
    Length: number;
    Name: string;
    Start: number;
}

/**
 * Interface for point data
 */
interface IPointDataLocalCalculationResult {
    legendIndex: number;
    legendId: number;
    researchObjectName: string;
    fragmentsData: IFragmentDataPoint[];
    rank?: number;
    name?: string;
}


/**
 * Interface for fragment data point
 */
interface IFragmentDataPoint {
    rank: number;
    legendIndex: number;
    name: string;
    characteristics: number[];
}

/**
 * Interface for similarity matrix response
 */
interface ILocalCharacteristicsSimilarityMatrixResponse {
    result: number[][];
    aligner: number;
    distanceCalculator: number;
    aggregator: number;
}

/**
 * Interface for the angular controller's scope
 */
interface LocalCalculationResultScope extends ng.IScope, LocalCalculationResultData {
    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;

    // Chart data and settings
    points: IPointDataLocalCalculationResult[];
    chartCharacteristics: Characteristic[];
    legend: LegendItem[];
    chartData: Partial<plotly.PlotData>[];
    layout: Partial<plotly.Layout>;
    lineChart: boolean;
    pointSize: number;
    chartElement: HTMLElement;
    colorScale: d3.ScaleSequential<string>;

    // UI state
    isCharacteristicsTableVisible: boolean;
    selectedPointIndex: number;
    selectedResearchObjectIndex: number;
    tooltipVisible: boolean;
    tooltip: { id: number; name: string; fragmentName: string; characteristics: number[] };

    // Comparison matrix data
    comparisonMatrix: number[][];
    usedAligner: string;
    usedDistanceCalculator: string;
    usedAggregator: string;

    // Dropdown selections
    aligner: Aligner;
    distanceCalculator: DistanceCalculator;
    aggregator: Aggregator;

    // Functions
    calculateLocalCharacteristicsSimilarityMatrix: () => void;
    changeCharacteristicsTableVisibility: () => void;
    fillLegend: () => void;
    addCharacteristic: () => void;
    deleteCharacteristic: (characteristic: Characteristic) => void;
    fillPoints: () => void;
    showTooltip: (selectedTrace: IPointDataLocalCalculationResult) => void;
    fillLinePlotData: () => void;
    fillScatterPlotData: () => void;
    fill3dScatterPlotData: () => void;
    fillParallelCoordinatesPlotData: () => void;
    draw: () => void;
    legendClick: (legendItem: LegendItem) => void;
    legendSetVisibilityForAll: (visibility: boolean) => void;
    dragbarMouseDown: () => void;
}

/**
 * Controller for displaying local calculation results
 */
class LocalCalculationResultHandler {
    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const localCalculationResult = async ($scope: LocalCalculationResultScope, $http: ng.IHttpService): Promise<void> => {
            function calculateLocalCharacteristicsSimilarityMatrix(): void {
                $http.get<ILocalCharacteristicsSimilarityMatrixResponse>("/api/LocalCalculationApi/CalculateLocalCharacteristicsSimilarityMatrix", {
                    params: {
                        taskId: $scope.taskId,
                        aligner: $scope.aligner.Value,
                        distanceCalculator: $scope.distanceCalculator.Value,
                        aggregator: $scope.aggregator.Value
                    }
                }).then(function (result) {
                    const response = result.data;
                    $scope.comparisonMatrix = response.result;
                    // TODO: fill this data from response
                    $scope.usedAligner = $scope.aligners[response.aligner - 1].Text;
                    $scope.usedDistanceCalculator = $scope.distanceCalculators[response.distanceCalculator - 1].Text;
                    $scope.usedAggregator = $scope.aggregators[response.aggregator - 1].Text;
                }, function (error) {
                    alert("Failed loading alignment data");
                    //$scope.loading = false;
                });
            }

            function changeCharacteristicsTableVisibility(): void {
                $scope.isCharacteristicsTableVisible = true;
            }

            function fillLegend(): void {
                $scope.legend = [];

                $scope.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.characteristics.length]);
                for (let k = 0; k < $scope.characteristics.length; k++) {
                    const color = $scope.colorScale(k + 1);
                    $scope.legend.push({
                        id: k + 1,
                        name: $scope.characteristics[k].ResearchObjectName,
                        visible: true,
                        color: color
                    });

                    // hack for the legend's dot color
                    document.styleSheets[0].insertRule(`.legend${k + 1}:after { background:${color} }`);
                }
            }

            $scope.addCharacteristic = () => $scope.chartCharacteristics.push($scope.characteristicsList.find(cl => $scope.chartCharacteristics.every(cc => cc !== cl))!);

            $scope.deleteCharacteristic = characteristic => $scope.chartCharacteristics.splice($scope.chartCharacteristics.indexOf(characteristic), 1);


            // initializes data for chart
            function fillPoints(): void {
                $scope.points = [];

                for (let i = 0; i < $scope.characteristics.length; i++) {
                    let characteristic = $scope.characteristics[i];
                    $scope.points.push({
                        legendIndex: i,
                        legendId: i + 1,
                        researchObjectName: characteristic.ResearchObjectName,
                        fragmentsData: []
                    });
                    for (let j = 0; j < characteristic.FragmentsData.length; j++) {
                        let fragmentData = characteristic.FragmentsData[j];
                        $scope.points[i].fragmentsData.push({
                            rank: j,
                            legendIndex: i,
                            name: fragmentData.Name,
                            characteristics: fragmentData.Characteristics
                        });
                    }
                }
            }

            // shows tooltip for dot or group of dots
            function showTooltip(selectedTrace: IPointDataLocalCalculationResult): void {
                $("button[data-bs-target='#tooltip-tab-pane']").tab("show");
                let selectedPoint = selectedTrace.fragmentsData[$scope.selectedPointIndex];
                $scope.tooltipVisible = true;
                $scope.tooltip = {
                    id: selectedTrace.rank,
                    name: selectedTrace.researchObjectName,
                    fragmentName: selectedPoint.name,
                    characteristics: selectedPoint.characteristics
                };
                let update: { [key: string]: any } = {};
                switch ($scope.chartCharacteristics.length) {
                    case 1:
                    case 2:
                        update = {
                            "marker.symbol": $scope.points.map(t => t.fragmentsData.map(p => p === selectedPoint ? "diamond-wide" : "circle")),
                            "marker.size": $scope.points.map(t => t.fragmentsData.map(p => p === selectedPoint ? 10 : $scope.pointSize * 1.5))
                        };
                        break;
                    case 3:
                        break;
                    default:
                }

                Plotly.restyle($scope.chartElement, update);

                $scope.$apply();
            }

            function fillLinePlotData(): void {
                let characteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0]);
                let characteristicsValues = $scope.points.map((p => p.fragmentsData.map(fd => fd.characteristics[characteristicIndex]))).flat();
                let {min, max} = getArrayMinMax(characteristicsValues);
                let range = Math.abs(max - min);

                // adding margins
                min -= Math.abs(range * 0.05);
                max += Math.abs(range * 0.05);

                let ranks: Array<{ x: number[], y: number[] }> = [];
                if ($scope.lineChart) {
                    for (let i = 0; i < $scope.points.length; i++) {
                        let y = $scope.points[i].fragmentsData.map(sd => sd.characteristics[characteristicIndex]);
                        y.sort((first, second) => second - first);
                        ranks.push({
                            //x is range from 1 to fragmentsData length
                            x: Array.from({length: y.length}, (x, i) => i + 1),
                            y: y
                        });
                    }
                } else {
                    for (let i = 0; i < $scope.points.length; i++) {
                        ranks.push({
                            //$scope.points[i].fragmentsData.map(sd => { return { x: sd.rank, y: sd.characteristics[characteristicIndex] } }) 
                            x: $scope.points[i].fragmentsData.map(sd => sd.rank),
                            y: $scope.points[i].fragmentsData.map(sd => sd.characteristics[characteristicIndex])
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
                    yaxis: {
                        range: [min, max],
                        title: {
                            text: $scope.characteristicNames[characteristicIndex],
                        }
                    }
                };
                $scope.pointSize = 2;
                $scope.chartData = $scope.points.map((p, i) => ({
                    hoverinfo: "x+y+text",
                    x: ranks[i].x,
                    y: ranks[i].y,
                    marker: {
                        color: $scope.legend[p.legendIndex].color,
                        size: $scope.pointSize,
                        opacity: 0.8
                    },
                    type: "scatter",
                    line: {
                        color: $scope.legend[p.legendIndex].color,
                        width: 0.75
                    },
                    mode: "lines+markers",
                    customdata: [p.legendId],
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

                $scope.pointSize = 3;
                $scope.chartData = $scope.points.map(p => ({
                    hoverinfo: "x+y+text",
                    type: "scattergl",
                    x: p.fragmentsData.map(fd => fd.characteristics[firstCharacteristicIndex]),
                    y: p.fragmentsData.map(fd => fd.characteristics[secondCharacteristicIndex]),
                    text: p.researchObjectName,
                    mode: "markers",
                    marker: {
                        opacity: 0.8,
                        color: $scope.legend[p.legendIndex].color,
                        size: $scope.pointSize
                    },
                    name: p.researchObjectName,
                    customdata: [p.legendId],
                    visible: $scope.legend[p.legendIndex].visible
                }));
            }

            function fill3dScatterPlotData(): void {
                let firstCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0]);
                let secondCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1]);
                let thirdCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[2]);

                $scope.pointSize = 3;
                $scope.chartData = $scope.points.map(p => ({
                    hoverinfo: "x+y+z+text",
                    x: p.fragmentsData.map(fd => fd.characteristics[firstCharacteristicIndex]),
                    y: p.fragmentsData.map(fd => fd.characteristics[secondCharacteristicIndex]),
                    z: p.fragmentsData.map(fd => fd.characteristics[thirdCharacteristicIndex]),
                    text: p.researchObjectName,
                    mode: "markers",
                    marker: {
                        opacity: 0.8,
                        color: $scope.legend[p.legendIndex].color,
                        size: $scope.pointSize
                    },
                    name: p.researchObjectName,
                    type: "scatter3d",
                    customdata: [p.legendId],
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
                    //pad: [80, 80, 80, 80],
                    line: {
                        color: $scope.points.map(p => p.fragmentsData.map(sd => sd.legendIndex)).flat(),
                        colorscale: "Turbo",
                    },

                    dimensions: characteristicsIndices.map(ci => ({
                        label: $scope.characteristicNames[ci],
                        values: $scope.points.map(p => p.fragmentsData.map(sd => sd.characteristics[ci])).flat()
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

                Plotly.newPlot($scope.chartElement, $scope.chartData, $scope.layout, {responsive: true});

                $scope.chartElement.on("plotly_click", (data: any) => {
                    $scope.selectedPointIndex = data.points[0].pointNumber;
                    $scope.selectedResearchObjectIndex = data.points[0].curveNumber;
                    let selectedPoint = $scope.points[data.points[0].curveNumber];
                    $scope.showTooltip(selectedPoint);
                });
            }

            function legendClick(legendItem: LegendItem): void {
                if ($scope.chartData && $scope.chartData[0].customdata && $scope.chartData[0].customdata.length > 0) {
                    let index: number[] = [];
                    let update = {visible: legendItem.visible ? "legendonly" : true};
                    for (let i = 0; i < $scope.chartData.length; i++) {
                        if ($scope.chartData[i].customdata![0] === legendItem.id) {
                            index.push(i);
                        }
                    }

                    Plotly.restyle($scope.chartElement, update, index);
                }
            }

            function legendSetVisibilityForAll(visibility: boolean): void {
                let update = {visible: visibility ? true : "legendonly"};
                $scope.legend.forEach(l => l.visible = visibility);
                Plotly.restyle($scope.chartElement, update);
            }

            function dragbarMouseDown(): void {
                let right = document.getElementById("sidebar");
                let bar = document.getElementById("dragbar");

                const drag = (e: MouseEvent) => {
                    document.selection ? document.selection.empty() : window.getSelection()?.removeAllRanges();
                    $scope.chartElement.style.width = `${e.pageX - (bar?.offsetWidth || 0) / 2}px`;

                    Plotly.relayout($scope.chartElement, {autosize: true});
                };

                if (bar) {
                    bar.addEventListener("mousedown", () => {
                        document.addEventListener("mousemove", drag);
                    });

                    bar.addEventListener("mouseup", () => {
                        document.removeEventListener("mousemove", drag);
                    });
                }
            }

            $scope.calculateLocalCharacteristicsSimilarityMatrix = calculateLocalCharacteristicsSimilarityMatrix;
            $scope.changeCharacteristicsTableVisibility = changeCharacteristicsTableVisibility;
            $scope.fillLinePlotData = fillLinePlotData;
            $scope.fillScatterPlotData = fillScatterPlotData;
            $scope.fill3dScatterPlotData = fill3dScatterPlotData;
            $scope.fillParallelCoordinatesPlotData = fillParallelCoordinatesPlotData;
            $scope.draw = draw;
            $scope.fillPoints = fillPoints;
            $scope.showTooltip = showTooltip;
            $scope.fillLegend = fillLegend;
            $scope.legendClick = legendClick;
            $scope.legendSetVisibilityForAll = legendSetVisibilityForAll;
            $scope.dragbarMouseDown = dragbarMouseDown;
            
            $scope.isCharacteristicsTableVisible = false;
            try {
                $scope.loadingScreenHeader = "Loading local characteristics calculation results"; 
                $scope.loading = true;
                // Extract task ID from URL
                let location: string[] = window.location.href.split("/");
                $scope.taskId = location[location.length - 1];

                // Fetch data from server
                const result: { data: LocalCalculationResultData } = 
                    await $http.get<LocalCalculationResultData>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`);

                MapModelFromJson($scope, result.data);
                $scope.chartCharacteristics = [$scope.characteristicsList[0]];
                $scope.aligner = $scope.aligners[0];
                $scope.distanceCalculator = $scope.distanceCalculators[0];
                $scope.aggregator = $scope.aggregators[0];
                $scope.fillLegend();
            } catch(error) {
                //TODO: change it to alertify
                alert("Failed loading local characteristics data");

            } finally {
                $scope.loading = false;
                $scope.$apply();
            }
            //await initScopeFromServer<LocalCalculationResultData>(
            //    $http,
            //    $scope,
            //    "Loading local characteristics calculation results",
            //    "Failed loading local characteristics data");
            
            $scope.chartElement = document.getElementById("chart") ?? throwHelper("Chart div element not found");
            
            // initializing tooltips for tabs
            InitializeBootstrapTooltips();
        }

        angular.module("libiada").controller("LocalCalculationResultCtrl", ["$scope", "$http", localCalculationResult]);
    }
}

export default function LocalCalculationResultController(): LocalCalculationResultHandler {
    return new  LocalCalculationResultHandler();
}
