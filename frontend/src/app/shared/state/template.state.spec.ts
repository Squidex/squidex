/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { DialogService, TemplateDto, TemplatesDto, TemplatesService, TemplatesState } from '@app/shared/internal';
import { createTemplate } from '../services/templates.service.spec';

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
                .returns(() => of(new TemplatesDto({ items: [template1, template2], _links: {} }))).verifiable();

            templatesState.load().subscribe();

            expect(templatesState.snapshot.templates).toEqual([template1, template2]);
            expect(templatesState.snapshot.isLoaded).toBeTruthy();
            expect(templatesState.snapshot.isLoading).toBeFalsy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should provide starters', () => {
            templatesService.setup(x => x.getTemplates())
                .returns(() => of(new TemplatesDto({ items: [template1, template2], _links: {} }))).verifiable();

            templatesState.load().subscribe();

            let starters: TemplateDto[] = [];
            templatesState.starters.subscribe(x => {
                starters = x;
            });

            expect(starters).toEqual([template1]);
        });

        it('should reset loading state if loading failed', () => {
            templatesService.setup(x => x.getTemplates())
                .returns(() => throwError(() => 'Service Error'));

            templatesState.load().pipe(onErrorResumeNextWith()).subscribe();

            expect(templatesState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            templatesService.setup(x => x.getTemplates())
                .returns(() => of(new TemplatesDto({ items: [template1, template2], _links: {} }))).verifiable();

            templatesState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });
});
