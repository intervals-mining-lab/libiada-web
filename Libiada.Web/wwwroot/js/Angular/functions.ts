/// <reference types="angular" />

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
interface ICharacteristicType {
    id: number;
    name: string;
    description?: string;
    Links: ILink[];
    ArrangementTypes: IArrangementType[];
    //[key: string]: any;
}

/**
 * Interface for the characteristic
 */
interface ICharacteristic {
    characteristicType: ICharacteristicType;
    link?: ILink;
    arrangementType?: IArrangementType;
//    [key: string]: any;
}

/**
 * Interface for the scope in Angular controllers
 */
interface IAngularScope extends ng.IScope {

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
        "use strict";

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
        "use strict";

        characteristic.link = characteristic.characteristicType.Links[0];
        characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
    }
}

// For backward compatibility with existing JavaScript code
function MapModelFromJson(scope: IAngularScope, data: IDataObject): void {
    LibiadaWebUtils.MapModelFromJson(scope, data);
}

function SelectLink(characteristic: ICharacteristic): void {
    LibiadaWebUtils.SelectLink(characteristic);
}
