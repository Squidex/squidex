/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export type ScriptCompletion = {
    // The autocompletion path.
    path: string;

    // The description of the autocompletion field.
    description: string;

    // The type of the autocompletion field.
    type: 'Any' | 'Array' | 'Boolean' | 'Function' | 'Object' | 'String';

    // The allowed values if the property is a string enum.
    allowedValues?: string[];

    // If the property is deprecated, a description is given.
    deprecationReason?: string;
};

export type ScriptCompletions = ReadonlyArray<ScriptCompletion>;