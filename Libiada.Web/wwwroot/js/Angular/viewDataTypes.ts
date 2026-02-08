interface SelectListItem {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Group: null; // TODO: check what type it could be
}

interface SelectListItemWithNature extends SelectListItem {
    Nature: number;
}

export interface SequenceType extends SelectListItemWithNature { }

export interface SequenceGroup extends SelectListItemWithNature { }

export interface Nature extends SelectListItem { }

export interface Notation extends SelectListItemWithNature { }

export interface Language extends SelectListItem { }

export interface PauseTreatment extends SelectListItem { }

export interface Trajectory extends SelectListItem { }

export interface Translator extends SelectListItem { }

export interface Group extends SelectListItemWithNature { }

export interface Feature extends SelectListItemWithNature { }

export interface Link extends SelectListItem { }

export interface ArrangementType extends SelectListItem { }

export interface ClusterizatorType extends SelectListItem { }

export interface ImageTransformer extends SelectListItem { }

export interface OrderTransformation extends SelectListItem { }

export interface DeviationCalculationMethod extends SelectListItem { }

export interface SegmentationCriterion extends SelectListItem { }

export interface Threshold extends SelectListItem { }

export interface CharacteristicType {
    ArrangementTypes: ArrangementType[];
    Links: Link[];
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Group: null; // TODO: check what type it could be
}

/**
* Interface for the research object
*/
export interface ResearchObject {
    id: number;
    name: string;
    nature?: number;
    group?: number;
    sequenceType?: number;
    selected?: boolean;
}

export interface SequenceImportResult {
    Matter: string;
    Name: string;
    Nature: string;
    Status: string;
    ErrorMessage?: string;
}

export interface ImportResult {
    Status: string;
    ResearchObjectName: string;
    Result: string;
    Group?: string;
    SequenceType?: string;
}
