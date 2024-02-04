function SubsequencesDistributionResultController() {
    "use strict";

    function subsequencesDistributionResult($scope, $http) {

        function onInit() {
            $scope.dotRadius = 4;
            $scope.selectedDotRadius = $scope.dotRadius * 3;

            $scope.points = [];
            $scope.visiblePoints = [];
            $scope.matters = [];
            $scope.legend = [];
            $scope.characteristicComparers = [];
            $scope.filters = [];
            $scope.plotTypeX = "";
            $scope.plotTypeY = "";
            $scope.productFilter = "";
            $scope.tooltipVisible = false;
            $scope.tooltipElements = [];
            $scope.alignmentInProcess = false;
            $scope.pointsSimilarity = Object.freeze({ "same": 0, "similar": 1, "different": 2 });

            // initialyzing tooltips for tabs
            $('[data-bs-toggle="tooltip"]').tooltip();

            // preventing scroll in key up and key down
            window.addEventListener("keydown", e => {
                if ($scope.isKeyUpOrDown(e.keyCode)) {
                    e.preventDefault();
                    $scope.keyUpDownPress(e.keyCode);
                }
            }, false);

            $scope.loadingScreenHeader = "Loading genes map data";
            $scope.loading = true;

            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            $http.get(`/api/TaskManagerWebApi/GetTaskData/${$scope.taskId}`)
                .then((data) => {
                    MapModelFromJson($scope, data.data);
                    $scope.plot = document.getElementById("chart");
                    $scope.subsequenceCharacteristic = $scope.subsequencesCharacteristicsList[0];

                    $scope.fillPoints();

                    let comparer = (first, second) => first.subsequenceCharacteristics[$scope.subsequenceCharacteristic.Value] - second.subsequenceCharacteristics[$scope.subsequenceCharacteristic.Value];

                    $scope.points = $scope.points.map(points => points.sort(comparer));
                    $scope.visiblePoints = $scope.visiblePoints.map(points => points.sort(comparer));

                    $scope.addCharacteristicComparer();

                    $scope.redrawGenesMap();
                    $scope.loading = false;
                }, () => {
                    alert("Failed loading genes map data");

                    $scope.loading = false;
                });
        }

        // initializes data for genes map
        function fillPoints() {
            for (let i = 0; i < $scope.result.length; i++) {
                let sequenceData = $scope.result[i];
                $scope.matters.push({ id: sequenceData.MatterId, name: sequenceData.MatterName, visible: true, index: i, color: $scope.colorScale(i) });
                // hack for legend dot color
                document.styleSheets[0].insertRule(".legend" + sequenceData.MatterId + ":after { background:" + $scope.colorScale(i) + "}");
                $scope.points.push([]);
                $scope.visiblePoints.push([]);
                for (let j = 0; j < sequenceData.SubsequencesData.length; j++) {
                    let subsequenceData = sequenceData.SubsequencesData[j];

                    let point = {
                        id: subsequenceData.Id,
                        matterId: sequenceData.MatterId,
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
        function addCharacteristicComparer() {
            $scope.characteristicComparers.push({ characteristic: $scope.subsequencesCharacteristicsList[0], precision: 0 });
        }

        // deletes given characteristics filter
        function deleteCharacteristicComparer(characteristicComparer) {
            $scope.characteristicComparers.splice($scope.characteristicComparers.indexOf(characteristicComparer), 1);
        }

        // fills array of currently visible points
        function fillVisiblePoints() {
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
        function getAttributeIdByName(dot, attributeName) {
            return dot.attributes.find(a => $scope.attributes[$scope.attributeValues[a].attribute] === attributeName);
        }

        // returns true if dot has given attribute and its value equal to the given value
        function isAttributeEqual(dot, attributeName, expectedValue) {
            let attributeId = $scope.getAttributeIdByName(dot, attributeName);
            if (attributeId) {
                let product = $scope.attributeValues[attributeId].value.toUpperCase();
                return product.indexOf(expectedValue) !== -1;
            }

            return false;
        }

        // adds and applies new filter
        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });

                let filterValue = $scope.filters[$scope.filters.length - 1].value.toUpperCase();

                for (let i = 0; i < $scope.points.length; i++) {
                    for (let j = 0; j < $scope.points[i].length; j++) {
                        let point = $scope.points[i][j];
                        let visible = $scope.isAttributeEqual(point, "product", filterValue);
                        visible = visible || $scope.isAttributeEqual(point, "locus_tag", filterValue);
                        point.filtersVisible.push(visible);
                        point.visible = $scope.dotVisible(point);
                    }
                }

                $scope.redrawGenesMap();

                $scope.newFilter = "";
            }
            // todo: add error message if filter is empty
        }

        // deletes given filter
        function deleteFilter(filter) {
            for (let i = 0; i < $scope.points.length; i++) {
                for (let j = 0; j < $scope.points[i].length; j++) {
                    let point = $scope.points[i][j];

                    point.filtersVisible.splice($scope.filters.indexOf(filter), 1);
                    point.visible = $scope.dotVisible(point);
                }
            }

            $scope.filters.splice($scope.filters.indexOf(filter), 1);
            $scope.redrawGenesMap();
        }

        // filters dots by subsequences feature
        function filterByFeature(feature) {
            for (let i = 0; i < $scope.points.length; i++) {
                for (let j = 0; j < $scope.points[i].length; j++) {
                    let point = $scope.points[i][j];

                    if (point.featureId === parseInt(feature.Value)) {
                        point.featureVisible = feature.Selected;
                        point.visible = $scope.dotVisible(point);
                    }
                }
            }

            $scope.redrawGenesMap();
        }

        // checks if dot is visible
        function dotVisible(dot) {
            let filterVisible = dot.filtersVisible.length === 0 || dot.filtersVisible.some(fv => fv);
            return dot.legendVisible && dot.featureVisible && filterVisible;
        }

        // determines if dots are similar by product
        function dotsSimilar(d, dot) {
            if (d.featureId !== dot.featureId) {
                return false;
            }

            switch (d.featureId) {
                case 1: // CDS
                case 2: // RRNA
                case 3: // TRNA
                    let firstProductId = $scope.getAttributeIdByName(d, "product");
                    let secondProductId = $scope.getAttributeIdByName(dot, "product");
                    if ($scope.attributeValues[firstProductId].value.toUpperCase() !== $scope.attributeValues[secondProductId].value.toUpperCase()) {
                        return $scope.pointsSimilarity.different;
                    }
                    break;
            }

            return $scope.pointsSimilarity.similar;
        }

        // gets attributes text for given subsequence
        function getAttributesText(attributes) {
            let attributesText = [];
            for (let i = 0; i < attributes.length; i++) {
                let attributeValue = $scope.attributeValues[attributes[i]];
                attributesText.push($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : " = " + attributeValue.value));
            }

            return attributesText;
        }

        // shows tooltip for dot or group of dots
        function showTooltip(selectedPoint) {
            $("button[data-bs-target='#tooltip-tab-pane']").tab("show");

            $scope.tooltipVisible = true;
            $scope.tooltipElements.length = 0;
            let matterName = $scope.matters.find(value => value.id === selectedPoint.matterId).name;
            $scope.tooltipElements.push(fillPointTooltip(selectedPoint, matterName, $scope.pointsSimilarity.same));
            let similarPoints = [];

            for (let i = 0; i < $scope.visiblePoints.length; i++) {
                for (let j = 0; j < $scope.visiblePoints[i].length; j++) {
                    if (selectedPoint !== $scope.visiblePoints[i][j] && $scope.highlight) {
                        let similar = $scope.characteristicComparers.every(filter => {
                            let selectedPointValue = selectedPoint.subsequenceCharacteristics[filter.characteristic.Value];
                            let anotherPointValue = $scope.visiblePoints[i][j].subsequenceCharacteristics[filter.characteristic.Value];
                            return Math.abs(selectedPointValue - anotherPointValue) <= filter.precision;
                        });

                        if (similar) {
                            let point = $scope.visiblePoints[i][j];
                            similarPoints.push(point);
                            matterName = $scope.matters[i].name;
                            $scope.tooltipElements.push(fillPointTooltip(point, matterName, $scope.dotsSimilar(point, selectedPoint)));
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

            let update = {
                "marker.symbol": $scope.visiblePoints.map(points => points.map(point => point === selectedPoint || similarPoints.includes(point) ? "diamond-wide" : "circle-open")),
                "marker.size": $scope.visiblePoints.map(points => points.map(point => point === selectedPoint || similarPoints.includes(point) ? 15 : 6))
            };

            Plotly.restyle($scope.plot, update);

            $scope.$apply();
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(point, matterName, similarity) {
            let color = similarity === $scope.pointsSimilarity.same ? "default"
                : similarity === $scope.pointsSimilarity.similar ? "success"
                    : similarity === $scope.pointsSimilarity.different ? "danger" : "danger";

            let tooltipElement = {
                id: point.id,
                name: matterName,
                sequenceRemoteId: point.sequenceRemoteId,
                feature: $scope.features[point.featureId].Text,
                attributes: $scope.getAttributesText(point.attributes),
                partial: point.partial,
                color: color,
                characteristics: point.subsequenceCharacteristics,
                similarity: similarity,
                selectedForAlignment: false
            };

            if (point.subsequenceRemoteId) {
                tooltipElement.remoteId = point.subsequenceRemoteId;
            }

            tooltipElement.position = "(";
            tooltipElement.length = 0;
            tooltipElement.positions = point.positions;
            tooltipElement.lengths = point.lengths;

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

            return tooltipElement;
        }

        function isKeyUpOrDown(keyCode) {
            return keyCode === 40 || keyCode === 38;
        }

        function cText(points, index) {
            return points.map(() => $scope.matters[index].name);
        }

        // selects nearest diffieret point of the same organism when "up" or "down" key pressed 
        function keyUpDownPress(keyCode) {
            let nextPointIndex = -1;
            let visibleMattersPoints = $scope.visiblePoints[$scope.selectedMatterIndex];

            if ($scope.selectedPointIndex >= 0) {
                let characteristic;
                let firstPointCharacteristic;
                let secondPointCharacteristic;
                switch (keyCode) {
                    case 38: // up
                        for (let i = $scope.selectedPointIndex + 1; i < visibleMattersPoints.length; i++) {
                            characteristic = $scope.subsequenceCharacteristic.Value;
                            firstPointCharacteristic = visibleMattersPoints[$scope.selectedPointIndex].subsequenceCharacteristics[characteristic];
                            secondPointCharacteristic = visibleMattersPoints[i].subsequenceCharacteristics[characteristic];

                            if (firstPointCharacteristic !== secondPointCharacteristic) {
                                nextPointIndex = i;
                                break;
                            }
                        }
                        break;
                    case 40: // down
                        for (let j = $scope.selectedPointIndex - 1; j >= 0; j--) {
                            characteristic = $scope.subsequenceCharacteristic.Value;
                            firstPointCharacteristic = visibleMattersPoints[$scope.selectedPointIndex].subsequenceCharacteristics[characteristic];
                            secondPointCharacteristic = visibleMattersPoints[j].subsequenceCharacteristics[characteristic];

                            if (firstPointCharacteristic !== secondPointCharacteristic) {
                                nextPointIndex = j;
                                break;
                            }
                        }
                        break;
                }
            }

            if (nextPointIndex >= 0) {
                $scope.showTooltip(visibleMattersPoints[nextPointIndex]);
                $scope.selectedPointIndex = nextPointIndex;
            }
        }

        // main drawing method
        function redrawGenesMap() {
            $scope.fillVisiblePoints();
            $scope.selectedPointIndex = -1;
            $scope.layout = {
                showlegend: false,
                hovermode: "closest",
                xaxis: {
                    type: $scope.plotTypeX ? 'log' : '',
                    title: {
                        text: $scope.sequenceCharacteristicName,
                        font: {
                            family: 'Courier New, monospace',
                            size: 12
                        }
                    }
                },
                yaxis: {
                    type: $scope.plotTypeY ? 'log' : '',
                    title: {
                        text: $scope.subsequenceCharacteristic.Text,
                        font: {
                            family: 'Courier New, monospace',
                            size: 12
                        }
                    }
                }
            };

            let data = $scope.visiblePoints.map((points, index) => ({
                hoverinfo: 'text+x+y',
                type: 'scattergl',
                x: points.map(p => $scope.numericXAxis ? p.numericX : p.x),
                y: points.map(p => p.subsequenceCharacteristics[$scope.subsequenceCharacteristic.Value]),
                text: cText(points, index),
                mode: "markers",
                marker: { opacity: 0.5, symbol: "circle-open", color: $scope.colorScale(index) },
                name: $scope.matters[index].name,
            }));

            Plotly.newPlot($scope.plot, data, $scope.layout, { responsive: true });

            $scope.plot.on("plotly_click", data => {
                $scope.selectedPointIndex = data.points[0].pointNumber;
                $scope.selectedMatterIndex = data.points[0].curveNumber;
                let selectedPoint = $scope.visiblePoints[data.points[0].curveNumber][data.points[0].pointNumber];
                $scope.showTooltip(selectedPoint);
            });
        }

        // hides or shows selected organism on genes map
        function legendClick(legendItem) {
            for (let j = 0; j < $scope.points[legendItem.index].length; j++) {
                let point = $scope.points[legendItem.index][j];
                point.legendVisible = !point.legendVisible;
            }

            $scope.redrawGenesMap();
        }

        function legendSetVisibilityForAll(visibility) {
            for (let i = 0; i < $scope.matters.length; i++) {
                let index = $scope.matters[i].index;
                for (let j = 0; j < $scope.points[index].length; j++) {
                    $scope.points[index][j].legendVisible = visibility;
                }
            }

            for (let k = 0; k < $scope.matters.length; k++) {
                $scope.matters[k].visible = visibility;
            }

            $scope.redrawGenesMap();
        }

        // dragbar
        function dragbarMouseDown() {
            let chart = document.getElementById('chart');
            let right = document.getElementById('sidebar');
            let bar = document.getElementById('dragbar');

            const drag = (e) => {
                document.selection ? document.selection.empty() : window.getSelection().removeAllRanges();
                let chart_width = chart.style.width = (e.pageX - bar.offsetWidth / 2) + 'px';

                Plotly.relayout('chart', { autosize: true });
            };

            bar.addEventListener('mousedown', () => {
                document.addEventListener('mousemove', drag);
            });

            bar.addEventListener('mouseup', () => {
                document.removeEventListener('mousemove', drag);
            });

        }

        // opens alignment of given subsequences with clustal on new tab
        function alignAllWithClustal() {
            let ids = $scope.tooltipElements.map(te => te.id);
            alignWithClustal(ids);
        }

        // opens alignment of given subsequences with clustal on new tab
        function alignSimilarWithClustal() {
            let ids = $scope.tooltipElements
                .filter(te => te.similarity === $scope.pointsSimilarity.same
                    || te.similarity === $scope.pointsSimilarity.similar)
                .map(te => te.id);
            alignWithClustal(ids);
        }

        // opens alignment of given subsequences with clustal on new tab
        function alignSelectedWithClustal() {
            let ids = $scope.tooltipElements
                .filter(te => te.selectedForAlignment)
                .map(te => te.id);
            alignWithClustal(ids);
        }

        // opens alignment of given subsequences with clustal on new tab
        function alignWithClustal(subsequencesIds) {
            $scope.alignmentInProcess = true;
            $http.get("/SubsequencesDistribution/CreateAlignmentTask/", { params: { subsequencesIds: subsequencesIds } })
                .then(response => {
                    $scope.alignmentInProcess = false;
                    let result = response.data;
                    if (result.Status === "Success") {
                        window.open("https://www.ebi.ac.uk/Tools/services/web/toolresult.ebi?jobId=" + result.Result, '_blank');
                    }
                    else {
                        alert("Failed to create alignment task", result.Message);
                        console.log(result.Message);
                    }
                }, () => {
                    alert("Failed to create alignment task");
                    $scope.alignmentInProcess = false;
                });
        }

        // calculates color for given matter index 
        // using d3.interpolate and matters count
        function colorScale(index) {

            // upper limit of color range
            let lastIndex = $scope.result.length;

            // color are shifted for odd and even indexes
            let colorCode = index / (2 * lastIndex);
            if (index % 2 != 0) {
                colorCode = colorCode + 0.5;
            }

            return d3.interpolateTurbo(colorCode);
        }

        $scope.onInit = onInit;
        $scope.fillPoints = fillPoints;
        $scope.setCheckBoxesState = SetCheckBoxesState;
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
        $scope.isKeyUpOrDown = isKeyUpOrDown;
        $scope.addCharacteristicComparer = addCharacteristicComparer;
        $scope.deleteCharacteristicComparer = deleteCharacteristicComparer;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.getAttributeIdByName = getAttributeIdByName;
        $scope.isAttributeEqual = isAttributeEqual;
        $scope.dragbarMouseDown = dragbarMouseDown;
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
