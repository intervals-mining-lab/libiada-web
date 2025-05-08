/// <reference types="angular" />
/// <reference types="d3" />
/// <reference types="functions" />
/// <reference path="./Interfaces/commonInterfaces.d.ts" />

type IScope = ng.IScope;
interface IPoint extends IBasePoint {
    value: number;      // Значение (идентификатор порядка)
    transformationVisibility: ITransformationVisibility[];
}



// Интерфейс для линии между точками
interface ILine {
    id: number;
    value: string;
    arrowType: number;
    iterator: number;
    x1: number;
    y1: number;
    x2: number;
    y2: number;
    startOrderId: number;
    colorId: number;
}

// Интерфейс для элемента легенды
interface ILegendItem {
    id: number;
    name: string;
    visible: boolean;
}

 //Интерфейс для видимости трансформации
interface ITransformationVisibility {
    id: number;
    name: string;
    visible: boolean;
}

// Интерфейс для ID порядка
interface IOrderId {
    id: number;
}

// Интерфейс для типа трансформации
interface ITransformationType {
    Text: string;
    Value?: number;
}

// Интерфейс для данных трансформации
interface ITransformationData {
    ResultTransformation: IResultTransformation[];
}

// Интерфейс для результата трансформации
interface IResultTransformation {
    Transformation: string;
    OrderId: number;
}

// Интерфейс для данных контроллера
interface IOrderTransformationVisualizationData {
    orders: string[];
    transformationsData: { [key: number]: ITransformationData };
    transformationsList: ITransformationType[];
    [key: string]: any;
}

// Интерфейс для области видимости контроллера
interface IOrderTransformationVisualizationScope extends ng.IScope {
    // Данные от сервера
    orders: string[];
    transformationsData: { [key: number]: ITransformationData };
    transformationsList: ITransformationType[];

    // Настройки графика
    width: number;
    height: number;
    dotRadius: number;
    selectedDotRadius: number;
    legendHeight: number;

    // Элементы графика
    points: IPoint[];
    lines: ILine[];
    legend: ILegendItem[];
    ordersIds: IOrderId[];

    // Текущие настройки
    initialOrder: IOrderId;
    transformationType: ITransformationType;
    counterIteration: number;

    // Свойства для загрузки данных
    taskId: string;
    loadingScreenHeader: string;
    loading: boolean;

    // Функции графика
    xMap: (d: IPoint) => number;
    yMap: (d: IPoint) => number;

    // Методы
    draw: () => void;
    fillPointsAndLines: () => void;
    fillLegend: () => void;
    fillPointTooltip: (d: IPoint) => string;
    showTooltip: (event: MouseEvent, d: IPoint, tooltip: any, svg: any) => void;
    clearTooltip: (tooltip: any) => void;
    xValue: (d: IPoint) => number;
    yValue: (d: IPoint) => number;
}

// Определение интерфейса для tooltip с дополнительными свойствами
interface ID3Tooltip extends d3.Selection<HTMLDivElement, unknown, HTMLElement, any> {
    hideTooltip?: boolean;
    selectedDots?: any;
}

// Методы в интерфейсе IOrderTransformationVisualizationScope




/**
 * Контроллер для визуализации результатов трансформации порядков
 */
class OrderTransformationVisualizationResultHandler {
    constructor() {
        this.initializeController();
    }

    private initializeController(): void {
        "use strict";

        const orderTransformationVisualizationResult = ($scope: IOrderTransformationVisualizationScope, $http: ng.IHttpService): void => {

            /**
             * Инициализирует данные для легенды
             */
            function fillLegend(): void {
                $scope.legend = [];
                for (let i = 1; i < $scope.transformationsList.length; i++) {
                    $scope.legend.push({ id: i - 1, name: $scope.transformationsList[i].Text, visible: true });
                }
            }

            /**
             * Инициализирует данные для точек графика
             */
            function fillPoints(): void {
                let initialOrder = $scope.initialOrder.id;
                let checkedOrders = [initialOrder];
                let ordersForChecking = [initialOrder];
                let transformationVisibility: ITransformationVisibility[] = [];

                for (let l = 0; l < $scope.legend.length; l++) {
                    transformationVisibility.push({
                        id: l,
                        name: $scope.legend[l].name,
                        visible: $scope.legend[l].visible
                    });
                }

                $scope.points.push({
                    id: 0,
                    value: initialOrder,
                    x: 0,
                    y: initialOrder,
                    transformationVisibility: transformationVisibility,
                    colorId: 0
                });

                let counterIdPoints = 1;
                $scope.counterIteration = 1;

                while (ordersForChecking.length > 0) {
                    let newOrdersForChecking: number[] = [];

                    for (let i = 0; i < ordersForChecking.length; i++) {
                        let resultTransformations = $scope.transformationsData[ordersForChecking[i]].ResultTransformation;

                        for (let j = 0; j < resultTransformations.length; j++) {
                            let resultTransformation = resultTransformations[j];

                            if ($scope.transformationType.Text === "All" || resultTransformation.Transformation === $scope.transformationType.Text) {
                                let pointExist = false;
                                let orderId = resultTransformation.OrderId;

                                for (let k = 0; k < $scope.points.length; k++) {
                                    if ($scope.points[k].x === $scope.counterIteration &&
                                        $scope.points[k].y === orderId) {
                                        pointExist = true;
                                        break;
                                    }
                                }

                                if (!pointExist) {
                                    $scope.points.push({
                                        id: counterIdPoints++,
                                        value: orderId,
                                        x: $scope.counterIteration,
                                        y: orderId,
                                        transformationVisibility: transformationVisibility,
                                        colorId: 0
                                    });
                                }

                                if (checkedOrders.indexOf(orderId) === -1) {
                                    checkedOrders.push(orderId);
                                    newOrdersForChecking.push(orderId);
                                }
                            }
                        }
                    }

                    ordersForChecking = newOrdersForChecking;
                    $scope.counterIteration++;
                }
            }

            /**
             * Инициализирует данные для линий графика
             */
            function fillLines(): void {
                let counterIdLines = 0;

                for (let i = 0; i < $scope.points.length; i++) {
                    let resultTransformation = $scope.transformationsData[$scope.points[i].value].ResultTransformation;

                    for (let j = 0; j < resultTransformation.length; j++) {
                        let transformationType = resultTransformation[j].Transformation;

                        if ($scope.transformationType.Text === "All" || transformationType === $scope.transformationType.Text) {
                            let colorId = $scope.legend.find(d => d.name === transformationType).id;
                            let childOrder = resultTransformation[j].OrderId;

                            let line = {
                                x1: $scope.points[i].x,
                                y1: $scope.points[i].y,
                                x2: $scope.points[i].x + 1,
                                y2: childOrder,
                                value: transformationType,
                                startOrderId: $scope.points[i].value,
                                colorId: colorId
                            };

                            let orderExist = false;

                            for (let k = 0; k < $scope.points.length; k++) {
                                if ($scope.points[k].x === $scope.points[i].x + 1 &&
                                    $scope.points[k].y === childOrder) {
                                    orderExist = true;
                                    break;
                                }
                            }

                            if (orderExist) {
                                let lineExist = false;
                                let lineIterator = 0;

                                for (let m = 0; m < $scope.lines.length; m++) {
                                    if ($scope.lines[m].x1 === line.x1 &&
                                        $scope.lines[m].y1 === line.y1 &&
                                        $scope.lines[m].x2 === line.x2 &&
                                        $scope.lines[m].y2 === line.y2) {
                                        lineExist = true;
                                        lineIterator = ++$scope.lines[m].iterator;
                                        break;
                                    }
                                }

                                if (lineExist) {
                                    let cyline = (line.y1 + line.y2) / 2.0;
                                    let cxline = (line.x1 + line.x2) / 2.0;
                                    let yAmplitude = line.y2 - line.y1;
                                    let shifty = 0.2 * lineIterator;
                                    let shiftx = 0.05 * lineIterator * Math.abs(yAmplitude) / $scope.ordersIds.length;

                                    $scope.lines.push({
                                        id: counterIdLines++,
                                        value: line.value,
                                        arrowType: -1,
                                        iterator: 0,
                                        x1: line.x1,
                                        y1: line.y1,
                                        x2: cxline + shiftx,
                                        y2: cyline + shifty,
                                        startOrderId: line.startOrderId,
                                        colorId: line.colorId
                                    });

                                    $scope.lines.push({
                                        id: counterIdLines++,
                                        value: line.value,
                                        arrowType: j,
                                        iterator: 0,
                                        x1: cxline + shiftx,
                                        y1: cyline + shifty,
                                        x2: line.x2,
                                        y2: line.y2,
                                        startOrderId: line.startOrderId,
                                        colorId: line.colorId
                                    });
                                } else {
                                    $scope.lines.push({
                                        id: counterIdLines++,
                                        value: line.value,
                                        arrowType: j,
                                        iterator: 0,
                                        x1: line.x1,
                                        y1: line.y1,
                                        x2: line.x2,
                                        y2: line.y2,
                                        startOrderId: line.startOrderId,
                                        colorId: line.colorId
                                    });
                                }
                            }
                        }
                    }
                }
            }

            /**
             * Инициализирует данные для точек и линий графика
             */
            function fillPointsAndLines(): void {
                $scope.points = [];
                $scope.lines = [];
                fillPoints();
                fillLines();
            }

            /**
             * Формирует текст всплывающей подсказки
             * @param d Точка, для которой формируется подсказка
             */
            function fillPointTooltip(d: IPoint): string {
                let tooltipContent: string[] = [];
                tooltipContent.push(`Order ID: ${d.value}`);
                tooltipContent.push(`Order: ${$scope.orders[d.value]}`);
                return tooltipContent.join("</br>");
            }

            /**
             * Показывает всплывающую подсказку при клике на точку
             * @param event Событие мыши
             * @param d Точка данных
             * @param tooltip Элемент подсказки
             * @param svg Элемент SVG
             */
            function showTooltip(event: MouseEvent, d: IPoint, tooltip: ID3Tooltip, svg: any): void {
                $scope.clearTooltip(tooltip);
                let color = d3.scaleOrdinal(d3.schemeCategory10);

                tooltip.style("opacity", 0.9);

                let tooltipHtml: string[] = [];

                tooltip.selectedDots = svg.selectAll(".dot")
                    .filter((dot: IPoint) => {
                        if (dot.x === d.x && dot.y === d.y) {
                            tooltipHtml.push($scope.fillPointTooltip(dot));
                            return true;
                        } else {
                            return false;
                        }
                    })
                    .attr("rx", $scope.selectedDotRadius)
                    .attr("ry", $scope.selectedDotRadius);

                tooltip.html(tooltipHtml.join("</br></br>"));

                tooltip.append("br");
                tooltip.append("div")
                    .append("svg")
                    .attr("height", $scope.legend.length * 20)
                    .attr("width", 20)
                    .selectAll(".dotlegend")
                    .data(d.transformationVisibility)
                    .enter()
                    .append("g")
                    .attr("class", "dotlegend")
                    .attr("transform", (vt: ITransformationVisibility, i: number) => `translate(0,${i * 20})`)
                    .append("rect")
                    .attr("width", 15)
                    .attr("height", 15)
                    .style("fill", (vt: ITransformationVisibility) => color(vt.name))
                    .style("stroke", (vt: ITransformationVisibility) => color(vt.name))
                    .style("stroke-width", 4)
                    .style("fill-opacity", (vt: ITransformationVisibility) => vt.visible ? 1 : 0)
                    .on("click", function (event: MouseEvent, vt: ITransformationVisibility) {
                        vt.visible = !vt.visible;
                        d3.select(event.currentTarget as Element).style("fill-opacity", () => vt.visible ? 1 : 0);
                        svg.selectAll(".transform-line")
                            .filter((line: ILine) => line.startOrderId === d.value && line.value === vt.name)
                            .attr("visibility", () => vt.visible ? "visible" : "hidden");
                    });

                tooltip.style("left", `${event.pageX + 10}px`)
                    .style("top", `${event.pageY - 8}px`);

                tooltip.hideTooltip = false;
            }

            /**
             * Очищает всплывающую подсказку
             * @param tooltip Элемент подсказки
             */
            function clearTooltip(tooltip: ID3Tooltip): void {
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
             * Возвращает значение X для точки
             * @param d Точка данных
             */
            function xValue(d: IPoint): number {
                return d.x;
            }

            /**
             * Возвращает значение Y для точки
             * @param d Точка данных
             */
            function yValue(d: IPoint): number {
                return d.y;
            }

            /**
             * Отрисовывает график
             */
            function draw(): void {
                $scope.fillPointsAndLines();

                // Удаление предыдущего графика и подсказки, если они есть
                d3.select(".chart-tooltip").remove();
                d3.select(".chart-svg").remove();

                // Настройки размеров и отступов графика
                let margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
                let width = $scope.width - margin.left - margin.right;
                let height = $scope.height - margin.top - margin.bottom;

                // Настройка оси X
                // Расчет отступов для точек
                let xMin = d3.min($scope.points, $scope.xValue) || 0;
                let xMax = d3.max($scope.points, $scope.xValue) || 0;
                let xMargin = (xMax - xMin) * 0.05;

                let xScale = d3.scaleLinear()
                    .domain([xMin - xMargin, xMax + xMargin])
                    .range([0, width]);

                let xAxis = d3.axisBottom(xScale)
                    .ticks($scope.counterIteration)
                    .tickSizeInner(-height)
                    .tickSizeOuter(0)
                    .tickPadding(10);

                $scope.xMap = (d: IPoint) => xScale($scope.xValue(d));

                // Настройка оси Y
                // Расчет отступов для точек
                let yMax = d3.max($scope.points, $scope.yValue) || 0;
                let yMin = d3.min($scope.points, $scope.yValue) || 0;
                let yMargin = (yMax - yMin) * 0.05;

                let yAmplitude = yMax - yMin;

                let yScale = d3.scaleLinear()
                    .domain([yMin - yMargin, yMax + yMargin])
                    .range([height, 0]);

                let yAxis = d3.axisLeft(yScale)
                    .ticks(yAmplitude > 20 ? yAmplitude / 10 : yAmplitude)
                    .tickSizeInner(-width)
                    .tickSizeOuter(0)
                    .tickPadding(10);

                $scope.yMap = (d: IPoint) => yScale($scope.yValue(d));

                // Настройка цвета заливки
                let color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);

                // Добавление холста графика на страницу
                let svg = d3.select("#chart").append("svg")
                    .attr("width", $scope.width)
                    .attr("height", $scope.height)
                    .attr("class", "chart-svg");

                let g = svg.append("g")
                    .attr("transform", `translate(${margin.left},${margin.top})`);

                // Добавление определений для концов линий
                let defs = svg.append("defs");
                for (let i = 0; i < $scope.legend.length; i++) {
                    defs.append("marker")
                        .attr("id", `arrow${i}`)
                        .attr("viewBox", "0 -5 10 10")
                        .attr("refX", 6)
                        .attr("refY", 0)
                        .attr("markerWidth", 6)
                        .attr("markerHeight", 6)
                        .attr("orient", "auto")
                        .append("path")
                        .attr("d", "M0,-5L10,0L0,5")
                        .attr("stroke", color($scope.legend[i].id))
                        .attr("fill", color($scope.legend[i].id));
                }

                // Добавление области подсказки на страницу
                let tooltip = d3.select("#chart").append("div")
                    .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
                    .style("opacity", 0);

                // Предотвращение скрытия подсказки при клике на нее
                tooltip.on("click", () => { (tooltip as any).hideTooltip = false; });

                // Скрытие подсказки при клике вне элементов
                d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });

                // Ось X
                g.append("g")
                    .attr("class", "x axis")
                    .attr("transform", `translate(0,${height})`)
                    .call(xAxis);

                g.append("text")
                    .attr("class", "label")
                    .attr("transform", `translate(${width / 2} ,${height + margin.top - $scope.legendHeight})`)
                    .style("text-anchor", "middle")
                    .text("Iteration")
                    .style("font-size", "12pt");

                // Ось Y
                g.append("g")
                    .attr("class", "y axis")
                    .call(yAxis);

                g.append("text")
                    .attr("class", "label")
                    .attr("transform", "rotate(-90)")
                    .attr("y", 0 - margin.left)
                    .attr("x", 0 - (height / 2))
                    .attr("dy", ".71em")
                    .style("text-anchor", "middle")
                    .text("Order Id")
                    .style("font-size", "12pt");

                // Отрисовка линий
                g.selectAll(".transform-line")
                    .data($scope.lines)
                    .enter()
                    .append("line")
                    .attr("class", "transform-line")
                    .attr("x1", (d: ILine) => xScale(d.x1))
                    .attr("y1", (d: ILine) => yScale(d.y1))
                    .attr("x2", (d: ILine) => xScale(d.x2))
                    .attr("y2", (d: ILine) => yScale(d.y2))
                    .attr("marker-end", (d: ILine) => `url(#arrow${d.arrowType})`)
                    .style("stroke", (d: ILine) => color(d.colorId))
                    .style("stroke-width", "2")
                    .attr("visibility", "visible");

                // Отрисовка легенды
                let legend = g.selectAll(".legend")
                    .data($scope.legend)
                    .enter()
                    .append("g")
                    .attr("class", "legend")
                    .attr("transform", (d: ILegendItem, i: number) => `translate(0,${i * 20})`)
                    .on("click", function (event: MouseEvent, d: ILegendItem) {
                        d.visible = !d.visible;
                        let legendEntry = d3.select(event.currentTarget as Element);

                        legendEntry.select("text")
                            .style("opacity", () => d.visible ? 1 : 0.5);

                        legendEntry.select("rect")
                            .style("fill-opacity", () => d.visible ? 1 : 0);

                        svg.selectAll(".transform-line")
                            .filter((line: ILine) => line.value === d.name)
                            .attr("visibility", () => d.visible ? "visible" : "hidden");

                        for (let k = 0; k < $scope.points.length; k++) {
                            for (let j = 0; j < $scope.legend.length; j++) {
                                if ($scope.points[k].transformationVisibility[j].name === d.name) {
                                    $scope.points[k].transformationVisibility[j].visible = d.visible;
                                }
                            }
                        }
                    });

                // Отрисовка точек
                g.selectAll(".dot")
                    .data($scope.points)
                    .enter()
                    .append("ellipse")
                    .attr("class", "dot")
                    .attr("rx", $scope.dotRadius)
                    .attr("ry", $scope.dotRadius)
                    .attr("cx", $scope.xMap)
                    .attr("cy", $scope.yMap)
                    .style("fill-opacity", 0.6)
                    .style("fill", "black")
                    .style("stroke", "black")
                    .on("click", (event: MouseEvent, d: IPoint) => $scope.showTooltip(event, d, tooltip, g));

                // Отрисовка цветных прямоугольников легенды
                legend.append("rect")
                    .attr("width", 15)
                    .attr("height", 15)
                    .style("fill", (d: ILegendItem) => color(d.id))
                    .style("stroke", (d: ILegendItem) => color(d.id))
                    .style("stroke-width", 4)
                    .attr("transform", `translate(0, -${$scope.legendHeight})`);

                // Отрисовка текста легенды
                legend.append("text")
                    .attr("x", 24)
                    .attr("y", 9)
                    .attr("dy", ".35em")
                    .attr("transform", `translate(0, -${$scope.legendHeight})`)
                    .text((d: ILegendItem) => d.name)
                    .style("font-size", "9pt");
            }

            // Назначение методов в $scope
            $scope.draw = draw;
            $scope.fillPointsAndLines = fillPointsAndLines;
            $scope.fillPointTooltip = fillPointTooltip;
            $scope.showTooltip = showTooltip;
            $scope.clearTooltip = clearTooltip;
            $scope.yValue = yValue;
            $scope.xValue = xValue;

            // Инициализация свойств $scope
            $scope.width = 800;
            $scope.dotRadius = 4;
            $scope.selectedDotRadius = $scope.dotRadius * 2;

            $scope.fillLegend = fillLegend;

            $scope.loadingScreenHeader = "Loading data";

            // Получение ID задачи из URL
            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];

            $scope.loading = true;

            // Загрузка данных с сервера
            $http.get < any > (`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                    MapModelFromJson($scope, data.data);

                    $scope.ordersIds = [];
                    for (let i = 0; i < $scope.orders.length; i++) {
                        $scope.ordersIds.push({ id: i });
                    }

                    $scope.fillLegend();
                    $scope.legendHeight = $scope.legend.length * 20;
                    $scope.height = 800 + $scope.legendHeight;
                    $scope.initialOrder = $scope.ordersIds[0];
                    $scope.transformationType = $scope.transformationsList[0];

                    $scope.loading = false;
                })
                .catch(function () {
                    alert("Failed loading characteristic data");
                    $scope.loading = false;
                });
        };

        // Регистрация контроллера в модуле Angular
        angular.module("libiada").controller("OrderTransformationVisualizationResultCtrl", ["$scope", "$http", orderTransformationVisualizationResult]);
    }
}

/**
 * Обертка для обратной совместимости
 */
function OrderTransformationVisualizationResultController(): OrderTransformationVisualizationResultHandler {
    return new OrderTransformationVisualizationResultHandler();
}