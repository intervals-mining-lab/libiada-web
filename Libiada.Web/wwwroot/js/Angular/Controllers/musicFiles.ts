// Angular controller class
class MusicFilesOperator {
    constructor() {
        this.ngOnInit();
    }

    private ngOnInit(): void {
        const musicFiles = (): void => {
        };

        angular.module("libiada").controller("MusicFilesCtrl", [musicFiles]);
    }
}

// Wrapper function for backwards compatibility
export default function MusicFilesController(): MusicFilesOperator {
    return new MusicFilesOperator();
}
