/// <reference types="angular" />
/// <reference types="d3" />
/// <reference types="plotly.js" />
/// <reference types="jquery" />

/**
 * Interface for characteristic data
 */
interface IChartsCharacteristic {
    ResearchObjectName: string;
    Characteristics: number[];
    SequenceGroupId?: number;
}

/**
 * Interface for characteristic list item
 */
interface IChartsCharacteristicListItem {
    Value: number | string;
    Text: string;
}

/**
 * Interface for legend item
 */
interface IChartsLegendItem {
    id: number;
    name: string;
    visible: boolean;
    color: string;
}

/**
 * Interface for point data
 */
interface IChartsPointData {
    id: number;
    legendIndex: number;
    legendId: number;
    name: string;
    characteristics: number[];
}

/**
 * Interface for tooltip data
 */
interface IChartsTooltip {
    id: number;
    name: string;
    characteristics: number[];
}

/**
 * Interface for chart characteristic selection
 */
interface IChartsCharacteristicSelection {
    id: number;
    value: IChartsCharacteristicListItem;
}

/**
 * Interface for controller scope
 */
interface IChartsScope extends ng.IScope {
    // Chart data and settings
    characteristics: IChartsCharacteristic[];
    characteristicsList: IChartsCharacteristicListItem[];
    characteristicNames: string[];
    sequencesName: string;
    chartCharacteristics: IChartsCharacteristicSelection[];
    chartsCharacterisrticsCount: number;
    chartData: any[];
    layout: any;
    legend: IChartsLegendItem[];
    points: IChartsPointData[];
    colorScale: d3.ScaleSequential<string>;
    chartElement: HTMLElement;
    sequenceGroups?: any[];

    // UI state
    rawData: string;
    parsedData: string[][];
    inFocus: boolean;
    tooltipVisible: boolean;
    tooltip: IChartsTooltip;
    selectedPointIndex: number;
    selectedResearchObjectIndex: number;

    // Key codes
    keyCodes: { [key: string]: number };

    // Functions
    parseTabularData: (text: string) => string[][];
    textChanged: () => void;
    handleKeyDown: (e: JQuery.KeyDownEvent, args: any) => void;
    fillBarPlotData: () => void;
    fillScatterPlotData: () => void;
    fill3dScatterPlotData: () => void;
    fillParallelCoordinatesPlotData: () => void;
    draw: () => void;
    fillPoints: () => void;
    fillPointTooltip: (d: IChartsPointData) => string;
    showTooltip: (selectedPoint: IChartsPointData) => void;
    fillLegend: () => void;
    legendClick: (legendItem: IChartsLegendItem) => void;
    legendSetVisibilityForAll: (visibility: boolean) => void;
    addCharacteristic: () => void;
    deleteCharacteristic: (characteristic: IChartsCharacteristicSelection) => void;
}

/**
 * Controller for charts visualization
 */
class ChartsControllerHandler {
    /**
     * Creates a new instance of the controller
     * @param data Initial data for the controller
     */
    constructor(private data: any) {
        this.initializeController();
    }

    /**
     * Initializes the Angular controller
     */
    private initializeController(): void {
        const charts = ($scope: IChartsScope, $document: ng.IDocumentService): void => {
            "use strict";

            /**
             * Parses tabular data from text
             * @param text Text with tab-delimited data
             * @returns Array of parsed data rows or null if parsing fails
             */
            function parseTabularData(text: string): string[][] {
                //The array we will return
                let result: string[][] = [];
                try {
                    //Pasted data split into rows
                    let rows = text.split(/[\n\f\r]/);
                    // extracting first row that contains characteristics names
                    let characteristics = rows.shift()?.split("\t") || [];
                    // extracting sequence name column
                    $scope.sequencesName = characteristics.shift() || "";
                    let rawCharacteristics = rows.map(r => r.split("\t"));
                    $scope.characteristics = [];
                    for (let i = 0; i < rawCharacteristics.length; i++) {
                        $scope.characteristics[i] = {
                            Characteristics: [],
                            ResearchObjectName: rawCharacteristics[i][0]
                        };
                        for (let j = 1; j < rawCharacteristics[i].length; j++) {
                            $scope.characteristics[i].Characteristics[j - 1] = +rawCharacteristics[i][j].replace(",", ".");
                        }
                    }
                    $scope.characteristicsList = characteristics.map((c, i) => ({ Value: i, Text: c }));
                    $scope.characteristicNames = characteristics.map(c => c);
                    $scope.chartCharacteristics = [{
                        id: $scope.chartsCharacterisrticsCount++,
                        value: $scope.characteristicsList[0]
                    }];
                    $scope.fillLegend();

                    rows.forEach(thisRow => {
                        let row = thisRow.trim();
                        if (row) {
                            let cols = row.split("\t").map(c => c.replace(",", "."));
                            result.push(cols);
                        }
                    });
                }
                catch (err) {
                    console.log("error parsing as tabular data");
                    console.log(err);
                    return null;
                }
                return result;
            }

            /**
             * Fills legend data
             */
            function fillLegend(): void {
                $scope.legend = [];
                if ($scope.sequenceGroups) {
                    $scope.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.sequenceGroups.length]);
                    for (let j = 0; j < $scope.sequenceGroups.length; j++) {
                        const color = $scope.colorScale(j + 1);
                        $scope.legend.push({
                            id: parseInt($scope.sequenceGroups[j].Value),
                            name: $scope.sequenceGroups[j].Text,
                            visible: true,
                            color: color
                        });

                        // hack for the legend's dot color
                        document.styleSheets[0].insertRule(`.legend${$scope.sequenceGroups[j].Value}:after { background:${color} }`);
                    }
                } else {
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
            }

            /**
             * Constructs tooltip text content
             * @param d Point data to show in tooltip
             * @returns HTML string for tooltip
             */
            function fillPointTooltip(d: IChartsPointData): string {
                let tooltipContent: string[] = [];
                tooltipContent.push(`Name: ${d.name}`);

                let pointData = $scope.characteristics[d.id - 1].Characteristics;
                let pointsCharacteristics: string[] = [];
                for (let i = 0; i < pointData.length; i++) {
                    pointsCharacteristics.push(`${$scope.characteristicsList[i].Text}: ${pointData[i]}`);
                }

                tooltipContent.push(pointsCharacteristics.join("<br/>"));

                return tooltipContent.join("</br>");
            }

            /**
             * Shows tooltip for selected point
             * @param selectedPoint Point to show tooltip for
             */
            function showTooltip(selectedPoint: IChartsPointData): void {
                $("button[data-bs-target='#tooltip-tab-pane']").tab("show");

                $scope.tooltipVisible = true;
                $scope.tooltip = {
                    id: selectedPoint.id,
                    name: selectedPoint.name,
                    characteristics: selectedPoint.characteristics
                };
                let update: Record<string, any> = {};
                switch ($scope.chartCharacteristics.length) {
                    case 1:
                        break;
                    case 2:
                        update = {
                            "marker.symbol": $scope.points.map(point => point === selectedPoint ? "diamond-wide" : "circle"),
                            "marker.size": $scope.points.map(point => point === selectedPoint ? 15 : 6)
                        };
                        break;
                    case 3:
                        break;
                    default:
                }

                Plotly.restyle($scope.chartElement, update);
                $scope.$apply();
            }

            /**
             * Handles legend item click
             * @param legendItem Legend item that was clicked
             */
            function legendClick(legendItem: IChartsLegendItem): void {
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

            /**
             * Sets visibility for all legend items
             * @param visibility Whether items should be visible
             */
            function legendSetVisibilityForAll(visibility: boolean): void {
                if ($scope.chartData && $scope.chartData[0].customdata) {
                    let update = { visible: visibility ? true : "legendonly" };
                    $scope.legend.forEach(l => l.visible = visibility);
                    Plotly.restyle($scope.chartElement, update);
                }
            }

            /**
             * Add a characteristic to chart
             */
            $scope.addCharacteristic = (): void => {
                $scope.chartCharacteristics.push({
                    id: $scope.chartsCharacterisrticsCount++,
                    value: $scope.characteristicsList.find(cl =>
                        $scope.chartCharacteristics.every(cc => cc.value !== cl)) as IChartsCharacteristicListItem
                });
            };

            /**
             * Delete a characteristic from chart
             * @param characteristic Characteristic to delete
             */
            $scope.deleteCharacteristic = (characteristic: IChartsCharacteristicSelection): void => {
                $scope.chartCharacteristics.splice($scope.chartCharacteristics.indexOf(characteristic), 1);
            };

            /**
             * Handle text changes in data paste box
             */
            function textChanged(): void {
                let text = $("#dataPasteBox").val() as string;
                if (text) {
                    $scope.rawData = text;
                    let asArray = $scope.parseTabularData(text);
                    if (asArray) {
                        $scope.parsedData = asArray;
                        $scope.$apply();
                    }
                }
            }

            /**
             * Handle key down events for copy/paste
             * @param e Key event
             * @param args Additional arguments
             */
            function handleKeyDown(e: JQuery.KeyDownEvent, args: any): void {
                if (!$scope.inFocus && e.which === $scope.keyCodes.V && (e.ctrlKey || e.metaKey)) { // CTRL + V
                    //reset value of our box
                    $("#dataPasteBox").val("");
                    //set it in focus so that pasted text goes inside the box
                    $("#dataPasteBox").focus();
                }
            }

            $document.ready(() => {
                //Handles the Ctrl + V keys for pasting


                //If this is true, we wont respond to Ctrl + V
                $("body").on("focus", "input, textarea", () => { $scope.inFocus = true; });

                //We are not on a text element so we will respond
                //to Ctrl + V
                $("body").on("blur", "input, textarea", () => { $scope.inFocus = false; });

                //Handle the key down event
                $(document).keydown($scope.handleKeyDown);

                //We will respond to when the textbox value changes
                $("#dataPasteBox").bind("input propertychange", $scope.textChanged);
            });


            /**
             * Initialize data for chart visualization
             */
            function fillPoints(): void {
                $scope.points = [];

                for (let i = 0; i < $scope.characteristics.length; i++) {
                    let characteristic = $scope.characteristics[i];
                    const legendIndex = characteristic.SequenceGroupId
                        ? $scope.legend.findIndex(l => l.id === characteristic.SequenceGroupId)
                        : i;
                    $scope.points.push({
                        id: i + 1,
                        legendIndex: legendIndex,
                        legendId: characteristic.SequenceGroupId ? characteristic.SequenceGroupId : i + 1,
                        name: characteristic.ResearchObjectName,
                        characteristics: characteristic.Characteristics
                    });
                }
            }

            /**
             * Fill data for bar plot visualization
             */
            function fillBarPlotData(): void {
                let characteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
                let min = Math.min(...$scope.points.map(p => p.characteristics[characteristicIndex]));
                let max = Math.max(...$scope.points.map(p => p.characteristics[characteristicIndex]));
                let range = Math.abs(max - min);
                let maxNameLength = Math.max(...$scope.points.map(p => p.name.length));

                // adding margins
                min -= Math.abs(range * 0.05);
                max += Math.abs(range * 0.05);

                $scope.layout = {
                    margin: {
                        l: 50,
                        r: 20,
                        t: 10,
                        b: Math.min(150, maxNameLength * 10)
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

                $scope.chartData = $scope.points.map(p => ({
                    hoverinfo: "text+x+y",
                    x: [p.name],
                    y: [p.characteristics[characteristicIndex]],
                    marker: { color: $scope.legend[p.legendIndex].color },
                    type: "bar",
                    customdata: { legendId: p.legendId },
                    name: p.name,
                    visible: $scope.legend[p.legendIndex].visible ? "true" : "legendonly"
                }));
            }

            /**
             * Fill data for scatter plot visualization
             */
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

                $scope.chartData = $scope.points.map(p => ({
                    hoverinfo: "text+x+y",
                    type: "scattergl",
                    x: [p.characteristics[firstCharacteristicIndex]],
                    y: [p.characteristics[secondCharacteristicIndex]],
                    text: p.name,
                    mode: "markers",
                    marker: { opacity: 0.8, color: $scope.legend[p.legendIndex].color },
                    name: p.name,
                    customdata: { legendId: p.legendId },
                    visible: $scope.legend[p.legendIndex].visible
                }));
            }

            /**
             * Fill data for 3D scatter plot visualization
             */
            function fill3dScatterPlotData(): void {
                let firstCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
                let secondCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1].value);
                let thirdCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[2].value);

                $scope.chartData = $scope.points.map(p => ({
                    hoverinfo: "text+x+y+z",
                    x: [p.characteristics[firstCharacteristicIndex]],
                    y: [p.characteristics[secondCharacteristicIndex]],
                    z: [p.characteristics[thirdCharacteristicIndex]],
                    text: p.name,
                    mode: "markers",
                    marker: { opacity: 0.8, color: $scope.legend[p.legendIndex].color },
                    name: p.name,
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

            /**
             * Fill data for parallel coordinates plot visualization
             */
            function fillParallelCoordinatesPlotData(): void {
                let characteristicsIndices = $scope.chartCharacteristics.map(c =>
                    $scope.characteristicsList.indexOf(c.value));

                $scope.chartData = [{
                    type: "parcoords",
                    line: {
                        color: $scope.points.map(p => p.legendIndex),
                        colorscale: "Turbo"
                    },
                    dimensions: characteristicsIndices.map(ci => ({
                        label: $scope.characteristicNames[ci],
                        values: $scope.points.map(p => p.characteristics[ci])
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

            /**
             * Draw the appropriate chart based on selected characteristics
             */
            function draw(): void {
                $scope.fillPoints();

                switch ($scope.chartCharacteristics.length) {
                    case 1:
                        $scope.fillBarPlotData();
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

            // Assign functions to scope
            $scope.parseTabularData = parseTabularData;
            $scope.textChanged = textChanged;
            $scope.handleKeyDown = handleKeyDown;
            $scope.fillBarPlotData = fillBarPlotData;
            $scope.fillScatterPlotData = fillScatterPlotData;
            $scope.fill3dScatterPlotData = fill3dScatterPlotData;
            $scope.fillParallelCoordinatesPlotData = fillParallelCoordinatesPlotData;
            $scope.draw = draw;
            $scope.fillPoints = fillPoints;
            $scope.fillPointTooltip = fillPointTooltip;
            $scope.showTooltip = showTooltip;
            $scope.fillLegend = fillLegend;
            $scope.legendClick = legendClick;
            $scope.legendSetVisibilityForAll = legendSetVisibilityForAll;

            // Initialize scope variables
            $scope.chartsCharacterisrticsCount = 1;
            $scope.chartElement = document.getElementById("chart") as HTMLElement;
            $scope.keyCodes = {
                "C": 67,
                "V": 86
            };
            $scope.inFocus = false;
            $scope.rawData = "";
            $scope.parsedData = [];

            // Initialize with any provided data
            if (this.data) {
                MapModelFromJson($scope, this.data);
            }

            // Set up document events
            $document.ready(() => {
                // Handle focus events for input elements
                $("body").on("focus", "input, textarea", () => { $scope.inFocus = true; });
                $("body").on("blur", "input, textarea", () => { $scope.inFocus = false; });

                // Handle key down for copy-paste
                $(document).keydown($scope.handleKeyDown);

                // Handle paste box input changes
                $("#dataPasteBox").bind("input propertychange", $scope.textChanged);
            });
        };

        // Register controller
        angular.module("libiada").controller("ChartsCtrl", ["$scope", "$document", charts]);
    }
}

/**
 * Factory function for creating a charts controller
 * @param data Initial data for controller
 * @returns New instance of ChartsControllerHandler
 */
function ChartsController(data: any): ChartsControllerHandler {
    "use strict";
    return new ChartsControllerHandler(data);
}
