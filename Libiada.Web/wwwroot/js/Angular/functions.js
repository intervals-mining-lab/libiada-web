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
//TODO: try making this method generic
// For backward compatibility with existing JavaScript code
export function MapModelFromJson($scope, data) {
    LibiadaWebUtils.MapModelFromJson($scope, data);
}
/**
 * Initializes angular controller's scope using data from server
 * @param $http
 * @param $scope scope to initialize
 * @param loadingScreenHeader displayed maeesage of loading screen
 * @param errorMessage displayed message in case of error
 */
export async function initScopeFromServer($http, $scope, loadingScreenHeader, errorMessage = "Failed loading data from server") {
    // Set loading message
    $scope.loadingScreenHeader = loadingScreenHeader;
    $scope.loading = true;
    try {
        // Extract task ID from URL
        let location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];
        // Fetch data from server
        const result = await $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`);
        MapModelFromJson($scope, result.data);
    }
    catch (error) {
        //TODO: change it to alertify
        alert(errorMessage);
    }
    finally {
        $scope.loading = false;
        $scope.$apply();
    }
}
export function SelectLink(characteristic) {
    LibiadaWebUtils.SelectLink(characteristic);
}
// Finds minimum and maximum values in the array of numbers
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
// Finds minimum value in the array of numbers
export function arrayMin(array) {
    let min = array[0];
    let length = array.length;
    for (let i = 1; i < length; i++) {
        min = array[i] < min ? array[i] : min;
    }
    return min;
}
// Finds maximum value in the array of numbers
export function arrayMax(array) {
    let max = array[0];
    let length = array.length;
    for (let i = 1; i < length; i++) {
        max = array[i] > max ? array[i] : max;
    }
    return max;
}
// Helper function to throw errors in ?? operator
export function throwHelper(errorMessage) {
    throw new Error(errorMessage);
}
//# sourceMappingURL=functions.js.map