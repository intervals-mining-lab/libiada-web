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
export function initScopeFromServer($http, $scope, loadingScreenHeader, errorMessage = "Failed loading data from server") {
    // loading import results from the server
    $scope.loadingScreenHeader = loadingScreenHeader;
    $scope.loading = true;
    let location = window.location.href.split("/");
    $scope.taskId = location[location.length - 1];
    $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
        .then(function (data) {
        MapModelFromJson($scope, data.data);
        $scope.loading = false;
    })
        .catch(function () {
        alert(errorMessage);
        $scope.loading = false;
    });
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
//# sourceMappingURL=functions.js.map