/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export class HistoryChannelUpdated {}

export class QueryExecuted {}

export class ClientTourStated {}

export class AnnotationCreate {
    constructor(
        public readonly editorId: string,
        public readonly annotation: AnnotationSelection,
    ) {
    }
}

export class AnnotationCreateAfterNavigate {
    constructor(
        public readonly editorId: string,
        public readonly annotation: AnnotationSelection,
    ) {
    }
}

export class AnnotationsSelect {
    constructor(
        public readonly annotations: ReadonlyArray<string>,
    ) {
    }
}

export class AnnotationsSelectAfterNavigate {
    constructor(
        public readonly annotations: ReadonlyArray<string>,
    ) {
    }
}