// Interface for the $scope controller
interface MusicFilesScope extends ng.IScope {
}

// Controller class
class MusicFilesOperator {
    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const musicFiles = ($scope: MusicFilesScope): void => {
        };

        angular.module("libiada").controller("MusicFilesCtrl", ["$scope", musicFiles]);
    }
}

// Wrapper function for backwards compatibility
export default function MusicFilesController(): MusicFilesOperator {
    return new MusicFilesOperator();
}
