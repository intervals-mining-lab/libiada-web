import * as plotly from "plotly.js";
import type { Characteristic, Cluster,SequenceCharacteristics } from "viewDataTypes";
import { getArrayMinMax, arrayMax, throwHelper } from "functions";

interface Point {
    id: number;
    name: string;
    legendIndex: number;
    legendId: number;
    characteristics: number[];
}

interface LegendItem {
    id: number, name: string | number, visible: boolean, color: string
}

interface CharacteristicsVisualizationsComponentController extends ng.IController {
    characteristicsList: Characteristic[];
    characteristics: SequenceCharacteristics[];
    sequenceGroups?: Cluster[];
    legend: LegendItem[];
    chartCharacteristics: Characteristic[];
    points: Point[];
    chartElement: HTMLElement;
    colorScale: d3.ScaleSequential<string>;
    tooltip: { id: number, name: string, characteristics: number[] };
    tooltipVisible: boolean;
    selectedPointIndex: number;
    selectedResearchObjectIndex: number;
    layout: Partial<plotly.Layout>;
    chartData: Partial<plotly.PlotData>[];
    
    $onInit(): void; 
    fillLegend(): void;
    addCharacteristic(): void;
    deleteCharacteristic(characteristic: Characteristic): void;
    fillPoints(): void;
    fillPointTooltip(d: Point): string;
    showTooltip(selectedPoint: Point): void;
    fillBarPlotData(): void;
    fillScatterPlotData(): void;
    fill3dScatterPlotData(): void;
    fillParallelCoordinatesPlotData(): void;
    draw(): void;
}

function CharacteristicsVisualizationsController(this: CharacteristicsVisualizationsComponentController, $scope: ng.IScope,) {
    const ctrl: CharacteristicsVisualizationsComponentController = this;

    ctrl.$onInit = () => {
        ctrl.fillLegend();

        ctrl.chartCharacteristics = [ctrl.characteristicsList[0]];
        ctrl.chartElement = document.getElementById("chart") ?? throwHelper("Chart div element not found");
    };

    ctrl.fillLegend = async () => {
        ctrl.legend = [];
        if (ctrl.sequenceGroups) {
            ctrl.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, ctrl.sequenceGroups.length]);
            for (let j = 0; j < ctrl.sequenceGroups.length; j++) {
                const color = ctrl.colorScale(j + 1);
                ctrl.legend.push({
                    id: parseInt(ctrl.sequenceGroups[j].Value),
                    name: ctrl.sequenceGroups[j].Text,
                    visible: true,
                    color: color
                });

                // hack for the legend's dot color
                document.styleSheets[0].insertRule(`.legend${ctrl.sequenceGroups[j].Value}:after { background:${color} }`);
            }
        } else {
            ctrl.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, ctrl.characteristics.length]);
            for (let k = 0; k < ctrl.characteristics.length; k++) {
                const color = ctrl.colorScale(k + 1);
                ctrl.legend.push({
                    id: k + 1,
                    name: ctrl.characteristics[k].ResearchObjectName,
                    visible: true,
                    color: color
                });

                // hack for the legend's dot color
                document.styleSheets[0].insertRule(`.legend${k + 1}:after { background:${color} }`);
            }
        }
    }

    ctrl.addCharacteristic = () => ctrl.chartCharacteristics.push(ctrl.characteristicsList.find(cl => ctrl.chartCharacteristics.every(cc => cc !== cl))!);

    ctrl.deleteCharacteristic = characteristic => ctrl.chartCharacteristics.splice(ctrl.chartCharacteristics.indexOf(characteristic), 1);

    // initializes data for chart
    ctrl.fillPoints = async () => {
        ctrl.points = [];

        for (let i = 0; i < ctrl.characteristics.length; i++) {
            let characteristic = ctrl.characteristics[i];
            const legendIndex = characteristic.SequenceGroupId ? ctrl.legend.findIndex(l => l.id === characteristic.SequenceGroupId) : i;
            ctrl.points.push({
                id: i + 1,
                legendIndex: legendIndex,
                legendId: characteristic.SequenceGroupId ? characteristic.SequenceGroupId : i + 1,
                name: characteristic.ResearchObjectName,
                characteristics: characteristic.Characteristics
            });
        }
    }


    // constructs string representing tooltip text (inner html)
    ctrl.fillPointTooltip = (d: Point) => {
        let tooltipContent = [];
        tooltipContent.push(`Name: ${d.name}`);

        let pointData = ctrl.characteristics[d.id - 1].Characteristics;
        let pointsCharacteristics = [];
        for (let i = 0; i < pointData.length; i++) {
            pointsCharacteristics.push(`${ctrl.characteristicsList[i].Text}: ${pointData[i]}`);
        }

        tooltipContent.push(pointsCharacteristics.join("<br/>"));

        return tooltipContent.join("</br>");
    }

    // shows tooltip for dot or group of dots
    ctrl.showTooltip = async (selectedPoint: Point) => {
        $("button[data-bs-target='#tooltip-tab-pane']").tab("show");

        ctrl.tooltipVisible = true;
        ctrl.tooltip = {
            id: selectedPoint.id,
            name: selectedPoint.name,
            characteristics: selectedPoint.characteristics
        };
        let update = {};
        switch (ctrl.chartCharacteristics.length) {
            case 1:
                break;
            case 2:
                update = {
                    "marker.symbol": ctrl.points.map(point => point === selectedPoint ? "diamond-wide" : "circle"),
                    "marker.size": ctrl.points.map(point => point === selectedPoint ? 15 : 6)
                };
                break;
            case 3:
                break;
            default:
        }


        Plotly.restyle(ctrl.chartElement, update);

        $scope.$apply();
    }


    ctrl.fillBarPlotData = () => {
        let characteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[0]);
        let { min, max } = getArrayMinMax(ctrl.points.map(p => p.characteristics[characteristicIndex]));
        let range = max - min;
        let maxNameLength = arrayMax(ctrl.points.map(p => p.name.length));

        // adding margins
        min -= range * 0.05;
        max += range * 0.05;

        ctrl.layout = {
            margin: {
                l: 50,
                r: 20,
                t: 10,
                b: Math.min(150, maxNameLength * 10) // limiting maximum space taken by research objects' names

            },
            showlegend: false,
            xaxis: { categoryorder: "total ascending" },
            yaxis: {
                range: [min, max],
                title: {
                    text: ctrl.characteristicNames[characteristicIndex],
                }
            }
        };

        ctrl.chartData = ctrl.points.map<Partial<plotly.PlotData>>(p => ({
            hoverinfo: "x+y+text",
            x: [p.name],
            y: [p.characteristics[characteristicIndex]],
            marker: { color: ctrl.legend[p.legendIndex].color },
            type: "bar",
            customdata: [p.legendId],
            name: p.name,
            visible: ctrl.legend[p.legendIndex].visible ? true : "legendonly"
        }));
    }

    ctrl.fillScatterPlotData = async () => {
        let firstCharacteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[0]);
        let secondCharacteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[1]);

        ctrl.layout = {
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
                    text: ctrl.characteristicNames[firstCharacteristicIndex]
                }
            },
            yaxis: {
                //type: $scope.plotTypeY ? "log" : "",
                title: {
                    text: ctrl.characteristicNames[secondCharacteristicIndex]
                }
            }
        };

        ctrl.chartData = ctrl.points.map<Partial<plotly.PlotData>>(p => ({
            hoverinfo: "x+y+text",
            type: "scattergl",
            x: [p.characteristics[firstCharacteristicIndex]],
            y: [p.characteristics[secondCharacteristicIndex]],
            text: p.name,
            mode: "markers",
            marker: { opacity: 0.8, color: ctrl.legend[p.legendIndex].color },
            name: p.name,
            customdata: [p.legendId],
            visible: ctrl.legend[p.legendIndex].visible
        }));
    }

    ctrl.fill3dScatterPlotData = async () => {

        let firstCharacteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[0]);
        let secondCharacteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[1]);
        let thirdCharacteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[2]);

        ctrl.chartData = ctrl.points.map<Partial<plotly.PlotData>>(p => ({
            hoverinfo: "x+y+z+text",
            x: [p.characteristics[firstCharacteristicIndex]],
            y: [p.characteristics[secondCharacteristicIndex]],
            z: [p.characteristics[thirdCharacteristicIndex]],
            text: p.name,
            mode: "markers",
            marker: { opacity: 0.8, color: ctrl.legend[p.legendIndex].color },
            name: p.name,
            type: "scatter3d",
            customdata: [p.legendId],
            visible: ctrl.legend[p.legendIndex].visible
        }));

        ctrl.layout = {
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
                        text: ctrl.characteristicNames[firstCharacteristicIndex],
                        font: {
                            size: 10
                        }
                    },
                },
                yaxis: {
                    //type: $scope.plotTypeY ? "log" : "",
                    title: {
                        text: ctrl.characteristicNames[secondCharacteristicIndex],
                        font: {
                            size: 10
                        }
                    }
                },
                zaxis: {
                    //type: $scope.plotTypeY ? "log" : "",
                    title: {
                        text: ctrl.characteristicNames[thirdCharacteristicIndex],
                        font: {
                            size: 10
                        }
                    }
                }
            }
        };
    }

    ctrl.fillParallelCoordinatesPlotData = async () => {
        let characteristicsIndices = ctrl.chartCharacteristics.map(c => ctrl.characteristicsList.indexOf(c));

        ctrl.chartData = [{
            type: "parcoords",
            //pad: [80, 80, 80, 80],
            line: {
                color: ctrl.points.map(p => p.legendIndex),
                colorscale: "Turbo"
            },

            dimensions: characteristicsIndices.map(ci => ({
                label: ctrl.characteristicNames[ci],
                values: ctrl.points.map(p => p.characteristics[ci])
            }))
        }];

        ctrl.layout = {
            margin: {
                l: 50,
                r: 50,
                b: 20,
                t: 70
            },
            showlegend: false
        };
    }

    ctrl.draw = async () => {
        ctrl.fillPoints();

        switch (ctrl.chartCharacteristics.length) {
            case 1:
                ctrl.fillBarPlotData();
                break;
            case 2:
                ctrl.fillScatterPlotData();
                break;
            case 3:
                ctrl.fill3dScatterPlotData();
                break;
            default:
                ctrl.fillParallelCoordinatesPlotData();
        }

        Plotly.newPlot(ctrl.chartElement, ctrl.chartData, ctrl.layout, { responsive: true });

        ctrl.chartElement.on("plotly_click", data => {
            ctrl.selectedPointIndex = data.points[0].pointNumber;
            ctrl.selectedResearchObjectIndex = data.points[0].curveNumber;
            let selectedPoint = ctrl.points[data.points[0].curveNumber];
            ctrl.showTooltip(selectedPoint);
        });
    }

    ctrl.legendClick = async (legendItem: LegendItem) => {
        if (ctrl.chartData && ctrl.chartData[0].customdata && ctrl.chartData[0].customdata.length > 0) {
            let index = [];
            let update: plotly.Data = { visible: legendItem.visible ? "legendonly" : true };
            for (let i = 0; i < ctrl.chartData.length; i++) {
                if (ctrl.chartData[i].customdata![0] === legendItem.id) {
                    index.push(i);
                }
            }

            Plotly.restyle(ctrl.chartElement, update, index);
        }
    }

    ctrl.legendSetVisibilityForAll = async (visibility: boolean) => {
        if (ctrl.chartData && ctrl.chartData[0].customdata && ctrl.chartData[0].customdata.length > 0) {
            let update: plotly.Data = { visible: visibility ? true : "legendonly" };
            ctrl.legend.forEach(l => l.visible = visibility);
            Plotly.restyle(ctrl.chartElement, update);
        }
    }

    ctrl.dragbarMouseDown = async () => {
        let right = document.getElementById("sidebar");
        let bar: HTMLElement = document.getElementById("dragbar")!;

        const drag = (e: MouseEvent) => {
            document.selection ? document.selection.empty() : window.getSelection().removeAllRanges();
            ctrl.chartElement.style.width = `${e.pageX - bar.offsetWidth / 2}px`;

            Plotly.relayout(ctrl.chartElement, { autosize: true });
        };

        bar.addEventListener("mousedown", () => {
            document.addEventListener("mousemove", drag);
        });

        bar.addEventListener("mouseup", () => {
            document.removeEventListener("mousemove", drag);
        });
    }
}

angular.module("libiada").component("characteristicsVisualizations", {
    templateUrl: `/AngularTemplates/_CharacteristicsVisualizations`,
    controller: ["$scope",CharacteristicsVisualizationsController],
    bindings: {
        characteristicsList: "<",
        characteristicNames: "<",
        characteristics: "<",
        sequenceGroups: "<?"
    }
});
