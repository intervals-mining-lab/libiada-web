declare interface IBasePoint {
    id: number;         // Уникальный идентификатор точки
    x?: number;         // Позиция по оси X
    y?: number;         // Позиция по оси Y
    colorId?: number;   // Идентификатор цвета
    featureVisible?: boolean; // Видимость на графике
}

declare interface ITransformationVisibility {
    id: number;
    name: string;
    visible: boolean;
   
}