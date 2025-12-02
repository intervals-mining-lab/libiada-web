/// <reference types="angular" />
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
        "use strict";
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
        "use strict";
        characteristic.link = characteristic.characteristicType.Links[0];
        characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
    }
}
// For backward compatibility with existing JavaScript code
function MapModelFromJson(scope, data) {
    LibiadaWebUtils.MapModelFromJson(scope, data);
}
function SelectLink(characteristic) {
    LibiadaWebUtils.SelectLink(characteristic);
}
//# sourceMappingURL=functions.js.map