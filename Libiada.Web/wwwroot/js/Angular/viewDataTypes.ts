export interface SequenceType {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

export interface SequenceGroup {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: string;
}

export interface Nature {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

export interface Notation {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

export interface Language {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

export interface PauseTreatment {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

export interface Trajectory {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

export interface Translator {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

export interface Group {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

export interface Feature {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Nature: number;
    Group: null; // TODO: check what type it could be
}

export interface Link {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Group: null; // TODO: check what type it could be
}

export interface ArrangementType {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Group: null; // TODO: check what type it could be
}

export interface CharacterisrticType {
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
