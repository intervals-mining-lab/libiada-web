interface SelectListItem {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Group: null; // TODO: check what type it could be
}

export interface SelectListItemWithNature extends SelectListItem {
    Nature: number;
}

export interface Characteristic extends SelectListItem { }

export interface Cluster extends SelectListItem { }

export interface SequenceType extends SelectListItemWithNature { }

export interface Group extends SelectListItemWithNature { }

export interface Nature extends SelectListItem { }

export interface Notation extends SelectListItemWithNature { }

export interface RemoteDb extends SelectListItemWithNature { }

export interface Language extends SelectListItem { }

export interface PauseTreatment extends SelectListItem { }

export interface Trajectory extends SelectListItem { }

export interface Translator extends SelectListItem { }

export interface Feature extends SelectListItemWithNature { }

export interface ClusterizatorType extends SelectListItem { }

export interface ImageTransformer extends SelectListItem { }

export interface OrderTransformation extends SelectListItem { }

export interface SequenceGroup extends SelectListItemWithNature { }

export interface Multisequence extends SelectListItemWithNature { }

export interface DeviationCalculationMethod extends SelectListItem { }

export interface SegmentationCriterion extends SelectListItem { }

export interface Threshold extends SelectListItem { }

export interface Link extends SelectListItem { }

export interface ArrangementType extends SelectListItem { }

export interface CharacteristicType extends SelectListItem {
    ArrangementTypes: ArrangementType[];
    Links: Link[];
}

export interface ResearchObject extends SelectListItemWithNature {
    SequenceType: string;
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

export interface SequenceCharacteristics {
    ResearchObjectName: string;
    Characteristics: number[];
    SequenceGroupId?: number;
}
