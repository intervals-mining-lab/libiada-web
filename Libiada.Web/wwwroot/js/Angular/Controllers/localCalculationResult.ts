/// <reference types="angular" />
/// <reference types="d3" />
/// <reference types="plotly.js" />
/// <reference types="jquery" />

/**
 * Interface for local calculation result data
 */
interface ILocalCalculationResultData {
    // Data returned from TaskManagerApi/GetTaskData
    characteristics: ICharacteristicLocalCalculationResult[];
    characteristicsList: string[];
    characteristicNames: string[];
    aligners: ISelectItem[];
    distanceCalculators: ISelectItem[];
    aggregators: ISelectItem[];
    // Any additional properties
    [key: string]: any;
}

/**
 * Interface for characteristic data
 */
interface ICharacteristicLocalCalculationResult {
    ResearchObjectName: string;
    FragmentsData: IFragmentData[];
    // Any additional properties
    [key: string]: any;
}

/**
 * Interface for fragment data
 */
interface IFragmentData {
    Name: string;
    Characteristics: number[];
    // Any additional properties
    [key: string]: any;
}

/**
 * Interface for select list items
 */
interface ISelectItem {
    Text: string;
    Value: number;
    // Any additional properties
    [key: string]: any;
}

/**
 * Interface for characteristic point data
 */
interface IChartCharacteristic {
    id: number;
    value: string;
}

/**
 * Interface for legend item
 */
interface ILegendItem {
    id: number;
    name: string;
    visible: boolean;
    color: string;
}

/**
 * Interface for point data
 */
interface IPointDataLocalCalculationResult {
    legendIndex: number;
    legendId: number;
    researchObjectName: string;
    fragmentsData: IFragmentDataPoint[];
    rank?: number; // Optional as it's not clear if this is used
    name?: string; // Добавляем свойство name для использования в fillScatterPlotData
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
 * Interface for controller scope
 */
interface ILocalCalculationResultScope extends ng.IScope {
    // Task ID for API calls
    taskId: string;

    // Chart data and settings
    characteristics: ICharacteristicLocalCalculationResult[];
    characteristicsList: string[];
    characteristicNames: string[];
    points: IPointDataLocalCalculationResult[];
    chartCharacteristics: IChartCharacteristic[];
    chartsCharacterisrticsCount: number;
    legend: ILegendItem[];
    chartData: any[];
    layout: any;
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
    loading: boolean;
    loadingScreenHeader: string;
    height: number;
    legendHeight: number;

    // Comparison matrix data
    comparisonMatrix: number[][];
    usedAligner: string;
    usedDistanceCalculator: string;
    usedAggregator: string;

    // Dropdown selections
    aligner: ISelectItem;
    distanceCalculator: ISelectItem;
    aggregator: ISelectItem;
    aligners: ISelectItem[];
    distanceCalculators: ISelectItem[];
    aggregators: ISelectItem[];

    // Functions
    calculateLocalCharacteristicsSimilarityMatrix: () => void;
    changeCharacteristicsTableVisibility: () => void;
    fillLegend: () => void;
    addCharacteristic: () => void;
    deleteCharacteristic: (characteristic: IChartCharacteristic) => void;
    fillPoints: () => void;
    showTooltip: (selectedTrace: IPointDataLocalCalculationResult) => void;
    fillLinePlotData: () => void;
    fillScatterPlotData: () => void;
    fill3dScatterPlotData: () => void;
    fillParallelCoordinatesPlotData: () => void;
    draw: () => void;
    legendClick: (legendItem: ILegendItem) => void;
    legendSetVisibilityForAll: (visibility: boolean) => void;
    dragbarMouseDown: () => void;
    xValue: (d: IPointDataLocalCalculationResult | IFragmentDataPoint) => number;
    yValue: (d: IPointDataLocalCalculationResult | IFragmentDataPoint) => number;
    xMap?: (d: IPointDataLocalCalculationResult | IFragmentDataPoint) => number;
    yMap?: (d: IPointDataLocalCalculationResult | IFragmentDataPoint) => number;
}

/**
 * Controller for displaying local calculation results
 */
function LocalCalculationResultController(): void {
    "use strict";

    function localCalculationResult($scope: ILocalCalculationResultScope, $http: ng.IHttpService): void {
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
                // TODO: fill this data from responce
                $scope.usedAligner = $scope.aligners[response.aligner - 1].Text;
                $scope.usedDistanceCalculator = $scope.distanceCalculators[response.distanceCalculator - 1].Text;
                $scope.usedAggregator = $scope.aggregators[response.aggregator - 1].Text;
            }, function (error) {
                alert("Failed loading alignment data");
                $scope.loading = false;
            });
        }

        $scope.isCharacteristicsTableVisible = false;

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

        $scope.addCharacteristic = () => $scope.chartCharacteristics.push({
            id: $scope.chartsCharacterisrticsCount++,
            value: $scope.characteristicsList.find(cl => $scope.chartCharacteristics.every(cc => cc.value !== cl))!
        });

        $scope.deleteCharacteristic = (characteristic: IChartCharacteristic) =>
            $scope.chartCharacteristics.splice($scope.chartCharacteristics.indexOf(characteristic), 1);

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
            let selectetPoint = selectedTrace.fragmentsData[$scope.selectedPointIndex];
            $scope.tooltipVisible = true;
            $scope.tooltip = {
                id: selectedTrace.rank,
                name: selectedTrace.researchObjectName,
                fragmentName: selectetPoint.name,
                characteristics: selectetPoint.characteristics
            };
            let update: { [key: string]: any } = {};
            switch ($scope.chartCharacteristics.length) {
                case 1:
                case 2:
                    update = {
                        "marker.symbol": $scope.points.map(t => t.fragmentsData.map(p => p === selectetPoint ? "diamond-wide" : "circle")),
                        "marker.size": $scope.points.map(t => t.fragmentsData.map(p => p === selectetPoint ? 10 : $scope.pointSize * 1.5))
                    };
                    break;
                case 3:
                    break;
                default:
            }

            Plotly.restyle($scope.chartElement, update);

            $scope.$apply();
        }

        function xValue(d: IPointDataLocalCalculationResult | IFragmentDataPoint): number {
            return d.rank;
        }

        function yValue(d: IPointDataLocalCalculationResult | IFragmentDataPoint): number {
            return (d as any).y; // in some contexts this will be a property on the object
        }

        function fillLinePlotData(): void {
            let characteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let characteristicsValues = $scope.points.map((p => p.fragmentsData.map(fd => fd.characteristics[characteristicIndex]))).flat();
            let min = Math.min(...characteristicsValues);
            let max = Math.max(...characteristicsValues);
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
                        x: Array.from({ length: y.length }, (x, i) => i + 1),
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
                hoverinfo: "text+x+y",
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
                customdata: { legendId: p.legendId },
                name: p.researchObjectName,
                visible: $scope.legend[p.legendIndex].visible ? "true" : "legendonly"
            }));
        }

        function fillScatterPlotData(): void {
            let firstCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let secondCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1].value);

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
                hoverinfo: "text+x+y",
                type: "scattergl",
                x: p.fragmentsData.map(fd => fd.characteristics[firstCharacteristicIndex]),
                y: p.fragmentsData.map(fd => fd.characteristics[secondCharacteristicIndex]),
                text: p.name,
                mode: "markers",
                marker: {
                    opacity: 0.8,
                    color: $scope.legend[p.legendIndex].color,
                    size: $scope.pointSize
                },
                name: p.researchObjectName,
                customdata: { legendId: p.legendId },
                visible: $scope.legend[p.legendIndex].visible
            }));
        }

        function fill3dScatterPlotData(): void {
            let firstCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let secondCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1].value);
            let thirdCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[2].value);

            $scope.pointSize = 3;
            $scope.chartData = $scope.points.map(p => ({
                hoverinfo: "text+x+y+z",
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
                customdata: { legendId: p.legendId },
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
            let characteristicsIndices = $scope.chartCharacteristics.map(c => $scope.characteristicsList.indexOf(c.value));

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

            Plotly.newPlot($scope.chartElement, $scope.chartData, $scope.layout, { responsive: true });

            $scope.chartElement.on("plotly_click", (data: any) => {
                $scope.selectedPointIndex = data.points[0].pointNumber;
                $scope.selectedResearchObjectIndex = data.points[0].curveNumber;
                let selectedPoint = $scope.points[data.points[0].curveNumber];
                $scope.showTooltip(selectedPoint);
            });
        }

        function legendClick(legendItem: ILegendItem): void {
            if ($scope.chartData && $scope.chartData[0].customdata) {
                let index: number[] = [];
                let update = { visible: legendItem.visible ? "legendonly" : true };
                for (let i = 0; i < $scope.chartData.length; i++) {
                    if ($scope.chartData[i].customdata.legendId === legendItem.id) {
                        index.push(i);
                    }
                }

                Plotly.restyle($scope.chartElement, update, index);
            }
        }

        function legendSetVisibilityForAll(visibility: boolean): void {
            if ($scope.chartData && $scope.chartData[0].customdata) {
                let update = { visible: visibility ? true : "legendonly" };
                $scope.legend.forEach(l => l.visible = visibility);
                Plotly.restyle($scope.chartElement, update);
            }
        }

        function dragbarMouseDown(): void {
            let right = document.getElementById("sidebar");
            let bar = document.getElementById("dragbar");

            const drag = (e: MouseEvent) => {
                document.selection ? document.selection.empty() : window.getSelection()?.removeAllRanges();
                $scope.chartElement.style.width = `${e.pageX - (bar?.offsetWidth || 0) / 2}px`;

                Plotly.relayout($scope.chartElement, { autosize: true });
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
        $scope.xValue = xValue;
        $scope.yValue = yValue;

        $scope.chartsCharacterisrticsCount = 1;
        $scope.chartElement = document.getElementById("chart") as HTMLElement;

        $scope.loadingScreenHeader = "Loading data";

        let location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http.get<ILocalCalculationResultData>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, data.data);

                $scope.fillLegend();

                $scope.chartCharacteristics = [{ id: $scope.chartsCharacterisrticsCount++, value: $scope.characteristicsList[0] }];
                $scope.aligner = $scope.aligners[0];
                $scope.distanceCalculator = $scope.distanceCalculators[0];
                $scope.aggregator = $scope.aggregators[0];

                $scope.legendHeight = $scope.legend.length * 20;
                $scope.height = 800 + $scope.legendHeight;

                $scope.loading = false;
            }, function () {
                alert("Failed loading local characteristics data");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("LocalCalculationResultCtrl", ["$scope", "$http", localCalculationResult]);
}
