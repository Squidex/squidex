/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { DialogService, TemplatesService, TemplatesState } from '@app/shared/internal';
import { createTemplate } from './../services/templates.service.spec';

describe('TemplatesState', () => {
    const template1 = createTemplate(12);
    const template2 = createTemplate(13);

    let dialogs: IMock<DialogService>;
    let templatesService: IMock<TemplatesService>;
    let templatesState: TemplatesState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        templatesService = Mock.ofType<TemplatesService>();
        templatesState = new TemplatesState(templatesService.object, dialogs.object);
    });

    afterEach(() => {
        templatesService.verifyAll();
    });

    describe('Loading', () => {
        it('should load templates', () => {
            templatesService.setup(x => x.getTemplates())
                .returns(() => of({ items: [template1, template2] })).verifiable();

            templatesState.load().subscribe();

            expect(templatesState.snapshot.templates).toEqual([template1, template2]);
            expect(templatesState.snapshot.isLoaded).toBeTruthy();
            expect(templatesState.snapshot.isLoading).toBeFalsy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            templatesService.setup(x => x.getTemplates())
                .returns(() => throwError(() => 'Service Error'));

            templatesState.load().pipe(onErrorResumeNext()).subscribe();

            expect(templatesState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            templatesService.setup(x => x.getTemplates())
                .returns(() => of({ items: [template1, template2] })).verifiable();

            templatesState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });
});
