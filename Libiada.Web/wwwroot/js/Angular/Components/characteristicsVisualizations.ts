import type * as plotly from "plotly.js";
import type {
    Characteristic,
    SequencesGroup,
    SequenceCharacteristics,
    LegendItem,
    ParallelCoordinatesPlotData
} from "viewDataTypes";
import {
    getArrayMinMax,
    arrayMax,
    throwHelper,
    InitializeBootstrapTooltips
} from "functions";

interface Point {
    name: string;
    legendIndex: number;
    characteristics: number[];
}

interface CharacteristicsVisualizationsComponentController extends ng.IController {
    characteristicsList: Characteristic[];
    characteristicNames: string[];
    characteristics: SequenceCharacteristics[];
    sequenceGroups?: SequencesGroup[];
    chartCharacteristics: Characteristic[];
    legend: LegendItem[];
    points: Point[];
    chartElement: plotly.PlotlyHTMLElement;
    colorScale: d3.ScaleSequential<string>;
    tooltip: { name: string, characteristics: number[] };
    tooltipVisible: boolean;
    layout: Partial<plotly.Layout>;
    chartData: Partial<ParallelCoordinatesPlotData>[];

    $onInit: () => Promise<void>;
    fillLegend: () => Promise<void>;
    addCharacteristic: () => Promise<void>;
    deleteCharacteristic: (characteristic: Characteristic) => Promise<void>;
    fillPoints: () => Promise<void>;
    fillBarPlotData: () => Promise<void>;
    fillScatterPlotData: () => Promise<void>;
    fill3dScatterPlotData: () => Promise<void>;
    fillParallelCoordinatesPlotData: () => Promise<void>;
    draw: () => Promise<void>;
    showTooltip: (selectedPoint: Point) => Promise<void>;
    legendClick: (legendItem: LegendItem) => Promise<void>;
    legendSetVisibilityForAll: (visibility: boolean) => Promise<void>;
    dragbarMouseDown: () => Promise<void>;
}

function CharacteristicsVisualizationsController(this: CharacteristicsVisualizationsComponentController, $scope: ng.IScope) {
    const ctrl: CharacteristicsVisualizationsComponentController = this;

    ctrl.$onInit = async () => {
        ctrl.fillLegend();

        ctrl.chartCharacteristics = [ctrl.characteristicsList[0]];
        ctrl.chartElement = document.getElementById("chart") as plotly.PlotlyHTMLElement ?? throwHelper("Chart div element not found");

        // initializing tooltips for tabs
        InitializeBootstrapTooltips();
    };

    ctrl.fillLegend = async () => {
        ctrl.legend = [];
        if (ctrl.sequenceGroups) {
            ctrl.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, ctrl.sequenceGroups.length]);
            for (let j = 0; j < ctrl.sequenceGroups.length; j++) {
                const color: string = ctrl.colorScale(j + 1);
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
                const color: string = ctrl.colorScale(k + 1);
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
    };

    ctrl.addCharacteristic = async () => {
        // searching for the characteristic that is not already in the chartCharacteristics array 
        const newSelectedCharacteristic: Characteristic = ctrl.characteristicsList.find(
            (cl: Characteristic) => ctrl.chartCharacteristics.every((cc: Characteristic) => cc !== cl))!
        ctrl.chartCharacteristics.push(newSelectedCharacteristic);
    };


    ctrl.deleteCharacteristic = async (characteristic: Characteristic) => {
        ctrl.chartCharacteristics.splice(ctrl.chartCharacteristics.indexOf(characteristic), 1);
    };

    // initializes data for chart
    ctrl.fillPoints = async () => {
        ctrl.points = [];

        for (let i = 0; i < ctrl.characteristics.length; i++) {
            const characteristic: SequenceCharacteristics = ctrl.characteristics[i];
            const legendIndex: number = characteristic.SequenceGroupId ? ctrl.legend.findIndex(l => l.id === characteristic.SequenceGroupId) : i;
            ctrl.points.push({
                legendIndex: legendIndex,
                name: characteristic.ResearchObjectName,
                characteristics: characteristic.Characteristics
            });
        }
    };

    ctrl.fillBarPlotData = async () => {
        const characteristicIndex: number = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[0]);
        let { min, max }: { min: number; max: number } = getArrayMinMax(ctrl.points.map<number>(p => p.characteristics[characteristicIndex]));
        const range: number = max - min;
        const maxNameLength: number = arrayMax(ctrl.points.map(p => p.name.length));

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

        ctrl.chartData = ctrl.points.map<Partial<plotly.PlotData>>((p: Point) => ({
            hoverinfo: "x+y+text",
            x: [p.name],
            y: [p.characteristics[characteristicIndex]],
            marker: { color: ctrl.legend[p.legendIndex].color },
            type: "bar",
            customdata: [p.legendIndex],
            name: p.name,
            visible: ctrl.legend[p.legendIndex].visible ? true : "legendonly"
        }));
    };

    ctrl.fillScatterPlotData = async () => {
        const firstCharacteristicIndex: number = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[0]);
        const secondCharacteristicIndex: number = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[1]);

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
                //type: ctrl.plotTypeX ? "log" : "",
                title: {
                    text: ctrl.characteristicNames[firstCharacteristicIndex]
                }
            },
            yaxis: {
                //type: ctrl.plotTypeY ? "log" : "",
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
            marker: {
                opacity: 0.8,
                color: ctrl.legend[p.legendIndex].color
            },
            name: p.name,
            customdata: [p.legendIndex],
            visible: ctrl.legend[p.legendIndex].visible
        }));
    };

    ctrl.fill3dScatterPlotData = async () => {
        const firstCharacteristicIndex: number = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[0]);
        const secondCharacteristicIndex: number = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[1]);
        const thirdCharacteristicIndex: number = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[2]);

        ctrl.chartData = ctrl.points.map<Partial<plotly.PlotData>>(p => ({
            hoverinfo: "x+y+z+text",
            x: [p.characteristics[firstCharacteristicIndex]],
            y: [p.characteristics[secondCharacteristicIndex]],
            z: [p.characteristics[thirdCharacteristicIndex]],
            text: p.name,
            mode: "markers",
            marker: {
                opacity: 0.8,
                color: ctrl.legend[p.legendIndex].color
            },
            name: p.name,
            type: "scatter3d",
            customdata: [p.legendIndex],
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
                    //type: ctrl.plotTypeX ? "log" : "",
                    title: {
                        text: ctrl.characteristicNames[firstCharacteristicIndex],
                        font: {
                            size: 10
                        }
                    },
                },
                yaxis: {
                    //type: ctrl.plotTypeY ? "log" : "",
                    title: {
                        text: ctrl.characteristicNames[secondCharacteristicIndex],
                        font: {
                            size: 10
                        }
                    }
                },
                zaxis: {
                    //type: ctrl.plotTypeZ ? "log" : "",
                    title: {
                        text: ctrl.characteristicNames[thirdCharacteristicIndex],
                        font: {
                            size: 10
                        }
                    }
                }
            }
        };
    };

    ctrl.fillParallelCoordinatesPlotData = async () => {
        const characteristicsIndices: number[] = ctrl.chartCharacteristics.map<number>((c: Characteristic) => ctrl.characteristicsList.indexOf(c));

        ctrl.chartData = [{
            type: "parcoords",
            //pad: [80, 80, 80, 80],
            line: {
                color: ctrl.points.map<number>((p: Point) => p.legendIndex),
                //colorscale: "Turbo"
            },
            //customdata: ctrl.points.map<number>((p: Point) => p.legendIndex),
            dimensions: characteristicsIndices.map((ci: number) => ({
                label: ctrl.characteristicNames[ci],
                values: ctrl.points.map<number>((p: Point) => p.characteristics[ci])
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
    };

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

        ctrl.chartElement.on("plotly_click", (data: plotly.PlotMouseEvent) => {
            const selectedPoint: Point = ctrl.points[data.points[0].curveNumber];
            ctrl.showTooltip(selectedPoint);
        });
    };

    ctrl.showTooltip = async (selectedPoint: Point) => {
        $("button[data-bs-target='#tooltip-tab-pane']").tab("show");

        ctrl.tooltipVisible = true;
        ctrl.tooltip = {
            name: selectedPoint.name,
            characteristics: selectedPoint.characteristics
        };
        let update: plotly.Data = {};
        switch (ctrl.chartCharacteristics.length) {
            case 1:
                break;
            case 2:
                update = {
                    "marker.symbol": ctrl.points.map<string>(point => point === selectedPoint ? "diamond-wide" : "circle"),
                    "marker.size": ctrl.points.map<number>(point => point === selectedPoint ? 15 : 6)
                };
                break;
            case 3:
                break;
            default:
        }

        Plotly.restyle(ctrl.chartElement, update);

        // method called outside of angular's digest cycle, so we need to call $apply() manually
        $scope.$apply();
    };

    ctrl.legendClick = async (legendItem: LegendItem) => {
        // if points have data on legend item they belong to
        if (ctrl.chartData?.[0]?.customdata && ctrl.chartData[0].customdata.length > 0) {
            let index: number[] = [];
            const update: plotly.Data = { visible: legendItem.visible ? "legendonly" : true };
            for (let i = 0; i < ctrl.chartData.length; i++) {
                const legendId: number = ctrl.legend[ctrl.chartData[i].customdata![0] as number].id;
                if (legendId === legendItem.id) {
                    index.push(i);
                }
            }

            Plotly.restyle(ctrl.chartElement, update, index);
        }
    };

    ctrl.legendSetVisibilityForAll = async (visibility: boolean) => {
        const update: plotly.Data = { visible: visibility ? true : "legendonly" };
        ctrl.legend.forEach((l: LegendItem) => l.visible = visibility);
        Plotly.restyle(ctrl.chartElement, update);
    };

    // TODO: maybe extract this into separate component
    ctrl.dragbarMouseDown = async () => {
        const right: HTMLElement = document.getElementById("sidebar") ?? throwHelper("Sidebar element not found");
        const bar: HTMLElement = document.getElementById("dragbar") ?? throwHelper("Dragbar element not found");

        const drag = async (e: MouseEvent) => {
            const selection: Selection | null = window.getSelection();
            if (selection) {
                selection.empty();
                selection.removeAllRanges();
            }

            ctrl.chartElement.style.width = `${e.pageX - bar.offsetWidth / 2}px`;
            if (ctrl.chartElement.childNodes.length > 0) {
                Plotly.relayout(ctrl.chartElement, { autosize: true });
            }
        };

        bar.addEventListener("mousedown", () => {
            document.addEventListener("mousemove", drag);
        });

        window.addEventListener("mouseup", () => {
            document.removeEventListener("mousemove", drag);
        });
    };
}

angular.module("libiada").component("characteristicsVisualizations", {
    templateUrl: `/AngularTemplates/_CharacteristicsVisualizations`,
    controller: ["$scope", CharacteristicsVisualizationsController],
    bindings: {
        characteristicsList: "<",
        characteristicNames: "<",
        characteristics: "<",
        sequenceGroups: "<?"
    }
});
