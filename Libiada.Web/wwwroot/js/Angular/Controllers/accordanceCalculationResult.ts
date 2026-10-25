import { initScopeFromServer } from "functions";
import type { ResultsLoadingScreenData } from "viewDataTypes";

interface AccordanceCalculationCompareAllResultData {
    firstAlphabet: string[];
    secondAlphabet: string[];
}

interface AccordanceCalculationCompareEqualResultData {
    alphabet: string[];
}

/**
 * Interface for the data object fetched from the server
 */
type AccordanceCalculationResultData = {
    calculationType: string;
    characteristicName: string;
    characteristics: number[][];
    researchObjectNames: number[];
} & AccordanceCalculationCompareAllResultData | AccordanceCalculationCompareEqualResultData;

/**
 * Interface for the angular controller's scope
 */
type AccordanceCalculationResultScope = ng.IScope & AccordanceCalculationResultData & ResultsLoadingScreenData;

/**
 * Angular controller class
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
