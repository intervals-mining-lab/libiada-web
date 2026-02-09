// Interface for the controller's scope
interface MusicFilesScope extends ng.IScope {
}

// Angular controller class
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
