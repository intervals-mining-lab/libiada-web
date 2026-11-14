/*import * as d3 from "d3";*/
import type * as plotly from "plotly.js";
import type {
    Characteristic,
    LegendItem,
    ParallelCoordinatesPlotData,
    SequenceCharacteristics,
    SequencesGroup
} from "viewDataTypes";
import { getArrayMinMax, arrayMax, throwHelper } from "functions";

interface Point {
    name: string;
    legendIndex: number;
    characteristics: number[];
}

interface Tooltip {
    name: string;
    characteristics: number[];
}

/**
 * Interface for the angular controller's scope
 */
interface ChartsScope extends ng.IScope {
    characteristicsList: Characteristic[];
    characteristicNames: string[];
    characteristics: SequenceCharacteristics[];
    sequenceGroups?: SequencesGroup[];
    chartCharacteristics: Characteristic[];
    legend: LegendItem[];
    points: Point[];
    chartElement: plotly.PlotlyHTMLElement;
    colorScale: d3.ScaleSequential<string>;
    tooltip: Tooltip;
    tooltipVisible: boolean;
    layout: Partial<plotly.Layout>;
    chartData: Partial<ParallelCoordinatesPlotData>[];
    sequencesColumnName: string;
    rawData: string;
    parsedData: string[][];
    inFocus: boolean;

    //const key codes
    keyCodes: { C: 67, V: 86 };

    fillLegend: () => void;
    addCharacteristic: () => void;
    deleteCharacteristic: (characteristic: Characteristic) => void;
    fillPoints: () => void;
    fillBarPlotData: () => void;
    fillScatterPlotData: () => void;
    fill3dScatterPlotData: () => void;
    fillParallelCoordinatesPlotData: () => void;
    draw: () => void;
    showTooltip: (selectedPoint: Point) => void;
    legendClick: (legendItem: LegendItem) => void;
    legendSetVisibilityForAll: (visibility: boolean) => void;
    parseTabularData: (text: string) => string[][] | null;
    textChanged: () => void;
    handleKeyDown: (e: JQuery.KeyDownEvent) => void;
}

/**
 * Controller for custom data charts visualization
 */
class ChartsControllerHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const charts = ($scope: ChartsScope): void => {
            /**
             * Fills chart legend data
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
             * Adds new characteristic to the list of selected for visualization
             */
            $scope.addCharacteristic = () =>
                $scope.chartCharacteristics.push($scope.characteristicsList.find(cl => $scope.chartCharacteristics.every(cc => cc !== cl))!);

            /**
             * Deletes selected characteristic from the list for visualization
             * @param characteristic characteristic to remove from the list
             */
            $scope.deleteCharacteristic = characteristic =>
                $scope.chartCharacteristics.splice($scope.chartCharacteristics.indexOf(characteristic), 1);

            /**
             * Initialize data for chart visualization
             */
            function fillPoints(): void {
                $scope.points = [];

                for (let i = 0; i < $scope.characteristics.length; i++) {
                    let characteristic: SequenceCharacteristics = $scope.characteristics[i];
                    const legendIndex: number = characteristic.SequenceGroupId
                        ? $scope.legend.findIndex(l => l.id === characteristic.SequenceGroupId)
                        : i;
                    $scope.points.push({
                        legendIndex: legendIndex,
                        name: characteristic.ResearchObjectName,
                        characteristics: characteristic.Characteristics
                    });
                }
            }

            /**
             * Fill data for bar plot visualization
             */
            function fillBarPlotData(): void {
                let characteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0]);
                let { min, max } = getArrayMinMax($scope.points.map(p => p.characteristics[characteristicIndex]));
                let range: number = max - min;
                let maxNameLength: number = arrayMax($scope.points.map(p => p.name.length));

                // adding margins
                min -= range * 0.05;
                max += range * 0.05;

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
                    hoverinfo: "x+y+text",
                    x: [p.name],
                    y: [p.characteristics[characteristicIndex]],
                    marker: { color: $scope.legend[p.legendIndex].color },
                    type: "bar",
                    customdata: [p.legendIndex],
                    name: p.name,
                    visible: $scope.legend[p.legendIndex].visible ? true : "legendonly"
                }));
            }

            /**
             * Fill data for scatter plot visualization
             */
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

                $scope.chartData = $scope.points.map(p => ({
                    hoverinfo: "x+y+text",
                    type: "scattergl",
                    x: [p.characteristics[firstCharacteristicIndex]],
                    y: [p.characteristics[secondCharacteristicIndex]],
                    text: p.name,
                    mode: "markers",
                    marker: {
                        opacity: 0.8,
                        color: $scope.legend[p.legendIndex].color
                    },
                    name: p.name,
                    customdata: [p.legendIndex],
                    visible: $scope.legend[p.legendIndex].visible
                }));
            };

            /**
             * Fill data for 3D scatter plot visualization
             */
            function fill3dScatterPlotData(): void {
                let firstCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0]);
                let secondCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1]);
                let thirdCharacteristicIndex: number = $scope.characteristicsList.indexOf($scope.chartCharacteristics[2]);

                $scope.chartData = $scope.points.map<Partial<plotly.PlotData>>(p => ({
                    hoverinfo: "x+y+z+text",
                    x: [p.characteristics[firstCharacteristicIndex]],
                    y: [p.characteristics[secondCharacteristicIndex]],
                    z: [p.characteristics[thirdCharacteristicIndex]],
                    text: p.name,
                    mode: "markers",
                    marker: {
                        opacity: 0.8,
                        color: $scope.legend[p.legendIndex].color
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
                            //type: $scope.plotTypeZ ? "log" : "",
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
                let characteristicsIndices: number[] = $scope.chartCharacteristics.map(c => $scope.characteristicsList.indexOf(c));

                $scope.chartData = [{
                    type: "parcoords",
                    line: {
                        color: $scope.points.map(p => p.legendIndex),
                        //colorscale: "Turbo"
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
            };

            /**
             * Draw the appropriate chart based on the number of selected characteristics
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

                $scope.chartElement.on("plotly_click", (data: plotly.PlotMouseEvent) => {
                    const selectedPoint: Point = $scope.points[data.points[0].curveNumber];
                    $scope.showTooltip(selectedPoint);
                });
            }

            /**
             * Shows tooltip for selected point
             * @param selectedPoint Point to show tooltip for
             */
            function showTooltip(selectedPoint: Point): void {
                $("button[data-bs-target='#tooltip-tab-pane']").tab("show");

                $scope.tooltipVisible = true;
                $scope.tooltip = {
                    name: selectedPoint.name,
                    characteristics: selectedPoint.characteristics
                };
                let update: plotly.Data = {};
                switch ($scope.chartCharacteristics.length) {
                    case 1:
                        break;
                    case 2:
                        update = {
                            "marker.symbol": $scope.points.map<string>(point => point === selectedPoint ? "diamond-wide" : "circle"),
                            "marker.size": $scope.points.map<number>(point => point === selectedPoint ? 15 : 6)
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
            function legendClick(legendItem: LegendItem): void {
                if ($scope.chartData?.[0]?.customdata && $scope.chartData?.[0]?.customdata?.length > 0) {
                    let index: number[] = [];
                    let update: plotly.Data = { visible: legendItem.visible ? "legendonly" : true };
                    const legendIndex: number = $scope.legend.findIndex((l: LegendItem) => l.id === legendItem.id);
                    for (let i = 0; i < $scope.chartData.length; i++) {
                        if ($scope.chartData[i].customdata![0] as number === legendIndex) {
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
                let update: plotly.Data = { visible: visibility ? true : "legendonly" };
                $scope.legend.forEach(l => l.visible = visibility);
                Plotly.restyle($scope.chartElement, update);
            }

            /**
             * Parses tabular data from text
             * @param text Text with tab-delimited data
             * @returns Array of parsed data rows or null if parsing fails
             */
            function parseTabularData(text: string): string[][] | null {
                //The array we will return
                let result: string[][] = [];
                try {
                    //Pasted data split into rows
                    let rows: string[] = text.split(/[\n\f\r]/);

                    // extracting first row that contains characteristics names
                    let row: string = rows.shift() ?? throwHelper("No data to parse");
                    let characteristics: string[] = row.split("\t");

                    // extracting sequence name column
                    $scope.sequencesColumnName = characteristics.shift() ?? throwHelper("No characteristics to parse");
                    let rawCharacteristics: string[][] = rows.map<string[]>(r => r.split("\t"));
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
                    $scope.characteristicsList = characteristics.map<Characteristic>((c, i) => ({
                        Value: i.toString(),
                        Text: c,
                        Selected: false,
                        Disabled: false
                     }));
                    $scope.characteristicNames = characteristics.map<string>(c => c);
                    $scope.chartCharacteristics = [$scope.characteristicsList[0]];
                    $scope.fillLegend();

                    rows.forEach(thisRow => {
                        let row: string = thisRow.trim();
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
             * Handle text changes in data paste box
             */
            function textChanged(): void {
                let text: string = $("#dataPasteBox").val() as string;
                if (text) {
                    $scope.rawData = text;
                    let parsedDataAsArray: string[][] | null = $scope.parseTabularData(text);
                    if (parsedDataAsArray) {
                        $scope.parsedData = parsedDataAsArray;
                        $scope.$apply();
                    }
                }
            }

            /**
             * Handle key down events for copy/paste
             * @param e Key event
             */
            function handleKeyDown(e: JQuery.KeyDownEvent): void {
                if (!$scope.inFocus && e.which === $scope.keyCodes.V && (e.ctrlKey || e.metaKey)) { // CTRL + V
                    //reset value of our box
                    $("#dataPasteBox").val("");
                    //set it in focus so that pasted text goes inside the box
                    $("#dataPasteBox").trigger("focus");
                }
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
            $scope.showTooltip = showTooltip;
            $scope.fillLegend = fillLegend;
            $scope.legendClick = legendClick;
            $scope.legendSetVisibilityForAll = legendSetVisibilityForAll;

            // Initialize scope variables
            $scope.chartElement = document.getElementById("chart") as plotly.PlotlyHTMLElement ?? throwHelper("Chart div element not found");
            $scope.keyCodes = {
                C: 67,
                V: 86
            };
            $scope.inFocus = false;
            $scope.rawData = "";
            $scope.parsedData = [];

            $(() => {
                //Handles the Ctrl + V keys for pasting

                //If this is true, we wont respond to Ctrl + V
                $("body").on("focus", "input, textarea", () => { $scope.inFocus = true; });

                //We are not on a text element so we will respond
                //to Ctrl + V
                $("body").on("blur", "input, textarea", () => { $scope.inFocus = false; });

                //Handle the key down event
                $(document).on("keydown", $scope.handleKeyDown);

                //We will respond to when the textbox value changes
                $("#dataPasteBox").on("input propertychange", $scope.textChanged);
            });
        };

        // Register controller
        angular.module("libiada").controller("ChartsCtrl", ["$scope", charts]);
    }
}

/**
 * Factory function for creating a charts controller
 * @param data Initial data for controller
 * @returns New instance of ChartsControllerHandler
 */
export default function ChartsController(): ChartsControllerHandler {
    return new ChartsControllerHandler();
}
