import { initScopeFromServer, throwHelper } from "functions";
export default function SubsequencesComparerResultController() {
    async function subsequencesComparerResult($scope, $http) {
        // adds and applies new filter
        function addFilter(_newFilter) {
            for (let i = 0; i < $scope.equalElements.length; i++) {
                for (let j = 0; j < $scope.equalElements[i].length; j++) {
                    const elements = $scope.equalElements[i][j];
                    if (elements) {
                        $scope.applyFilters(elements);
                    }
                }
            }
        }
        // checks if element is visible
        function elementVisible(element) {
            return (element.filtersVisible?.length ?? 0) === 0 || element.filtersVisible?.some(e => e) === true;
        }
        // deletes given filter
        function deleteFilter(_filter, filterIndex) {
            for (let i = 0; i < $scope.equalElements.length; i++) {
                for (let j = 0; j < $scope.equalElements[i].length; j++) {
                    const elements = $scope.equalElements[i][j];
                    if (elements) {
                        for (let k = 0; k < elements.length; k++) {
                            elements[k].filtersVisible?.splice(filterIndex, 1);
                        }
                    }
                }
            }
        }
        // applies filters
        function applyFilters(elements) {
            for (let i = 0; i < elements.length; i++) {
                const element = elements[i] ?? throwHelper("Equal subsequence pair is not available");
                const filtersVisible = [];
                element.filtersVisible = filtersVisible;
                for (let j = 0; j < $scope.filters.length; j++) {
                    let filterValue = $scope.filters[j].value.toUpperCase();
                    let firstSubsequenceIndex = element.Item1;
                    let firstVisible = $scope.isAttributeEqual($scope.firstResearchObjectIndex, firstSubsequenceIndex, "product", filterValue);
                    firstVisible = firstVisible || $scope.isAttributeEqual($scope.firstResearchObjectIndex, firstSubsequenceIndex, "locus_tag", filterValue);
                    let secondSubsequenceIndex = element.Item2;
                    let secondVisible = $scope.isAttributeEqual($scope.secondResearchObjectIndex, secondSubsequenceIndex, "product", filterValue);
                    secondVisible = secondVisible || $scope.isAttributeEqual($scope.secondResearchObjectIndex, secondSubsequenceIndex, "locus_tag", filterValue);
                    filtersVisible.push(firstVisible || secondVisible);
                }
            }
        }
        // returns attribute index by its name if any
        function getAttributeIdByName(researchObjectIndex, subsequenceIndex, attributeName) {
            const subsequence = $scope.characteristics?.[researchObjectIndex]?.[subsequenceIndex]
                ?? throwHelper("Subsequence characteristics have not been loaded");
            const attributeValues = $scope.attributeValues
                ?? throwHelper("Subsequence attribute values have not been loaded");
            return subsequence.Attributes.find(a => $scope.attributes[attributeValues[a].attribute] === attributeName);
        }
        // returns true if dot has given attribute and its value equal to the given value
        function isAttributeEqual(researchObjectIndex, subsequenceIndex, attributeName, expectedValue) {
            let attributeId = $scope.getAttributeIdByName(researchObjectIndex, subsequenceIndex, attributeName);
            if (attributeId !== undefined) {
                const attributeValues = $scope.attributeValues
                    ?? throwHelper("Subsequence attribute values have not been loaded");
                let product = attributeValues[attributeId].value.toUpperCase();
                return product.indexOf(expectedValue) !== -1;
            }
            return false;
        }
        // shows list of equal elements only for given pair of research objects
        function showEqualPairs(firstIndex, secondIndex, similarityValue) {
            $scope.loading = true;
            $scope.loadingScreenHeader = "Loading equal subsequences list...";
            $scope.firstResearchObjectIndex = firstIndex;
            $scope.secondResearchObjectIndex = secondIndex;
            $scope.similarityValue = similarityValue;
            $scope.similarityValueSelected = true;
            const selectedRow = $scope.equalElements[firstIndex]
                ?? throwHelper("Selected research object pair is not available");
            const cachedElements = selectedRow[secondIndex];
            if (cachedElements) {
                $scope.equalElementsToShow = cachedElements;
                $scope.loading = false;
            }
            else {
                $http.get("/api/TaskManagerApi/GetSubsequencesComparerDataElement", {
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
                    selectedRow[secondIndex] = response.data;
                })
                    .then(() => $http.get("/api/TaskManagerApi/GetTaskDataByKey", {
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
                    .then(() => $http.get("/api/TaskManagerApi/GetTaskDataByKey", {
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
                    const elements = selectedRow[secondIndex]
                        ?? throwHelper("Equal subsequences were not returned");
                    $scope.applyFilters(elements);
                    $scope.equalElementsToShow = elements;
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
        function getHighlightColor(value) {
            let color = d3.scaleLinear()
                .domain([0, 0.1, 0.5, 1])
                .range(["lightcoral", "gold", "yellow", "limegreen"]);
            return { "background-color": color(value) };
        }
        // calculates and displays local characteristics for given subsequences
        function calculateLocalCharacteristics(firstSubsequenceId, secondSubsequenceId, index) {
            $scope.loading = true;
            $scope.loadingScreenHeader = "Loading local characteristics...";
            let characteristicType = $scope.characteristic.characteristicType?.Value
                ?? throwHelper("Characteristic type has not been selected");
            let link = $scope.characteristic.link?.Value
                ?? throwHelper("Characteristic link has not been selected");
            let arrangementType = $scope.characteristic.arrangementType?.Value
                ?? throwHelper("Characteristic arrangement type has not been selected");
            let characteristicId = $scope.characteristicsDictionary[`(${characteristicType}, ${link}, ${arrangementType})`]
                ?? throwHelper("Selected characteristic was not found");
            $http.get("/api/LocalCalculationApi/GetSubsequenceCharacteristic", {
                params: {
                    subsequenceId: firstSubsequenceId,
                    characteristicLinkId: characteristicId,
                    windowSize: $scope.slidingWindowParams.windowSize,
                    step: $scope.slidingWindowParams.step
                }
            }).then(firstCharacteristics => {
                $scope.firstSubsequenceLocalCharacteristics = firstCharacteristics.data;
                $http.get("/api/LocalCalculationApi/GetSubsequenceCharacteristic", {
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
        function drawLocalCharacteristics(firstSubsequenceId, secondSubsequenceId, index) {
            const firstCharacteristics = $scope.firstSubsequenceLocalCharacteristics
                ?? throwHelper("First subsequence characteristics have not been loaded");
            const secondCharacteristics = $scope.secondSubsequenceLocalCharacteristics
                ?? throwHelper("Second subsequence characteristics have not been loaded");
            const characteristicType = $scope.characteristic.characteristicType
                ?? throwHelper("Characteristic type has not been selected");
            let legendData = [
                { id: firstSubsequenceId, name: "First", visible: true, points: [] },
                { id: secondSubsequenceId, name: "Second", visible: true, points: [] }
            ];
            for (let j = 0; j < firstCharacteristics.length; j++) {
                legendData[0].points.push({
                    id: firstSubsequenceId,
                    x: j,
                    value: +firstCharacteristics[j]
                });
            }
            for (let k = 0; k < secondCharacteristics.length; k++) {
                legendData[1].points.push({
                    id: secondSubsequenceId,
                    x: k,
                    value: +secondCharacteristics[k]
                });
            }
            // removing previous chart if any
            d3.select(`.chart${index}`).remove();
            // chart size and margin settings
            let margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            let width = $scope.width - margin.left - margin.right;
            let height = $scope.height - margin.top - margin.bottom;
            // calculating margins for dots
            let xMinArray = [];
            let xMaxArray = [];
            let yMaxArray = [];
            let yMinArray = [];
            legendData.forEach(data => {
                xMinArray.push(d3.min(data.points, (d) => d.x) ?? throwHelper("No local characteristic points to plot"));
                xMaxArray.push(d3.max(data.points, (d) => d.x) ?? throwHelper("No local characteristic points to plot"));
                yMinArray.push(d3.min(data.points, (d) => d.value) ?? throwHelper("No local characteristic points to plot"));
                yMaxArray.push(d3.max(data.points, (d) => d.value) ?? throwHelper("No local characteristic points to plot"));
            });
            // setup x
            // calculating margins for dots
            let xMin = d3.min(xMinArray) ?? throwHelper("No local characteristic points to plot");
            let xMax = d3.max(xMaxArray) ?? throwHelper("No local characteristic points to plot");
            let xMargin = (xMax - xMin) * 0.05;
            let xScale = d3.scaleLinear()
                .domain([xMin - xMargin, xMax + xMargin])
                .range([0, width]);
            let xAxis = d3.axisBottom(xScale)
                .tickSizeInner(-height)
                .tickSizeOuter(0)
                .tickPadding(10);
            let xMap = (d) => xScale(d.x);
            // setup y
            let yMin = d3.min(yMinArray) ?? throwHelper("No local characteristic points to plot");
            let yMax = d3.max(yMaxArray) ?? throwHelper("No local characteristic points to plot");
            let yMargin = (yMax - yMin) * 0.05;
            let yScale = d3.scaleLinear()
                .domain([yMin - yMargin, yMax + yMargin])
                .range([height, 0]);
            let yAxis = d3.axisLeft(yScale)
                .tickSizeInner(-width)
                .tickSizeOuter(0)
                .tickPadding(10);
            let yMap = (d) => yScale(d.value);
            // setup fill color
            let color = d3.scaleOrdinal(["red", "blue"]);
            // add the graph canvas to the body of the webpage
            let svg = d3.select(`#chart${index}`).append("svg")
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
                .text(characteristicType.Text)
                .style("font-size", "12pt");
            let line = d3.line()
                .x(xMap)
                .y(yMap);
            legendData.forEach(data => {
                // Nest the entries by symbol
                let dataGroups = d3.group(data.points, (d) => d.id);
                // Loop through each symbol / key
                dataGroups.forEach((value) => {
                    svg.append("path")
                        .datum(value)
                        .attr("class", "line")
                        .attr("d", line)
                        .attr("stroke", (d) => color((d[0] ?? throwHelper("Local characteristic line has no points")).id))
                        .attr("stroke-width", 1)
                        .attr("fill", "none")
                        .attr("opacity", 0.6);
                });
            });
            // draw legend
            let legend = svg.selectAll(".legend")
                .data(legendData)
                .enter()
                .append("g")
                .attr("class", "legend")
                .attr("transform", (_d, i) => `translate(0,${i * 20})`)
                .on("click", function (event, d) {
                d.visible = !d.visible;
                let legendEntry = d3.select(event.currentTarget);
                legendEntry.select("text")
                    .style("opacity", () => d.visible ? 1 : 0.5);
                legendEntry.select("rect")
                    .style("fill-opacity", () => d.visible ? 1 : 0);
                svg.selectAll(".line")
                    .filter((line) => (line[0] ?? throwHelper("Local characteristic line has no points")).id === d.id)
                    .attr("visibility", () => d.visible ? "visible" : "hidden");
            });
            // draw legend's colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", (d) => color(d.id))
                .style("stroke", (d) => color(d.id))
                .style("stroke-width", 4)
                .attr("transform", `translate(0, -${$scope.legendHeight})`);
            // draw legend's text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", `translate(0, -${$scope.legendHeight})`)
                .text((d) => d.name)
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
        $scope.similarityValue = null;
        $scope.similarityValueSelected = false;
        $scope.loadingScreenHeader = "Loading data";
        $scope.characteristic = {};
        $scope.flags = { displayFiltered: false };
        await initScopeFromServer($http, $scope, "Loading subsequences comparison data", "Failed loading subsequences comparison data");
        $scope.equalElements = new Array($scope.researchObjectsNames.length);
        for (let i = 0; i < $scope.researchObjectsNames.length; i++) {
            $scope.equalElements[i] = new Array($scope.researchObjectsNames.length);
        }
        $scope.$apply();
    }
    function makePositive() {
        return (num) => Math.abs(num);
    }
    angular.module("libiada")
        .filter("makePositive", makePositive)
        .controller("SubsequencesComparerResultCtrl", ["$scope", "$http", subsequencesComparerResult]);
}
//# sourceMappingURL=subsequencesComparerResult.js.map