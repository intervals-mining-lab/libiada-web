import type {
    CharacteristicType,
    SelectedCharacteristic,
    CharacteristicsDictionary
} from "viewDataTypes";

interface CharacteristicsWithoutNotationComponentController extends ng.IController {
    characteristicTypes: CharacteristicType[];
    characteristicsDictionary: CharacteristicsDictionary;
    characteristics: SelectedCharacteristic[];

    $onInit(): void;
    addCharacteristic: () => void;
    deleteCharacteristic: (characteristic: SelectedCharacteristic) => void;
    selectLink: (characteristic: SelectedCharacteristic) => void;

}

function CharacteristicsWithoutNotationController(
    this: CharacteristicsWithoutNotationComponentController
): void {
    const ctrl: CharacteristicsWithoutNotationComponentController = this;

    ctrl.$onInit = (): void => {
        ctrl.characteristics = [];
        ctrl.addCharacteristic();
    };

    ctrl.addCharacteristic = (): void => {
        ctrl.characteristics.push({
            characteristicType: ctrl.characteristicTypes[0],
            link: ctrl.characteristicTypes[0].Links[0],
            arrangementType: ctrl.characteristicTypes[0].ArrangementTypes[0],
        });
    };

    ctrl.deleteCharacteristic = (characteristic: SelectedCharacteristic): void => {
        const characteristicIndex = ctrl.characteristics.indexOf(characteristic);
        ctrl.characteristics.splice(characteristicIndex, 1);
    };

    ctrl.selectLink = (characteristic: SelectedCharacteristic): void => {
        characteristic.link = characteristic.characteristicType.Links[0];
        characteristic.arrangementType = characteristic.characteristicType.ArrangementTypes[0];
    };
}

angular.module("libiada").component("characteristicsWithoutNotation", {
    templateUrl: `/AngularTemplates/_CharacteristicsWithoutNotation`,
    controller: CharacteristicsWithoutNotationController,
    bindings: {
        characteristicTypes: "<",
        characteristicsDictionary: "<",
    }
});
