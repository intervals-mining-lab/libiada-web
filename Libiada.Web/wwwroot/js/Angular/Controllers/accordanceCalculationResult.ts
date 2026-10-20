import { initScopeFromServer } from "functions";
import type { ResultsLoadingScreenData } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface AccordanceCalculationResultData {
    alphabet?: string[];
    calculationType: string;
    characteristicName: string;
    characteristics: number[][];
    researchObjectNames: number[];
    firstAlphabet?: string[];
    secondAlphabet?: string[];
}

/**
 * Interface for the angular controller's scope
 */
interface AccordanceCalculationResultScope extends ng.IScope, AccordanceCalculationResultData, ResultsLoadingScreenData {
}

/**
 * Controller for accordance functionality
 */
class AccordanceCalculationResultHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    private ngOnInit(): void {
        const accordanceCalculationResult = ($scope: AccordanceCalculationResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<AccordanceCalculationResultData>(
                $http, $scope, "Loading accordance data", "Failed loading accordance data");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("AccordanceCalculationResultCtrl", ["$scope", "$http", accordanceCalculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of accordance calculation result handler
 */
export default function AccordanceCalculationResultController(): AccordanceCalculationResultHandler {
    return new AccordanceCalculationResultHandler();
}
