/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DialogService, PatternsPayload, PatternsService, PatternsState, versioned } from '@app/shared/internal';
import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { createPatterns } from './../services/patterns.service.spec';
import { TestValues } from './_test-helpers';

describe('PatternsState', () => {
    const {
        app,
        appsState,
        newVersion,
        version
    } = TestValues;

    const oldPatterns = createPatterns(1, 2, 3);

    let dialogs: IMock<DialogService>;
    let patternsService: IMock<PatternsService>;
    let patternsState: PatternsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        patternsService = Mock.ofType<PatternsService>();
        patternsState = new PatternsState(appsState.object, dialogs.object, patternsService.object);
    });

    afterEach(() => {
        patternsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load patterns', () => {
            patternsService.setup(x => x.getPatterns(app))
                .returns(() => of(versioned(version, oldPatterns))).verifiable();

            patternsState.load().subscribe();

            expect(patternsState.snapshot.isLoaded).toBeTruthy();
            expect(patternsState.snapshot.isLoading).toBeFalsy();
            expect(patternsState.snapshot.patterns).toEqual(oldPatterns.items);
            expect(patternsState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading when loading failed', () => {
            patternsService.setup(x => x.getPatterns(app))
                .returns(() => throwError('error'));

            patternsState.load().pipe(onErrorResumeNext()).subscribe();

            expect(patternsState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load when reload is true', () => {
            patternsService.setup(x => x.getPatterns(app))
                .returns(() => of(versioned(version, oldPatterns))).verifiable();

            patternsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            patternsService.setup(x => x.getPatterns(app))
                .returns(() => of(versioned(version, oldPatterns))).verifiable();

            patternsState.load().subscribe();
        });

        it('should add pattern to snapshot when created', () => {
            const updated = createPatterns(4, 5);

            const request = { name: 'new', pattern: 'a-z' };

            patternsService.setup(x => x.postPattern(app, request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            patternsState.create(request).subscribe();

            expectNewPatterns(updated);
        });

        it('should update properties when updated', () => {
            const updated = createPatterns(4, 5);

            const request = { name: 'name2_1', pattern: 'pattern2_1', message: 'message2_1' };

            patternsService.setup(x => x.putPattern(app, oldPatterns.items[1], request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            patternsState.update(oldPatterns.items[1], request).subscribe();

            expectNewPatterns(updated);
        });

        it('should remove pattern from snapshot when deleted', () => {
            const updated = createPatterns(4, 5);

            patternsService.setup(x => x.deletePattern(app, oldPatterns.items[0], version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            patternsState.delete(oldPatterns.items[0]).subscribe();

            expectNewPatterns(updated);
        });

        function expectNewPatterns(updated: PatternsPayload) {
            expect(patternsState.snapshot.patterns).toEqual(updated.items);
            expect(patternsState.snapshot.version).toEqual(newVersion);
        }
    });
});