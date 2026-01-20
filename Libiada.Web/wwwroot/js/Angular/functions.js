/**
 * Utility functions for LibiadaWeb Angular application
 */
class LibiadaWebUtils {
    /**
     * Maps properties from source data object to the target scope
     * @param scope - The Angular scope to map properties to
     * @param data - The source data object to map from
     */
    static MapModelFromJson(scope, data) {
        let param;
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
    static SelectLink(characteristic) {
        characteristic.link = characteristic.characteristicType.Links[0];
        characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
    }
}
// For backward compatibility with existing JavaScript code
export function MapModelFromJson(scope, data) {
    LibiadaWebUtils.MapModelFromJson(scope, data);
}
export function SelectLink(characteristic) {
    LibiadaWebUtils.SelectLink(characteristic);
}
export function getArrayMinMax(array) {
    let min = array[0];
    let max = array[0];
    let length = array.length;
    for (let i = 1; i < length; i++) {
        if (array[i] < min)
            min = array[i];
        else if (array[i] > max)
            max = array[i];
    }
    return { min, max };
}
export function arrayMin(array) {
    let min = array[0];
    let length = array.length;
    for (let i = 1; i < length; i++) {
        min = array[i] < min ? array[i] : min;
    }
    return min;
}
export function arrayMax(array) {
    let max = array[0];
    let length = array.length;
    for (let i = 1; i < length; i++) {
        max = array[i] > max ? array[i] : max;
    }
    return max;
}
//# sourceMappingURL=functions.js.map