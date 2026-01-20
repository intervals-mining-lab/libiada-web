/**
 * Interface for the characteristic type
 */
interface ILink {
    id: number;
    name: string;
}

/**
 * Interface for arrangement type
 */
interface IArrangementType {
    id: number;
    name: string;
}

/**
 * Interface for the characteristic type
 */
export interface ICharacteristicType {
    id: number;
    name: string;
    description?: string;
    Links: ILink[];
    ArrangementTypes: IArrangementType[];
}

/**
 * Interface for the characteristic
 */
export interface ICharacteristic {
    characteristicType: ICharacteristicType;
    link?: ILink;
    arrangementType?: IArrangementType;
}

/**
 * Interface for the scope in Angular controllers
 */
interface IAngularScope extends angular.IScope {
    [key: string]: any;
}
/**
 * Interface for data object
 */
interface IDataObject {
    [key: string]: any;
}


/**
 * Utility functions for LibiadaWeb Angular application
 */
class LibiadaWebUtils {
    /**
     * Maps properties from source data object to the target scope
     * @param scope - The Angular scope to map properties to
     * @param data - The source data object to map from
     */
    public static MapModelFromJson(scope: IAngularScope, data: IDataObject): void {
        let param: string;
        for (param in data) {
            if (Object.prototype.hasOwnProperty.call(data, param)) {
                scope[param] = data[param];
            }
        }
    }

    /**
     * Selects the first link and arrangement type for a characteristic
     * @param characteristic - The characteristic object to modify
     */
    public static SelectLink(characteristic: ICharacteristic): void {
        characteristic.link = characteristic.characteristicType.Links[0];
        characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
    }
}

// For backward compatibility with existing JavaScript code
export function MapModelFromJson(scope: IAngularScope, data: IDataObject): void {
    LibiadaWebUtils.MapModelFromJson(scope, data);
}

export function SelectLink(characteristic: ICharacteristic): void {
    LibiadaWebUtils.SelectLink(characteristic);
}

export function getArrayMinMax(array: number[]): { min: number, max: number } {
    let min: number = array[0];
    let max: number = array[0];
    let length: number = array.length;

    for (let i: number = 1; i < length; i++) {
        if (array[i] < min) min = array[i];
        else if (array[i] > max) max = array[i];
    }

    return { min, max };
}

export function arrayMin(array: number[]): number {
    let min: number = array[0];
    let length: number = array.length;

    for (let i: number = 1; i < length; i++) {
        min = array[i] < min ? array[i] : min;
    }

    return min;
}

export function arrayMax(array: number[]): number {
    let max: number = array[0];
    let length: number = array.length;

    for (let i: number = 1; i < length; i++) {
        max = array[i] > max ? array[i] : max;
    }

    return max;
}
