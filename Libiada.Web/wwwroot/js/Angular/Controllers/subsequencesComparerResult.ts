import { getArrayMinMax, initScopeFromServer, throwHelper } from "functions";
import type { ResultScope } from "functions";
import type {
    AttributeValue,
    CharacteristicsDictionary,
    CharacteristicType,
    Filter,
    Notation,
    SelectedCharacteristic,
    SubsequenceData,
} from "viewDataTypes";

interface CellHighlight {
    "background-color": string;
}

interface SimilarityData {
    Formula1: number;
    Formula2: number;
    Formula3: number;
    FirstAbsolutelyEqualElementsCount: number;
    FirstNearlyEqualElementsCount: number;
    FirstNotEqualElementsCount: number;
    FirstTotalElementsCount: number;
    SecondAbsolutelyEqualElementsCount: number;
    SecondNearlyEqualElementsCount: number;
    SecondNotEqualElementsCount: number;
    SecondTotalElementsCount: number;
}

interface EqualElement {
    Item1: number;
    Item2: number;
    Item3: number;
    filtersVisible: boolean[];
}

interface ErrorResponse {
    Status: "Error";
    Message: string;
}

interface LocalCharacteristicPoint {
    id: number;
    x: number;
    value: number;
}

interface LocalCharacteristicLegendItem {
    id: number;
    name: string;
    visible: boolean;
    points: LocalCharacteristicPoint[];
}

interface SubsequencesComparerResultData {
    researchObjectsNames: string[];
    characteristicNames: string[];
    similarities: SimilarityData[][];
    filteredSimilarities: SimilarityData[][] | null;
    features: { [key: number]: string };
    attributes: { [key: number]: string };
    maxPercentageDifferences: string[];
    sequenceCharacteristicName: string;
    nature: number;
    notations: Notation[];
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: CharacteristicsDictionary;
}

interface SubsequencesComparerResultScope extends ResultScope, SubsequencesComparerResultData {
    characteristics: SubsequenceData[][];
    attributeValues: AttributeValue[];
    equalElements: (EqualElement[] | undefined)[][];
    equalElementsToShow: EqualElement[];
    filters: Filter[];
    firstResearchObjectIndex: number;
    secondResearchObjectIndex: number;
    similarityValue: SimilarityData | null;
    similarityValueSelected: boolean;
    flags: { displayFiltered: boolean };
    characteristic: SelectedCharacteristic;
    slidingWindowParams: { windowSize: number; step: number };
    firstSubsequenceLocalCharacteristics: number[];
    secondSubsequenceLocalCharacteristics: number[];
    legendHeight: number;
    width: number;
    height: number;
    dotRadius: number;
    selectedDotRadius: number;

    addFilter: (newFilter: string) => void;
    deleteFilter: (filter: Filter, filterIndex: number) => void;
    elementVisible: (element: EqualElement) => boolean;
    applyFilters: (elements: EqualElement[]) => void;
    getAttributeIdByName: (researchObjectIndex: number, subsequenceIndex: number, attributeName: string) => number | undefined;
    isAttributeEqual: (researchObjectIndex: number, subsequenceIndex: number, attributeName: string, expectedValue: string) => boolean;
    showEqualPairs: (firstIndex: number, secondIndex: number, similarityValue: SimilarityData) => void;
    getHighlightColor: (value: number) => { "background-color": string };
    calculateLocalCharacteristics: (firstSubsequenceId: number, secondSubsequenceId: number, index: number) => void;
    drawLocalCharacteristics: (firstSubsequenceId: number, secondSubsequenceId: number, index: number) => void;
}

export default function SubsequencesComparerResultController() {
    async function subsequencesComparerResult($scope: SubsequencesComparerResultScope, $http: ng.IHttpService): Promise<void> {

        // adds and applies new filter
        function addFilter(_newFilter: string): void {
            for (let i = 0; i < $scope.equalElements.length; i++) {
                for (let j = 0; j < $scope.equalElements[i].length; j++) {
                    const elements: EqualElement[] | undefined = $scope.equalElements[i][j];
                    if (elements) {
                        $scope.applyFilters(elements);
                    }
                }
            }
        }

        // checks if element is visible
        function elementVisible(element: EqualElement): boolean {
            return element.filtersVisible.length === 0 || element.filtersVisible.some(e => e);
        }

        // deletes given filter
        function deleteFilter(_filter: Filter, filterIndex: number): void {
            for (let i = 0; i < $scope.equalElements.length; i++) {
                for (let j = 0; j < $scope.equalElements[i].length; j++) {
                    const elements: EqualElement[] | undefined = $scope.equalElements[i][j];
                    if (elements) {
                        for (let k = 0; k < elements.length; k++) {
                            elements[k].filtersVisible.splice(filterIndex, 1);
                        }
                    }
                }
            }
        }

        // applies filters
        function applyFilters(elements: EqualElement[]): void {
            for (let i = 0; i < elements.length; i++) {
                elements[i].filtersVisible = [];
                for (let j = 0; j < $scope.filters.length; j++) {
                    let filterValue: string = $scope.filters[j].value.toUpperCase();

                    let firstSubsequenceIndex: number = elements[i].Item1;
                    let firstVisible: boolean = $scope.isAttributeEqual($scope.firstResearchObjectIndex, firstSubsequenceIndex, "product", filterValue);
                    firstVisible = firstVisible || $scope.isAttributeEqual($scope.firstResearchObjectIndex, firstSubsequenceIndex, "locus_tag", filterValue);

                    let secondSubsequenceIndex: number = elements[i].Item2;
                    let secondVisible: boolean = $scope.isAttributeEqual($scope.secondResearchObjectIndex, secondSubsequenceIndex, "product", filterValue);
                    secondVisible = secondVisible || $scope.isAttributeEqual($scope.secondResearchObjectIndex, secondSubsequenceIndex, "locus_tag", filterValue);

                    elements[i].filtersVisible.push(firstVisible || secondVisible);
                }
            }
        }

        // returns attribute index by its name if any
        function getAttributeIdByName(researchObjectIndex: number, subsequenceIndex: number, attributeName: string): number | undefined {
            return $scope.characteristics[researchObjectIndex][subsequenceIndex].Attributes.find(a =>
                $scope.attributes[$scope.attributeValues[a].attribute] === attributeName
            );
        }

        // returns true if dot has given attribute and its value equal to the given value
        function isAttributeEqual(
            researchObjectIndex: number,
            subsequenceIndex: number,
            attributeName: string,
            expectedValue: string): boolean {
            let attributeId: number | undefined = $scope.getAttributeIdByName(researchObjectIndex, subsequenceIndex, attributeName);
            if (attributeId) {
                let product: string = $scope.attributeValues[attributeId].value.toUpperCase();
                return product.indexOf(expectedValue) !== -1;
            }

            return false;
        }

        // shows list of equal elements only for given pair of research objects
        function showEqualPairs(firstIndex: number, secondIndex: number, similarityValue: SimilarityData): void {
            $scope.loading = true;
            $scope.loadingScreenHeader = "Loading equal subsequences list...";

            $scope.firstResearchObjectIndex = firstIndex;
            $scope.secondResearchObjectIndex = secondIndex;

            $scope.similarityValue = similarityValue;
            $scope.similarityValueSelected = true;

            if ($scope.equalElements[firstIndex][secondIndex]) {
                // no need to load elements, they are already in cache
                $scope.equalElementsToShow = $scope.equalElements[firstIndex][secondIndex];
                $scope.loading = false;
                return;
            } else {
                $http.get<EqualElement[] | ErrorResponse>("/api/TaskManagerApi/GetSubsequencesComparerDataElement", {
                    params: {
                        taskId: $scope.taskId,
                        firstIndex: firstIndex,
                        secondIndex: secondIndex,
                        filtered: $scope.flags.displayFiltered
                    }
                })
                    .then(response => {
                        if (!Array.isArray(response.data)) {
                            throw new Error(response.data.Message);
                        }
                        $scope.equalElements[firstIndex][secondIndex] = response.data;
                    })
                    .then(() => $http.get<SubsequenceData[][] | ErrorResponse>("/api/TaskManagerApi/GetTaskDataByKey", {
                        params: {
                            id: $scope.taskId,
                            key: "characteristics"
                        }
                    }))
                    .then(response => {
                        if (!Array.isArray(response.data)) {
                            throw new Error(response.data.Message);
                        }
                        $scope.characteristics = response.data;
                    })
                    .then(() => $http.get<AttributeValue[] | ErrorResponse>("/api/TaskManagerApi/GetTaskDataByKey", {
                        params: {
                            id: $scope.taskId,
                            key: "attributeValues"
                        }
                    }))
                    .then(response => {
                        if (!Array.isArray(response.data)) {
                            throw new Error(response.data.Message);
                        }
                        $scope.attributeValues = response.data;
                    })
                    .then(() => {
                        const equalElements: EqualElement[] = $scope.equalElements[firstIndex][secondIndex]!;
                        $scope.applyFilters(equalElements);
                        $scope.equalElementsToShow = equalElements;
                        $scope.loading = false;
                    })
                    .catch(error => {
                        alert("Failed to load subsequences data");
                        console.error(error);
                        $scope.loading = false;
                    });
            }
        }

        // calculates cell highlight color using d3.js color scale
        function getHighlightColor(value: number): CellHighlight {
            let color: d3.ScaleLinear<string, string> = d3.scaleLinear<string>()
                .domain([0, 0.1, 0.5, 1])
                .range(["lightcoral", "gold", "yellow", "limegreen"]);
            return { "background-color": color(value) };
        }

        // calculates and displays local characteristics for given subsequences
        function calculateLocalCharacteristics(firstSubsequenceId: number, secondSubsequenceId: number, index: number): void {
            $scope.loadingScreenHeader = "Loading local characteristics...";
            $scope.loading = true;

            let characteristicType: string  = $scope.characteristic.characteristicType.Value;
            let link: string = $scope.characteristic.link.Value;
            let arrangementType: string = $scope.characteristic.arrangementType.Value;
            let characteristicId: number = $scope.characteristicsDictionary[`(${characteristicType}, ${link}, ${arrangementType})`];

            $http.get<number[]>("/api/LocalCalculationApi/GetSubsequenceCharacteristic", {
                params: {
                    subsequenceId: firstSubsequenceId,
                    characteristicLinkId: characteristicId,
                    windowSize: $scope.slidingWindowParams.windowSize,
                    step: $scope.slidingWindowParams.step
                }
            }).then(firstCharacteristics => {
                $scope.firstSubsequenceLocalCharacteristics = firstCharacteristics.data;

                $http.get<number[]>("/api/LocalCalculationApi/GetSubsequenceCharacteristic", {
                    params: {
                        subsequenceId: secondSubsequenceId,
                        characteristicLinkId: characteristicId,
                        windowSize: $scope.slidingWindowParams.windowSize,
                        step: $scope.slidingWindowParams.step
                    }
                }).then(secondCharacteristics => {
                    $scope.secondSubsequenceLocalCharacteristics = secondCharacteristics.data;
                    $scope.drawLocalCharacteristics(firstSubsequenceId, secondSubsequenceId, index);

                    $scope.loading = false;
                }, () => {
                    alert("Failed loading characteristics data");

                    $scope.loading = false;
                });
            }, () => {
                alert("Failed loading local characteristics data");

                $scope.loading = false;
            });
        }

        // draws local characteristics line-chart
        function drawLocalCharacteristics(firstSubsequenceId: number, secondSubsequenceId: number, index: number): void {
            let legendData: LocalCharacteristicLegendItem[] = [
                { id: firstSubsequenceId, name: "First", visible: true, points: [] },
                { id: secondSubsequenceId, name: "Second", visible: true, points: [] }
            ];

            for (let j = 0; j < $scope.firstSubsequenceLocalCharacteristics.length; j++) {
                legendData[0].points.push({
                    id: firstSubsequenceId,
                    x: j,
                    value: $scope.firstSubsequenceLocalCharacteristics[j]
                });
            }

            for (let k = 0; k < $scope.secondSubsequenceLocalCharacteristics.length; k++) {
                legendData[1].points.push({
                    id: secondSubsequenceId,
                    x: k,
                    value: $scope.secondSubsequenceLocalCharacteristics[k]
                });
            }

            // removing previous chart if any
            d3.select(`.chart${index}`).remove();

            // chart size and margin settings
            let margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            let width: number = $scope.width - margin.left - margin.right;
            let height: number = $scope.height - margin.top - margin.bottom;

            // setup x
            // calculating margins for dots
            const xValues: number[] = legendData.map(data => data.points.map((p: LocalCharacteristicPoint) => p.x)).flat();
            let { min: xMin, max: xMax } = getArrayMinMax(xValues);
            let xMargin: number = (xMax - xMin) * 0.05;

            let xScale: d3.ScaleLinear<number, number> = d3.scaleLinear()
                .domain([xMin - xMargin, xMax + xMargin])
                .range([0, width]);
            let xAxis: d3.Axis<d3.NumberValue> = d3.axisBottom(xScale)
                .tickSizeInner(-height)
                .tickSizeOuter(0)
                .tickPadding(10);


            let xMap = (d: LocalCharacteristicPoint): number => xScale(d.x);

            // setup y
            const yValues: number[] = legendData.map(data => data.points.map((p: LocalCharacteristicPoint) => p.value)).flat();
            let { min: yMin, max: yMax } = getArrayMinMax(yValues);
            let yMargin: number = (yMax - yMin) * 0.05;

            let yScale: d3.ScaleLinear<number, number> = d3.scaleLinear()
                .domain([yMin - yMargin, yMax + yMargin])
                .range([height, 0]);
            let yAxis: d3.Axis<d3.NumberValue> = d3.axisLeft(yScale)
                .tickSizeInner(-width)
                .tickSizeOuter(0)
                .tickPadding(10);

            let yMap = (d: LocalCharacteristicPoint): number => yScale(d.value);

            // setup fill color
            let color: d3.ScaleOrdinal<number, string> = d3.scaleOrdinal<number, string>(["red", "blue"]);

            // add the graph canvas to the body of the webpage
            let svg: d3.Selection<SVGGElement, null, HTMLElement, null> = d3.select<SVGElement, null>(`#chart${index}`).append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height)
                .attr("class", `chart${index}`)
                .append("g")
                .attr("transform", `translate(${margin.left},${margin.top})`);

            // x-axis
            svg.append("g")
                .attr("class", "x axis")
                .attr("transform", `translate(0,${height})`)
                .call(xAxis);

            svg.append("text")
                .attr("class", "label")
                .attr("transform", `translate(${width / 2} ,${height + margin.top - $scope.legendHeight})`)
                .style("text-anchor", "middle")
                .text("Fragment №")
                .style("font-size", "12pt");

            // y-axis
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
                .text($scope.characteristic.characteristicType.Text)
                .style("font-size", "12pt");

            let line: d3.Line<LocalCharacteristicPoint> = d3.line(xMap, yMap);

            legendData.forEach(data => {
                // Nest the entries by symbol
                let dataGroups: d3.InternMap<number,LocalCharacteristicPoint[]> = d3.group(data.points, (d: LocalCharacteristicPoint) => d.id);

                // Loop through each symbol / key
                dataGroups.forEach((value: LocalCharacteristicPoint[]) => {
                    svg.append("path")
                        .datum(value)
                        .attr("class", "line")
                        .attr("d", line)
                        .attr("stroke", (d: LocalCharacteristicPoint[]) => color(d[0].id))
                        .attr("stroke-width", 1)
                        .attr("fill", "none")
                        .attr("opacity", 0.6);
                });
            });

            // draw legend
            let legend: d3.Selection<SVGGElement, LocalCharacteristicLegendItem, SVGGElement, null> = svg.selectAll<SVGGElement, LocalCharacteristicLegendItem>(".legend")
                .data(legendData)
                .enter()
                .append("g")
                .attr("class", "legend")
                .attr("transform", (_d: LocalCharacteristicLegendItem, i: number) => `translate(0,${i * 20})`)
                .on("click", function (event: MouseEvent, d: LocalCharacteristicLegendItem) {
                    d.visible = !d.visible;
                    let legendEntry: d3.Selection<SVGGElement, LocalCharacteristicLegendItem, null, undefined> = d3.select<SVGGElement, LocalCharacteristicLegendItem>(event.currentTarget as SVGGElement);
                    legendEntry.select("text")
                        .style("opacity", () => d.visible ? 1 : 0.5);
                    legendEntry.select("rect")
                        .style("fill-opacity", () => d.visible ? 1 : 0);

                    svg.selectAll<SVGGElement, LocalCharacteristicPoint[]>(".line")
                        .filter((line: LocalCharacteristicPoint[]) => line[0].id === d.id)
                        .attr("visibility", () => d.visible ? "visible" : "hidden");
                });

            // draw legend's colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", (d: LocalCharacteristicLegendItem) => color(d.id))
                .style("stroke", (d: LocalCharacteristicLegendItem) => color(d.id))
                .style("stroke-width", 4)
                .attr("transform", `translate(0, -${$scope.legendHeight})`);

            // draw legend's text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", `translate(0, -${$scope.legendHeight})`)
                .text((d: LocalCharacteristicLegendItem) => d.name)
                .style("font-size", "9pt");
        }

        $scope.getHighlightColor = getHighlightColor;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.getAttributeIdByName = getAttributeIdByName;
        $scope.isAttributeEqual = isAttributeEqual;
        $scope.elementVisible = elementVisible;
        $scope.showEqualPairs = showEqualPairs;
        $scope.applyFilters = applyFilters;
        $scope.calculateLocalCharacteristics = calculateLocalCharacteristics;
        $scope.drawLocalCharacteristics = drawLocalCharacteristics;

        $scope.slidingWindowParams = {
            windowSize: 50,
            step: 1
        };

        $scope.equalElementsToShow = [];
        $scope.filters = [];
        $scope.legendHeight = 40;
        $scope.width = 1050;
        $scope.height = 800;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 2;
        $scope.similarityValueSelected = false;
        $scope.flags = { displayFiltered: false };

        await initScopeFromServer<SubsequencesComparerResultData>(
            $http,
            $scope,
            "Loading subsequences comparison data",
            "Failed loading subsequences comparison data");

        $scope.characteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.characteristicTypes[0].Links[0],
            arrangementType: $scope.characteristicTypes[0].ArrangementTypes[0],
        };
        $scope.equalElements = new Array($scope.researchObjectsNames.length);

        for (let i = 0; i < $scope.researchObjectsNames.length; i++) {
            $scope.equalElements[i] = new Array($scope.researchObjectsNames.length);
        }

        $scope.$apply();
    }

    function makePositive(): (num: number) => number {
        return (num: number): number => Math.abs(num);
    }

    angular.module("libiada")
        .filter("makePositive", makePositive)
        .controller("SubsequencesComparerResultCtrl", ["$scope", "$http", subsequencesComparerResult]);
}