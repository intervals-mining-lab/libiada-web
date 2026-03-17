import { MapModelFromJson } from "functions";
import type {
    Nature,
    SequenceType,
    Group,
    Language,
    Multisequence,
    Notation,
    RemoteDb,
    ResearchObject,
    Trajectory,
    Translator,
    SelectListItemWithNature
} from "viewDataTypes";

/**
 * Interface for the data object passed from the server
 */
interface CombinedSequencesEntityCreateData {
    groups: Group[];
    languages: Language[];
    multisequences: Multisequence[];
    natures: Nature[];
    notations: Notation[];
    remoteDbs: RemoteDb[];
    researchObjects: ResearchObject[];
    sequenceTypes: SequenceType[];
    trajectories: Trajectory[]
    translators: Translator[];
}

/**
 * Interface for the angular controller's scope
 */
interface CombinedSequencesEntityCreateScope extends ng.IScope, CombinedSequencesEntityCreateData {
    nature: string;
    languageId: string;
    translatorId: string;
    trajectorId: string;
    notation: string;
    notationsFiltered: Notation[];
    remoteDbId: number;
    remoteDbsFiltered: RemoteDb[];
    researchObjectId?: string;
    researchObjectsFiltered: ResearchObject[];
    group: string;
    groupsFiltered: Group[];
    sequenceType: string;
    sequenceTypesFiltered: SequenceType[];
    original: boolean;
    name: string;

    filterByNature: () => void;
    remoteIdChanged: (remoteId: string) => void;
    isRemoteDbDefined: () => boolean;
    filterByNatureIfExists: <T extends SelectListItemWithNature>(array: T[]) => T[] | void;
}

/**
 * Angular controller class for combined sequences entity creation view
 */
class CombinedSequencesEntityCreator {

    constructor(data: CombinedSequencesEntityCreateData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: CombinedSequencesEntityCreateData): void {
        const combinedSequencesEntityCreate = ($scope: CombinedSequencesEntityCreateScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);

            function filterByNatureIfExists<T extends SelectListItemWithNature>(array: T[]): T[] | void {
                if (angular.isDefined(array)) {
                    return filterFilter(array, { Nature: $scope.nature });
                }
            }

            function filterByNature(): void {
                $scope.notationsFiltered = $scope.filterByNatureIfExists<Notation>($scope.notations) ?? [];
                $scope.notation = $scope.notationsFiltered[0].Value;

                $scope.groupsFiltered = $scope.filterByNatureIfExists<Group>($scope.groups) ?? [];
                $scope.group = $scope.groupsFiltered[0].Value;

                $scope.sequenceTypesFiltered = $scope.filterByNatureIfExists<SequenceType>($scope.sequenceTypes) ?? [];
                $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;

                $scope.remoteDbsFiltered = $scope.filterByNatureIfExists<RemoteDb>($scope.remoteDbs) ?? [];

                $scope.researchObjectsFiltered = $scope.filterByNatureIfExists<ResearchObject>($scope.researchObjects) ?? [];
                if (angular.isDefined($scope.researchObjectsFiltered) &&
                    angular.isDefined($scope.researchObjectsFiltered[0])) {
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
            $scope.filterByNatureIfExists = filterByNatureIfExists;

            $scope.original = false;
            $scope.languageId = $scope.languages[0].Value;
            $scope.translatorId = $scope.translators[0].Value;
            $scope.nature = $scope.natures[0].Value;
            $scope.name = "";

        };

        angular.module("libiada").controller("CombinedSequencesEntityCreateCtrl", ["$scope", "filterFilter", combinedSequencesEntityCreate]);
    }
}

// Wrapper function for backwards compatibility
export default function CombinedSequencesEntityCreateController(data: CombinedSequencesEntityCreateData): CombinedSequencesEntityCreator {
    return new CombinedSequencesEntityCreator(data);
}
