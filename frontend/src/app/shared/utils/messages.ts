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
        public readonly annotation?: AnnotationSelection,
    ) {
    }
}

export class AnnotationCreateAfterNavigate {
    constructor(
        public readonly editorId: string,
        public readonly annotation?: AnnotationSelection,
    ) {
    }
}

export class AnnotationsSelected {
    constructor(
        public readonly commentIds: ReadonlyArray<string>,
    ) {
    }
}

export class AnnotationsSelectedAfterNavigate {
    constructor(
        public readonly commentIds: ReadonlyArray<string>,
    ) {
    }
}

export class FieldSelected {
    constructor(
        public readonly editorId: string,
    ) {
    }
}

export class CommentSelected {
    constructor(
        public readonly editorId: string,
    ) {
    }
}