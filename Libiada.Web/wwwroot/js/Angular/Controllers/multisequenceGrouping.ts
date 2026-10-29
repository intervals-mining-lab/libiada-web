import { MapModelFromJson } from "functions";

/**
 * Interface for groupable research object in multisequence grouping
 */
interface IMultisequenceGroupingResearchObject {
    Id: number;
    Name: string;
}


interface Multisequence {
    name: string;
    researchObjectIds: number[];

}

interface MultisequenceGroupingData{
    researchObjects: {
        [key:number]: string;
    };
    result: Multisequence[];
    ungroupedResearchObjects: {
        Id: number;
        Name: string;
    }[];
}

/**
 * Interface for the angular controller's scope
 */
interface IMultisequenceGroupingScope extends ng.IScope, MultisequenceGroupingData {
    // Loading state
    loadingScreenHeader: string;
    loading: boolean;

    // Methods
    bindResearchObject: (multiSequence: Multisequence, index: number) => void;
    unbindResearchObject: (multiSequence: Multisequence, researchObjectId: number) => void;
}

/**
 * Controller for multisequence grouping
 */
class MultisequenceGroupingHandler {
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
        const multisequenceGrouping = async ($scope: IMultisequenceGroupingScope, $http: ng.IHttpService): Promise<void> => {
            /**
             * Removes a research object from a multisequence and adds it to ungrouped list
             * @param multiSequence The multisequence to remove from
             * @param researchObjectId The ID of the research object to remove
             */
            $scope.unbindResearchObject = function (multiSequence: Multisequence, researchObjectId: number): void {
                multiSequence.researchObjectIds.splice(multiSequence.researchObjectIds.indexOf(researchObjectId), 1);
                $scope.ungroupedResearchObjects.push({
                    Id: researchObjectId,
                    Name: $scope.researchObjects[researchObjectId]
                });
            }

            /**
             * Adds a research object to a multisequence
             * @param multiSequence The multisequence to add to
             * @param index The index of the research object in ungrouped list
             */
            $scope.bindResearchObject = function (multiSequence: Multisequence, index: number): void {
                let researchObject = $scope.ungroupedResearchObjects[index];
                $scope.researchObjects[researchObject.Id] = researchObject.Name;
                multiSequence.researchObjectIds.push(researchObject.Id);
                $scope.ungroupedResearchObjects.splice(index, 1);
            }

            $scope.loadingScreenHeader = "Loading grouping results";
            $scope.loading = true;
            try {
                const response = await $http.get<MultisequenceGroupingData>(`/Multisequence/GroupResearchObjectsIntoMultisequences/`);
                MapModelFromJson($scope, response.data);
            } catch(error) {
                //TODO: change it to alertify
                alert("Failed loading grouping results");
            } finally {
                $scope.loading = false;
                $scope.$apply();
            }
        };

        // Register controller in Angular module
        angular.module("libiada").controller("MultisequenceGroupingCtrl", ["$scope", "$http", multisequenceGrouping]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of multisequence grouping handler
 */
export default function MultisequenceGroupingController(): MultisequenceGroupingHandler {
    return new MultisequenceGroupingHandler();
}
