/// <reference path="./commonInterfaces.d.ts" />

// Интерфейс для характеристики
interface IChartCharacteristic {
    id: number;
    value: string;
}

// Интерфейс для легенды
interface ILegendItem {
    id: number;
    name: string;
    visible: boolean;
    color: string;
}

// Интерфейс для элемента подсказки
interface ITooltipElement {
    id: number;
    name: string;
    sequenceRemoteId?: string;
    feature: string;
    attributes: string[];
    partial: boolean;
    color: string;
    characteristics: number[];
    similarity: number;
    selectedForAlignment: boolean;
    remoteId?: string;
    position: string;
    length: number;
    positions: number[];
    lengths: number[];
}

// Интерфейс для точки подпоследовательности
interface ISubsequencePoint {
    id: number;
    legendIndex: number;
    name: string;
    attributes: number[];
    partial: boolean;
    featureId: number;
    positions: number[];
    lengths: number[];
    subsequenceRemoteId?: string;
    sequenceRemoteId?: string;
    rank: number;
    characteristics: number[];
    featureVisible: boolean;
    legendVisible: boolean;
    filtersVisible: boolean[];
}

// Интерфейс для данных точки на графике, наследующийся от IBasePoint
interface ISubsequencesCalculationPoint extends IBasePoint {
    legendIndex?: number;
    legendId?: number;
    researchObjectName?: string;
    name?: string;  // Добавлено для совместимости с исходным JavaScript кодом
    researchObjectId?: number;
    sequenceRemoteId?: string;
    subsequencesData?: ISubsequencePoint[];
}

// Интерфейс для данных последовательности
interface ISequenceData {
    ResearchObjectName: string;
    ResearchObjectId: number;
    RemoteId?: string;
    SubsequencesData: {
        Id: number;
        Attributes: number[];
        Partial: boolean;
        FeatureId: number;
        Starts: number[];
        Lengths: number[];
        RemoteId?: string;
        CharacteristicsValues: number[];
    }[];
}

// Интерфейс для значения атрибута
interface IAttributeValue {
    attribute: number;
    value: string;
}

// Интерфейс для данных всего контроллера
interface ISubsequencesCalculationResultData {
    sequencesData: ISequenceData[];
    characteristicsList: string[];
    characteristicNames: string[];
    features: { Value: number; Text: string }[];
    attributes: string[];
    attributeValues: IAttributeValue[];
}

// Интерфейс для объекта области видимости контроллера
interface ISubsequencesCalculationResultScope extends ng.IScope {
    // Данные графика
    sequencesData: ISequenceData[];
    points: ISubsequencesCalculationPoint[];
    legend: ILegendItem[];
    visiblePoints: ISubsequencePoint[];
    chartCharacteristics: IChartCharacteristic[];
    chartsCharacterisrticsCount: number;
    characteristicsList: string[];
    characteristicNames: string[];
    tooltipElements: ITooltipElement[];
    characteristicComparers: any[];

    // Элементы управления и состояния
    chartElement: HTMLElement;
    colorScale: d3.ScaleSequential<string>;
    features: { Value: number; Text: string }[];
    attributes: string[];
    attributeValues: IAttributeValue[];
    productFilter: string;
    selectedPointIndex: number;
    selectedResearchObjectIndex: number;
    lineChart?: boolean;

    // Настройки графика
    chartData: any[];
    layout: any;
    pointSize: number;
    pointsSimilarity: { same: number; similar: number; different: number };

    // Состояния загрузки
    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
    characteristicsTableRendering: { rendered: boolean };

    // Методы
    addCharacteristic: () => number;
    deleteCharacteristic: (characteristic: IChartCharacteristic) => IChartCharacteristic[];
    fillPoints: () => void;
    showTooltip: (selectedTrace: ISubsequencesCalculationPoint) => void;
    fillPointTooltip: (point: ISubsequencePoint, researchObjectName: string, similarity: number) => ITooltipElement;
    fillLinePlotData: () => void;
    fillScatterPlotData: () => void;
    fill3dScatterPlotData: () => void;
    fillParallelCoordinatesPlotData: () => void;
    draw: () => void;
    fillLegend: () => void;
    legendClick: (legendItem: ILegendItem) => void;
    legendSetVisibilityForAll: (visibility: boolean) => void;
    dragbarMouseDown: () => void;
    getAttributesText: (attributes: number[]) => string[];
    getAttributeIdByName: (dot: ISubsequencePoint, attributeName: string) => number | undefined;
    isAttributeEqual: (dot: ISubsequencePoint, attributeName: string, expectedValue: string) => boolean;
    addFilter: (newFilter: string) => void;
    deleteFilter: (filter: string, filterIndex: number) => void;
    dotsSimilar: (d: ISubsequencePoint, dot: ISubsequencePoint) => boolean;
    dotVisible: (d: ISubsequencePoint) => boolean;
    fillVisiblePoints: () => void;
}
