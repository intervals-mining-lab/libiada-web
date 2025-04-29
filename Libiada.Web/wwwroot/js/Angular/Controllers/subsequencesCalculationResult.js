/// <reference types="angular" />
/// <reference types="d3" />
/// <reference path="../functions.d.ts" />
/**
 * Обработчик для отображения результатов расчета подпоследовательностей
 */
class SubsequencesCalculationResultHandler {
    constructor() {
        this.initializeController();
    }
    /**
     * Инициализирует контроллер Angular
     */
    initializeController() {
        const subsequencesCalculationResult = ($scope, $http, $sce) => {
            "use strict";
            /**
             * Заполняет массив видимых точек
             */
            function fillVisiblePoints() {
                $scope.visiblePoints = [];
                for (let i = 0; i < $scope.points.length; i++) {
                    $scope.visiblePoints.push([]);
                    for (let j = 0; j < $scope.points[i].length; j++) {
                        if ($scope.dotVisible($scope.points[i][j])) {
                            $scope.visiblePoints[i].push($scope.points[i][j]);
                        }
                    }
                }
            }
            /**
             * Получает текст атрибутов для заданной подпоследовательности
             * @param attributes Массив идентификаторов атрибутов
             */
            function getAttributesText(attributes) {
                const attributesText = [];
                for (let i = 0; i < attributes.length; i++) {
                    const attributeValue = $scope.attributeValues[attributes[i]];
                    attributesText.push($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : ` = ${attributeValue.value}`));
                }
                return $sce.trustAsHtml(attributesText.join("<br/>"));
            }
            /**
             * Возвращает индекс атрибута по его имени, если таковой имеется
             * @param dot Точка данных
             * @param attributeName Имя атрибута
             */
            function getAttributeIdByName(dot, attributeName) {
                return dot.attributes.find(a => $scope.attributes[$scope.attributeValues[a].attribute] === attributeName);
            }
            /**
             * Возвращает true, если точка имеет заданный атрибут и его значение равно заданному значению
             * @param dot Точка данных
             * @param attributeName Имя атрибута
             * @param expectedValue Ожидаемое значение
             */
            function isAttributeEqual(dot, attributeName, expectedValue) {
                const attributeId = $scope.getAttributeIdByName(dot, attributeName);
                if (attributeId !== undefined) {
                    const product = $scope.attributeValues[attributeId].value.toUpperCase();
                    return product.indexOf(expectedValue) !== -1;
                }
                return false;
            }
            /**
             * Применяет новый фильтр
             * @param newFilter Строка фильтра
             */
            function addFilter(newFilter) {
                d3.selectAll(".dot")
                    .attr("visibility", (d) => {
                    const filterValue = newFilter.toUpperCase();
                    let visible = $scope.isAttributeEqual(d, "product", filterValue);
                    visible = visible || $scope.isAttributeEqual(d, "locus_tag", filterValue);
                    d.filtersVisible.push(visible);
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
                $scope.fillVisiblePoints();
            }
            /**
             * Удаляет заданный фильтр
             * @param filter Строка фильтра
             * @param filterIndex Индекс фильтра
             */
            function deleteFilter(filter, filterIndex) {
                d3.selectAll(".dot")
                    .attr("visibility", (d) => {
                    d.filtersVisible.splice(filterIndex, 1);
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
                $scope.fillVisiblePoints();
            }
            /**
             * Инициализирует данные для карты генов
             */
            function fillPoints() {
                $scope.researchObjects = [];
                $scope.points = [];
                for (let i = 0; i < $scope.sequencesData.length; i++) {
                    const sequenceData = $scope.sequencesData[i];
                    $scope.researchObjects.push({
                        id: sequenceData.ResearchObjectId,
                        name: sequenceData.ResearchObjectName,
                        visible: true,
                        colorId: i,
                        Nature: "",
                        Group: 0,
                        SequenceType: 0,
                        Multisequence: false,
                        Matter: 0,
                        MatterIds: [],
                        RemoteId: sequenceData.RemoteId || null,
                        Notation: 0,
                        NotationValue: 0,
                        LanguageId: null,
                        TranslatorId: null,
                        Characteristics: [],
                        Description: null
                    });
                    $scope.points.push([]);
                    for (let j = 0; j < sequenceData.SubsequencesData.length; j++) {
                        const subsequenceData = sequenceData.SubsequencesData[j];
                        const point = {
                            id: subsequenceData.Id,
                            researchObjectId: sequenceData.ResearchObjectId,
                            researchObjectName: sequenceData.ResearchObjectName,
                            sequenceRemoteId: sequenceData.RemoteId,
                            //attributes: subsequenceData.Attributes,
                            partial: subsequenceData.Partial,
                            featureId: subsequenceData.FeatureId,
                            positions: subsequenceData.Starts,
                            lengths: subsequenceData.Lengths,
                            subsequenceRemoteId: subsequenceData.RemoteId,
                            rank: j + 1,
                            characteristicsValues: subsequenceData.CharacteristicsValues,
                            colorId: i,
                            featureVisible: true,
                            legendVisible: true,
                            filtersVisible: [],
                            remoteId: subsequenceData.RemoteId // Добавляем для совместимости с JS-версией
                        };
                        $scope.points[i].push(point);
                    }
                }
            }
            /**
             * Фильтрует точки по признаку подпоследовательностей
             * @param feature Признак для фильтрации
             */
            function filterByFeature(feature) {
                const featureValue = parseInt(feature.Value);
                d3.selectAll(".dot")
                    .filter((dot) => dot.featureId === featureValue)
                    .attr("visibility", (d) => {
                    d.featureVisible = feature.Selected;
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
                for (let i = 0; i < $scope.points.length; i++) {
                    for (let j = 0; j < $scope.points[i].length; j++) {
                        if ($scope.points[i][j].featureId === parseInt(feature.Value)) {
                            $scope.points[i][j].featureVisible = feature.Selected;
                        }
                    }
                }
                // TODO: оптимизировать вызовы этого метода
                $scope.fillVisiblePoints();
            }
            /**
             * Проверяет, видима ли точка
             * @param dot Точка для проверки
             */
            function dotVisible(dot) {
                const filterVisible = dot.filtersVisible.length === 0 || dot.filtersVisible.some(element => element);
                return dot.featureVisible && dot.legendVisible && filterVisible;
            }
            /**
             * Определяет, похожи ли точки по продукту
             * @param d Первая точка
             * @param dot Вторая точка
             */
            function dotsSimilar(d, dot) {
                if (d.featureId !== dot.featureId) {
                    return false;
                }
                switch (d.featureId) {
                    case 1: // CDS
                    case 2: // RRNA
                    case 3: // TRNA
                        const firstProductId = $scope.getAttributeIdByName(d, "product");
                        const secondProductId = $scope.getAttributeIdByName(dot, "product");
                        if (firstProductId === undefined || secondProductId === undefined) {
                            return false;
                        }
                        const firstAttributeValue = $scope.attributeValues[firstProductId].value.toUpperCase();
                        const secondAttributeValue = $scope.attributeValues[secondProductId].value.toUpperCase();
                        if (firstAttributeValue !== secondAttributeValue) {
                            return false;
                        }
                        break;
                }
                return true;
            }
            /**
             * Показывает подсказку для точки или группы точек
             * @param event Событие мыши
             * @param d Точка данных
             * @param tooltip Элемент подсказки
             * @param svg Элемент SVG
             */
            function showTooltip(event, d, tooltip, svg) {
                $scope.clearTooltip(tooltip);
                const tooltipHtml = [];
                tooltip.style("opacity", 0.9);
                tooltip.selectedDots = svg.selectAll(".dot")
                    .filter((dot) => {
                    if ($scope.xValue(dot) === $scope.xValue(d)
                        && $scope.yValue(dot) === $scope.yValue(d)) {
                        tooltipHtml.push($scope.fillPointTooltip(dot));
                        return true;
                    }
                    else {
                        return false;
                    }
                })
                    .attr("rx", $scope.selectedDotRadius)
                    .attr("ry", $scope.selectedDotRadius);
                tooltip.html(tooltipHtml.join("</br></br>"));
                tooltip.style("left", `${event.pageX + 10}px`)
                    .style("top", `${event.pageY - 8}px`);
                tooltip.hideTooltip = false;
            }
            /**
             * Создает строку, представляющую текст всплывающей подсказки
             * @param d Точка данных
             */
            function fillPointTooltip(d) {
                const tooltipContent = [];
                const genBankLink = "<a target='_blank' rel='noopener' href='https://www.ncbi.nlm.nih.gov/nuccore/";
                // Используем remoteId вместо sequenceRemoteId для соответствия JS-версии
                const header = d.remoteId ? `${genBankLink}${d.remoteId}'>${d.researchObjectName}</a>` : d.researchObjectName;
                tooltipContent.push(header);
                if (d.remoteId) {
                    const peptideGenbankLink = `${genBankLink}${d.remoteId}'>Peptide ncbi page</a>`;
                    tooltipContent.push(peptideGenbankLink);
                }
                tooltipContent.push($scope.features[d.featureId]);
                tooltipContent.push($scope.getAttributesText(d.attributes));
                if (d.partial) {
                    tooltipContent.push("partial");
                }
                const start = d.positions[0] + 1;
                const end = d.positions[0] + d.lengths[0];
                const positionGenbankLink = d.remoteId ?
                    `${genBankLink}${d.remoteId}?from=${start}&to=${end}'>${d.positions.join(", ")}</a>` :
                    d.positions.join(", ");
                tooltipContent.push(`Position: ${positionGenbankLink}`);
                tooltipContent.push(`Length: ${d.lengths.join(", ")}`);
                // TODO: показать все характеристики
                tooltipContent.push(`(${$scope.xValue(d)}, ${$scope.yValue(d)})`);
                return tooltipContent.join("</br>");
            }
            /**
             * Очищает подсказку и снимает выделение с точек
             * @param tooltip Элемент подсказки
             */
            function clearTooltip(tooltip) {
                if (tooltip) {
                    if (tooltip.hideTooltip) {
                        tooltip.html("").style("opacity", 0);
                        if (tooltip.selectedDots) {
                            tooltip.selectedDots.attr("rx", $scope.dotRadius)
                                .attr("ry", $scope.dotRadius);
                        }
                    }
                    tooltip.hideTooltip = true;
                }
            }
            /**
             * Возвращает значение X для точки данных
             * @param d Точка данных
             */
            function xValue(d) {
                return $scope.lineChart ? d.rank : d.characteristicsValues[+$scope.firstCharacteristic.Value];
            }
            /**
             * Возвращает значение Y для точки данных
             * @param d Точка данных
             */
            function yValue(d) {
                return $scope.lineChart ? d.characteristicsValues[+$scope.firstCharacteristic.Value] : d.characteristicsValues[+$scope.secondCharacteristic.Value];
            }
            /**
             * Основной метод отрисовки графика
             */
            function draw() {
                $scope.loading = true;
                $scope.loadingScreenHeader = "Drawing...";
                $scope.fillPoints();
                // Удаление предыдущего графика и подсказки, если таковые имеются
                d3.select(".chart-tooltip").remove();
                d3.select(".chart-svg").remove();
                // Сортировка точек по выбранной характеристике
                if ($scope.lineChart) {
                    for (let i = 0; i < $scope.points.length; i++) {
                        $scope.points[i].sort((first, second) => $scope.yValue(second) - $scope.yValue(first));
                        for (let j = 0; j < $scope.points[i].length; j++) {
                            $scope.points[i][j].rank = j + 1;
                        }
                    }
                }
                // Все организмы видимы после перерисовки
                $scope.researchObjects.forEach(researchObject => { researchObject.visible = true; });
                $scope.points.forEach(points => {
                    points.forEach(point => {
                        point.legendVisible = true;
                        point.FeatureVisible = $scope.features[point.featureId].Selected;
                    });
                });
                // Настройки размера и отступов графика
                const margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
                const width = $scope.width - margin.left - margin.right;
                const height = $scope.height - margin.top - margin.bottom;
                // Расчет границ для точек
                const xMinArray = [];
                const xMaxArray = [];
                const yMaxArray = [];
                const yMinArray = [];
                $scope.points.forEach(points => {
                    xMinArray.push(d3.min(points, $scope.xValue));
                    xMaxArray.push(d3.max(points, $scope.xValue));
                    yMinArray.push(d3.min(points, $scope.yValue));
                    yMaxArray.push(d3.max(points, $scope.yValue));
                });
                // Настройка оси X
                // Расчет границ для точек
                const xMin = d3.min(xMinArray);
                const xMax = d3.max(xMaxArray);
                const xMargin = (xMax - xMin) * 0.05;
                const xScale = d3.scaleLinear()
                    .domain([xMin - xMargin, xMax + xMargin])
                    .range([0, width]);
                const xAxis = d3.axisBottom(xScale)
                    .tickSizeInner(-height)
                    .tickSizeOuter(0)
                    .tickPadding(10);
                $scope.xMap = (d) => xScale($scope.xValue(d));
                // Настройка оси Y
                const yMin = d3.min(yMinArray);
                const yMax = d3.max(yMaxArray);
                const yMargin = (yMax - yMin) * 0.05;
                const yScale = d3.scaleLinear()
                    .domain([yMin - yMargin, yMax + yMargin])
                    .range([height, 0]);
                const yAxis = d3.axisLeft(yScale)
                    .tickSizeInner(-width)
                    .tickSizeOuter(0)
                    .tickPadding(10);
                $scope.yMap = (d) => yScale($scope.yValue(d));
                // Настройка цвета заливки
                const color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.researchObjects.length]);
                // Добавление холста графика в тело веб-страницы
                const svg = d3.select("#chart").append("svg")
                    .attr("width", $scope.width)
                    .attr("height", $scope.height)
                    .attr("class", "chart-svg")
                    .append("g")
                    .attr("transform", `translate(${margin.left},${margin.top})`);
                // Добавление области подсказки на веб-страницу
                const tooltip = d3.select("#chart").append("div")
                    .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
                    .style("opacity", 0);
                // Предотвращение скрытия подсказки при клике на нее
                tooltip.on("click", () => { tooltip.hideTooltip = false; });
                // Скрытие подсказки при клике вне нее
                d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });
                // Ось X
                svg.append("g")
                    .attr("class", "x axis")
                    .attr("transform", `translate(0,${height})`)
                    .call(xAxis);
                svg.append("text")
                    .attr("class", "label")
                    .attr("transform", `translate(${width / 2} ,${height + margin.top - $scope.legendHeight})`)
                    .style("text-anchor", "middle")
                    .text($scope.lineChart ? "Rank" : $scope.firstCharacteristic.Text)
                    .style("font-size", "12pt");
                // Ось Y
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
                    .text($scope.lineChart ? $scope.firstCharacteristic.Text : $scope.secondCharacteristic.Text)
                    .style("font-size", "12pt");
                const researchObjectsGroups = svg.selectAll(".researchObject")
                    .data($scope.points)
                    .enter()
                    .append("g")
                    .attr("class", "researchObject");
                // Рисование точек
                researchObjectsGroups.selectAll(".dot")
                    .data((d) => d)
                    .enter()
                    .append("ellipse")
                    .attr("class", "dot")
                    .attr("rx", $scope.dotRadius)
                    .attr("ry", $scope.dotRadius)
                    .attr("cx", $scope.xMap)
                    .attr("cy", $scope.yMap)
                    .style("fill-opacity", 0.6)
                    .style("fill", (d) => color(d.colorId))
                    .style("stroke", (d) => color(d.colorId))
                    .attr("visibility", (d) => $scope.dotVisible(d) ? "visible" : "hidden")
                    .on("click", (event, d) => $scope.showTooltip(event, d, tooltip, svg));
                // Рисование легенды
                const legend = svg.selectAll(".legend")
                    .data($scope.researchObjects)
                    .enter()
                    .append("g")
                    .attr("class", "legend")
                    .attr("transform", (_d, i) => "translate(0," + i * 20 + ")")
                    .on("click", function (event, d) {
                    d.visible = !d.visible;
                    const legendEntry = d3.select(event.currentTarget);
                    legendEntry.select("text")
                        .style("opacity", () => d.visible ? 1 : 0.5);
                    legendEntry.select("rect")
                        .style("fill-opacity", () => d.visible ? 1 : 0);
                    svg.selectAll(".dot")
                        .filter((dot) => dot.researchObjectId === d.id)
                        .attr("visibility", (dot) => {
                        dot.legendVisible = d.visible;
                        return $scope.dotVisible(dot) ? "visible" : "hidden";
                    });
                });
                // Рисование цветных прямоугольников легенды
                legend.append("rect")
                    .attr("width", 15)
                    .attr("height", 15)
                    .style("fill", (d) => color(d.colorId))
                    .style("stroke", (d) => color(d.colorId))
                    .style("stroke-width", 4)
                    .attr("transform", `translate(0, -${$scope.legendHeight})`);
                // Рисование текста легенды
                legend.append("text")
                    .attr("x", 24)
                    .attr("y", 9)
                    .attr("dy", ".35em")
                    .attr("transform", `translate(0, -${$scope.legendHeight})`)
                    .text((d) => d.name)
                    .style("font-size", "9pt");
                $scope.loading = false;
            }
            // Регистрация функций в $scope
            $scope.draw = draw;
            $scope.dotVisible = dotVisible;
            $scope.dotsSimilar = dotsSimilar;
            $scope.fillVisiblePoints = fillVisiblePoints;
            $scope.filterByFeature = filterByFeature;
            $scope.getAttributesText = getAttributesText;
            $scope.fillPoints = fillPoints;
            $scope.fillPointTooltip = fillPointTooltip;
            $scope.showTooltip = showTooltip;
            $scope.clearTooltip = clearTooltip;
            $scope.yValue = yValue;
            $scope.xValue = xValue;
            $scope.addFilter = addFilter;
            $scope.deleteFilter = deleteFilter;
            $scope.getAttributeIdByName = getAttributeIdByName;
            $scope.isAttributeEqual = isAttributeEqual;
            // Инициализация свойств $scope
            $scope.dotRadius = 3;
            $scope.selectedDotRadius = $scope.dotRadius * 3;
            $scope.visiblePoints = [];
            $scope.characteristicComparers = [];
            $scope.productFilter = "";
            $scope.loadingScreenHeader = "Loading subsequences characteristics";
            $scope.loading = true;
            // Получение идентификатора задачи из URL
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            // Загрузка данных с сервера
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.legendHeight = $scope.sequencesData.length * 20;
                $scope.height = 800 + $scope.legendHeight;
                $scope.width = 800;
                $scope.firstCharacteristic = $scope.subsequencesCharacteristicsList[0];
                $scope.secondCharacteristic = $scope.subsequencesCharacteristicsList[$scope.subsequencesCharacteristicsList.length - 1];
                $scope.loading = false;
            })
                .catch(function () {
                alert("Failed loading subsequences characteristics");
                $scope.loading = false;
            });
        };
        // Регистрация контроллера в модуле Angular
        angular.module("libiada").controller("SubsequencesCalculationResultCtrl", ["$scope", "$http", "$sce", subsequencesCalculationResult]);
    }
}
/**
 * Функция-обертка для обратной совместимости
 */
function SubsequencesCalculationResultController() {
    return new SubsequencesCalculationResultHandler();
}
//# sourceMappingURL=subsequencesCalculationResult.js.map