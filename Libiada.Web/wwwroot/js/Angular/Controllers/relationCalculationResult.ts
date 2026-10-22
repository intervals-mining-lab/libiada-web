import { initScopeFromServer } from "functions";
import type { ResultsLoadingScreenData } from "viewDataTypes";

/**
 * Interface for the data object fetched from the server
 */
interface RelationCalculationResultData {
    characteristicName: string;
    characteristics?: number[][];
    elements?: { Id: number, Name: string }[];
    isFilter: boolean;
    notationName: string;
    researchObjectName: string;
    filterSize?: number;
    filteredResult?: { FirstElementId: number, SecondElementId: number, Value: number }[][];
    firstElements?: string[];
    secondElements?: string[];
}

/**
 * Interface for the angular controller's scope
 */
interface RelationCalculationResultScope extends ng.IScope, RelationCalculationResultData, ResultsLoadingScreenData {
}

/**
 * Controller for relation calculation result view
 */
class RelationCalculationResultHandler {
    /**
     * Creates a new controller instance
     */
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes Angular controller
     */
    private ngOnInit(): void {
        const relationCalculationResult = ($scope: RelationCalculationResultScope, $http: ng.IHttpService): void => {
            initScopeFromServer<RelationCalculationResultData>(
                $http, $scope, "Loading calculation results", "Failed loading calculation results");
        };

        // Register controller in Angular module
        angular.module("libiada").controller("RelationCalculationResultCtrl", ["$scope", "$http", relationCalculationResult]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of relation calculation result handler
 */
export default function AccordanceResultController(): RelationCalculationResultHandler {
    return new RelationCalculationResultHandler();
}
