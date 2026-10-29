import type { SelectedCharacteristic } from "viewDataTypes";

/**
 * Interface for the scope in Angular controllers
 */
export interface AngularScope extends ng.IScope {
    [key: string]: any;
}
/**
 * Interface for data object
 */
interface DataObject {
    [key: string]: any;
}

export interface ResultScope extends AngularScope {
    loadingScreenHeader: string;
    loading: boolean;
    taskId: string;
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
    public static MapModelFromJson(scope: AngularScope, data: DataObject): void {
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
    public static SelectLink(characteristic: SelectedCharacteristic): void {
        characteristic.link = characteristic.characteristicType.Links[0];
        characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
    }
}

//TODO: try making this method generic
// For backward compatibility with existing JavaScript code
export function MapModelFromJson($scope: AngularScope, data: DataObject): void {
    LibiadaWebUtils.MapModelFromJson($scope, data);
}

/**
 * Initializes angular controller's scope using data from server
 * @param $http
 * @param $scope scope to initialize
 * @param loadingScreenHeader displayed message of loading screen
 * @param errorMessage displayed message in case of error
 */
export async function initScopeFromServer<ResponseType extends DataObject>(
    $http: ng.IHttpService,
    $scope: ResultScope,
    loadingScreenHeader: string,
    errorMessage: string = "Failed loading data from server"): Promise<void> {

    // Set loading message
    $scope.loadingScreenHeader = loadingScreenHeader;
    $scope.loading = true;
    try {
        // Extract task ID from URL
        let location: string[] = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        // Fetch data from server
        const result: { data: ResponseType } = await $http.get<ResponseType>(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`);

        MapModelFromJson($scope, result.data);

    } catch (error) {
        //TODO: change it to alertify
        alert(errorMessage);

    } finally {
        $scope.loading = false;
        $scope.$apply();
    }
}

export function SelectLink(characteristic: SelectedCharacteristic): void {
    LibiadaWebUtils.SelectLink(characteristic);
}

// Finds minimum and maximum values in the array of numbers
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

// Finds minimum value in the array of numbers
export function arrayMin(array: number[]): number {
    let min: number = array[0];
    let length: number = array.length;

    for (let i: number = 1; i < length; i++) {
        min = array[i] < min ? array[i] : min;
    }

    return min;
}


// Finds maximum value in the array of numbers
export function arrayMax(array: number[]): number {
    let max: number = array[0];
    let length: number = array.length;

    for (let i: number = 1; i < length; i++) {
        max = array[i] > max ? array[i] : max;
    }

    return max;
}

// Helper function to throw errors in ?? operator
export function throwHelper(errorMessage: string): never {
    throw new Error(errorMessage);
}

export function InitializeBootstrapTooltips() {
    const tooltipElements: HTMLElement[] = $('[data-bs-toggle="tooltip"]').toArray();
    tooltipElements.forEach((tooltipElement) => new bootstrap.Tooltip(tooltipElement));
}
