/// <reference types="angular" />
/// <reference types="functions" />

// Interface for the data object passed to the controller
interface IResearchObjectSequenceCreateData {
    natures: INature[];
    languages?: ISelectOption[];
    translators?: ISelectOption[];
    notations?: ISelectOption[];
    remoteDbs?: ISelectOption[];
    researchObjects?: ISelectOption[];
    groups?: ISelectOption[];
    sequenceTypes?: ISelectOption[];
    [key: string]: any; // For any additional properties
}

// Interface for the controller scope
interface IResearchObjectSequenceCreateScope extends ng.IScope {
    nature: string | number;
    natures: INature[];
    languageId: string;
    languages?: ISelectOption[];
    translatorId: string;
    translators?: ISelectOption[];
    trajectorId: string;
    notationId: string;
    notations?: ISelectOption[];
    notationsFiltered?: ISelectOption[];
    remoteDbId: number;
    remoteDbs?: ISelectOption[];
    remoteDbsFiltered?: ISelectOption[];
    researchObjectId: string;
    researchObjects?: ISelectOption[];
    researchObjectsFiltered?: ISelectOption[];
    group: string;
    groups?: ISelectOption[];
    groupsFiltered?: ISelectOption[];
    sequenceType: string;
    sequenceTypes?: ISelectOption[];
    sequenceTypesFiltered?: ISelectOption[];
    original: boolean;
    name: string;
    multisequences: ISelectOption[];
    filterByNature: () => void;
    remoteIdChanged: (remoteId: string) => void;
    isRemoteDbDefined: () => boolean;
    [key: string]: any; // For any additional properties
}


// Auxiliary interfaces
interface INature {
    Value: number;
    Text: string;
    Nature: number;
    Group: number;
    Disabled: boolean;
    Selected: boolean;
}

interface ISelectOption {
    Value: string;
    Text: string;
    Nature?: number;
    Group?: number | null;
    Disabled?: boolean;
    Selected?: boolean;
    SequenceType?: string;
}

// Controller class
class ResearchObjectSequenceCreator {
    private data: IResearchObjectSequenceCreateData;

    constructor(data: IResearchObjectSequenceCreateData) {
        this.data = data;
        this.initializeController();
    }

    private initializeController(): void {
        "use strict";

        const researchObjectSequenceCreate = ($scope: IResearchObjectSequenceCreateScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, this.data);

         

            function filterByNature(): void {
                const arraysForFiltration: string[] = ["notations", "remoteDbs",
                    "researchObjects", "groups", "sequenceTypes"];

                arraysForFiltration.forEach(arrayName => {
                    if (angular.isDefined($scope[arrayName])) {
                        $scope[`${arrayName}Filtered`] = filterFilter($scope[arrayName], { Nature: $scope.nature });
                    }
                });

                $scope.notationId = $scope.notationsFiltered[0].Value
                $scope.group = $scope.groupsFiltered[0].Value;
                $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;
                if (angular.isDefined($scope.researchObjectsFiltered) && angular.isDefined(
                    $scope.researchObjectsFiltered[0])) {
                    $scope.researchObjectId = $scope.researchObjectsFiltered[0].Value;
                }
            }

            function remoteIdChanged(remoteId: string): void {
                const nameParts: string[] = $scope.name.split(" | ");
                if (nameParts.length <= 2) {
                    $scope.name = `${nameParts[0]}${remoteId ? ` | ${remoteId}` : ""}`;
                }
            }

            function isRemoteDbDefined(): boolean {
                return $scope.remoteDbsFiltered.length > 0 && $scope.remoteDbId > 0;
            }

            $scope.filterByNature = filterByNature;
            $scope.isRemoteDbDefined = isRemoteDbDefined;
            $scope.remoteIdChanged = remoteIdChanged;

            $scope.original = false;
            $scope.languageId = $scope.languages[0].Value;
            $scope.translatorId = $scope.translators[0].Value;
            $scope.nature = $scope.natures[0].Value as number;
            $scope.name = "";
            console.log("Scope - ", $scope, "\n");
        };

        angular.module("libiada").controller("ResearchObjectSequenceCreateCtrl", ["$scope", "filterFilter", researchObjectSequenceCreate]);
    }
}

// Wrapper function for backwards compatibility
function ResearchObjectSequenceCreateController(data: IResearchObjectSequenceCreateData): ResearchObjectSequenceCreator {
    return new ResearchObjectSequenceCreator(data);
}
