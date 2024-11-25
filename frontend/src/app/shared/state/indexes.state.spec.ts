/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { CreateIndexDto, DialogService, IndexesService, IndexesState, SchemasState } from '@app/shared/internal';
import { createIndex } from '../services/indexes.service.spec';
import { TestValues } from './_test-helpers';

describe('IndexesState', () => {
    const {
        app,
        appsState,
    } = TestValues;

    const index1 = createIndex(12);
    const index2 = createIndex(13);
    const schema = 'my-schema';

    let dialogs: IMock<DialogService>;
    let schemasState: IMock<SchemasState>;
    let indexesService: IMock<IndexesService>;
    let indexesState: IndexesState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        schemasState = Mock.ofType<SchemasState>();
        schemasState.setup(x => x.schemaName).returns(() => schema);

        indexesService = Mock.ofType<IndexesService>();
        indexesState = new IndexesState(appsState.object, schemasState.object, indexesService.object, dialogs.object);
    });

    afterEach(() => {
        indexesService.verifyAll();
    });

    describe('Loading', () => {
        it('should load indexes', () => {
            indexesService.setup(x => x.getIndexes(app, schema))
                .returns(() => of({ items: [index1, index2] } as any)).verifiable();

            indexesState.load().subscribe();

            expect(indexesState.snapshot.indexes).toEqual([index1, index2]);
            expect(indexesState.snapshot.isLoaded).toBeTruthy();
            expect(indexesState.snapshot.isLoading).toBeFalsy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            indexesService.setup(x => x.getIndexes(app, schema))
                .returns(() => throwError(() => 'Service Error'));

            indexesState.load().pipe(onErrorResumeNextWith()).subscribe();

            expect(indexesState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            indexesService.setup(x => x.getIndexes(app, schema))
                .returns(() => of({ items: [index1, index2] } as any)).verifiable();

            indexesState.load(true, false).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should show notification on load error if silent is false', () => {
            indexesService.setup(x => x.getIndexes(app, schema))
                .returns(() => throwError(() => 'Service Error'));

            indexesState.load(true, false).pipe(onErrorResumeNextWith()).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyError(It.isAny()), Times.once());
        });

        it('should not show notification on load error if silent is true', () => {
            indexesService.setup(x => x.getIndexes(app, schema))
                .returns(() => throwError(() => 'Service Error'));

            indexesState.load(true, true).pipe(onErrorResumeNextWith()).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyError(It.isAny()), Times.never());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            indexesService.setup(x => x.getIndexes(app, schema))
                .returns(() => of({ items: [index1, index2] } as any)).verifiable();

            indexesState.load().subscribe();
        });

        it('should not add index to snapshot', () => {
            const request: CreateIndexDto = { fields: [{ name: 'field1', order: 'Ascending' }] };

            indexesService.setup(x => x.postIndex(app, schema, request))
                .returns(() => of({})).verifiable();

            indexesState.create(request).subscribe();

            expect(indexesState.snapshot.indexes.length).toBe(2);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should not remove index from snapshot', () => {
            indexesService.setup(x => x.deleteIndex(app, index1))
                .returns(() => of({})).verifiable();

            indexesState.delete(index1).subscribe();

            expect(indexesState.snapshot.indexes.length).toBe(2);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });
});
