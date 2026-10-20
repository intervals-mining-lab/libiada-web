import { getArrayMinMax, throwHelper, InitializeBootstrapTooltips } from "functions";
function LocalCharacteristicsVisualizationsController($scope) {
    const ctrl = this;
    ctrl.$onInit = async () => {
        ctrl.fillLegend();
        ctrl.chartCharacteristics = [ctrl.characteristicsList[0]];
        ctrl.chartElement = document.getElementById("chart") ?? throwHelper("Chart div element not found");
        // initializing bootstrap tooltips for tabs
        InitializeBootstrapTooltips();
    };
    ctrl.fillLegend = async () => {
        ctrl.legend = [];
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
    };
    ctrl.addCharacteristic = async () => {
        // searching for the characteristic that is not already in the chartCharacteristics array 
        const newSelectedCharacteristic = ctrl.characteristicsList.find((cl) => ctrl.chartCharacteristics.every((cc) => cc !== cl));
        ctrl.chartCharacteristics.push(newSelectedCharacteristic);
    };
    ctrl.deleteCharacteristic = async (characteristic) => {
        ctrl.chartCharacteristics.splice(ctrl.chartCharacteristics.indexOf(characteristic), 1);
    };
    // initializes data for chart
    ctrl.fillPoints = async () => {
        ctrl.points = [];
        for (let i = 0; i < ctrl.characteristics.length; i++) {
            const characteristic = ctrl.characteristics[i];
            ctrl.points.push({
                legendIndex: i,
                researchObjectName: characteristic.ResearchObjectName,
                fragmentsData: []
            });
            for (let j = 0; j < characteristic.FragmentsData.length; j++) {
                const fragmentData = characteristic.FragmentsData[j];
                ctrl.points[i].fragmentsData.push({
                    rank: j,
                    name: fragmentData.Name,
                    characteristics: fragmentData.Characteristics
                });
            }
        }
    };
    ctrl.fillLinePlotData = async () => {
        const characteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[0]);
        const characteristicValues = ctrl.points
            .map((p) => p.fragmentsData
            .map((fd) => fd.characteristics[characteristicIndex]))
            .flat();
        let { min, max } = getArrayMinMax(characteristicValues);
        const range = max - min;
        // adding margins
        min -= range * 0.05;
        max += range * 0.05;
        let ranks = [];
        if (ctrl.lineChart) {
            for (let i = 0; i < ctrl.points.length; i++) {
                let y = ctrl.points[i].fragmentsData.map(sd => sd.characteristics[characteristicIndex]);
                y.sort((first, second) => second - first);
                ranks.push({
                    //x is range from 1 to fragmentsData length
                    x: Array.from({ length: y.length }, (_, i) => i + 1),
                    y: y
                });
            }
        }
        else {
            for (let i = 0; i < ctrl.points.length; i++) {
                ranks.push({
                    //ctrl.points[i].fragmentsData.map(sd => { return { x: sd.rank, y: sd.characteristics[characteristicIndex] } }) 
                    x: ctrl.points[i].fragmentsData.map((fd) => fd.rank),
                    y: ctrl.points[i].fragmentsData.map((fd) => fd.characteristics[characteristicIndex])
                });
            }
        }
        ;
        ctrl.layout = {
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
                    text: ctrl.characteristicNames[characteristicIndex],
                }
            }
        };
        ctrl.pointSize = 2;
        ctrl.chartData = ctrl.points.map((p, i) => ({
            hoverinfo: "x+y+text",
            x: ranks[i].x,
            y: ranks[i].y,
            marker: {
                color: ctrl.legend[p.legendIndex].color,
                size: ctrl.pointSize,
                opacity: 0.8
            },
            type: "scatter",
            line: {
                color: ctrl.legend[p.legendIndex].color,
                width: 0.75
            },
            mode: "lines+markers",
            customdata: [p.legendIndex],
            name: p.researchObjectName,
            visible: ctrl.legend[p.legendIndex].visible ? true : "legendonly"
        }));
    };
    ctrl.fillScatterPlotData = async () => {
        const firstCharacteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[0]);
        const secondCharacteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[1]);
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
        ctrl.pointSize = 3;
        ctrl.chartData = ctrl.points.map(p => ({
            hoverinfo: "x+y+text",
            type: "scattergl",
            x: p.fragmentsData.map(fd => fd.characteristics[firstCharacteristicIndex]),
            y: p.fragmentsData.map(fd => fd.characteristics[secondCharacteristicIndex]),
            text: p.researchObjectName,
            mode: "markers",
            marker: {
                opacity: 0.8,
                color: ctrl.legend[p.legendIndex].color,
                size: ctrl.pointSize
            },
            name: p.researchObjectName,
            customdata: [p.legendIndex],
            visible: ctrl.legend[p.legendIndex].visible
        }));
    };
    ctrl.fill3dScatterPlotData = async () => {
        const firstCharacteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[0]);
        const secondCharacteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[1]);
        const thirdCharacteristicIndex = ctrl.characteristicsList.indexOf(ctrl.chartCharacteristics[2]);
        ctrl.pointSize = 3;
        ctrl.chartData = ctrl.points.map(p => ({
            hoverinfo: "x+y+z+text",
            x: p.fragmentsData.map(fd => fd.characteristics[firstCharacteristicIndex]),
            y: p.fragmentsData.map(fd => fd.characteristics[secondCharacteristicIndex]),
            z: p.fragmentsData.map(fd => fd.characteristics[thirdCharacteristicIndex]),
            text: p.researchObjectName,
            mode: "markers",
            marker: {
                opacity: 0.8,
                color: ctrl.legend[p.legendIndex].color,
                size: ctrl.pointSize
            },
            name: p.researchObjectName,
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
        const characteristicsIndices = ctrl.chartCharacteristics.map((c) => ctrl.characteristicsList.indexOf(c));
        ctrl.chartData = [{
                type: "parcoords",
                //pad: [80, 80, 80, 80],
                line: {
                    color: ctrl.points.map((p) => p.fragmentsData.map(_ => p.legendIndex)).flat(),
                    //colorscale: "Turbo"
                },
                dimensions: characteristicsIndices.map((ci) => ({
                    label: ctrl.characteristicNames[ci],
                    values: ctrl.points.map((p) => p.fragmentsData.map((fd) => fd.characteristics[ci]))
                        .flat()
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
                ctrl.fillLinePlotData();
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
        ctrl.chartElement.on("plotly_click", (data) => {
            const selectedFragmentIndex = data.points[0].pointNumber;
            const selectedPoint = ctrl.points[data.points[0].curveNumber];
            ctrl.showTooltip(selectedPoint, selectedFragmentIndex);
        });
    };
    ctrl.showTooltip = async (selectedTrace, selectedFragmentIndex) => {
        $("button[data-bs-target='#tooltip-tab-pane']").tab("show");
        const selectedPoint = selectedTrace.fragmentsData[selectedFragmentIndex];
        ctrl.tooltipVisible = true;
        ctrl.tooltip = {
            name: selectedTrace.researchObjectName,
            fragmentName: selectedPoint.name,
            characteristics: selectedPoint.characteristics
        };
        let update = {};
        switch (ctrl.chartCharacteristics.length) {
            case 1:
            case 2:
                update = {
                    "marker.symbol": ctrl.points.map((t) => t.fragmentsData.map((p) => p === selectedPoint ? "diamond-wide" : "circle")),
                    "marker.size": ctrl.points.map((t) => t.fragmentsData.map((p) => p === selectedPoint ? 10 : ctrl.pointSize * 1.5))
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
    ctrl.legendClick = async (legendItem) => {
        // if points have data on legend item they belong to
        if (ctrl.chartData?.[0]?.customdata && ctrl.chartData[0].customdata.length > 0) {
            let index = [];
            const update = {
                visible: legendItem.visible ? "legendonly" : true,
                line: ctrl.chartCharacteristics.length == 1 ? {
                    color: legendItem.color,
                    width: 0.75
                } : {}
            };
            for (let i = 0; i < ctrl.chartData.length; i++) {
                const legendId = ctrl.legend[ctrl.chartData[i].customdata[0]].id;
                if (legendId === legendItem.id) {
                    index.push(i);
                }
            }
            Plotly.restyle(ctrl.chartElement, update, index);
        }
    };
    ctrl.legendSetVisibilityForAll = async (visibility) => {
        const update = { visible: visibility ? true : "legendonly" };
        ctrl.legend.forEach((l) => l.visible = visibility);
        Plotly.restyle(ctrl.chartElement, update);
    };
    // TODO: maybe extract this into separate component
    ctrl.dragbarMouseDown = async () => {
        const right = document.getElementById("sidebar") ?? throwHelper("Sidebar element not found");
        const bar = document.getElementById("dragbar") ?? throwHelper("Dragbar element not found");
        const drag = async (e) => {
            const selection = window.getSelection();
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
        bar.addEventListener("mouseup", () => {
            document.removeEventListener("mousemove", drag);
        });
    };
}
angular.module("libiada").component("localCharacteristicsVisualizations", {
    templateUrl: `/AngularTemplates/_LocalCharacteristicsVisualizations`,
    controller: ["$scope", LocalCharacteristicsVisualizationsController],
    bindings: {
        characteristicsList: "<",
        characteristicNames: "<",
        characteristics: "<"
    }
});
//# sourceMappingURL=localCharacteristicsVisualizations.js.map