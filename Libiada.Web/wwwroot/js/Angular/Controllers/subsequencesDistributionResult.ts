import { InitializeBootstrapTooltips, initScopeFromServer, throwHelper } from "functions";
import type * as plotly from "plotly.js";
import type {
    Characteristic,
    Feature,
    ResultsLoadingScreenData,
    PointsSimilarity,
    CharacteristicComparer,
    ColorClass,
    SubsequenceTooltip,
    SubsequenceData,
    LegendItem,
    SequenceData,
    AttributeValue
} from "viewDataTypes";

interface ClustalResponse {
    Status: string;
    Result: string;
    Message?: string
}

interface DistributionPoint {
    id: number;
    researchObjectId: number;
    sequenceRemoteId: string | undefined;
    attributes: number[];
    partial: boolean;
    featureId: number;
    positions: number[];
    lengths: number[];
    subsequenceRemoteId?: string | undefined;
    numericX: number;
    x: number;
    subsequenceCharacteristics: number[];
    featureVisible: boolean;
    legendVisible: boolean;
    filtersVisible: boolean[];
    visible?: boolean;
}

interface SubsequencesDistributionResultData {
    attributeValues: AttributeValue[];
    attributes: { [key: number]: string };
    features: { [key: number]: Feature };
    result: SequenceData[];
    subsequencesCharacteristicsNames: string[];
    subsequencesCharacteristicsList: Characteristic[];
    sequenceCharacteristicName: string;
}

interface SubsequencesDistributionResultScope extends ng.IScope, SubsequencesDistributionResultData, ResultsLoadingScreenData {
    dotRadius: number;
    selectedDotRadius: number;
    points: DistributionPoint[][];
    visiblePoints: DistributionPoint[][];
    researchObjects: LegendItem[];
    legend: LegendItem[];
    characteristicComparers: CharacteristicComparer[];
    plotTypeX: boolean;
    plotTypeY: boolean;
    tooltipVisible: boolean;
    tooltipElements: SubsequenceTooltip[];
    alignmentInProcess: boolean;
    pointsSimilarity: { same: 0; similar: 1; different: 2 };
    chartElement: plotly.PlotlyHTMLElement;
    subsequenceCharacteristic: Characteristic;
    layout: Partial<plotly.Layout>;
    numericXAxis: boolean;
    selectedPointIndex: number;
    selectedResearchObjectIndex: number;
    highlight: boolean;

    onInit: () => void;
    pointsComparer: (first: DistributionPoint, second: DistributionPoint) => number
    fillPoints: () => void;
    addCharacteristicComparer: () => void;
    deleteCharacteristicComparer: (characteristicComparer: CharacteristicComparer) => void;
    fillVisiblePoints: () => void;
    getAttributeIdByName: (point: DistributionPoint, attributeName: string) => number;
    isAttributeEqual: (point: DistributionPoint, attributeName: string, expectedValue: string) => boolean;
    addFilter: (newFilter: string) => void;
    deleteFilter: (filter: string, filterIndex: number) => void;
    filterByFeature: (feature: Characteristic) => void;
    dotVisible: (point: DistributionPoint) => boolean;
    dotsSimilar: (first: DistributionPoint, second: DistributionPoint) => PointsSimilarity;
    getAttributesText: (attributes: number[]) => string[];
    showTooltip: (selectedPoint: DistributionPoint) => void;
    fillPointTooltip: (point: DistributionPoint, researchObjectName: string, similarity: PointsSimilarity) => SubsequenceTooltip;
    keyUpDownPress: (key: "ArrowUp" | "ArrowDown") => void;
    redrawGenesMap: (sortPoints?: boolean) => void;
    legendClick: (legendItem: LegendItem) => void;
    legendSetVisibilityForAll: (visibility: boolean) => void;
    dragBarMouseDown: () => void;
    alignAllWithClustal: () => void;
    alignSimilarWithClustal: () => void;
    alignSelectedWithClustal: () => void;
    alignWithClustal: (subsequencesIds: number[]) => void;
    colorScale: (index: number) => string;
}

export default function SubsequencesDistributionResultController() {
    function subsequencesDistributionResult($scope: SubsequencesDistributionResultScope, $http: ng.IHttpService): void {

        async function onInit(): Promise<void> {
            $scope.dotRadius = 4;
            $scope.selectedDotRadius = $scope.dotRadius * 3;

            $scope.points = [];
            $scope.visiblePoints = [];
            $scope.researchObjects = [];
            $scope.legend = [];
            $scope.characteristicComparers = [];
            //$scope.plotTypeX = "";
            //$scope.plotTypeY = "";
            $scope.tooltipVisible = false;
            $scope.tooltipElements = [];
            $scope.alignmentInProcess = false;
            $scope.pointsSimilarity = Object.freeze({ "same": 0, "similar": 1, "different": 2 });

            InitializeBootstrapTooltips();

            // preventing scroll in key up and key down
            window.addEventListener("keydown", e => {
                if (e.key === "ArrowDown" || e.key === "ArrowUp") {
                    e.preventDefault();
                    if (!e.repeat) {
                        $scope.keyUpDownPress(e.key);
                    }
                }
            }, false);

            await initScopeFromServer<SubsequencesDistributionResultData>(
                $http, $scope, "Loading genes map data", "Failed loading genes map data");

            $scope.chartElement = document.getElementById("chart") as plotly.PlotlyHTMLElement
                ?? throwHelper("Chart div element not found");
            $scope.subsequenceCharacteristic = $scope.subsequencesCharacteristicsList[0];

            $scope.fillPoints();

            $scope.addCharacteristicComparer();

            $scope.redrawGenesMap(true);

            $scope.$apply();
        }

        $scope.pointsComparer = (first: DistributionPoint, second: DistributionPoint): number => {
            const characteristicIndex: number = parseInt($scope.subsequenceCharacteristic.Value);
            return first.subsequenceCharacteristics[characteristicIndex] - second.subsequenceCharacteristics[characteristicIndex];
        }

        // initializes data for genes map
        function fillPoints(): void {
            for (let i = 0; i < $scope.result.length; i++) {
                let sequenceData: SequenceData = $scope.result[i];
                $scope.researchObjects.push({
                    id: sequenceData.ResearchObjectId,
                    name: sequenceData.ResearchObjectName,
                    visible: true,
                    color: $scope.colorScale(i)
                });
                // hack for legend dot color
                document.styleSheets[0].insertRule(`.legend${sequenceData.ResearchObjectId}:after { background:${$scope.colorScale(i)} }`);
                $scope.points.push([]);
                $scope.visiblePoints.push([]);
                for (let j = 0; j < sequenceData.SubsequencesData.length; j++) {
                    let subsequenceData: SubsequenceData = sequenceData.SubsequencesData[j];

                    let point: DistributionPoint = {
                        id: subsequenceData.Id,
                        researchObjectId: sequenceData.ResearchObjectId,
                        sequenceRemoteId: sequenceData.RemoteId,
                        attributes: subsequenceData.Attributes,
                        partial: subsequenceData.Partial,
                        featureId: subsequenceData.FeatureId,
                        positions: subsequenceData.Starts,
                        lengths: subsequenceData.Lengths,
                        subsequenceRemoteId: subsequenceData.RemoteId,
                        numericX: i + 1,
                        x: sequenceData.Characteristic,
                        subsequenceCharacteristics: subsequenceData.CharacteristicsValues,
                        featureVisible: true,
                        legendVisible: true,
                        filtersVisible: [],
                    };
                    $scope.points[i].push(point);
                    $scope.visiblePoints[i].push(point);
                }
            }
        }

        // adds new characteristics value based filter
        function addCharacteristicComparer(): void {
            $scope.characteristicComparers.push({ characteristic: $scope.subsequencesCharacteristicsList[0], precision: 0 });
        }

        // deletes given characteristics filter
        function deleteCharacteristicComparer(characteristicComparer: CharacteristicComparer): void {
            $scope.characteristicComparers.splice($scope.characteristicComparers.indexOf(characteristicComparer), 1);
        }

        // fills array of currently visible points
        function fillVisiblePoints(): void {
            $scope.visiblePoints = [];
            for (let i = 0; i < $scope.points.length; i++) {
                $scope.visiblePoints[i] = [];
                for (let j = 0; j < $scope.points[i].length; j++) {
                    if ($scope.dotVisible($scope.points[i][j])) {
                        $scope.visiblePoints[i].push($scope.points[i][j]);
                    }
                }
            }
        }

        // returns attribute index by its name if any
        function getAttributeIdByName(dot: DistributionPoint, attributeName: string): number {
            return dot.attributes.find(a => $scope.attributes[$scope.attributeValues[a].attribute] === attributeName) ?? throwHelper(`Attribute with name ${attributeName} not found.`);
        }

        // returns true if dot has given attribute and its value equal to the given value
        function isAttributeEqual(dot: DistributionPoint, attributeName: string, expectedValue: string): boolean {
            let attributeId: number = $scope.getAttributeIdByName(dot, attributeName);
            if (attributeId) {
                let product: string = $scope.attributeValues[attributeId].value.toUpperCase();
                return product.indexOf(expectedValue) !== -1;
            }

            return false;
        }

        // adds and applies new filter
        function addFilter(newFilter: string): void {
            let filterValue: string = newFilter.toUpperCase();

            for (let i = 0; i < $scope.points.length; i++) {
                for (let j = 0; j < $scope.points[i].length; j++) {
                    let point: DistributionPoint = $scope.points[i][j];
                    let visible: boolean = $scope.isAttributeEqual(point, "product", filterValue);
                    visible = visible || $scope.isAttributeEqual(point, "locus_tag", filterValue);
                    point.filtersVisible.push(visible);
                    point.visible = $scope.dotVisible(point);
                }
            }

            $scope.redrawGenesMap(false);
        }

        // deletes given filter
        function deleteFilter(_filter: string, filterIndex: number): void {
            for (let i = 0; i < $scope.points.length; i++) {
                for (let j = 0; j < $scope.points[i].length; j++) {
                    let point = $scope.points[i][j];

                    point.filtersVisible.splice(filterIndex, 1);
                    point.visible = $scope.dotVisible(point);
                }
            }

            $scope.redrawGenesMap(false);
        }

        // filters dots by subsequences feature
        function filterByFeature(feature: Characteristic): void {
            for (let i = 0; i < $scope.points.length; i++) {
                for (let j = 0; j < $scope.points[i].length; j++) {
                    let point: DistributionPoint = $scope.points[i][j];

                    if (point.featureId === parseInt(feature.Value)) {
                        point.featureVisible = feature.Selected;
                        point.visible = $scope.dotVisible(point);
                    }
                }
            }

            $scope.redrawGenesMap(false);
        }

        // checks if dot is visible
        function dotVisible(dot: DistributionPoint): boolean {
            let filterVisible: boolean = dot.filtersVisible.length === 0 || dot.filtersVisible.some(fv => fv);
            return dot.legendVisible && dot.featureVisible && filterVisible;
        }

        // determines if dots are similar by product
        function dotsSimilar(d: DistributionPoint, dot: DistributionPoint): PointsSimilarity {
            if (d.featureId !== dot.featureId) {
                return $scope.pointsSimilarity.different;
            }

            switch (d.featureId) {
                case 1: // CDS
                case 2: // RRNA
                case 3: // TRNA
                    const firstProductId: number = $scope.getAttributeIdByName(d, "product");
                    const secondProductId: number = $scope.getAttributeIdByName(dot, "product");
                    const firstProduct: string = $scope.attributeValues[firstProductId].value.toUpperCase();
                    const secondProduct: string = $scope.attributeValues[secondProductId].value.toUpperCase();
                    if (firstProduct !== secondProduct) {
                        return $scope.pointsSimilarity.different;
                    }
                    break;
            }

            return $scope.pointsSimilarity.similar;
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

        // shows tooltip for dot or group of dots
        function showTooltip(selectedPoint: DistributionPoint): void {
            $("button[data-bs-target='#tooltip-tab-pane']").tab("show");

            $scope.tooltipVisible = true;
            $scope.tooltipElements.length = 0;
            let researchObjectName: string = $scope.researchObjects.find(ro => ro.id === selectedPoint.researchObjectId)!.name;
            $scope.tooltipElements.push(fillPointTooltip(selectedPoint, researchObjectName, $scope.pointsSimilarity.same));
            let similarPoints: DistributionPoint[] = [];

            for (let i = 0; i < $scope.visiblePoints.length; i++) {
                for (let j = 0; j < $scope.visiblePoints[i].length; j++) {
                    if (selectedPoint !== $scope.visiblePoints[i][j] && $scope.highlight) {
                        let similar: boolean = $scope.characteristicComparers.every(filter => {
                            let characteristicIndex: number = parseInt(filter.characteristic.Value);
                            let selectedPointValue: number = selectedPoint.subsequenceCharacteristics[characteristicIndex];
                            let anotherPointValue: number = $scope.visiblePoints[i][j].subsequenceCharacteristics[characteristicIndex];
                            return Math.abs(selectedPointValue - anotherPointValue) <= filter.precision;
                        });

                        if (similar) {
                            let point: DistributionPoint = $scope.visiblePoints[i][j];
                            similarPoints.push(point);
                            researchObjectName = $scope.researchObjects[i].name;
                            $scope.tooltipElements.push(fillPointTooltip(point, researchObjectName, $scope.dotsSimilar(point, selectedPoint)));
                        }
                    }
                }
            }

            // connecting all similar dots with lines
            //                svg.append("line")
            //                    .attr("class", "similar-line")
            //                    .attr("x1", $scope.xMap(point))
            //                    .attr("y1", $scope.yMap(point))
            //                    .attr("x2", $scope.xMap(dot))
            //                    .attr("y2", $scope.yMap(dot))
            //                    .attr("stroke-width", 1)
            //                    .attr("opacity", 0.4)
            //                    .attr("stroke", $scope.colorMap($scope.cValue(point)));
            //                return true;
            //            }
            //        }

            //        return false;
            //    })
            //    .attr("rx", $scope.selectedDotRadius);
            //tooltip.lines = svg.selectAll(".similar-line");

            let update: Plotly.Data = {
                "marker.symbol": $scope.visiblePoints.map(points => points.map(point => point === selectedPoint || similarPoints.includes(point) ? "diamond-wide" : "circle-open")),
                "marker.size": $scope.visiblePoints.map(points => points.map(point => point === selectedPoint || similarPoints.includes(point) ? 15 : 6))
            };

            Plotly.restyle($scope.chartElement, update);

            $scope.$apply();
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(point: DistributionPoint, researchObjectName: string, similarity: PointsSimilarity): SubsequenceTooltip {
            let color: ColorClass = similarity === $scope.pointsSimilarity.same ? "default"
                : similarity === $scope.pointsSimilarity.similar ? "success"
                    : similarity === $scope.pointsSimilarity.different ? "danger" : "danger";

            let tooltipElement: SubsequenceTooltip = {
                id: point.id,
                name: researchObjectName,
                sequenceRemoteId: point.sequenceRemoteId,
                feature: $scope.features[point.featureId].Text,
                attributes: $scope.getAttributesText(point.attributes),
                partial: point.partial,
                color: color,
                characteristics: point.subsequenceCharacteristics,
                similarity: similarity,
                selectedForAlignment: false,
                position: "(",
                length: 0,
                positions: point.positions,
                lengths: point.lengths
            };

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

            if (point.subsequenceRemoteId) {
                tooltipElement.remoteId = point.subsequenceRemoteId;
            }

            return tooltipElement;
        }

        function cText(points: DistributionPoint[], index: number): string[] {
            return points.map(() => $scope.researchObjects[index].name);
        }

        // selects nearest different point of the same organism when "up" or "down" key pressed
        function keyUpDownPress(key: "ArrowUp" | "ArrowDown"): void {
            let nextPointIndex: number = -1;
            let visibleResearchObjectsPoints: DistributionPoint[] = $scope.visiblePoints[$scope.selectedResearchObjectIndex];

            if ($scope.selectedPointIndex >= 0) {
                let characteristic: number = parseInt($scope.subsequenceCharacteristic.Value);
                let firstPointCharacteristic: number;
                let secondPointCharacteristic: number;

                switch (key) {
                    case "ArrowUp":
                        for (let i = $scope.selectedPointIndex + 1; i < visibleResearchObjectsPoints.length; i++) {
                            firstPointCharacteristic = visibleResearchObjectsPoints[$scope.selectedPointIndex].subsequenceCharacteristics[characteristic];
                            secondPointCharacteristic = visibleResearchObjectsPoints[i].subsequenceCharacteristics[characteristic];

                            if (firstPointCharacteristic !== secondPointCharacteristic) {
                                nextPointIndex = i;
                                break;
                            }
                        }
                        break;
                    case "ArrowDown":
                        for (let j = $scope.selectedPointIndex - 1; j >= 0; j--) {
                            firstPointCharacteristic = visibleResearchObjectsPoints[$scope.selectedPointIndex].subsequenceCharacteristics[characteristic];
                            secondPointCharacteristic = visibleResearchObjectsPoints[j].subsequenceCharacteristics[characteristic];

                            if (firstPointCharacteristic !== secondPointCharacteristic) {
                                nextPointIndex = j;
                                break;
                            }
                        }
                        break;
                }
            }

            if (nextPointIndex >= 0) {
                $scope.showTooltip(visibleResearchObjectsPoints[nextPointIndex]);
                $scope.selectedPointIndex = nextPointIndex;
            }
        }

        // main drawing method
        function redrawGenesMap(sortPoints: boolean = false): void {
            $scope.fillVisiblePoints();
            $scope.selectedPointIndex = -1;

            if (sortPoints) {
                $scope.points = $scope.points.map(points => points.sort($scope.pointsComparer));
                $scope.visiblePoints = $scope.visiblePoints.map(points => points.sort($scope.pointsComparer));
            }

            $scope.layout = {
                showlegend: false,
                hovermode: "closest",
                xaxis: {
                    type: $scope.plotTypeX ? "log" : "linear",
                    title: {
                        text: $scope.sequenceCharacteristicName,
                        font: {
                            family: "Courier New, monospace",
                            size: 12
                        }
                    }
                },
                yaxis: {
                    type: $scope.plotTypeY ? "log" : "linear",
                    title: {
                        text: $scope.subsequenceCharacteristic.Text,
                        font: {
                            family: "Courier New, monospace",
                            size: 12
                        }
                    }
                }
            };
            const characteristicIndex: number = parseInt($scope.subsequenceCharacteristic.Value);
            let data: Partial<plotly.PlotData>[] = $scope.visiblePoints.map<Partial<plotly.PlotData>>((points, index) => ({
                hoverinfo: "x+y+text",
                type: "scattergl",
                x: points.map(p => $scope.numericXAxis ? p.numericX : p.x),
                y: points.map(p => p.subsequenceCharacteristics[characteristicIndex]),
                text: cText(points, index),
                mode: "markers",
                marker: { opacity: 0.5, symbol: "circle-open", color: $scope.colorScale(index) },
                name: $scope.researchObjects[index].name,
            }));

            Plotly.newPlot($scope.chartElement, data, $scope.layout, { responsive: true });

            $scope.chartElement.on("plotly_click", (data: plotly.PlotMouseEvent) => {
                $scope.selectedPointIndex = data.points[0].pointNumber;
                $scope.selectedResearchObjectIndex = data.points[0].curveNumber;
                let selectedPoint: DistributionPoint = $scope.visiblePoints[data.points[0].curveNumber][data.points[0].pointNumber];
                $scope.showTooltip(selectedPoint);
            });
        }

        // hides or shows selected organism on genes map
        function legendClick(legendItem: LegendItem): void {
            const legendIndex: number = $scope.researchObjects.findIndex((l: LegendItem) => l.id === legendItem.id);
            for (let j = 0; j < $scope.points[legendIndex].length; j++) {
                let point: DistributionPoint = $scope.points[legendIndex][j];
                point.legendVisible = !point.legendVisible;
            }

            $scope.redrawGenesMap(false);
        }

        function legendSetVisibilityForAll(visibility: boolean): void {
            for (let i = 0; i < $scope.researchObjects.length; i++) {
                for (let j = 0; j < $scope.points[i].length; j++) {
                    $scope.points[i][j].legendVisible = visibility;
                }
            }

            for (let k = 0; k < $scope.researchObjects.length; k++) {
                $scope.researchObjects[k].visible = visibility;
            }

            $scope.redrawGenesMap(false);
        }

        // Drag event handler
        function dragBarMouseDown(): void {
            let bar: HTMLElement = document.getElementById("drag-bar") ?? throwHelper("Drag bar element not found");
            let right: HTMLElement = document.getElementById('sidebar') ?? throwHelper("Sidebar element not found");

            const drag = (e: MouseEvent): void => {
                const selection: Selection | null = window.getSelection();
                if (selection) {
                    selection.empty();
                    selection.removeAllRanges();
                }

                $scope.chartElement.style.width = (e.pageX - bar.offsetWidth / 2) + "px";

                Plotly.relayout("chart", { autosize: true });
            };

            bar.addEventListener("mousedown", () => {
                document.addEventListener("mousemove", drag);
            });

            window.addEventListener("mouseup", () => {
                document.removeEventListener("mousemove", drag);
            });
        }

        // opens alignment of given subsequences with clustal on new tab
        function alignAllWithClustal(): void {
            let ids: number[] = $scope.tooltipElements.map(te => te.id);
            alignWithClustal(ids);
        }

        // opens alignment of given subsequences with clustal on new tab
        function alignSimilarWithClustal(): void {
            let ids: number[] = $scope.tooltipElements
                .filter(te => te.similarity === $scope.pointsSimilarity.same
                    || te.similarity === $scope.pointsSimilarity.similar)
                .map(te => te.id);
            alignWithClustal(ids);
        }

        // opens alignment of given subsequences with clustal on new tab
        function alignSelectedWithClustal(): void {
            let ids: number[] = $scope.tooltipElements
                .filter(te => te.selectedForAlignment)
                .map(te => te.id);
            alignWithClustal(ids);
        }

        // opens alignment of given subsequences with clustal on new tab
        function alignWithClustal(subsequencesIds: number[]): void {
            $scope.alignmentInProcess = true;
            $http.get<ClustalResponse>("/SubsequencesDistribution/CreateAlignmentTask/", { params: { subsequencesIds: subsequencesIds } })
                .then(response => {
                    $scope.alignmentInProcess = false;
                    let result: ClustalResponse = response.data;
                    if (result.Status === "Success") {
                        window.open(`https://www.ebi.ac.uk/jdispatcher/msa/clustalo/summary?jobId=${result.Result}`, "_blank");
                    }
                    else {
                        alert(`Failed to create alignment task. Error: ${result.Message}`);
                        console.log(result.Message);
                    }
                }, () => {
                    alert("Failed to create alignment task");
                    $scope.alignmentInProcess = false;
                });
        }

        // calculates color for given research object index
        // using d3.interpolate and research objects count
        function colorScale(index: number): string {

            // upper limit of color range
            let lastIndex: number = $scope.result.length;

            // color are shifted for odd and even indexes
            let colorCode: number = index / (2 * lastIndex);
            if (index % 2 !== 0) {
                colorCode = colorCode + 0.5;
            }

            return d3.interpolateTurbo(colorCode);
        }

        $scope.onInit = onInit;
        $scope.fillPoints = fillPoints;
        $scope.redrawGenesMap = redrawGenesMap;
        $scope.dotVisible = dotVisible;
        $scope.dotsSimilar = dotsSimilar;
        $scope.fillVisiblePoints = fillVisiblePoints;
        $scope.filterByFeature = filterByFeature;
        $scope.legendClick = legendClick;
        $scope.legendSetVisibilityForAll = legendSetVisibilityForAll;
        $scope.getAttributesText = getAttributesText;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.addCharacteristicComparer = addCharacteristicComparer;
        $scope.deleteCharacteristicComparer = deleteCharacteristicComparer;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.getAttributeIdByName = getAttributeIdByName;
        $scope.isAttributeEqual = isAttributeEqual;
        $scope.dragBarMouseDown = dragBarMouseDown;
        $scope.keyUpDownPress = keyUpDownPress;
        $scope.alignWithClustal = alignWithClustal;
        $scope.alignAllWithClustal = alignAllWithClustal;
        $scope.alignSimilarWithClustal = alignSimilarWithClustal;
        $scope.alignSelectedWithClustal = alignSelectedWithClustal;
        $scope.colorScale = colorScale;

        $scope.onInit();
    }

    angular.module("libiada").controller("SubsequencesDistributionResultCtrl", ["$scope", "$http", subsequencesDistributionResult]);
}
